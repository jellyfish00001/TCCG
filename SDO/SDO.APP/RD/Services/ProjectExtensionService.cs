using SDO.Models;
using SDO.Dac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Base.Utils;
using SDO.Utils;
using System.Transactions;

namespace SDO.Services
{
    public class ProjectExtensionService : Service, IProjectExtensionService
    {
        private readonly IProjectExtensionDac projectExtensionDac;
        private readonly IUserProfile userProfile;
        private readonly IProjectCommonDac projectCommonDac;
        private readonly IProjectCommonService projectCommonService;
        private readonly IRDProjectAuditService projectAuditService;
        private readonly IRDProjectAuditDac projectAuditDac;
        public ProjectExtensionService(
            IRDProjectAuditService projectAuditService,
            IRDProjectAuditDac projectAuditDac,
            IProjectExtensionDac projectExtensionDac,
            IUserProfile userProfile,
            IProjectCommonDac projectCommonDac,
            IProjectCommonService projectCommonService
        )
        {
            this.projectAuditService = projectAuditService;
            this.projectAuditDac = projectAuditDac;
            this.projectCommonService = projectCommonService;
            this.userProfile = userProfile;
            this.projectExtensionDac = projectExtensionDac;
            this.projectCommonDac = projectCommonDac;
        }

        /// <summary>
        /// 取得展延紀錄清單
        /// </summary>
        /// <param name="PLAN_NO">計畫編號</param>
        /// <returns></returns>
        public async Task<List<ProjectExtensionModel>> GetExtensionList(string PLAN_NO)
        {
            return await projectExtensionDac.GetRDExtensionList(PLAN_NO);
        }

        /// <summary>
        /// 取得展延紀錄明細
        /// </summary>
        /// <param name="EXTENSION_NO">展延編號</param>
        /// <returns></returns>
        public async Task<ProjectExtensionModel> GetRDExtension(string EXTENSION_NO)
        {
            // 取得展延紀錄明細
            ProjectExtensionModel model = await projectExtensionDac.GetRDExtension(EXTENSION_NO);
            // 判斷 model 是不是 null，是 null 底下撈資料就會死
            if (model != null)
            {
                // 取得展延紀錄明細的評核指標資料
                model.policyIndex = await projectExtensionDac.GetExtensionPolicyIndexData(EXTENSION_NO);
                // 取得展延紀錄的檔案
                List<ProjectAttachmentModel> files = await projectCommonDac.GetProjectAttachmentList(new ProjectAttachmentQueryModel
                {
                    PROJECT_NO = model.PLAN_NO,
                    FILE_UP_SOURCE = "01",
                    FILE_KIND = new List<string> { "05" },
                    SOURCE_ID = model.EXTENSION_ID,
                    DB = (int)DBConnectionEnum.RDDBKey
                });
                model.FILE = files.FirstOrDefault();
            }
            else
            {
                // 撈不到資料回傳空 model
                model = new ProjectExtensionModel();
            }
            return model;
        }

