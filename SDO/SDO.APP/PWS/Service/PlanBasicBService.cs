using SDO.Base.Utils;
using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SDO.Services
{
    public class PlanBasicBService : Service, IPlanBasicBService
    {
        private readonly IPlanBasicBDac dac;
        private readonly IPlanBasicADac dacPlanA;
        private readonly IPlanBasicAService servicePlanA;
        private readonly IUserData userInfo;
        private readonly IProjectCommonDac projectCommonDac;
        public PlanBasicBService(IPlanBasicBDac dac, IPlanBasicADac dacPlanA,
            IUserProfile userProfile,
            IPlanBasicAService servicePlanA,
            IProjectCommonDac projectCommonDac)
        {
            this.dac = dac;
            this.dacPlanA = dacPlanA;
            this.servicePlanA = servicePlanA;
            this.userInfo = userProfile.GetLoginUser();
            this.projectCommonDac = projectCommonDac;
        }

        /// <summary>
        /// 基本計畫資料存檔
        /// </summary>
        /// <returns></returns>
        public async Task<PlanBasicBModel> SavePlanBasicB(PlanBasicBModel model)
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
                var type = model.PLANKIND == "1" ? "A" : "B";
                // 取民國年
                var year = (DateTime.Now.Year);
                //  預設計畫期程為今天度+1
                var PlanYear = $"{year + 1}-01-31";
                // 計畫年
                model.PLANYEAR = year - 1911 + 1;
                // 計畫期程
                model.PLANSTARTDATE = PlanYear;
                model.PLANENDDATE = PlanYear;
                model.AWARDYM = PlanYear;
                model.MIDREPORTYM = PlanYear;
                model.FINAKREPORTYM = PlanYear;
                model.CLOSEYM = PlanYear;
                // 產生計劃編號
                PlanNo = await servicePlanA.GenProjectNo(model.PLANYEAR.ToString(), type, model.OU_ID);
            }

            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                // 新增
                if (string.IsNullOrEmpty(model.PLANNO))
                {
                    model.PLANNO = PlanNo;
                    // 存入資料庫
                    await dac.SavePlanBasicB(model);
                    SuccessMessage = "新增成功";
                }
                // 修改
                else
                {
                    // 更新基本計畫數據
                    await dac.UpdatePlanBasicB(model);
                    // 编辑跨年度經費
                    await servicePlanA.EditCrossAMTA(model.PlanCrossAMTB);
                    SuccessMessage = "存檔成功";
                }
                scope.Complete();
            }
            // 檔案上傳
            servicePlanA.UploadFiles(model.Files);
            var resultData = new PlanBasicBModel 
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
        /// 取得基本計畫資料
        /// </summary>
        /// <returns></returns>
        public async Task<PlanBasicBModel> GetPlanBasicB(string PROJECT_NO)
        {
            PlanBasicBModel model = await dac.GetPlanBasicB(PROJECT_NO);
            // 取得跨年度經費
            model.PlanCrossAMTB = await dacPlanA.GetCrossAMTA(PROJECT_NO);
            // 取得檔案
            ProjectAttachmentQueryModel attQueryModle = new()
            {
                PROJECT_NO = PROJECT_NO,
                FILE_KIND = new List<string> { "04", "05" },
                // 5代表PWS DB
                DB = (int)DBConnectionEnum.RDDBKey,
            };
            model.Files = await projectCommonDac.GetProjectAttachmentList(attQueryModle);
            return model;
        }
    }
}
