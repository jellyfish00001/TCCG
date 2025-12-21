using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Base.Utils;
using System.Transactions;

namespace SDO.Services
{
    public class ProjectSituationService : Service, IProjectSituationService
    {
        private readonly IProjectSituationDac dac;
        private readonly IProjectCommonService projectCommonService;
        private readonly IProjectCommonDac projectCommonDac;
        private readonly IRDProjectAuditService projectAuditService;
        private readonly IRDProjectAuditDac projectAuditDac;
        public ProjectSituationService(
            IRDProjectAuditService projectAuditService,
            IRDProjectAuditDac projectAuditDac,
            IProjectSituationDac dac,
            IProjectCommonService projectCommonService,
            IProjectCommonDac projectCommonDac)
        {
            this.projectAuditService = projectAuditService;
            this.projectAuditDac = projectAuditDac;
            this.dac = dac;
            this.projectCommonService = projectCommonService;
            this.projectCommonDac = projectCommonDac;
        }

        /// <summary>
        /// 取得參採情形/結案成果填報結果
        /// </summary>
        /// <param name="planNo">計畫編號</param>
        /// <returns></returns>
        public async Task<ResSituationModel> GetRDResSituation(string planNo)
        {
            // 撈取 參採情形/結案成果填報結果 資料
            ResSituationModel model = await dac.GetRDResSituation(planNo);
            // 判斷 model 是不是 null，是 null 底下撈資料就會死
            if (model != null)
            {
                List<ProjectAttachmentModel> files = await projectCommonDac.GetProjectAttachmentList(new ProjectAttachmentQueryModel
                {
                    PROJECT_NO = planNo,
                    // 01:相關檔案上傳、02:其他地方上傳
                    FILE_UP_SOURCE = "01",
                    FILE_KIND = new List<string> { "02" },
                    // 6: RD 委託研究
                    DB = (int)DBConnectionEnum.RDDBKey
                });
                model.FILE = files.FirstOrDefault();
            }
            else
            {
                // 撈不到資料回傳空 model
                model = new ResSituationModel();
            }
            return model;
        }

        /// <summary>
        /// 取得續列管一年內參採情形
        /// </summary>
        /// <param name="planNo">計畫編號</param>
        /// <returns></returns>
        public async Task<ResSituationModel> GetRDResSituaContinue(string planNo)
        {
            // 撈取 續列管一年內參採情形 資料
            ResSituationModel result = await dac.GetRDResSituaContinue(planNo);

            // 如果沒有資料，回傳空 Model
            if (result == null)
            {
                result = new ResSituationModel();
            }

            return result;
        }

        /// <summary>
        /// 儲存參採情形/結案成果填報結果
        /// </summary>
        /// <param name="model">結案成果填報 Model</param>
        /// <returns>true</returns>
        public async Task SaveRDResSituation(ResSituationModel model)
        {
            if(model != null)
            {
                // 處理檔案
                if (model.FILE != null)
                {
                    // 01:相關檔案上傳、02:其他地方上傳
                    model.FILE.FILE_UP_SOURCE = "01";
                    // 6: RD 委託研究
                    model.FILE.DB = (int)DBConnectionEnum.RDDBKey;
                    // 檔案存放資料夾名稱
                    model.FILE.FOLDER_NAME = "RD";
                    // 儲存檔案
                    projectCommonService.SaveProjectFiles(model.FILE);
                }
                // 透過計畫編號撈取結案成果填報結果的資料
                ResSituationModel resSituationModel = await GetRDResSituation(model.PLAN_NO);
                if(resSituationModel != null)
                {
                    // 無撈出資料就 Insert，撈出資料就 Update
                    if(resSituationModel.CLOSING_DATE == null)
                    {
                        await dac.InsertRDResSituation(model);
                    }
                    else
                    {
                        using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            // 儲存結案成果填報結果
                            await dac.UpdateRDResSituation(model);
                            // 執行情形送審
                            if (model.AUDIT != null && model.AUDIT.IS_SEND == 1)
                            {
                                // 產生審查紀錄編號
                                model.AUDIT.AUDIT_ID = projectAuditService.GenRDAuditId(model.AUDIT.AUDIT_YEAR, model.AUDIT.AUDIT_MONTH, model.AUDIT.PLAN_REVIEW_TYPE);
                                // 建立審查紀錄
                                await projectAuditDac.InsertRDAudit(model.AUDIT);
                                // 審核狀態 Model
                                RDAuditStatusModel rdAuditStatusModel = new()
                                {
                                    PLAN_REVIEW_TYPE = model.AUDIT.PLAN_REVIEW_TYPE,
                                    MAIN_NO = model.AUDIT.MAIN_NO,
                                    SUB_NO = model.AUDIT.SUB_NO,
                                    STATUS = "2", // 審核狀態，待審核
                                };
                                // 更改各章節審核狀態
                                await projectAuditService.ChangeReviewStatus(rdAuditStatusModel);
                            }
                            scope.Complete();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 儲存續列管一年內參採情形
        /// </summary>
        /// <param name="model">續列管一年參採情形 Model</param>
        /// <returns>true</returns>
        public async Task SaveRDResSituaContinue(ResSituationModel model)
        {
            if(model != null)
            {
                // 透過計畫編號撈取續列管一年內參採情形的資料
                ResSituationModel resSituationModel = await GetRDResSituaContinue(model.PLAN_NO);
                // 無撈出資料就 Insert，撈出資料就 Update
                if (resSituationModel.PLAN_NO == null)
                {
                    await dac.InsertRDResSituaContinue(model);
                }
                else
                {
                    using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        // 儲存續列管一年內參採情形
                        await dac.UpdateRDResSituaContinue(model);
                        // 執行情形送審
                        if (model.AUDIT != null && model.AUDIT.IS_SEND == 1)
                        {
                            // 產生審查紀錄編號
                            model.AUDIT.AUDIT_ID = projectAuditService.GenRDAuditId(model.AUDIT.AUDIT_YEAR, model.AUDIT.AUDIT_MONTH, model.AUDIT.PLAN_REVIEW_TYPE);
                            // 建立審查紀錄
                            await projectAuditDac.InsertRDAudit(model.AUDIT);
                            // 審核狀態 Model
                            RDAuditStatusModel rdAuditStatusModel = new()
                            {
                                PLAN_REVIEW_TYPE = model.AUDIT.PLAN_REVIEW_TYPE,
                                MAIN_NO = model.AUDIT.MAIN_NO,
                                SUB_NO = model.AUDIT.SUB_NO,
                                STATUS = "2", // 審核狀態，待審核
                            };
                            // 更改各章節審核狀態
                            await projectAuditService.ChangeReviewStatus(rdAuditStatusModel);
                        }
                        scope.Complete();
                    }
                }
            }
        }
    }
}
