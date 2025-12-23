using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using SDO.Base.Utils;

namespace SDO.Services
{
    public class ProjectExecutionService : Service, IProjectExecutionService
    {
        private readonly IProjectExecutionDac dac;
        private readonly IRDProjectAuditService projectAuditService;
        private readonly IRDProjectAuditDac projectAuditDac;
        public ProjectExecutionService(
            IRDProjectAuditService projectAuditService,
            IRDProjectAuditDac projectAuditDac,
            IProjectExecutionDac dac
        )
        {
            this.projectAuditService = projectAuditService;
            this.projectAuditDac = projectAuditDac;
            this.dac = dac;
        }

        /// <summary>
        /// 取得執行情形填報清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ResPolicyListQueryModel>> GetRDResPolicyList(ResPolicyListQueryModel model)
        {
            return await dac.GetRDResPolicyList(model);
        }

        /// <summary>
        /// 取得執行情形填報明細
        /// </summary>
        /// <param name="SEQ">明細流水號</param>
        /// <returns></returns>
        public async Task<ResPolicyIndexModel> GetRDResPolicyIndex(int SEQ)
        {
            // 取得執行情形明細資料
            ResPolicyIndexModel model = await dac.GetRDResPolicyIndex(SEQ);
            // 判斷 model 是不是 null，是 null 底下撈資料就會死
            if (model != null)
            {
            }
            else
            {
                // 撈不到資料回傳空 model
                model = new ResPolicyIndexModel();
            }
            return model;
        }

        /// <summary>
        /// 儲存執行情形
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SaveRDResPolicyIndex(ResPolicyIndexModel model)
        {
            if(model != null)
            {
                using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // 儲存執行情形
                    await dac.SaveRDResPolicyIndex(model);
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
