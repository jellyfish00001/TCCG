using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Renci.SshNet.Messages.Authentication;
using SDO.Base.Utils;
using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;


namespace SDO.Services
{
    public class PlanBasicAService : Service, IPlanBasicAService
    {
        private readonly IPlanBasicADac dac;
        private readonly IUserData userInfo;
        private readonly IProjectCommonService projectCommonService;
        private readonly IProjectCommonDac projectCommonDac;
        private readonly IPlanBasicBDac planBasicBDac;

        public PlanBasicAService(
            IPlanBasicADac dac,
            IUserProfile userProfile,
            IProjectCommonService projectCommonService,
            IProjectCommonDac projectCommonDac,
            IPlanBasicBDac planBasicBDac
            )
        {
            this.dac = dac;
            this.userInfo = userProfile.GetLoginUser();
            this.projectCommonService = projectCommonService;
            this.projectCommonDac = projectCommonDac;
            this.planBasicBDac = planBasicBDac;
        }

        /// <summary>
        /// 取得基本計畫資料
        /// </summary>
        /// <returns></returns>
        public async Task<PlanBasicAModel> GetPlanBasicA(string PROJECT_NO)
        {
            PlanBasicAModel model = await dac.GetPlanBasicA(PROJECT_NO);
            // 取得跨年度經費
            model.PlanCrossAMTA = await dac.GetCrossAMTA(PROJECT_NO);
            // 取得檢核點
            model.CusCheckpointModels = await dac.GetProjectCheckPoint(PROJECT_NO);
            // 取得檔案
            ProjectAttachmentQueryModel attQueryModle = new ()
            {
                PROJECT_NO = PROJECT_NO,
                FILE_KIND = new List<string> { "01", "02", "03" },
                // 5代表PWS資料庫
                DB = (int)DBConnectionEnum.RDDBKey,
            };
            model.Files = await projectCommonDac.GetProjectAttachmentList(attQueryModle);
            return model;
        }

        /// <summary>
        /// 產生計劃編號
        /// </summary>
        /// <param name="planYear">民國年</param>
        /// <param name="OU_ID">執行機關的機關代碼</param>
        /// <returns></returns>
        public async Task<string> GenProjectNo(string planYear, string type, string OU_ID)
        {
            // 計劃編號 = 民國年 + G + 機關代碼第4、5 碼 + 三位數流水號
            string projectNoFirst6Char = $"{planYear}{type}{OU_ID.Substring(3, 2)}";
            // 取得流水號
            string seq = await dac.GetProjectNoSeq(projectNoFirst6Char);

            return $"{projectNoFirst6Char}{seq.PadLeft(3, '0')}";
        }

        /// <summary>
        /// 更新基本計畫資料
        /// </summary>
        /// <returns></returns>
        public async Task<PlanBasicAModel> SavePlanBasicA(PlanBasicAModel model)
        {
            // 回傳訊息
            string SuccessMessage = "";
            string PlanNo = "";
            // 新增時要準備的資料
            if (string.IsNullOrEmpty(model.PLANNO))
            {
                var userdetail = userInfo;
                // 取局處, 提報機關, 提報單位
                model.OU_ID = userInfo.ORG_ID;
                model.CREATEORGOUID = userInfo.ORG_ID;
                model.CREATEUNITOUID = userInfo.OuID;
                // 取計畫類別 A:重要施政計畫;B:委託研究計畫
                string type = model.PLANKIND == "1" ? "A" : "B";
                // 取民國年
                var year = (DateTime.Now.Year);
                //  預設計畫期程為今天度+1
                var PlanYear = $"{year + 1}-01-31";
                // 計畫需求年度
                model.PLANYEAR = year - 1911 + 1;
                // 計畫期程
                model.PLANSTARTDATE = PlanYear;
                model.PLANENDDATE = PlanYear;
                // 產生計劃編號
                PlanNo = await GenProjectNo(model.PLANYEAR.ToString(), type, model.OU_ID);
                model.IS_SEND = 0;
            }
            using (TransactionScope scope = new (TransactionScopeAsyncFlowOption.Enabled))
            {
                // 新增
                if (string.IsNullOrEmpty(model.PLANNO))
                {
                    model.PLANNO = PlanNo;
                    // 存入資料庫
                    await dac.SavePlanBasicA(model);
                    SuccessMessage = "新增成功";
                }
                // 修改
                else 
                { 
                    // 更新基本計畫數據
                    await dac.UpdatePlanBasicA(model);
                    // 编辑跨年度經費
                    await EditCrossAMTA(model.PlanCrossAMTA);
                    // 檢核點編輯
                    await EditProjectCheckPoints(model, model.PLANNO);
                    
                    SuccessMessage = "存檔成功";
                }
                scope.Complete();
            }
            // 檔案上傳
            UploadFiles(model.Files);
            // 回傳開啟分頁必要資料
            var resultData = new PlanBasicAModel 
            { 
                PLANNO = model.PLANNO,
                PLANNAME = model.PLANNAME,
                PLANKIND = model.PLANKIND,
                PLANYEAR = model.PLANYEAR,
                Message = SuccessMessage 
            };
            return resultData;
        }

