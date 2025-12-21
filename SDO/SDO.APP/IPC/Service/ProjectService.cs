using Microsoft.Extensions.Logging;
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
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class ProjectService : Service, IProjectService
    {
        private readonly IProjectDac dac;
        private readonly IProjectCommonService projectCommonService;
        private readonly IProjectCommonDac projectCommonDac;
        private readonly IUserData userInfo;
        private readonly IProjectAdjustDac projectAdjustDac;
        private readonly IMailSetService mailService;
        private readonly IProjectListDac projectListDac;

        public ProjectService(IProjectDac dac,
            IProjectCommonService projectcommonservice,
            IProjectCommonDac projectCommonDac,
            IUserProfile userProfile,
            IProjectAdjustDac projectAdjustDac,
            IMailSetService mailService,
            IProjectListDac projectListDac)
        {
            this.dac = dac;
            this.projectCommonService = projectcommonservice;
            this.projectCommonDac = projectCommonDac;
            this.userInfo = userProfile.GetLoginUser();
            this.projectAdjustDac = projectAdjustDac;
            this.mailService = mailService;
            this.projectListDac = projectListDac;
        }

        #region 計畫章節
        /// <summary>
        /// 取得計畫章節表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectChapterListModel>> GetProjectChapter(ProjectChapterQueryModel model)
        {
            return await dac.GetProjectChapter(model);
        }
        #endregion

        #region 計劃基本資料 
        /// <summary>
        /// 取得計畫基本資料(含計畫基本資料、計畫經費來源、計畫建設類別、計畫協辦機關)
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="logId">取歷程檔</param>
        /// <returns></returns>
        public async Task<ProjectBasicFillModel> GetProjectBasicFill(string projectNo, int logId)
        {
            ProjectBasicFillModel result;
            if (string.IsNullOrEmpty(projectNo))
            {
                result = new ProjectBasicFillModel()
                {
                    //取得計畫基本資料
                    ProjectBasic = new ProjectBasicModel()
                    {
                        PROJECT_YEAR = DateTime.Now.ToTwDateString("yyy"),
                        // 計畫屬性，預設'0'
                        PROJECT_TYPE = 0,
                        EXEC_ORGAN_C = userInfo.ORG_ID,
                        EXEC_UNDERTAKER_C = userInfo.USER_ID,
                        MASTER_ORGAN_C = userInfo.ORG_ID,
                        MASTER_UNDERTAKER_C = userInfo.USER_ID,
                        //預設地址為 桃園區縣府路1號 - 桃園市政府
                        // 單點地圖定位-坐標X
                        X_COORD = "24.993098524588383",
                        // 單點地圖定位-坐標Y
                        Y_COORD = "121.30101509392262",
                    }
                };
            }
            else
            {
                result = new ProjectBasicFillModel()
                {
                    //取得計畫基本資料
                    ProjectBasic = await dac.GetProjectBasic(projectNo, logId) ?? new ProjectBasicModel(),
                    //取得計畫經費來源
                    ProjectBudgetSourceG = await GetProjectBudgetSourceG(projectNo, logId),
                    //取得計畫建設類別
                    ProjectBuildKind = await dac.GetProjectBuildKind(projectNo, logId) ?? new List<ProjectBuildKindModel>(),
                    //取得計畫協辦機關
                    ProjectAsstOrg = await dac.GetProjectAsstOrg(projectNo, logId) ?? new List<ProjectAsstOrgModel>()
                };

                List<string> logStatuses = new List<string> { "3", "4", "6", "7", "8" };
                result.ProjectBasic.ProjLogs = (await projectListDac.GetProjectLogList(projectNo)).Where(x => logStatuses.Contains(x.LOG_STATUS_C)).ToList();
            }

            return result;
        }

        /// <summary>
        /// 取得計畫經費來源
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="logId">取歷程檔</param>
        /// <returns></returns>
        private async Task<List<ProjectBudgetSourceGModel>> GetProjectBudgetSourceG(string projectNo, int logId)
        {
            List<ProjectBudgetSourceGModel> result = await dac.GetProjectBudgetSourceG(projectNo, logId) ?? new List<ProjectBudgetSourceGModel>();
            // 取得 核定函檔案，(02:其他地方上傳;05:前瞻計畫核定文件)
            List<ProjectAttachmentModel> files = await projectCommonService.GetProjectAttachment(projectNo, "02", new List<string> { "05", "06" });
            result.ForEach(x =>
            {
                x.FILE = files.Where(y => x.IDENTITY_FIELD == y.SOURCE_ID).ToList();
            });
            return result;
        }

        /// <summary>
        /// 儲存計畫基本資料(含計畫基本資料、計畫經費來源、計畫建設類別、計畫協辦機關)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SaveProjectBasicAdd(ProjectBasicFillModel model)
        {
            //列管編號
            string projectNo = string.Empty;

            if (model.ProjectBasic == null)
            {
                return ChangeResult(false, "計畫基本資料有誤，存檔失敗");
            }

            #region 檢查所有檔案
            if (model.ProjectBudgetSourceG.Any())
            {
                List<ProjectAttachmentModel> fileList = model.ProjectBudgetSourceG.Where(x => x.FILE != null).SelectMany(x => x.FILE).ToList();
                List<string> result = projectCommonService.CheckFileName(fileList, model.ProjectBasic.PROJECT_NO);
                if (result.Any())
                {
                    return ChangeResult(false, $"存檔失敗，檔名 {string.Join("、", result)} 重複");
                }
            }
            #endregion

            dac.BeginTransaction();

            //儲存計畫基本資料
            projectNo = AddMdfProjectBasic(model.ProjectBasic);

            //儲存計畫經費來源
            AddMdfProjectBudgetSourceG(model.ProjectBudgetSourceG, projectNo);

            //儲存計畫建設類別
            AddMdfProjectBuildKind(model.ProjectBuildKind, projectNo);

            //儲存計畫協辦機關
            AddMdfProjectAsstOrg(model.ProjectAsstOrg, projectNo);

            dac.Commit();
            return ChangeResult(true, projectNo);
        }

        /// <summary>
        /// 儲存計畫基本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string AddMdfProjectBasic(ProjectBasicModel model)
        {
            if (string.IsNullOrEmpty(model.PROJECT_NO))
            {
                //產生計劃編號
                model.PROJECT_NO = GenProjectNo(model.PROJECT_YEAR.PadLeft(3, '0'), model.EXEC_ORGAN_C);
                //新增計畫基本資料
                dac.InsertProjectBasic(model);
                dac.InsertProjectBasicLog(new ProjectBasicLogModel
                {
                    PROJECT_NO = model.PROJECT_NO,
                    PROJECT_NAME = model.PROJECT_NAME,
                    PROJECT_YEAR = model.PROJECT_YEAR,
                    PROJECT_STAGE = "S1",
                    LOG_STATUS = "0"
                });
            }
            else
            {
                //修改計畫基本資料
                dac.UpdateProjectBasic(model);
            }
            return model.PROJECT_NO;
        }

        /// <summary>
        /// 儲存計畫經費來源
        /// </summary>
        /// <param name="model"></param>
        /// <param name="projectNo"></param>
        private void AddMdfProjectBudgetSourceG(List<ProjectBudgetSourceGModel> model, string projectNo)
        {
            foreach (ProjectBudgetSourceGModel item in model)
            {
                switch ((editTypeEnum)item.editType)
                {
                    case editTypeEnum.Add:
                        item.PROJECT_NO = projectNo;
                        item.IDENTITY_FIELD = dac.InsertProjectBudgetSourceG(item);
                        break;
                    case editTypeEnum.Modify:
                        dac.UpdateProjectBudgetSourceG(item);
                        break;
                    case editTypeEnum.Delete:
                        dac.DeleteProjectBudgetSourceG(item.IDENTITY_FIELD);
                        break;
                }

                if (item.FILE != null)
                {
                    foreach (var file in item.FILE)
                    {
                        file.PROJECT_NO = projectNo;
                        // 01:相關檔案上傳、02:其他地方上傳
                        file.FILE_UP_SOURCE = "02";
                        //對應經費來源的PK，ref PROJECT_BUDGET_SOURCE_G.IDENTITY_FIELD
                        file.SOURCE_ID = item.IDENTITY_FIELD;
                        // 如果中央預算來源是空白，則刪除檔案；沒空白則原邏輯
                        file.editType = string.IsNullOrEmpty(item.PLAN_ITEM_C) ? (int)editTypeEnum.Delete : file.editType;
                        //儲存計畫檔案資料
                        projectCommonService.MdfProjectAttachment(file);
                    }
                }
            }
        }

        /// <summary>
        /// 儲存計畫建設類別
        /// </summary>
        /// <param name="model"></param>
        /// <param name="projectNo"></param>
        private void AddMdfProjectBuildKind(List<ProjectBuildKindModel> model, string projectNo)
        {
            //刪除計畫建設類別 => 根據projectNo，砍掉已選取的資料
            dac.DeleteProjectBuildKind(projectNo);
            //新增已選取的資料
            dac.InsertProjectBuildKind(model, projectNo);
        }

        /// <summary>
        /// 儲存計畫協辦機關
        /// </summary>
        /// <param name="model"></param>
        /// <param name="projectNo"></param>
        private void AddMdfProjectAsstOrg(List<ProjectAsstOrgModel> model, string projectNo)
        {
            foreach (ProjectAsstOrgModel item in model)
            {
                switch ((editTypeEnum)item.editType)
                {
                    //新增計畫協辦機關
                    case editTypeEnum.Add:
                        {
                            item.PROJECT_NO = projectNo;
                            dac.InsertProjectAsstOrg(item);
                            break;
                        }
                    //修改計畫協辦機關
                    case editTypeEnum.Modify:
                        {
                            dac.UpdateProjectAsstOrg(item);
                            break;
                        }
                    //刪除計畫協辦機關
                    case editTypeEnum.Delete:
                        {
                            dac.DeleteProjectAsstOrg(item.ASST_ID);
                            break;
                        }
                    default:
                        break;
                }
            }
        }
        #endregion

        #region 計畫檢核點設定
        /// <summary>
        /// 取得計畫檢核點設定
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="LOG_ID">取歷程檔</param>
        /// <returns></returns>
        public async Task<ProjectCheckpointModel> GetProjectCheckpoint(string PROJECT_NO, int LOG_ID = 0)
        {
            ProjectCheckpointModel model = await dac.GetProjectCheckpoint(PROJECT_NO, LOG_ID) ?? new();
            model.AdjustScheHistoryModels = await projectAdjustDac.GetProjAdjScheHistory(new List<string> { PROJECT_NO });
            model.CusCheckpointModels = await dac.GetProjectCusCheckpoint(PROJECT_NO, LOG_ID);
            return model;
        }

        /// <summary>
        /// 儲存計劃檢核點設定
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SaveProjectCheckpoint(ProjectCheckpointModel model)
        {
            dac.BeginTransaction();
            // 更新計劃基本資料
            dac.UpdateProjectCheckpoint(model);
            // 更新計畫預定實際期程
            dac.UpdateProjectControlExecute(model);
            if (model.CusCheckpointModels != null && model.CusCheckpointModels.Any())
            {
                if (!model.RUNWAY_C.Equals(model.OLD_RUNWAY_C))
                {
                    dac.DeleteProjectCustomChkItemDate(model.PROJECT_NO);
                }

                List<ProjectCusCheckpointModel> insertData = model.CusCheckpointModels
                    .Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
                List<ProjectCusCheckpointModel> updateData = model.CusCheckpointModels
                    .Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
                List<ProjectCusCheckpointModel> deleteData = model.CusCheckpointModels
                    .Where(x => (editTypeEnum)x.editType == editTypeEnum.Delete).ToList();

                // 新增自訂檢核點設定資料
                dac.InsertProjectCustomChkItemDate(insertData);
                // 更新自訂檢核點設定資料
                dac.UpdateProjectCustomChkItemDate(updateData);
                // 刪除自訂檢核點設定資料
                dac.DeleteProjectCustomChkItemDate(deleteData);
            }

            dac.Commit();
            return ChangeResult(true, "存檔成功");
        }

        /// <summary>
        /// 判斷計畫最後一個檢核點是否有填實際完成日期
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> IsLasttActualEnddate(string PROJECT_NO)
        {
            return await dac.IsLasttActualEnddate(PROJECT_NO);
        }
        #endregion

        #region 計畫送審
        /// <summary>
        /// 驗證計劃送出
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectSubmitResultModel> CheckProjectCanSubmit(string PROJECT_NO)
        {
            ProjectSubmitResultModel submitResultModel = new();
            List<ProjectSubmitErrorModel> errModels = new();

            // 取得計畫基本資料驗證Model 
            ProjectBasicCheckModel projBasicChkModel = JsonDeserialize<ProjectBasicCheckModel>(JsonSerialize(await dac.GetProjectBasic(PROJECT_NO)));

            // 檢查是否已送出
            if (projBasicChkModel != null && projBasicChkModel.PROJECT_STATUS == "2")
            {
                submitResultModel.IsSubmitted = true;
            }
            else
            {
                // 計畫基本資料必填欄位
                List<string> projBasicErrMsgs = projectCommonService.CheckModelRequiredField<ProjectBasicModel>(projBasicChkModel);
                string chapterId;
                // 建設類別驗證
                bool buildKindValid = (await dac.GetProjectBuildKind(PROJECT_NO)).Any();
                if (!buildKindValid)
                    projBasicErrMsgs.Add("「建設類別」");

                // 經費來源驗證
                bool budgetValid = CheckBudgetSourceGIsValid(await dac.GetProjectBudgetSourceG(PROJECT_NO));
                if (!budgetValid)
                    projBasicErrMsgs.Add("「經費來源」");

                if (projBasicErrMsgs.Any())
                {
                    chapterId = "ProjectFillBasic";
                    errModels.Add(new ProjectSubmitErrorModel()
                    {
                        Chapter = "計畫基本資料",
                        ChapterId = chapterId,
                        ChapterUrl = await projectCommonDac.GetChapterPath(chapterId),
                        ErrMsg = $"{string.Join('、', projBasicErrMsgs)} 尚未填報"
                    });
                }

                // 驗證檢核點
                ProjectCheckpointModel checkpointModel = await GetProjectCheckpoint(PROJECT_NO);

                string checkpointErrMsg = projectCommonService.CheckProjectCheckpointValid(checkpointModel);
                if (checkpointErrMsg.Length != 0)
                {
                    chapterId = "ProjectFillCheckpoint";
                    errModels.Add(new ProjectSubmitErrorModel
                    {
                        Chapter = "檢核點設定",
                        ChapterId = chapterId,
                        ChapterUrl = await projectCommonDac.GetChapterPath(chapterId),
                        ErrMsg = checkpointErrMsg
                    });
                }

            }
            submitResultModel.ErrorModels = errModels;

            return submitResultModel;
        }

        /// <summary>
        /// 經費來源資料檢核
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool CheckBudgetSourceGIsValid(List<ProjectBudgetSourceGModel> models )
        {
            // 必填欄位檢核
            foreach(ProjectBudgetSourceGModel model in models)
            {
                if (string.IsNullOrEmpty(model.PLAN_YEAR) || string.IsNullOrEmpty(model.BUDGET_CLASS) 
                    || string.IsNullOrEmpty(model.PLAN_ITEM_L))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 儲存計畫立案送審
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public RtnResultModel SaveProjectFillAddSubmit(string PROJECT_NO)
        {

            dac.BeginTransaction();
            #region 基本資料相關
            // 更新計劃狀態為立案審核
            dac.UpdateProjectStatusAndMemo(PROJECT_NO, "2");

            // 新增計畫基本資料異動記錄檔
            // PROJECT_STAGE : 2 (立案作業階段)
            // LOG_STATUS : 2 (立案送審階段)
            int logId = InsertProjectBasicLog(PROJECT_NO, "S1", "2");

            // 新增計劃基本資料歷程檔
            int hisId = dac.InsertProjectBasicHis(PROJECT_NO, logId);
            #endregion

            // 寫入歷程檔
            InsertProjectFillAddSubmitHis(new ProjectHisModel()
            {
                LOG_ID = logId,
                HIS_ID = hisId,
                PROJECT_NO = PROJECT_NO
            });
            // 新增計畫審查檔
            InsertProjectAudit(new ProjectAuditModel()
            {
                PROJECT_NO = PROJECT_NO,
                // 立案審查
                PLAN_REVIEW_TYPE = "P1",
                REVIEW_RESULT = string.Empty
            });
            dac.Commit();

            // 立案送審寄信
            Task<bool> sendMailTask = Task.Run(async () => await SendProjectFillAddSubmitMail(PROJECT_NO));
            Task.Run(() => Task.WaitAll(sendMailTask)).Wait();

            return ChangeResult(true, "計畫送出成功！");
        }

        /// <summary>
        /// 立案送審寄信
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        private async Task<bool> SendProjectFillAddSubmitMail(string PROJECT_NO)
        {
            ProjectBasicModel basicModel = await dac.GetProjectBasic(PROJECT_NO) ?? new();
            #region 取得收件人資訊
            List<RecipientModel> allRcvs = new();
            List<DeptContactRcvQueryModel> rcvQueryModel = new()
            {
                new() { OrgId = "380220000A", MailType = "1" },          // 正本智發會窗口
                new() { OrgId = basicModel.EXEC_ORGAN_C, MailType = "2" }// 副本機關窗口
            };
            // 取得收件人資料
            List<RecipientModel> rdecRcvs = await projectCommonService.GetDeptContactRcvData(rcvQueryModel);
            // 副本 執行機關承辦人收件資訊
            RecipientModel execOrgHostRcv = await projectCommonService.GetSCContactRcvData(basicModel.EXEC_UNDERTAKER_C, "2");

            allRcvs.AddRange(rdecRcvs);
            allRcvs.Add(execOrgHostRcv);

            #endregion 取得收件人資訊
            MailTemplateParamModel mailTemplateParam = await projectCommonDac.GetMailTemplateParam(PROJECT_NO);
            if (mailTemplateParam != null)
            {
                MailTemplateSendModel<MailTemplateParamModel> mailModel = new()
                {
                    TemplateId = "PRJ_BASIC_SUBMIT",
                    MailAddrs = allRcvs,
                    TemplatePara = mailTemplateParam
                };
                return await mailService.SetTemplateSend(mailModel);
            }
            return false;
        }

        /// <summary>
        /// 立案送審寫入歷程檔
        /// </summary>
        /// <param name="model"></param>
        private void InsertProjectFillAddSubmitHis(ProjectHisModel model)
        {
            dac.InsertProjectCheckpointHis(model);     // 新增檢核點歷程檔
            dac.InsertProjectBudgetSourceGHis(model);  // 計畫經費來源歷程檔
            dac.InsertProjectBuildKindHis(model);      // 計畫建設類別歷程檔
            dac.InsertProjectAsstOrgHis(model);        // 計畫協辦機關歷程檔
            dac.InsertProjectControlExecuteHis(model); // 計畫預定實際期程歷程檔
        }

        #endregion

        #region 立案審核
        /// <summary>
        /// 取得立案審核資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectFillAddAuditModel> GetProjectFillAddAudit(string PROJECT_NO)
        {
            // 取得計劃基本資料
            ProjectBasicModel projectBasicModel = await dac.GetProjectBasic(PROJECT_NO);
            List<ProjectMappingDataModel> specNotesDatas = await projectCommonDac.GetProjectMappingData(PROJECT_NO, "SPEC_NOTE", PROJECT_NO);
            // 計畫審查資料
            ProjectAuditModel auditData = await dac.GetProjectAudit(PROJECT_NO, "P1");

            List<string> logStatuses = new List<string> { "3", "4", "6", "7", "8" };
            var projLogs = (await projectListDac.GetProjectLogList(PROJECT_NO)).Where(x => logStatuses.Contains(x.LOG_STATUS_C)).ToList();

            return new ProjectFillAddAuditModel
            {
                PROJECT_NO = PROJECT_NO,
                REVIEW_RESULT = auditData == null ? "" : auditData.REVIEW_RESULT,
                MEMO_EVALUATION = auditData == null ? "" : auditData.REVIEW_COMMENTS,
                SpecNoteDatas = specNotesDatas,
                ProjLogs = projLogs
            };
        }

        /// <summary>
        /// 儲存計劃立案審核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SaveProjectFillAddAudit(ProjectFillAddAuditModel model)
        {
            // 是否在填報週期內
            bool isfillCycle = dac.IsInTheFillCycle();

            dac.BeginTransaction();

            // 更新特殊加註資料
            projectCommonDac.DeleteProjectMappingData(model.PROJECT_NO, "SPEC_NOTE", model.PROJECT_NO);
            projectCommonDac.InsertProjectMappingData(model.SpecNoteDatas);

            // 若為確認送出，
            // 1. 更新計畫狀態 & 管考備註
            // 2. 寫入計畫基本資料異動記錄檔
            int logId = 0;
            if (model.SaveType == 2)
            {
                // 審查結果對應計畫狀態
                string projectStatus = model.REVIEW_RESULT == "Y" ? "4" : "3";
                // 更新計劃狀態 & 管考備註
                dac.UpdateProjectStatusAndMemo(model.PROJECT_NO, projectStatus, model.MEMO_EVALUATION);
                // 寫入計畫異動記錄檔
                logId = InsertProjectBasicLog(model.PROJECT_NO, "S1", projectStatus, model.MEMO_EVALUATION);
                // 在填報週期內且審核通過才要新增
                if (isfillCycle && model.REVIEW_RESULT == "Y")
                {
                    // 新增計畫工程進度 取PROJECT_FILL_CYCLE最新年月
                    dac.InsertProjectEngineeringProgress(model.PROJECT_NO);
                }
            }

            // 更新計畫審查資料檔
            dac.UpdateProjectAudit(new ProjectAuditModel()
            {
                LOG_ID = logId,
                PROJECT_NO = model.PROJECT_NO,
                // 立案審查
                PLAN_REVIEW_TYPE = "P1",
                // Y: 審核通過 R: 退回補正
                REVIEW_RESULT = model.REVIEW_RESULT,
                REVIEW_COMMENTS = model.MEMO_EVALUATION,
                IS_SEND = model.SaveType == 2
            });

            dac.Commit();
            if (model.SaveType == 2)
            {
                // 立案審核寄信
                Task<bool> sendMailTask = Task.Run(async () => await SendProjectFillAddAuditMail(model.PROJECT_NO, model.REVIEW_RESULT, model.MEMO_EVALUATION));
                Task.Run(() => Task.WaitAll(sendMailTask)).Wait();
            }
            return ChangeResult(true, model.SaveType == 1 ? "存檔成功" : "審查結果送出成功！");
        }

        /// <summary>
        /// 立案審核寄信
        /// </summary>
        /// <param name="PROJECT_NO">計畫編號</param>
        /// <param name="rvwResult">計畫狀態</param>
        /// <param name="MEMO_EVALUATION">管考意見</param>
        /// <returns></returns>
        private async Task<bool> SendProjectFillAddAuditMail(string PROJECT_NO, string rvwResult, string MEMO_EVALUATION)
        {
            ProjectBasicModel basicModel = await dac.GetProjectBasic(PROJECT_NO) ?? new();
            #region 取得收件人資訊
            List<RecipientModel> allRcvs = new();
            List<DeptContactRcvQueryModel> rcvQueryModel = new()
            {
                new() { OrgId = basicModel.EXEC_ORGAN_C, MailType = "1" },// 正本機關窗口
                new() { OrgId = "380220000A", MailType = "2" },           // 副本智發會窗口
            };
            // 取得收件人資料
            List<RecipientModel> rdecRcvs = await projectCommonService.GetDeptContactRcvData(rcvQueryModel);
            // 正本 執行機關承辦人收件資訊
            RecipientModel execOrgHostRcv = await projectCommonService.GetSCContactRcvData(basicModel.EXEC_UNDERTAKER_C, "1");

            allRcvs.AddRange(rdecRcvs);
            allRcvs.Add(execOrgHostRcv);

            #endregion 取得收件人資訊
            MailTemplateParamModel mailTemplateParam = await projectCommonDac.GetMailTemplateParam(PROJECT_NO);
            mailTemplateParam.MEMO_EVALUATION = MEMO_EVALUATION;
            if (mailTemplateParam != null)
            {
                MailTemplateSendModel<MailTemplateParamModel> mailModel = new()
                {
                    // Y: 審核通過 , R: 退回補正
                    TemplateId = rvwResult == "Y" ? "RVW_PRJ_BASIC_PASS" : "RVW_PRJ_BASIC_RETURN",
                    MailAddrs = allRcvs,
                    TemplatePara = mailTemplateParam
                };
                return await mailService.SetTemplateSend(mailModel);
            }
            return false;
        }

        #endregion

        #region 共用
        /// <summary>
        /// 產生計劃編號
        /// </summary>
        /// <param name="planYear">民國年</param>
        /// <param name="OU_ID">執行機關的機關代碼</param>
        /// <returns></returns>
        public string GenProjectNo(string planYear, string OU_ID)
        {
            // 計劃編號 = 民國年 + G + 機關代碼第4、5 碼 + 三位數流水號
            string projectNoFirst6Char = $"{planYear}G{OU_ID.Substring(3, 2)}";
            // 取得流水號
            string seq = dac.GetProjectNoSeq(projectNoFirst6Char);

            return $"{projectNoFirst6Char}{seq.PadLeft(3, '0')}";
        }

        /// <summary>
        /// 寫入計畫異動記錄檔
        /// </summary>
        /// <param name="PROJECT_NO">計畫編號</param>
        /// <param name="PROJECT_STAGE">作業階段</param>
        /// <param name="LOG_STATUS">異動狀態</param>
        /// <param name="MEMO"></param>
        public int InsertProjectBasicLog(string PROJECT_NO, string PROJECT_STAGE, string LOG_STATUS, string MEMO = null)
        {
            // 取得計劃基本資料
            var getProjectBasicTask = Task.Run(async () => await dac.GetProjectBasic(PROJECT_NO));
            Task.Run(() => Task.WaitAll(getProjectBasicTask)).Wait();
            ProjectBasicModel projectBasicModel = getProjectBasicTask.Result;

            // 新增計畫基本資料異動記錄檔
            return dac.InsertProjectBasicLog(new ProjectBasicLogModel
            {
                PROJECT_NO = PROJECT_NO,
                PROJECT_NAME = projectBasicModel.PROJECT_NAME,
                PROJECT_STAGE = PROJECT_STAGE,
                PROJECT_YEAR = projectBasicModel.PROJECT_YEAR,
                LOG_STATUS = LOG_STATUS,
                MEMO = MEMO
            });
        }

        /// <summary>
        /// 新增計畫審查資料檔
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectAudit(ProjectAuditModel model)
        {
            dac.InsertProjectAudit(model);
        }

        /// <summary>
        /// 取得計畫狀態
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<string> GetProjectStatus(string PROJECT_NO)
        {
            return await dac.GetProjectStatus(PROJECT_NO);
        }

        /// <summary>
        /// 取得是否使用國發會界接資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> GetIsUserFtyData(string PROJECT_NO)
        {
            return await dac.GetIsUserFtyData(PROJECT_NO);
        }
        #endregion
    }
}

