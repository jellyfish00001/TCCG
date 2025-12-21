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
    public class ProjectBasicService : Service, IProjectBasicService
    {
        private readonly IRDProjectAuditService projectAuditService;
        private readonly IRDProjectAuditDac projectAuditDac;
        private readonly IProjectBasicDac dac;
        private readonly IUserProfile userProfile;
        private readonly IProjectCommonService projectCommonService;
        private readonly IProjectCommonDac projectCommonDac;
        public ProjectBasicService(
            IRDProjectAuditService projectAuditService,
            IRDProjectAuditDac projectAuditDac,
            IProjectBasicDac dac,
            IUserProfile userProfile,
            IProjectCommonService projectCommonService,
            IProjectCommonDac projectCommonDac)
        {
            this.projectAuditService = projectAuditService;
            this.projectAuditDac = projectAuditDac;
            this.dac = dac;
            this.userProfile = userProfile;
            this.projectCommonService = projectCommonService;
            this.projectCommonDac = projectCommonDac;
        }

        /// <summary>
        /// 變更委託研究刪除與撤銷
        /// 執行類別 D 刪除 R 撤銷 L1 送出鎖定L2 解除鎖定</summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> SaveRDBasicStatus(ProjectBasicStatusModel model)
        {
            return await dac.SaveRDBasicStatus(model);
        }

        /// <summary>
        /// 取得委託研究的基本資料
        /// </summary>
        /// <param name="PLAN_NO">計畫編號</param>
        /// <returns></returns>
        public async Task<ResearchBasicModel> GetRDResearchBasic(string PLAN_NO)
        { 
            // 取得委託研究基本資料
            ResearchBasicModel model = await dac.GetRDResearchBasic(PLAN_NO);
            
            // 判斷 model 是不是 null，是 null 底下撈資料就會死
            if(model != null)
            {
                // 取得委託研究基本資料的評核指標資料
                model.policyIndex = await dac.GetPolicyIndexData(PLAN_NO);

                // 取得委託研究基本資料的檔案
                List<ProjectAttachmentModel> files = await projectCommonDac.GetProjectAttachmentList(new ProjectAttachmentQueryModel
                {
                    PROJECT_NO = PLAN_NO,
                    // 01:相關檔案上傳、02:其他地方上傳
                    FILE_UP_SOURCE = "01",
                    FILE_KIND = new List<string> { "01" },
                    // 6: RD 委託研究
                    DB = (int)DBConnectionEnum.RDDBKey
                });
                model.FILE = files.FirstOrDefault();
            }
            else
            {
                // 撈不到資料回傳空 model
                model = new ResearchBasicModel();
            }
            return model;
        }

        /// <summary>
        /// 儲存委託研究基本資料（新增與編輯）
        /// </summary>
        /// <param name="model"></param>
        /// <returns>計畫編號</returns>
        public async Task<string> SaveRDResearchBasic(ResearchBasicModel model)
        {
            // 取得該登入者資訊
            IUserData user = userProfile.GetLoginUser();
            // 存入登入者的機關 ID
            model.OU_ID = user.ORG_ID;
            // 計畫編號
            string planNo = "";
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
            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                // 判斷新增計畫還是編輯計畫
                if (string.IsNullOrEmpty(model.PLAN_NO))
                {
                    // 產生計畫編號
                    model.PLAN_NO = GenPlanNo(model.PLAN_YEAR);
                    // 新增計畫
                    await dac.InsertRDResearchBasic(model);
                    planNo = model.PLAN_NO;
                }
                else
                {
                    // 編輯計畫基本資料
                    await dac.UpdateRDResearchBasic(model);
                    // 先判斷評核指標 List 是否為 Null，後面 .Any() 則不會在 Null 時發生錯誤
                    if (model.policyIndex != null && model.policyIndex.Any())
                    {
                        List<PolicyIndexModel> insertData = model.policyIndex.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
                        // 遍歷此 List，存入 PLAN_NO 以及 PLAN_ID 
                        insertData.ForEach(
                            x => { x.PLAN_NO = model.PLAN_NO;
                                    x.PLAN_ID = model.PLAN_ID;});
                        // List 裡面有值才需要 Insert
                        if (insertData != null && insertData.Any())
                        {
                            await dac.InsertPolicyIndex(insertData);
                        }

                        List<PolicyIndexModel> updateData = model.policyIndex.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
                        // List 裡面有值才需要 Update
                        if (updateData != null && updateData.Any())
                        {
                            await dac.UpdatePolicyIndex(updateData);
                        }

                        List<int> deleteData = model.policyIndex.Where(x => (editTypeEnum)x.editType == editTypeEnum.Delete).Select(x => x.SEQ).ToList();
                        // List 裡面有值才需要 Delete
                        if (deleteData != null && deleteData.Any())
                        {
                            await dac.DeletePolicyIndex(deleteData);
                        }
                    }
                    // 基本資料送審
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
            return planNo;
        }

        /// <summary>
        /// 產生計畫編號
        /// 年度(3碼)+流水號(5碼)例：10400001
        /// </summary>
        /// <param name="planYear">計畫年度</param>
        /// <returns></returns>
        public string GenPlanNo(string planYear)
        {
            // 取得流水號
            string seq = dac.GetPlanNoSeq(planYear);
            return $"{planYear}{seq.PadLeft(5, '0')}"; 
        }
    }
}