        /// <summary>
        /// 上傳檔案
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public void UploadFiles(List<ProjectAttachmentModel> files)
        {
            if (files != null && files.Any())
            {
                foreach (var fileModel in files)
                {
                    if (fileModel != null)
                    {
                        // 檔案上傳 DB = 5 代表PWS資料庫
                        fileModel.DB = (int)DBConnectionEnum.RDDBKey;
                        fileModel.FILE_UP_SOURCE = "01";
                        fileModel.FILE_NAME = "PWS";
                        projectCommonService.SaveProjectFiles(fileModel);
                    }
                }
            }
        }


        /// <summary>
        /// 複製計畫
        /// </summary>
        /// <returns></returns>
        public async Task<object> copyProject(string PLANNO)
        {
            // 取得計畫資料
            PlanBasicAModel model = await dac.GetPlanBasicA(PLANNO);
            // 取計畫類別 A:重要施政計畫;B:委託研究計畫
            string type = PLANNO[3].ToString();
            // 取局處, 提報機關, 提報單位
            model.OU_ID = userInfo.ORG_ID;
            model.CREATEORGOUID = userInfo.ORG_ID;
            model.CREATEUNITOUID = userInfo.OuID;
            // 產生計劃編號
            string planNo = await GenProjectNo(model.PLANYEAR.ToString(), type, model.OU_ID);
            model.PLANNO = planNo;
            model.OldPlanNo = PLANNO;
            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                await dac.copyProject(model);
                await dac.copyProjectCheckPoint(model);
                
                scope.Complete();
            }
            var NO = new { PLANNO = planNo, PLANNAME = model.PLANNAME, PLANKIND = model.PLANKIND, model.PLANYEAR };
            return NO;
        }

        /// <summary>
        /// 跨年度經費處理
        /// </summary>
        /// <param name="crossAMTAList"></param>
        public async Task EditCrossAMTA(List<PlanCrossAMTAModel> models)
        {
            //  篩選出新增、修改、刪除的資料
            List<PlanCrossAMTAModel> insertList = models.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
            List<PlanCrossAMTAModel> updateList = models.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
            List<PlanCrossAMTAModel> deleteList = models.Where(x => (editTypeEnum)x.editType == editTypeEnum.Delete).ToList();
            // 如果有值再做
            if (insertList.Any())
            {
                await dac.SaveCrossAMTA(insertList);
            }

            if (updateList.Any())
            {
                await dac.UpdateCrossAMTA(updateList);
            }

            if (deleteList.Any())
            {
                await dac.DeleteCrossAMTA(deleteList);
            }
        }

        /// <summary>
        /// 檢核點處理
        /// </summary>
        /// <param name="model"></param>
        private async Task EditProjectCheckPoints(PlanBasicAModel model, string PLANNO)
        {

            /// 執行情形是否相同
            if (!model.RUNWAY_C.Equals(model.OLD_RUNWAY_C))
            {
                await dac.DeleteProjectCheckPoints(PLANNO);
            }

            // 檢查是否有值
            if (model.CusCheckpointModels != null && model.CusCheckpointModels.Any())
            {
                // 調整8小時時差
                foreach (var checkpoint in model.CusCheckpointModels)
                {
                    if (checkpoint.ESTIMATED_ENDDATE.HasValue)
                    {
                        checkpoint.ESTIMATED_ENDDATE = checkpoint.ESTIMATED_ENDDATE.Value.AddHours(8);
                    }
                }
            }

            // 依照 editType 篩選新增、修改、刪除的資料
            var insertList = model.CusCheckpointModels.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
            var updateList = model.CusCheckpointModels.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
            var deleteList = model.CusCheckpointModels.Where(x => (editTypeEnum)x.editType == editTypeEnum.Delete).ToList();
            // 如果有值再做
            if (insertList.Any())
            {
                await dac.InsertProjectCheckPoint(insertList);
            }

            if (updateList.Any())
            {
                await dac.UpdateProjectCheckPoint(updateList);
            }

            if (deleteList.Any())
            {
                await dac.DeleteProjectCheckPoint(deleteList);
            }
        }
    }
}
