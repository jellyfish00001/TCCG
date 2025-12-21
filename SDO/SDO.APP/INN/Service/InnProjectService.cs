using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Logging;
using SDO.APP.INN.Models.ProjectManage;
using SDO.Base.Utils;
using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SDO.Services
{
    public class InnProjectService : Service, IInnProjectService
    {
        private readonly IInnProjectDac dac;
        private readonly IUserData userInfo;
        private readonly IProjectCommonService projectCommonService;
        private readonly IProjectCommonDac projectCommonDac;

        public InnProjectService(
            IInnProjectDac dac,
            IUserProfile userProfile,
            IProjectCommonService projectCommonService,
            IProjectCommonDac projectCommonDac
            )
        {
            this.dac = dac;
            this.userInfo = userProfile.GetLoginUser();
            this.projectCommonService = projectCommonService;
            this.projectCommonDac = projectCommonDac;
        }

        /// <summary>
        /// 取得基本資料
        /// </summary>
        /// <param name="projectNo"></param>
        /// <returns></returns>
        public async Task<InnProjectBasicFillModel> GetInnBasic(string projectNo)
        {
            InnProjectBasicFillModel result;

            result = new InnProjectBasicFillModel()
            {
                //提案基本資料
                InnProjectBasic = await dac.GetProjectInnBasic(projectNo) ?? new InnProjectBasicModel(),

                //涉及其他提案類別
                InnProjectProposalType = await dac.GetInnProjectProposalType(projectNo) ?? new List<InnProjectProposalTypeModel>(),

                //參與提案人
                InnPartner = await dac.GetInnPartner(projectNo) ?? new List<InnPartnerModel>(),

                //自訂欄位
                InnProjectCusFieldValue = await dac.GetProjectCusFieldValue(projectNo) ?? new List<InnProjectCusFieldValueModel>()

            };
            result.InnProjectCusFields = await dac.GetInnProjectCusField(result.InnProjectBasic.INN_YEAR) ?? new List<ProjectCusFieldModel>();

            ProjectAttachmentQueryModel attQueryModel = new()
            {
                PROJECT_NO = projectNo,
                FILE_KIND = new List<string> { "01" },
                DB = (int)DBConnectionEnum.RDDBKey
            };
            result.Files = await projectCommonDac.GetProjectAttachmentList(attQueryModel);

            return result;
        }


        /// <summary>
        /// 儲存創新提案基本資料(含計畫基本資料、提案提案類別、自定義欄位、參與提案人)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<string> SaveInnBasic(InnProjectBasicFillModel model)
        {
            //編號
            string projectNo = model.INN_PLAN_NO;
            if (model.editType == 3)
            {
                await DeleteInnProject(projectNo);
            }
            else
            {
                using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
                {

                    //儲存計畫基本資料
                    projectNo = await AddMdfInnProjectBasic(model.InnProjectBasic);

                    await AddMdfInnPartner(model.InnPartner, projectNo);

                    await AddMdfProjectProposalType(model.InnProjectProposalType, projectNo);

                    await AddMdfProjectCusFieldValue(model.InnProjectCusFieldValue, projectNo);

                  
                    // 檔案上傳
                    if (model.Files != null && model.Files.Any() && model.Files[0] != null)
                    {
                        ProjectAttachmentModel fileModel = model.Files[0];
                        fileModel.DB = (int)DBConnectionEnum.RDDBKey;
                        fileModel.FOLDER_NAME = "INN";
                        projectCommonService.SaveProjectFiles(model.Files[0]);
                    }

                    scope.Complete();
                }
            }

            return projectNo;
        }

        /// <summary>
        /// 儲存創新提案基本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<string> AddMdfInnProjectBasic(InnProjectBasicModel model)
        {

            if (string.IsNullOrEmpty(model.INN_PLAN_NO))
            {
                //取得聯絡人資訊
                model.OU_ID = userInfo.ORG_ID;
                model.CONTACT_NAME = userInfo.USER_NAME;
                model.CONTACT_EMAIL = userInfo.USER_EMAIL;
                model.CONTACT_TEL = userInfo.USER_TEL;
                model.CONTACT_ORG = userInfo.ORG_NAME;
                model.CONTACT_TITLE = userInfo.UsrTitle;
                //提案機關
                model.SPONSOR_ORG = userInfo.ORG_ID;

                //產生計劃編號
                model.INN_PLAN_NO = GenProjectNo(model.INN_YEAR.PadLeft(3, '0'), model.OU_ID);
                //新增計畫基本資料
                await dac.InsertInnProjectBasic(model);
            }
            else
            {
                //修改計畫基本資料
                await dac.UpdateInnProjectBasic(model);
            }
            return model.INN_PLAN_NO;
        }

        /// <summary>
        /// 參與提案人
        /// </summary>
        /// <param name="model"></param>
        /// <param name="planNo">提案編號</param>
        private async Task AddMdfInnPartner(List<InnPartnerModel> model, string planNo)
        {
            if (model != null)
            {
                foreach (InnPartnerModel item in model)
                {
                    item.INN_PLAN_NO = planNo;
                }

                await dac.DeleteInnPartner(planNo);
                await dac.InsertInnPartner(model);
            }
        }


        /// <summary>
        /// 提案主題
        /// </summary>
        /// <param name="model"></param>
        /// <param name="planNo">提案編號</param>
        private async Task AddMdfProjectProposalType(List<InnProjectProposalTypeModel> model, string planNo)
        {
            if (model != null)
            {
                foreach (InnProjectProposalTypeModel item in model)
                {
                    item.INN_PLAN_NO = planNo;
                }

                await dac.DeleteProposalType(planNo);
                await dac.InsertProjectProposalType(model);
            }
        }

        /// <summary>
        /// 自訂欄位
        /// </summary>
        /// <param name="model"></param>
        /// <param name="planNo">提案編號</param>
        private async Task AddMdfProjectCusFieldValue(List<InnProjectCusFieldValueModel> model, string planNo)
        {
            if (model != null)
            {
                foreach (InnProjectCusFieldValueModel item in model)
                {
                    item.INN_PLAN_NO = planNo;
                }

                await dac.DeleteProjectCusFieldValue(planNo);
                await dac.InsertProjectCusFieldValue(model);
            }
        }

        /// <summary>
        /// 刪除提案
        /// </summary>
        /// <param name="planNo"></param>
        private async Task DeleteInnProject(string planNo)
        {
            await dac.DeleteInnProjectBasic(planNo);
            await dac.DeleteProjectCusFieldValue(planNo);
            await dac.DeleteProposalType(planNo);
            await dac.DeleteInnPartner(planNo);
        }


        /// <summary>
        /// 產生計劃編號
        /// </summary>
        /// <param name="planYear">民國年</param>
        /// <param name="OU_ID">執行機關的機關代碼</param>
        /// <returns></returns>
        public string GenProjectNo(string planYear, string OU_ID)
        {
            // 計劃編號 = 民國年 + G + 機關代碼第4、5 + 三位數流水號
            string projectNoFirst6Char = $"{planYear}G{OU_ID.Substring(3, 2)}";
            // 取得流水號
            string seq = dac.GetProjectNoSeq(projectNoFirst6Char);

            return $"{projectNoFirst6Char}{seq.PadLeft(3, '0')}";
        }


    }

}