        /// <summary>
        /// 儲存展延紀錄（新增與編輯）
        /// </summary>
        /// <param name="model">展延紀錄 Model</param>
        /// <returns></returns>
        public async Task<string> SaveRDExtension(ProjectExtensionModel model)
        {
            ProjectExtensionModel extensionModel = new();
            List<ExtensionPolicyIndexModel> extensionPolicyIndexModel = new();
            string extensionNo = "";
            if(model != null)
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // 無展延編號 => 新增
                    if (string.IsNullOrEmpty(model.EXTENSION_NO))
                    {
                        // 產生展延編號
                        model.EXTENSION_NO = GenRDExtensionNo(model.EXTENSION_YEAR);
                        // 取得該登入者資訊
                        IUserData user = userProfile.GetLoginUser();
                        model.MDF_USER = user.USER_NAME;

                        /* 將 RD_REASEARCH_BASIC 資料存至 RD_RES_EXTENSION */
                        // 建立展延紀錄
                        int SCOPE_IDENTITY = await projectExtensionDac.InsertRDExtension(model);

                        /* 將 RD_RES_POLICY_INDEX 資料複製至 RD_RES_POLICY_INDEX_ADJ */
                        // 透過計畫編號取得評核指標(RD_RES_POLICY_INDEX)
                        List<ExtensionPolicyIndexModel> policyIndexData = await projectExtensionDac.GetPolicyIndexData(model.PLAN_NO);

                        // 若該計畫是否有評核指標(RD_RES_POLICY_INDEX)，有的話將資料複製過去評核指標調整表(RD_RES_POLICY_INDEX_ADJ)
                        // 先判斷評核指標 List 是否為 Null，後面 .Any() 則不會在 Null 時發生錯誤
                        if (policyIndexData != null && policyIndexData.Any())
                        {
                            // 取評核指標流水號(SEQ)
                            List<int> policyIndexSEQ = policyIndexData.Select(x => x.SEQ).ToList();
                            // 建立評核指標調整表資料
                            await projectExtensionDac.TransferRDExtensionPolicyIndexAdj(policyIndexSEQ, model.EXTENSION_NO, SCOPE_IDENTITY);
                            // 取得剛剛建立評核指標調整表資料
                            extensionPolicyIndexModel = await projectExtensionDac.GetExtensionPolicyIndexData(model.EXTENSION_NO);
                        }
                        extensionNo = model.EXTENSION_NO;
                    }
                    // 有展延編號 => 編輯
                    else
                    {
                        // 儲存展延紀錄資料
                        await projectExtensionDac.SaveRDExtension(model);

                        /* 儲存展延紀錄的評核指標(調整表ADJ)資料 */
                        // 先判斷評核指標 List 是否為 Null，後面 .Any() 則不會在 Null 時發生錯誤
                        if (model.policyIndex != null && model.policyIndex.Any())
                        {
                            List<ExtensionPolicyIndexModel> insertData = model.policyIndex.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
                            // 遍歷此 List，存入 PLAN_NO 以及 PLAN_ID 
                            insertData.ForEach(
                                x => {
                                    x.PLAN_NO = model.PLAN_NO;
                                    x.PLAN_ID = model.PLAN_ID;
                                    x.EXTENSION_NO = model.EXTENSION_NO;
                                    x.EXTENSION_ID = model.EXTENSION_ID;
                                });
                            // List 裡面有值才需要 Insert
                            if (insertData != null && insertData.Any())
                            {
                                await projectExtensionDac.InsertPolicyIndex(insertData);
                            }

                            List<ExtensionPolicyIndexModel> updateData = model.policyIndex.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
                            // List 裡面有值才需要 Update
                            if (updateData != null && updateData.Any())
                            {
                                await projectExtensionDac.UpdatePolicyIndex(updateData);
                            }

                            List<ExtensionPolicyIndexModel> deleteData = model.policyIndex.Where(x => (editTypeEnum)x.editType == editTypeEnum.Delete).ToList();
                            // List 裡面有值才需要 Delete
                            if (deleteData != null && deleteData.Any())
                            {
                                await projectExtensionDac.DeletePolicyIndex(deleteData);
                            }
                        }
                        // 展延送審
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
                    }
                    scope.Complete();
                }
                // 儲存檔案
                if (model.FILE != null)
                {
                    // 01:相關檔案上傳、02:其他地方上傳
                    model.FILE.FILE_UP_SOURCE = "01";
                    // 6: RD 委託研究
                    model.FILE.DB = (int)DBConnectionEnum.RDDBKey;
                    // 檔案序號
                    model.FILE.SOURCE_ID = model.EXTENSION_ID;
                    // 檔案存放資料夾名稱
                    model.FILE.FOLDER_NAME = "RD";
                    // 儲存檔案
                    projectCommonService.SaveProjectFiles(model.FILE);
                }
            }
            return extensionNo;
        }

        /// <summary>
        /// 產生展延編號
        /// 年度(3碼)+流水號(5碼)例：10400001
        /// </summary>
        /// <returns></returns>
        public string GenRDExtensionNo(string planYear)
        {
            // 取得流水號
            string seq = projectExtensionDac.GetExtensionNoSeq(planYear);
            return $"{planYear}{seq.PadLeft(5, '0')}";
        }
    }
}
