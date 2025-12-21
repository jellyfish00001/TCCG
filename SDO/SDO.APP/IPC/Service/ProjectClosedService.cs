using SDO.Base.Utils;
using SDO.Base.Utils.Models;
using SDO.Dac;
using SDO.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class ProjectClosedService : Service, IProjectClosedService
    {
        private readonly IProjectClosedDac dac;
        private readonly IProjectService projectService;
        private readonly IProjectDac projectDac;
        private readonly IProjectListDac projectListDac;
        private readonly IProjectCommonDac projectCommonDac;
        private readonly IProjectExecuteService projectExecuteService;
        private readonly IProjectCommonService projectCommonService;
        private readonly IMailSetService mailService;


        public ProjectClosedService(IProjectClosedDac dac,
            IProjectDac projectDac,
            IProjectCommonDac projectCommonDac,
            IProjectListDac projectListDac, 
            IProjectService projectService,
            IProjectCommonService projectCommonService,
            IProjectExecuteService projectExecuteService, IMailSetService mailService)
        {
            this.dac = dac;
            this.projectDac = projectDac;
            this.projectCommonDac = projectCommonDac;
            this.projectListDac = projectListDac;
            this.projectService = projectService;
            this.projectCommonService = projectCommonService;
            this.projectExecuteService = projectExecuteService;
            this.mailService = mailService;
        }
        /// <summary>
        /// 取得計畫結案資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectFillCloseModel> GetProjectFillClose(string PROJECT_NO)
        {
            ProjectFillCloseModel model = await dac.GetProjectFillClose(PROJECT_NO);

            List<string> logStatuses = new List<string> { "6", "7", "8" };
            model.ProjLogs = (await projectListDac.GetProjectLogList(PROJECT_NO)).Where(x => logStatuses.Contains(x.LOG_STATUS_C)).ToList();

            return model;
        }

        /// <summary>
        /// 儲存計畫結案資料
        /// </summary>
        /// <param name="model"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public RtnResultModel SaveProjectFillClose(ProjectFillCloseModel model)
        {
            #region 檢查所有檔案
            if (model.EditFiles != null)
            {
                List<ProjectAttachmentModel> fileList = new () { 
                   new ProjectAttachmentModel
                   {
                       PROJECT_NO = model.PROJECT_NO,
                       FILE_KIND = "16",
                       EditFiles = model.EditFiles,
                   }
                };
                List<string> result = projectCommonService.CheckFileName(fileList);
                if (result.Any())
                {
                    return ChangeResult(false, $"存檔失敗，檔名 {string.Join("、", result)} 重複");
                }
            }
            #endregion

            dac.BeginTransaction();

            // 新增/修改計畫實際經費支用
            dac.AddMdfProjectPayment(model);

            // 更新計畫檔案資料
            if (model.EditFiles != null && model.EditFiles.Any())
            {
                foreach (var editFile in model.EditFiles.Where(x => x.EditType > 0))
                {
                    var projAttachment = model.ProjAttachments.Where(x => x.FILE_NAME == editFile.FileName || x.FILE_NAME == $"{editFile.FileName}{editFile.Extension}").FirstOrDefault();
                    ProjectAttachmentModel attachModel = new()
                    {
                        IDENTITY_FIELD = projAttachment != null ? projAttachment.IDENTITY_FIELD : 0,
                        PROJECT_NO = model.PROJECT_NO,
                        FILE_NAME = editFile.FileName,
                        FILE_KIND = "16",
                        FILE_UP_SOURCE = "02",
                        EditFiles = new List<UploadTempFileModel> { editFile },
                        editType = editFile.EditType == 1 ? (int)editTypeEnum.Add : (int)editTypeEnum.Delete,
                        SOURCE_ID = null,
                        IsMultiple = true
                    };
                    projectCommonService.MdfProjectAttachment(attachModel);
                }
            }

            // 結案審核異動資料
            if (model.isAudit)
            {
                int logId = 0;
                // 確認送出
                if (model.isSubmit)
                {
                    // Y: 審核通過 R: 退回補正 N: 審核未通過(計畫狀態回到執行情形)
                    model.PROJECT_STATUS = model.REVIEW_RESULT == "N" ? "4" : model.REVIEW_RESULT == "Y" ? "7" : "6";
                    model.MEMO_CLOSE = model.REVIEW_COMMENTS;
                    // 結案時間
                    model.FINISH_DATE = model.REVIEW_RESULT == "Y" ? System.DateTime.Now : null;
                    // 更新計畫狀態
                    dac.UpdateProjectCloseRvwResult(model);
                    // 新增計畫異動記錄檔
                    logId = projectService.InsertProjectBasicLog(model.PROJECT_NO, "S2", model.LOG_STATUS, model.REVIEW_COMMENTS);
                    // 審核未通過 清除驗收檢核點
                    if (model.REVIEW_RESULT == "N")
                    {
                        int SEQ = dac.GetWorkEnd(model.PROJECT_NO);
                        dac.ClearWorkEnd(model.PROJECT_NO, SEQ);
                    }
                }
                // 更新計畫審查資料檔
                projectDac.UpdateProjectAudit(new ProjectAuditModel()
                {
                    LOG_ID = logId,
                    PROJECT_NO = model.PROJECT_NO,
                    // 結案審查
                    PLAN_REVIEW_TYPE = "P2",
                    REVIEW_RESULT = model.REVIEW_RESULT,
                    REVIEW_COMMENTS = model.REVIEW_COMMENTS,
                    IS_SEND = model.isSubmit
                });
            }

            dac.Commit();
            // 結案審核寄信
            if(model.isAudit && model.isSubmit)
            {
                // 結案審核寄信
                Task<bool> sendMailTask = Task.Run(async () => await SendProjectClosedAuditMail(model.PROJECT_NO, model.REVIEW_RESULT, model.REVIEW_COMMENTS));
                Task.Run(() => Task.WaitAll(sendMailTask)).Wait();
            }

            return ChangeResult(true, "存檔成功");
        }

        /// <summary>
        /// 結案審核寄信
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="rvwResult"></param>
        /// <param name="rvwComments"></param>
        /// <returns></returns>
        private async Task<bool> SendProjectClosedAuditMail(string PROJECT_NO, string rvwResult, string rvwComments)
        {
            ProjectBasicModel basicModel = await projectDac.GetProjectBasic(PROJECT_NO) ?? new();
            // 取得計畫實際承辦人
            ProjectFillCkptComModel projContactModel = await projectExecuteService.GetProjectFillCkptCom(PROJECT_NO);
            #region 取得收件人資訊
            List<RecipientModel> allRcvs = new();
            List<DeptContactRcvQueryModel> rcvQueryModel = new()
            {
                new() { OrgId = basicModel.EXEC_ORGAN_C, MailType = "1" },// 正本機關窗口
                new() { OrgId = "380220000A", MailType = "2" },           // 副本智發會窗口
            };
            // 取得收件人資料
            List<RecipientModel> rdecRcvs = await projectCommonService.GetDeptContactRcvData(rcvQueryModel);
            allRcvs.AddRange(rdecRcvs);

            if (projContactModel != null)
            {
                // 正本 計畫實際承辦人收件資訊
                RecipientModel projRelContactRcv = new()
                {
                    MAIL_TITLE = projContactModel.REAL_CONTACT,
                    MAIL_ADDRESS = projContactModel.REAL_EMAIL,
                    MAIL_TYPE = "1"
                };
                allRcvs.Add(projRelContactRcv);
            }

            #endregion 取得收件人資訊
            MailTemplateParamModel mailTemplateParam = await projectCommonDac.GetMailTemplateParam(PROJECT_NO);
            mailTemplateParam.MEMO_EVALUATION = rvwComments;
            if (mailTemplateParam != null)
            {
                MailTemplateSendModel<MailTemplateParamModel> mailModel = new()
                {
                    // Y: 審核通過 R: 退回補正 N: 審核未通過
                    TemplateId = rvwResult == "Y" ? "RVW_PRJ_CLOSE_PASS" : rvwResult == "R" ? "RVW_PRJ_CLOSE_RETURN" : "RVW_PRJ_CLOSE_REJECT",
                    MailAddrs = allRcvs,
                    TemplatePara = mailTemplateParam
                };
                return await mailService.SetTemplateSend(mailModel);
            }
            return false;
        }
    }
}

