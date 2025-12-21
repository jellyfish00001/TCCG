using Aspose.Pdf.Facades;
using AutoMapper;
using Ionic.Zip;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.CodeAnalysis;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Base.Utils;
using SDO.Base.Utils.Models;
using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class ProjectAdjustService : Service, IProjectAdjustService
    {
        private readonly IProjectAdjustDac dac;
        private readonly IProjectCommonDac projectCommonDac;
        private readonly IProjectCommonService projectCommonService;
        private readonly IProjectDac projectDac;
        private readonly IProjectService projectService;
        private readonly IUploadFileService uploadFileService;
        private readonly IFTPService ftpService;
        private readonly IUserProfile userProfile;
        private readonly IDimRoleDac roleDac;
        private readonly IMailSetService mailSetService;
        private readonly IProjectExecuteDac projectExecuteDac;
        private readonly IZipService zipService;
        private readonly IProjectExecuteService projectExecuteService;
        public ProjectAdjustService(
            IProjectAdjustDac dac,
            IProjectCommonDac projectCommonDac, IProjectCommonService projectCommonService,
            IProjectDac projectDac, IProjectService projectService,
            IUploadFileService uploadFileService,
            IFTPService ftpService,
            IUserProfile userProfile,
            IDimRoleDac roleDac,
            IMailSetService mailSetService,
            IProjectExecuteDac projectExecuteDac,
            IZipService zipService, IProjectExecuteService projectExecuteService)
        {
            this.dac = dac;
            this.projectCommonDac = projectCommonDac;
            this.projectCommonService = projectCommonService;
            this.projectDac = projectDac;
            this.projectService = projectService;
            this.uploadFileService = uploadFileService;
            this.ftpService = ftpService;
            this.userProfile = userProfile;
            this.roleDac = roleDac;
            this.mailSetService = mailSetService;
            this.projectExecuteDac = projectExecuteDac;
            this.zipService = zipService;
            this.projectExecuteService = projectExecuteService;
        }

        /// <summary>
        /// 取得計畫調整撤銷清單
        /// </summary>
        /// <param name="model">篩選條件</param>
        /// <returns></returns>
        public async Task<List<ProjectAdjustListModel>> GetAdjustList(ProjectAdjustListQueryModel model)
        {
            List<ProjectAdjustListModel> result = new();

            // 檢查使用者是否為主辦 roles.Count = 0主辦、>0 管考
            List<DimRoleModel> roles = await roleDac.ReadListByUser(userProfile.GetLoginUser().USER_ID);
            roles = roles.Where(x => x.AP_ID == "IPC3" && x.ROLE_ID == "RDEC_RDEC_ROL_IPC3").ToList();
            // 在審核頁面，但身分只有主辦，無權限審核，回傳空List
            if (roles.Count == 0 && model.IsReview == 1)
            {
                return result;
            }

            // 填報頁面回傳該使用者所屬機關的清單；審核頁面則不限
            model.EXEC_ORGAN_C = model.IsReview == 0 ? userProfile.GetLoginUser().ORG_ID : model.EXEC_ORGAN_C;

            return await dac.GetAdjustList(model);
        }

        /// <summary>
        /// 主辦取消調整
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <param name="AW_KIND">調整項目</param>
        /// <returns></returns>
        public RtnResultModel SaveExecCancel(string PROJECT_NO, int PROJ_ADJ_ID, string AW_KIND)
        {
            string projAwStatus = AW_KIND == "AW01" ? "A06" : "B06";
            dac.BeginTransaction();
            // 刪除計畫調整檔 (修改 DEL_FLG = 1)
            dac.DeleteProjBasicAdj(PROJ_ADJ_ID, projAwStatus);
            // 修改計畫主檔狀態
            dac.UpdateProjBasicStatuses(PROJECT_NO, null);
            // 寫入異動記錄檔
            projectService.InsertProjectBasicLog(PROJECT_NO, "S2", projAwStatus);
            dac.Commit();
            return ChangeResult(true, "取消調整成功");
        }

        /// <summary>
        /// 主辦申請調整撤銷原因
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SaveExecReason(AdjustReasonModel model)
        {
            #region 檢查所有檔案
            List<ProjectAttachmentModel> fileList = new();
            if (model.Files != null)
            {
                fileList.Add(model.Files);
            }
            if (model.Files2 != null)
            {
                fileList.Add(model.Files2);
            }

            List<string> result = projectCommonService.CheckFileName(fileList, "", model.PROJ_ADJ_ID);
            if (result.Any())
            {
                return ChangeResult(false, $"存檔失敗，檔名 {string.Join("、", result)} 重複");
            }
            #endregion

            dac.BeginTransaction();
            // 1. 調整計畫相關檔
            switch (model.AW_KIND)
            {
                // 調整基本資料
                case "AW01": dac.UpdateProjectBasicAdj(model); break;
                // 調整期程
                case "AW02": dac.UpdateProjectBasicAdjForSchedule(model); break;
                // 撤銷
                case "AW03":
                    // PROJ_ADJ_ID != 0 表為修改 (退回補正用)，否則為新增
                    if (model.PROJ_ADJ_ID != 0)
                    {
                        dac.UpdateProjectBasicAdj(model);
                        dac.UpdateProjBasicAdjAwSatus(model.PROJ_ADJ_ID, "W02"); // 修改調整檔狀態
                    }
                    else
                    {
                        model.PROJ_ADJ_ID = dac.InsertProjectBasicAdjForRevoke(model);
                    }
                    // 修改計畫主檔狀態
                    dac.UpdateProjBasicStatuses(model.PROJECT_NO, "W02");

                    // 寫入異動記錄檔
                    projectService.InsertProjectBasicLog(model.PROJECT_NO, "S2", "W02");

                    break;
                default:
                    return ChangeResult(false, "存檔失敗");
            }

            // 2. 調整 mapping data (調整原因/撤銷原因)，沒資料全刪除、有資料刪除後新增
            if (model.Reasons != null)
            {
                string setItem = model.AW_KIND == "AW03" ? "REVOKE_REASON" : "ADJUST_REASON";
                foreach (ProjectMappingDataModel item in model.Reasons)
                {
                    item.PROJECT_NO = model.PROJECT_NO;
                    item.SOURCE_ID = model.PROJ_ADJ_ID.ToString();
                }
                projectCommonDac.DeleteProjectMappingData(model.PROJECT_NO, setItem, model.PROJ_ADJ_ID.ToString());
                projectCommonDac.InsertProjectMappingData(model.Reasons);
            }

            // 3. 調整附件檔案
            if (model.Files != null)
            {
                ProjectAttachmentModel fileModel = model.Files;
                #region SET INFO
                // 01:相關檔案上傳、02:其他地方上傳
                fileModel.FILE_UP_SOURCE = "02";
                //對應計畫調整檔的PK，ref PROJECT_BASIC_ADJ.PROJ_ADJ_ID
                fileModel.SOURCE_ID = model.PROJ_ADJ_ID;
                #endregion
                projectCommonService.SaveProjectFiles(model.Files);
            }

            // 4. 調整上傳核定函 (for 調整期程)
            if (model.Files2 != null)
            {
                ProjectAttachmentModel fileModel = model.Files2;
                #region SET INFO
                // 01:相關檔案上傳、02:其他地方上傳
                fileModel.FILE_UP_SOURCE = "02";
                //對應計畫調整檔的PK，ref PROJECT_BASIC_ADJ.PROJ_ADJ_ID
                fileModel.SOURCE_ID = model.PROJ_ADJ_ID;
                #endregion
                projectCommonService.SaveProjectFiles(model.Files2);
            }

            dac.Commit();

            // 撤銷送審需寄發郵件通知
            if (model.AW_KIND == "AW03")
            {
                Task<bool> mailTask = Task.Run(async () => await SendMail("PRJ_RVOKE_SUBMIT", model.PROJECT_NO, true, null));
                Task.Run(() => Task.WaitAll(mailTask)).Wait();
            }

            return ChangeResult(true, model.AW_KIND == "AW03" ? "送出成功" : "存檔成功");
        }

        #region 新增主辦申請調整計畫 (for 基本資料、期程)

        /// <summary>
        /// 新增主辦申請調整計畫 (for 基本資料、期程)
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="AW_KIND">調整申請項目</param>
        /// <returns>調整檔流水號</returns>
        public int AddAdujustExec(string PROJECT_NO, string AW_KIND)
        {
            // 驗證此計畫是否可以申請調整: 若此時 PROJECT_AW_STATUS 不為 null，表正在調整中
            // 一個計畫在啟動調整後到審核完畢前都無法再次申請調整
            string projectAwStatusNow = null;
            Task<string> projectAwStatusNowTask = Task.Run(async () => await dac.GetProjectAwStatus(PROJECT_NO));
            Task.Run(() => Task.WaitAll(projectAwStatusNowTask)).Wait();
            projectAwStatusNow = projectAwStatusNowTask.Result;
            if (projectAwStatusNow != null)
            {
                return 0;
            }

            // 先取得目前的計畫經費來源資料(資料&對應檔案)，用於基本資料調整轉檔
            List<ProjectBudgetSourceGModel> projectBudgetSourceG = new();
            if (AW_KIND == "AW01")
            {
                Task<List<ProjectBudgetSourceGModel>> projectBudgetSourceGTask = Task.Run(async () => await GetProjectBudgetSourceG(PROJECT_NO));
                Task.Run(() => Task.WaitAll(projectBudgetSourceGTask)).Wait();
                projectBudgetSourceG = projectBudgetSourceGTask.Result;

            }

            // 欲存入的計畫調整狀態: 基本資料 A01、期程 B01
            string projectAwStatus = AW_KIND == "AW01" ? "A01" : "B01";

            dac.BeginTransaction();

            // 新增基本資料
            int projAdjId = dac.TransferProjectBasic(PROJECT_NO, AW_KIND, projectAwStatus);

            // 基本資料調整
            if (AW_KIND == "AW01")
            {
                // 新增協辦機關/人員
                dac.TransferAsstOrg(PROJECT_NO, projAdjId);
                // 新增建設類別
                dac.TransferBuildKind(PROJECT_NO, projAdjId);
                // 新增經費來源
                TransferBudgetSourceG(projectBudgetSourceG, PROJECT_NO, projAdjId);
            }
            // 期程調整
            else if (AW_KIND == "AW02")
            {
                dac.TransferCheckItem(PROJECT_NO, projAdjId);
            }

            // 寫入異動記錄檔
            int logId = projectService.InsertProjectBasicLog(PROJECT_NO, "S2", projectAwStatus);

            // 寫入歷程檔
            InsertProjectHisByAudit(PROJECT_NO, logId);

            // 調整計畫主檔的調整狀態
            dac.UpdateProjBasicStatuses(PROJECT_NO, projectAwStatus);

            // 將新增的 LOG_ID 寫入調整檔
            dac.UpdateProjBasicAdjLogId(projAdjId, logId);

            dac.Commit();
            return projAdjId;
        }

        /// <summary>
        /// 取得計畫調整時的列管編號(for調整基本資料 經費來源)
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        /// <returns>PROJECT_NO-4碼的PROJ_ADJ_ID (不滿4碼補0) ex: 111G38001-0010</returns>
        private string GetProjAdjCombine(string PROJECT_NO, int PROJ_ADJ_ID)
        {
            string projAdjId = PROJ_ADJ_ID.ToString();
            return PROJECT_NO + "-" + projAdjId.PadLeft(4, '0');
        }

        /// <summary>
        /// 經費來源轉檔 (在 PROJECT_BUDGET_SOURCE_G 中複製一份新的，並改 PROJECT_NO 為 PROJECT_NO-4碼的PROJ_ADJ_ID)
        /// </summary>
        /// <param name="models">原資料models</param>
        /// <param name="PROJECT_NO">原列管編號</param>
        /// <param name="projAdjId">此次調整流水號</param>
        private void TransferBudgetSourceG(List<ProjectBudgetSourceGModel> models, string PROJECT_NO, int projAdjId)
        {
            string projAdjCombine = GetProjAdjCombine(PROJECT_NO, projAdjId);
            foreach (ProjectBudgetSourceGModel item in models)
            {
                item.PROJECT_NO = projAdjCombine;
                // 1. 新增進經費來源table: PROJECT_BUDGET_SOURCE_G，並取得剛新增的IDENTITY_FIELD
                int budgetId = projectDac.InsertProjectBudgetSourceG(item);

                foreach (var file in item.FILE)
                {
                    // 2. 新增進對應的經費來源檔案table: PROJECT_ATTACHMENT，並取得剛新增的IDENTITY_FIELD
                    ProjectAttachmentModel budgetFile = file;
                    if (budgetFile == null)
                    {
                        continue;
                    }
                    int fileId = projectCommonDac.InsertProjectAttachment(new ProjectAttachmentModel
                    {
                        PROJECT_NO = PROJECT_NO,
                        FILE_KIND = $"{file.FILE_KIND}-A",
                        FILE_NAME = budgetFile.FILE_NAME,
                        FILE_PATH = $"/{projAdjCombine}",
                        FILE_UP_SOURCE = "02",
                        SOURCE_ID = budgetId
                    });



                    // 3. FTP上複製檔案 (從/PROJECT_NO 複製到 /PROJECT_NO-4碼的PROJ_ADJ_ID)
                    // 取附檔名
                    string extension = Path.GetExtension(budgetFile.FILE_NAME);
                    // 組成來源路徑&目的地路徑
                    string fromPath = $"{budgetFile.FILE_PATH}/{budgetFile.IDENTITY_FIELD}{extension}";
                    string destPath = $"{projAdjCombine}/{fileId}{extension}";
                    // FTP複製檔案
                    uploadFileService.CopyFile(fromPath, destPath);
                }
            }
        }

        #endregion

        /// <summary>
        /// 取得主辦申請調整撤銷原因(for 調整基本資料、撤銷)
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <param name="AW_KIND">申請項目</param>
        /// <returns></returns>
        public async Task<AdjustReasonViewModel> GetExecReasonBasic(int PROJ_ADJ_ID, string AW_KIND)
        {
            AdjustReasonViewModel result = await dac.GetExecReasonBasic(PROJ_ADJ_ID);

            if (result != null && PROJ_ADJ_ID != 0)
            {
                List<string> fileKind = new();
                string mappingItem = "";
                switch (AW_KIND)
                {
                    case "AW01": fileKind.Add("09"); mappingItem = "ADJUST_REASON"; break;
                    case "AW03": fileKind.Add("19"); mappingItem = "REVOKE_REASON"; break;
                }

                result.Reasons = await projectCommonDac.GetProjectMappingData(result.PROJECT_NO, mappingItem, PROJ_ADJ_ID.ToString());

                // 取佐證資料
                result.Files = await projectCommonDac.GetProjectAttachmentList(new ProjectAttachmentQueryModel
                {
                    PROJECT_NO = result.PROJECT_NO,
                    FILE_UP_SOURCE = "02",
                    FILE_KIND = fileKind,
                    SOURCE_ID = PROJ_ADJ_ID
                });
            }

            return result;
        }

        /// <summary>
        /// 取得調整計畫基本資料(含計畫基本資料調整、計畫經費來源調整、計畫建設類別調整、計畫協辦機關調整)
        /// </summary>
		/// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        public async Task<ProjectBasicFillAdjustModel> GetProjectBasicAdj(string PROJECT_NO, int PROJ_ADJ_ID)
        {
            ProjectBasicFillAdjustModel result = new()
            {
                //取得計畫基本資料
                ProjectBasic = await dac.GetProjectBasicAdj(PROJECT_NO, PROJ_ADJ_ID) ?? new ProjectBasicAdjustModel(),
                //取得計畫經費來源
                ProjectBudgetSourceG = new List<ProjectBudgetSourceGModel>(),
                //取得計畫建設類別
                ProjectBuildKind = await dac.GetProjectBuildKindAdj(PROJ_ADJ_ID),
                //取得計畫協辦機關
                ProjectAsstOrg = await dac.GetAdjustAsstOrg(PROJ_ADJ_ID)
            };
            //取得計畫經費來源
            string projAdjCombine = GetProjAdjCombine(PROJECT_NO, PROJ_ADJ_ID);
            List<ProjectBudgetSourceGModel> budgetSourceData = await GetProjectBudgetSourceG(PROJECT_NO, projAdjCombine);
            result.ProjectBudgetSourceG = budgetSourceData.Where(x => x.PROJECT_NO == projAdjCombine).ToList();

            return result;
        }

        #region 儲存調整計畫基本資料

        /// <summary>
        /// 儲存調整計畫基本資料(含計畫基本資料調整、計畫經費來源調整、計畫建設類別調整、計畫協辦機關調整)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SetProjectBasicAdj(ProjectBasicFillAdjustModel model)
        {
            string projectNo = model.ProjectBasic.PROJECT_NO;
            int projAdjId = model.ProjectBasic.PROJ_ADJ_ID;

            #region 檢查所有檔案
            if (model.ProjectBudgetSourceG.Any())
            {
                List<ProjectAttachmentModel> fileList = model.ProjectBudgetSourceG.Where(x => x.FILE != null).SelectMany(x => x.FILE).ToList();
                List<string> result = projectCommonService.CheckFileName(fileList, projectNo);
                if (result.Any())
                {
                    return ChangeResult(false, $"存檔失敗，檔名 {string.Join("、", result)} 重複");
                }
            }
            #endregion

            dac.BeginTransaction();

            //儲存計畫基本資料
            dac.UpdateProjectBasicDataAdj(model.ProjectBasic);

            //儲存計畫經費來源
            AddMdfProjectBudgetSourceG(model.ProjectBudgetSourceG, projectNo, projAdjId);

            //儲存計畫建設類別
            AddMdfProjectBuildKind(model.ProjectBuildKind, projectNo, projAdjId);

            //儲存計畫協辦機關
            AddMdfAdjustAsstOrg(model.ProjectAsstOrg, projectNo, projAdjId);

            dac.Commit();
            return ChangeResult(true, "存檔成功");
        }

        /// <summary>
        /// 儲存計畫經費來源調整
        /// </summary>
        /// <param name="model"></param>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        private void AddMdfProjectBudgetSourceG(List<ProjectBudgetSourceGModel> model, string PROJECT_NO, int PROJ_ADJ_ID)
        {
            // 取得調整時的列管編號(PROJECT_NO-4碼PROJ_ADJ_ID)
            string projAdjCombine = GetProjAdjCombine(PROJECT_NO, PROJ_ADJ_ID);
            // 1. 處理 PROJECT_BUDGET_SOURCE_G
            foreach (ProjectBudgetSourceGModel item in model)
            {
                switch ((editTypeEnum)item.editType)
                {
                    //新增計畫經費來源
                    case editTypeEnum.Add:
                        item.PROJECT_NO = projAdjCombine;
                        item.IDENTITY_FIELD = projectDac.InsertProjectBudgetSourceG(item);
                        break;
                    //修改計畫經費來源
                    case editTypeEnum.Modify:
                        projectDac.UpdateProjectBudgetSourceG(item);
                        break;
                    //刪除計計畫經費來源
                    case editTypeEnum.Delete:
                        projectDac.DeleteProjectBudgetSourceG(item.IDENTITY_FIELD);
                        break;
                }

                if (item.FILE != null)
                {
                    foreach (var file in item.FILE)
                    {
                        file.PROJECT_NO = PROJECT_NO;
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
        /// 儲存計畫建設類別調整
        /// </summary>
        /// <param name="model"></param>
        private void AddMdfProjectBuildKind(List<ProjectBuildKindAdjustModel> model, string PROJECT_NO, int PROJ_ADJ_ID)
        {
            //刪除計畫建設類別
            dac.DeleteProjectBuildKindAdj(PROJ_ADJ_ID);
            //新增已選取的資料
            dac.InsertProjectBuildKindAdj(model, PROJECT_NO, PROJ_ADJ_ID);
        }

        /// <summary>
        /// 儲存計畫協辦機關調整
        /// </summary>
        /// <param name="model"></param>
        private void AddMdfAdjustAsstOrg(List<ProjectAsstOrgAdjustModel> model, string PROJECT_NO, int PROJ_ADJ_ID)
        {
            foreach (ProjectAsstOrgAdjustModel item in model)
            {
                switch ((editTypeEnum)item.editType)
                {
                    //新增計畫協辦機關
                    case editTypeEnum.Add:
                        item.PROJECT_NO = PROJECT_NO;
                        item.PROJ_ADJ_ID = PROJ_ADJ_ID;
                        dac.InsertAdjustAsstOrg(item);
                        break;
                    //修改計畫協辦機關
                    case editTypeEnum.Modify:
                        dac.UpdateAdjustProjectAsstOrg(item);
                        break;
                    //刪除計畫協辦機關
                    case editTypeEnum.Delete:
                        dac.DeleteAdjustAsstOrg(item.ASST_ID);
                        break;
                }
            }
        }

        #endregion

        #region 調整期程

        /// <summary>
        /// 取得主辦申請調整期程原因
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        public async Task<AdjustReasonScheduleViewModel> GetExecReasonSchedule(int PROJ_ADJ_ID)
        {
            AdjustReasonScheduleViewModel result = await dac.GetExecReasonSchedule(PROJ_ADJ_ID);

            if (result != null && PROJ_ADJ_ID != 0)
            {
                result.Reasons = await projectCommonDac.GetProjectMappingData(result.PROJECT_NO, "ADJUST_REASON", PROJ_ADJ_ID.ToString());
                // 取核定函、佐證資料
                List<ProjectAttachmentModel> attModels = await projectCommonDac.GetProjectAttachmentList(new ProjectAttachmentQueryModel
                {
                    PROJECT_NO = result.PROJECT_NO,
                    FILE_UP_SOURCE = "02",
                    SOURCE_ID = PROJ_ADJ_ID
                });
                result.Files2 = attModels.Where(m => m.FILE_KIND == "22").ToList();
                result.Files = attModels.Where(m => m.FILE_KIND == "12").ToList();
            }

            return result;
        }

        /// <summary>
        /// 取得檢核點調整
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水編號</param>
        /// <returns></returns>
        public async Task<AdjustCheckPointModel> GetAdjustCheckPoint(int PROJ_ADJ_ID)
        {
            AdjustCheckPointModel model = await dac.GetAdjustCheckPoint(PROJ_ADJ_ID) ?? new();
            model.AdjustScheHistoryModels = await dac.GetProjAdjScheHistory(new List<string> { model.PROJECT_NO });
            model.CusCheckpointModels = await dac.GetAdjustCusCheckPoint(PROJ_ADJ_ID);
            // 取 「工程竣工報告表或函報竣工文件」(FILE_KIND=13) 或 「核章版工程預定進度網圖」(FILE_KIND=23)
            model.Files = await projectCommonDac.GetProjectAttachmentList(new ProjectAttachmentQueryModel
            {
                PROJECT_NO = model.PROJECT_NO,
                FILE_UP_SOURCE = "02",
                FILE_KIND = new List<string> { "13", "23" },
                SOURCE_ID = PROJ_ADJ_ID
            });

            return model;
        }

        /// <summary>
        /// 儲存計劃檢核點調整
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SetAdjustCheckPoint(AdjustCheckPointModel model)
        {

            #region 檢查檔案是否重複檔名
            List<ProjectAttachmentModel> fileList = model.Files;

            List<string> result = projectCommonService.CheckFileName(fileList, "", model.PROJ_ADJ_ID);
            if (result.Any())
            {
                return ChangeResult(false, $"存檔失敗，檔名 {string.Join("、", result)} 重複");
            }
            #endregion

            dac.BeginTransaction();
            // 更新計畫調整檔
            dac.UpdateAdjustCheckPoint(model);
            // 檢核點
            if (model.CusCheckpointModels != null && model.CusCheckpointModels.Any())
            {
                // 若調整執行方式，則清除原有檢核點
                if (model.RUNWAY_C != model.OLD_RUNWAY_C)
                    dac.DeleteProjectAdjChkItem(model.PROJ_ADJ_ID);

                var insertData = model.CusCheckpointModels.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
                insertData.ForEach(x => { x.PROJ_ADJ_ID = model.PROJ_ADJ_ID; });
                dac.InsertAdjustCustomChkItem(insertData);

                var updateData = model.CusCheckpointModels.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
                dac.UpdateAdjustCustomChkItem(updateData);

                var deleteData = model.CusCheckpointModels.Where(x => (editTypeEnum)x.editType == editTypeEnum.Delete).Select(x => x.SEQ).ToList();
                dac.DeleteAdjustCustomChkItem(deleteData);
            }
            // 檔案 (工程竣工報告表或函報竣工文件)
            if (model.Files.Any() && model.Files[0] != null)
            {
                ProjectAttachmentModel fileModel = model.Files[0];
                #region SET INFO
                // 01:相關檔案上傳、02:其他地方上傳
                fileModel.FILE_UP_SOURCE = "02";
                //對應計畫調整檔的PK，ref PROJECT_BASIC_ADJ.PROJ_ADJ_ID
                fileModel.SOURCE_ID = model.PROJ_ADJ_ID;
                #endregion
                projectCommonService.SaveProjectFiles(model.Files[0]);
            }

            dac.Commit();
            return ChangeResult(true, "存檔成功");
        }

        #endregion

        #region 主辦調整檢核結果

        /// <summary>
        /// 取得主辦調整檢核結果
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水編號</param>
        /// <returns></returns>
        public async Task<RtnResultModel> GetAdjustChk(string PROJECT_NO, int PROJ_ADJ_ID)
        {
            AdjustReasonViewModel basicData = await dac.GetExecReasonBasic(PROJ_ADJ_ID);
            string awKind = basicData.AW_KIND;
            object rtnData = new();
            // 檢核基本資料
            if (awKind == "AW01")
            {
                rtnData = await GetAdjustChkBasic(basicData);
            }
            // 檢核期程
            else if (awKind == "AW02")
            {
                rtnData = await GetAdjustChkSchedule(PROJECT_NO, PROJ_ADJ_ID);
            }

            ObjectResultModel<object> result = new()
            {
                success = true,
                message = "",
                data = rtnData
            };
            return result;
        }

        /// <summary>
        /// 取得基本資料檢核結果
        /// </summary>
        /// <param name="basicData">調整檔基本資料</param>
        /// <returns></returns>
        private async Task<object> GetAdjustChkBasic(AdjustReasonViewModel basicData)
        {
            List<object> errList = new();

            // 1. 檢核基本資料調整事由是否填寫
            if (string.IsNullOrEmpty(basicData.ADJUST_REASON))
            {
                errList.Add(new
                {
                    errTitle = "1、基本資料調整事由",
                    errDesc = "尚未填報",
                    SOURCE_PATH = await projectCommonDac.GetChapterPath("ProjectAdjustBasicReason")
                });
            }

            // 2. 檢核計畫基本資料
            // 取得計畫基本資料驗證Model 
            ProjectBasicCheckModel projBasicChkModel = JsonDeserialize<ProjectBasicCheckModel>(
                JsonSerialize(await dac.GetProjectBasicAdj(basicData.PROJECT_NO, basicData.PROJ_ADJ_ID)));

            // 計畫基本資料必填欄位
            List<string> errMsgs = projectCommonService.CheckModelRequiredField<ProjectBasicModel>(projBasicChkModel);
            // 建設類別
            bool buildKindValid = (await dac.GetProjectBuildKindAdj(basicData.PROJ_ADJ_ID)).Any();
            if (!buildKindValid)
            {
                errMsgs.Add("「建設類別」");
            }
            // 經費來源
            string projAdjCombine = GetProjAdjCombine(basicData.PROJECT_NO, basicData.PROJ_ADJ_ID);
            List<ProjectBudgetSourceGModel> budgetSourceData = await GetProjectBudgetSourceG(basicData.PROJECT_NO, projAdjCombine);
            List<ProjectBudgetSourceGModel> budget = budgetSourceData.Where(x => x.PROJECT_NO == projAdjCombine).ToList();
            bool budgetValid = budget.Any();
            if (!budgetValid)
            {
                errMsgs.Add("「經費來源」");
            }

            if (errMsgs.Any())
            {
                errList.Add(new
                {
                    errTitle = "2、基本資料調整",
                    errDesc = $"{string.Join('、', errMsgs)} 尚未填報",
                    SOURCE_PATH = await projectCommonDac.GetChapterPath("ProjectAdjustBasicData")
                });
            }


            // 若無任何錯誤，直接回傳
            if (!errList.Any())
            {
                return new { basicData.PROJECT_NAME };
            }

            return new { basicData.PROJECT_NAME, errList };
        }

        /// <summary>
        /// 取得期程檢核結果
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        /// <returns></returns>
        private async Task<object> GetAdjustChkSchedule(string PROJECT_NO, int PROJ_ADJ_ID)
        {
            bool isHaveErr = false;
            List<object> errList = new();

            // 取期程調整檢核結果
            AdjustCheckModel checkScheResult = await dac.GetAdjustScheChk(PROJ_ADJ_ID);

            // 檢核期程調整事由是否填寫
            if (string.IsNullOrEmpty(checkScheResult.ADJUST_REASON))
            {
                isHaveErr = true;
                errList.Add(new
                {
                    errTitle = "1、期程調整事由",
                    errDesc = "尚未填報",
                    SOURCE_PATH = await projectCommonDac.GetChapterPath("ProjectAdjustScheduleReason")
                });
            }

            // 檢核「2、檢核點調整」
            // (1)檢核 檢核點
            #region 轉換model: AdjustCheckPointModel => ProjectCheckpointModel

            string errDesc = string.Empty;
            AdjustCheckPointModel data = await GetAdjustCheckPoint(PROJ_ADJ_ID);
            var config = new MapperConfiguration(cfg =>
                cfg.CreateMap<AdjustCheckPointModel, ProjectCheckpointModel>()); // 註冊Model間的對映
            var mapper = config.CreateMapper(); // 建立 Mapper
            ProjectCheckpointModel checkpointModel = mapper.Map<ProjectCheckpointModel>(data); // 轉換型別

            #endregion
            string checkPointErrMsg = projectCommonService.CheckProjectCheckpointValid(checkpointModel);
            if (!string.IsNullOrEmpty(checkPointErrMsg))
            {
                isHaveErr = true;
                errDesc = checkPointErrMsg;
            }
            if (string.IsNullOrEmpty(data.SCHE_TYPE))
            {
                isHaveErr = true;
                errDesc = "未執行檢核點調整";
            }
            // (2)當該筆計畫已填報實際竣工日(CTRL_POINT = 'B' and ACTUAL_ENDDATE is not NULL)，
            //   則需檢核「工程竣工報告表或函報竣工文件」是否上傳
            if (checkScheResult.FILE_KIND == "13" && checkScheResult.CNT_FILE == 0)
            {
                isHaveErr = true;
                errDesc = $@"{(!string.IsNullOrEmpty(errDesc) ? errDesc + "、" : string.Empty)}
                        「工程竣工報告表或竣工核定文件」尚未上傳";
            }
            // (3)當該筆計畫已填報實際開工日(CTRL_POINT = 'A' and ACTUAL_ENDDATE is not NULL)，但尚未填報實際竣工日
            //   則需檢核「工程竣工報告表或函報竣工文件」是否上傳
            else if (checkScheResult.FILE_KIND == "23" && checkScheResult.CNT_FILE == 0)
            {
                isHaveErr = true;
                errDesc = $@"{(!string.IsNullOrEmpty(errDesc) ? errDesc + "、" : string.Empty)}
                        「核章版工程預定進度網圖或預定竣工日展延核定文件」尚未上傳";
            }

            if (!string.IsNullOrEmpty(errDesc))
            {
                errList.Add(new
                {
                    errTitle = "2、檢核點調整",
                    errDesc = errDesc,
                    SOURCE_PATH = await projectCommonDac.GetChapterPath("ProjectAdjustScheduleCheckPoint")
                });
            }

            // 若無任何錯誤，回傳檔案 : 准簽、已核章申請表
            if (!isHaveErr)
            {
                List<ProjectAttachmentModel> attModels = await projectCommonDac.GetProjectAttachmentList(new ProjectAttachmentQueryModel
                {
                    PROJECT_NO = PROJECT_NO,
                    FILE_UP_SOURCE = "02",
                    SOURCE_ID = PROJ_ADJ_ID
                });
                List<ProjectAttachmentModel> approvalFiles = attModels.Where(m => m.FILE_KIND == "10").ToList();
                List<ProjectAttachmentModel> files = attModels.Where(m => m.FILE_KIND == "11").ToList();
                return new
                {
                    checkScheResult.PROJECT_NAME,
                    checkScheResult.SCHE_TYPE,
                    approvalFiles,
                    files
                };
            }

            return new
            {
                checkScheResult.PROJECT_NAME,
                checkScheResult.SCHE_TYPE,
                errList
            };
        }

        /// <summary>
        /// 主辦上傳准簽、已核章申請表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SaveScheAttach(AdjustReasonModel model)
        {
            #region 檢查所有檔案
            List<ProjectAttachmentModel> fileList = new();
            if (model.Files != null)
            {
                fileList.Add(model.Files);
            }
            if (model.Files2 != null)
            {
                fileList.Add(model.Files2);
            }

            List<string> result = projectCommonService.CheckFileName(fileList, "", model.PROJ_ADJ_ID);
            if (result.Any())
            {
                return ChangeResult(false, $"存檔失敗，檔名 {string.Join("、", result)} 重複");
            }
            #endregion

            dac.BeginTransaction();

            // 上傳准簽
            if (model.Files != null)
            {
                ProjectAttachmentModel fileModel = model.Files;
                #region SET INFO
                // 01:相關檔案上傳、02:其他地方上傳
                fileModel.FILE_UP_SOURCE = "02";
                //對應計畫調整檔的PK，ref PROJECT_BASIC_ADJ.PROJ_ADJ_ID
                fileModel.SOURCE_ID = model.PROJ_ADJ_ID;
                #endregion
                projectCommonService.SaveProjectFiles(model.Files);
            }

            // 上傳已核章申請表
            if (model.Files2 != null)
            {
                ProjectAttachmentModel fileModel = model.Files2;
                #region SET INFO
                // 01:相關檔案上傳、02:其他地方上傳
                fileModel.FILE_UP_SOURCE = "02";
                //對應計畫調整檔的PK，ref PROJECT_BASIC_ADJ.PROJ_ADJ_ID
                fileModel.SOURCE_ID = model.PROJ_ADJ_ID;
                #endregion
                projectCommonService.SaveProjectFiles(model.Files2);
            }

            dac.Commit();
            return ChangeResult(true, "存檔成功");
        }

        #endregion

        /// <summary>
        /// 主辦調整送審
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SendExecAdjust(AdjustReasonModel model)
        {
            #region 檢查所有檔案
            List<ProjectAttachmentModel> fileList = new();
            if (model.Files != null)
            {
                fileList.Add(model.Files);
            }
            if (model.Files2 != null)
            {
                fileList.Add(model.Files2);
            }

            List<string> result = projectCommonService.CheckFileName(fileList, "", model.PROJ_ADJ_ID);
            if (result.Any())
            {
                return ChangeResult(false, $"送出失敗，檔名 {string.Join("、", result)} 重複");
            }
            #endregion

            string templateId = ""; // 郵件範本ID
            if (model.AW_KIND == "AW01")
            {
                model.PROJECT_AW_STATUS = "A02";
                templateId = "PRJ_ADJ_BASIC_SUBMIT";
            }
            else if (model.AW_KIND == "AW02")
            {
                model.PROJECT_AW_STATUS = "B02";
                templateId = "PRJ_ADJ_SCH_SUBMIT";
            }

            dac.BeginTransaction();

            // 更新計畫主檔的調整狀態
            dac.UpdateProjBasicStatuses(model.PROJECT_NO, model.PROJECT_AW_STATUS);
            // 更新計畫調整檔的調整狀態
            dac.UpdateProjBasicAdjAwSatus(model.PROJ_ADJ_ID, model.PROJECT_AW_STATUS);

            // 上傳准簽、已核章申請表 (for 期程調整)
            if (model.Files != null)
            {
                ProjectAttachmentModel fileModel = model.Files;
                #region SET INFO
                // 01:相關檔案上傳、02:其他地方上傳
                fileModel.FILE_UP_SOURCE = "02";
                //對應計畫調整檔的PK，ref PROJECT_BASIC_ADJ.PROJ_ADJ_ID
                fileModel.SOURCE_ID = model.PROJ_ADJ_ID;
                #endregion
                projectCommonService.SaveProjectFiles(model.Files);
            }
            if (model.Files2 != null)
            {
                ProjectAttachmentModel fileModel = model.Files2;
                #region SET INFO
                // 01:相關檔案上傳、02:其他地方上傳
                fileModel.FILE_UP_SOURCE = "02";
                //對應計畫調整檔的PK，ref PROJECT_BASIC_ADJ.PROJ_ADJ_ID
                fileModel.SOURCE_ID = model.PROJ_ADJ_ID;
                #endregion
                projectCommonService.SaveProjectFiles(model.Files2);
            }

            // 寫入異動記錄檔
            projectService.InsertProjectBasicLog(model.PROJECT_NO, "S2", model.PROJECT_AW_STATUS);

            dac.Commit();

            // 寄發郵件通知
            Task<bool> mailTask = Task.Run(async () => await SendMail(templateId, model.PROJECT_NO, true, null));
            Task.Run(() => Task.WaitAll(mailTask)).Wait();

            return ChangeResult(true, "送出成功");
        }

        /// <summary>
        /// 管考取得主辦調整撤銷原因
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <param name="AW_KIND">申請項目</param>
        /// <returns></returns>
        public async Task<AdjustAuditModel> GetExecReasonByAudit(int PROJ_ADJ_ID, string AW_KIND)
        {
            AdjustAuditModel result = await dac.GetExecReasonByAudit(PROJ_ADJ_ID);

            // 取得調整/撤銷原因
            string setItem = AW_KIND == "AW03" ? "REVOKE_REASON" : "ADJUST_REASON";
            result.Reasons = await projectCommonDac.GetProjectMappingData(result.PROJECT_NO, setItem, PROJ_ADJ_ID.ToString());

            // 取得檔案(佐證資料)
            ProjectAttachmentQueryModel attQueryModel = new()
            {
                PROJECT_NO = result.PROJECT_NO,
                FILE_UP_SOURCE = "02",
                SOURCE_ID = PROJ_ADJ_ID
            };
            switch (AW_KIND)
            {
                case "AW01":
                    attQueryModel.FILE_KIND = new List<string> { "09" };
                    break;
                case "AW02":
                    attQueryModel.FILE_KIND = new List<string>
                    {
                        "12", // 機關期程調整佐證文件(填寫調整事由時)
                        "22", // 期程調整核定函(填寫調整事由時)
                        "10", // 機關期程調整准簽(送審時)
                        "11", // 機關期程調整申請表(送審時)
                        "13", // 工程竣工報告表或文件
                        "23", // 核章版工程預定進度網圖
                        "21"  // 智發會准簽(審核時)
                    };
                    break;
                case "AW03":
                    attQueryModel.FILE_KIND = new List<string> { "19" };
                    break;
            }

            result.Files = await projectCommonDac.GetProjectAttachmentList(attQueryModel);

            return result;
        }

        #region 儲存管考審核調整撤銷結果

        /// <summary>
        /// 儲存管考審核調整撤銷結果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SaveAuditReview(AdjustAuditModel model)
        {
            // 檢查是否在填報周期內，且執行情形已送出
            bool isInCycleAndIsSend = CheckIsSend(model.PROJECT_NO);

            // 先取得狀態，用於送出時寫入異動紀錄檔
            Dictionary<string, string> statuses = null;
            if (model.IS_SEND == 1)
            {
                statuses = GetProjectStatusesByAudit(model.PROJECT_NO, model.AW_KIND, model.REVIEW_RESULT, isInCycleAndIsSend);
            }

            // 若為基本資料調整審核通過，先取得經費來源資料(原本的+調整時的)
            List<ProjectBudgetSourceGModel> projectBudgetSourceG = new();
            List<ProjectCusCheckpointModel> projectCheckItemTransData = new();
            if (model.AW_KIND == "AW01" && model.IS_SEND == 1)
            {
                string projAdjCombine = GetProjAdjCombine(model.PROJECT_NO, model.PROJ_ADJ_ID);
                Task<List<ProjectBudgetSourceGModel>> projectBudgetSourceGTask = Task.Run(async () => await GetProjectBudgetSourceG(model.PROJECT_NO, projAdjCombine));
                Task.Run(() => Task.WaitAll(projectBudgetSourceGTask)).Wait();
                projectBudgetSourceG = projectBudgetSourceGTask.Result;
            }
            // 若期程調整審核通過，取得檢核點調整寫回資料
            // 實際完成日期來源為PROJECT_CHECKITEM, 其餘則為PROJECT_CHECKITEM_ADJ
            else if (model.AW_KIND == "AW02" && model.IS_SEND == 1)
            {
                Task<List<ProjectCusCheckpointModel>> projectCheckItemTransDataTask = Task.Run(async () => await dac.GetProjectCheckItemTransData(model.PROJ_ADJ_ID));
                Task.Run(() => Task.WaitAll(projectCheckItemTransDataTask)).Wait();
                projectCheckItemTransData = projectCheckItemTransDataTask.Result;
            }

            #region 檢查檔案是否重複檔名
            List<ProjectAttachmentModel> fileList = model.Files;

            List<string> result = projectCommonService.CheckFileName(fileList, "", model.PROJ_ADJ_ID);
            if (result.Any())
            {
                return ChangeResult(false, $"存檔失敗，檔名 {string.Join("、", result)} 重複");
            }
            #endregion

            dac.BeginTransaction();

            // 更新調整檔
            dac.UpdateProjectBasicAdjByAudit(model);

            // 檔案(for 期程調整審核)
            if (model.Files.Any() && model.Files[0] != null)
            {
                ProjectAttachmentModel fileModel = model.Files[0];
                #region SET INFO
                // 01:相關檔案上傳、02:其他地方上傳
                fileModel.FILE_UP_SOURCE = "02";
                //對應計畫調整檔的PK，ref PROJECT_BASIC_ADJ.PROJ_ADJ_ID
                fileModel.SOURCE_ID = model.PROJ_ADJ_ID;
                #endregion
                projectCommonService.SaveProjectFiles(model.Files[0]);
            }

            // 若為送出
            if (model.IS_SEND == 1)
            {
                if (model.REVIEW_RESULT == "Y")
                {
                    // 將目前主檔資料寫入歷程檔
                    // 目的在於保留資料覆蓋前的原始資料，以佐證管考在這段期間是否有改過資料
                    InsertProjectHisByAudit(model.PROJECT_NO, 0);

                    // 回寫資料到主檔
                    TransferProjectByAudit(model, projectBudgetSourceG, projectCheckItemTransData,isInCycleAndIsSend);
                }

                // 寫入異動記錄檔(調整審核結果) 
                int logId = projectService.InsertProjectBasicLog(model.PROJECT_NO, "S2", statuses["projAwStatusForAdjust"]);

                if (model.REVIEW_RESULT == "Y")
                {
                    // 將調整後的資料寫入歷程檔
                    InsertProjectHisByAudit(model.PROJECT_NO, logId);
                }

                // 寫入審查檔 (PROJECT_AUDIT)
                projectService.InsertProjectAudit(new ProjectAuditModel
                {
                    LOG_ID = logId,
                    PROJECT_NO = model.PROJECT_NO,
                    PLAN_REVIEW_TYPE = model.AW_KIND,
                    REVIEW_RESULT = model.REVIEW_RESULT,
                    REVIEW_COMMENTS = model.REVIEW_COMMENTS
                });

                // 更新計畫狀態
                UpdateProjectStatusesByAudit(model.PROJECT_NO, model.PROJ_ADJ_ID, statuses);
            }

            dac.Commit();
            // 重新檢視落後原因
            projectExecuteService.CheckDelayKind(model.PROJECT_NO);


            // 若為送出，寄發郵件通知
            if (model.IS_SEND == 1)
            {
                string templateId = "";
                #region 組郵件範本ID
                switch (model.REVIEW_RESULT)
                {
                    // 審核通過
                    case "Y":
                        switch (model.AW_KIND)
                        {
                            case "AW01": templateId = "RVW_PRJ_ADJ_BASIC_PASS"; break;
                            case "AW02":
                                templateId = isInCycleAndIsSend ?
                                    "RVW_PRJ_ADJ_SCH_PASS_IS_SEND" : "RVW_PRJ_ADJ_SCH_PASS"; break;
                            case "AW03": templateId = "RVW_PRJ_RVOKE_PASS"; break;
                        }
                        break;
                    // 審核未通過
                    case "N":
                        switch (model.AW_KIND)
                        {
                            case "AW01": templateId = "RVW_PRJ_ADJ_BASIC_REJECT"; break;
                            case "AW02": templateId = "RVW_PRJ_ADJ_SCH_REJECT"; break;
                            case "AW03": templateId = "RVW_PRJ_RVOKE_REJECT"; break;
                        }
                        break;
                    // 退回補正
                    case "R":
                        switch (model.AW_KIND)
                        {
                            case "AW01": templateId = "RVW_PRJ_ADJ_BASIC_RETURN"; break;
                            case "AW02": templateId = "RVW_PRJ_ADJ_SCH_RETURN"; break;
                            case "AW03": templateId = "RVW_PRJ_RVOKE_RETURN"; break;
                        }
                        break;
                }
                #endregion
                Task<bool> mailTask = Task.Run(async () => await SendMail(templateId, model.PROJECT_NO, false, model.REVIEW_COMMENTS));
                Task.Run(() => Task.WaitAll(mailTask)).Wait();

            }

            return ChangeResult(true, model.IS_SEND == 1 ? "送出成功" : "存檔成功");
        }

        /// <summary>
        /// 取得狀態(管考審核用)
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="AW_KIND">申請項目</param>
        /// <param name="REVIEW_RESULT">管考審核結果</param>
        /// <param name="isInCycleAndIsSend">是否在填報周期內且已送出</param>
        /// <returns>keys: projStatus 計畫狀態、projAwStatusForBasic主檔調整狀態、projAwStatusForAdjust 調整檔調整狀態</returns>
        private Dictionary<string, string> GetProjectStatusesByAudit(string PROJECT_NO, string AW_KIND, string REVIEW_RESULT, bool isInCycleAndIsSend)
        {
            // 處理 PROJECT_AW_STATUS
            string projAwStatusForBasic = ""; // 主檔的 PROJECT_AW_STATUS
            string projAwStatusForAdjust = ""; // 調整檔的 PROJECT_AW_STATUS
            switch (AW_KIND)
            {
                case "AW01":
                    projAwStatusForBasic = "A";
                    projAwStatusForAdjust = "A";
                    break;
                case "AW02":
                    projAwStatusForBasic = "B";
                    projAwStatusForAdjust = "B";
                    break;
                case "AW03":
                    projAwStatusForBasic = "W";
                    projAwStatusForAdjust = "W";
                    break;
            }

            switch (REVIEW_RESULT)
            {
                case "Y":
                    projAwStatusForBasic = null;
                    projAwStatusForAdjust += "05";
                    break;
                case "N":
                    projAwStatusForBasic = null;
                    projAwStatusForAdjust += "04";
                    break;
                case "R":
                    projAwStatusForBasic += "03";
                    projAwStatusForAdjust += "03";
                    break;
            }

            // 處理 PROJECT_STATUS
            Task<string> projStatusTask = Task.Run(async () => await dac.GetProjectStatus(PROJECT_NO));
            Task.Run(() => Task.WaitAll(projStatusTask)).Wait();
            string projStatus = projStatusTask.Result;

            // 撤銷通過時，PROJECT_STATUS = 8
            if (AW_KIND == "AW03" && REVIEW_RESULT == "Y")
            {
                projStatus = "8";
            }

            // 在填報期間內且已執行情形送出且期程調整審核通過
            if (AW_KIND == "AW02" && REVIEW_RESULT == "Y" && isInCycleAndIsSend)
            {
                projAwStatusForAdjust = "14";
            }

            return new Dictionary<string, string>() {
                { "projStatus", projStatus },
                { "projAwStatusForBasic", projAwStatusForBasic },
                { "projAwStatusForAdjust", projAwStatusForAdjust }
            };
        }

        /// <summary>
        /// 寫入歷程檔(PROJECT_BASIC_HIS、PROJECT_CHECKITEM_HIS、PROJECT_BUDGET_SOURCE_G_HIS、PROJECT_BUILD_KIND_HIS、PROJECT_ASST_ORG_HIS)
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="logId">異動記錄檔ID</param>
        /// <param name="AW_KIND">申請項目</param>
        /// <returns></returns>
        private void InsertProjectHisByAudit(string PROJECT_NO, int logId)
        {
            // 計畫基本資料歷程檔
            int hisId = projectDac.InsertProjectBasicHis(PROJECT_NO, logId);

            ProjectHisModel hisModel = new()
            {
                LOG_ID = logId,
                HIS_ID = hisId,
                PROJECT_NO = PROJECT_NO
            };
            projectDac.InsertProjectBudgetSourceGHis(hisModel);  // 計畫經費來源歷程檔
            projectDac.InsertProjectBuildKindHis(hisModel);      // 計畫建設類別歷程檔
            projectDac.InsertProjectAsstOrgHis(hisModel);        // 計畫協辦機關歷程檔
            projectDac.InsertProjectCheckpointHis(hisModel);     // 計畫檢核點歷程檔
        }

        #region 調整檔寫回主檔

        /// <summary>
        /// 回寫主檔資料
        /// </summary>
        /// <param name="model">管考審核撤銷model</param>
        /// <param name="projectBudgetSourceG">經費來源資料(原本的+調整後的)</param>
        /// <param name="projectCheckItemTransData">檢核點資料</param>
        private void TransferProjectByAudit(AdjustAuditModel model, List<ProjectBudgetSourceGModel> projectBudgetSourceG, 
            List<ProjectCusCheckpointModel> projectCheckItemTransData,bool isInCycleAndIsSend)

        {
            // 基本資料調整
            if (model.AW_KIND == "AW01")
            {
                dac.TransferProjectBasicByAudit(model.PROJ_ADJ_ID);
                // 協辦機關/人員
                TransferAsstOrgByAudit(model.PROJECT_NO, model.PROJ_ADJ_ID);
                // 經費來源
                TransferBudgetSourceByAudit(model, projectBudgetSourceG);
                // 建設類別
                TransferBuildKindByAudit(model.PROJECT_NO, model.PROJ_ADJ_ID);
            }
            // 期程調整
            else if (model.AW_KIND == "AW02")
            {
                // 計畫主檔
                dac.TransferProjectBasicByAuditForSchedule(model.PROJ_ADJ_ID);
                // 計畫開始日期寫回主檔
                dac.TransferControlDate1ByAudit(model.PROJ_ADJ_ID);
                // 刪除自訂檢核點設定資料
                projectDac.DeleteProjectCustomChkItemDate(model.PROJECT_NO);

                // 自訂檢核點設定資料寫回主檔
                dac.TransferCheckItemByAudit(projectCheckItemTransData);
                // 若於填報週期內辦理調整且當期執行情形已送出，則需重新執行送出
                if (isInCycleAndIsSend)
                {
                    dac.CancelLatestSendStatus(model.PROJECT_NO);
                }
            }
            // 撤銷
            else if (model.AW_KIND == "AW03")
            {
                dac.TransferProjectBasicByAuditForRevoke(model.PROJ_ADJ_ID);
                // 移除當月工程進度 & 落後原因
                projectCommonDac.DeleteLatestProjectEngProgess(new List<string>() { model.PROJECT_NO });
                projectCommonDac.DeleteLatestDelayCausal(new List<string>() { model.PROJECT_NO });
            }
        }

        /// <summary>
        /// 更新計畫狀態(主檔的 PROJECT_AW_STATUS、PROJECT_STATUS; 調整檔的 PROJECT_AW_STATUS)
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        /// <param name="statuses">狀態dict</param>
        private void UpdateProjectStatusesByAudit(string PROJECT_NO, int PROJ_ADJ_ID, Dictionary<string, string> statuses)
        {
            // 更新主檔狀態
            dac.UpdateProjBasicStatuses(PROJECT_NO, statuses["projAwStatusForBasic"], statuses["projStatus"]);
            // 更新調整檔狀態
            dac.UpdateProjBasicAdjAwSatus(PROJ_ADJ_ID, statuses["projAwStatusForAdjust"]);
        }

        /// <summary>
        /// 協辦機關/人員寫回主檔
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        private void TransferAsstOrgByAudit(string PROJECT_NO, int PROJ_ADJ_ID)
        {
            // 先刪除主檔全部
            dac.DeleteAsstOrgByProjectNo(PROJECT_NO);
            // 再從調整檔新增到主檔
            dac.InsertAsstOrgSelectAdjust(PROJ_ADJ_ID);
        }

        /// <summary>
        /// 經費來源寫回主檔
        /// </summary>
        /// <param name="model"></param>
        /// <param name="projectBudgetSourceG">經費來源資料(原本的+調整後的)</param>
        private void TransferBudgetSourceByAudit(AdjustAuditModel model, List<ProjectBudgetSourceGModel> projectBudgetSourceG)
        {
            // 處理資料
            string projAdjCombine = GetProjAdjCombine(model.PROJECT_NO, model.PROJ_ADJ_ID);
            List<int> budgetSourceIds = new(); // 原本的經費來源id: 用於table刪除
            List<int> budgetSourceAttIds = new(); // 原本經費來源對應的檔案id: 用於table刪除
            List<ProjectAttachmentModel> budgetSourceAttModels = new(); // 原本經費來源對應檔案的資料: 用於FTP刪除檔案
            List<ProjectAttachmentModel> budgetSourceAttAdjustModels = new(); // 調整後的檔案: 用於FTP複製檔案
            foreach (ProjectBudgetSourceGModel item in projectBudgetSourceG)
            {
                // 當PROJECT_NO == projAdjCombine，表為調整時的經費來源
                if (item.PROJECT_NO == projAdjCombine)
                {
                    if (item.FILE != null)
                    {
                        foreach (var file in item.FILE)
                        {
                            budgetSourceAttAdjustModels.Add(file);
                        }

                    }
                }
                // 否則為原來的經費來源
                else
                {
                    budgetSourceIds.Add(item.IDENTITY_FIELD);
                    if (item.FILE != null)
                    {
                        foreach (var file in item.FILE)
                        {
                            budgetSourceAttIds.Add(file.IDENTITY_FIELD);
                            budgetSourceAttModels.Add(file);
                        }

                    }
                }

            }
            // 1. DB
            // 刪除原本計畫經費來源資料，再修改調整後資料的PROJECT_NO 為原本的PROJECT_NO
            // ex: PROJECT_BUDGET_SOURCE_G.PROJECT_NO : 111G15099-0084 => 111G15099
            dac.TransferBudgetSourceGByAudit(budgetSourceIds, model.PROJECT_NO, projAdjCombine);
            // 刪除原本計畫經費來源對應檔案，再修改調整後資料的FILE_KIND、FILE_PATH 為原本的FILE_KIND、FILE_PATH
            // ex: PROJECT_ATTACHMENT.FILE_KIND: 05-A => 05、FILE_PATH: /111G15099-0084 => /111G15099
            dac.TransferBudgetSourceAttByAudit(
                budgetSourceAttIds,
                new ProjectAttachmentModel() { FILE_KIND = "05", PROJECT_NO = model.PROJECT_NO },
                projAdjCombine
            );

            // 2. FTP
            // 刪除原本計畫經費來源的檔案
            foreach (ProjectAttachmentModel item in budgetSourceAttModels)
            {
                ftpService.DeleteFile($"{item.FILE_PATH}/{item.IDENTITY_FIELD}{Path.GetExtension(item.FILE_NAME)}");
            }
            // 再複製調整後的檔案到原本計畫中，ex: /111G15099-0084 => /111G15099
            foreach (ProjectAttachmentModel item in budgetSourceAttAdjustModels)
            {
                // 取附檔名
                string extension = Path.GetExtension(item.FILE_NAME);
                // 組成來源路徑&目的地路徑
                string fromPath = $"{item.FILE_PATH}/{item.IDENTITY_FIELD}{extension}";
                string destPath = $"{model.PROJECT_NO}/{item.IDENTITY_FIELD}{extension}";
                // FTP複製檔案
                uploadFileService.CopyFile(fromPath, destPath);
            }
            // 刪除調整後的檔案資料夾
            ftpService.DeleteDir($"/{projAdjCombine}");
        }

        /// <summary>
        /// 建設類別寫回主檔
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        private void TransferBuildKindByAudit(string PROJECT_NO, int PROJ_ADJ_ID)
        {
            // 先刪除主檔全部
            projectDac.DeleteProjectBuildKind(PROJECT_NO);
            // 再從調整檔新增到主檔
            dac.TransferBuildKindByAudit(PROJ_ADJ_ID);
        }

        #endregion 調整檔寫回主檔

        /// <summary>
        /// 取得計畫經費來源
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="projAdjCombine">調整時的列管編號</param>
        /// <returns>若有傳入參數projAdjCombine，回傳原本的+調整後的經費來源，否則只回傳原本的</returns>
        private async Task<List<ProjectBudgetSourceGModel>> GetProjectBudgetSourceG(string PROJECT_NO, string projAdjCombine = null)
        {
            List<ProjectBudgetSourceGModel> returnModels = new();

            // 取得對應檔案
            List<ProjectAttachmentModel> files = await projectCommonDac.GetProjectAttachmentList(new ProjectAttachmentQueryModel
            {
                PROJECT_NO = PROJECT_NO,
                FILE_UP_SOURCE = "02",
                // 05,06:前瞻計畫,中央補助款 核定文件
                // 05-A, 06-A:基本資料調整時 前瞻計畫/中央補助款 核定文件
                FILE_KIND = new List<string> { "05", "06", "05-A", "06-A" },
            });


            // 取得原經費來源
            List<ProjectBudgetSourceGModel> budgetSource = await projectDac.GetProjectBudgetSourceG(PROJECT_NO);
            budgetSource.ForEach(x =>
            {
                x.FILE = files.Where(y => x.IDENTITY_FIELD == y.SOURCE_ID).ToList();
            });
            returnModels.AddRange(budgetSource);

            // 取得 調整後經費來源
            if (projAdjCombine != null)
            {
                List<ProjectBudgetSourceGModel> budgetSourceAdj = await projectDac.GetProjectBudgetSourceG(projAdjCombine);
                budgetSourceAdj.ForEach(x =>
                {
                    x.FILE = files.Where(y => x.IDENTITY_FIELD == y.SOURCE_ID).ToList();
                });
                returnModels.AddRange(budgetSourceAdj);
            }
            return returnModels;
        }

        #endregion

        /// <summary>
        /// 下載調整撤銷佐證資料壓縮檔
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        /// <param name="AW_KIND">申請項目</param>
        /// <returns>壓縮檔</returns>
        public async Task<(byte[] ms, string contentType, string fileName)> DownAdjustZip(string PROJECT_NO, int PROJ_ADJ_ID, string AW_KIND)
        {
            // 取得需要下載的檔案
            ProjectAttachmentQueryModel attQueryModel = new()
            {
                PROJECT_NO = PROJECT_NO,
                FILE_UP_SOURCE = "02",
                SOURCE_ID = PROJ_ADJ_ID
            };
            switch (AW_KIND)
            {
                case "AW01":
                    attQueryModel.FILE_KIND = new List<string> { "09" };
                    break;
                case "AW02":
                    attQueryModel.FILE_KIND = new List<string>
                    {
                        "12", // 機關期程調整佐證文件(填寫調整事由時)
                        "22", // 期程調整核定函(填寫調整事由時)
                        "10", // 機關期程調整准簽(送審時)
                        "11", // 機關期程調整申請表(送審時)
                        "13", // 工程竣工報告表或文件
                        "23"  // 核章版工程預定進度網圖
                    };
                    break;
                case "AW03":
                    attQueryModel.FILE_KIND = new List<string> { "19" };
                    break;
            }
            List<ProjectAttachmentModel> models = await projectCommonDac.GetProjectAttachmentList(attQueryModel);
            Dictionary<string, int> indexDict = models.OrderBy(x => x.SORT_ORDER).Select(x => x.NAME).Distinct().Select((name, idx) => new { idx, name }).ToDictionary(x => x.name, y => y.idx + 1);
            models.ForEach(x =>
            {
                x.NAME = $"{(indexDict.ContainsKey(x.NAME) ? indexDict[x.NAME] : 0)}.{x.NAME}";
            });
            return zipService.MakeZip(models, "佐證資料");
        }

        /// <summary>
        /// 取得期程調整申請表
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        /// <returns></returns>
        public async Task<RPTAdjustScheduleModel> GetRPTAdjustSchedule(int PROJ_ADJ_ID)
        {
            return await dac.GetRPTAdjustSchedule(PROJ_ADJ_ID);
        }

        /// <summary>
        /// 寄送計畫調整撤銷通知信
        /// </summary>
        /// <param name="TemplateId">範本ID</param>
        /// <param name="projectNo">列管編號</param>
        /// <param name="isRdecOrigin">智發會是否為正本</param>
        /// <param name="REVIEW_COMMENTS">管考審核調整撤銷意見</param>
        /// <returns></returns>
        private async Task<bool> SendMail(string TemplateId, string projectNo, bool isRdecOrigin, string REVIEW_COMMENTS)
        {
            ProjectBasicModel basicModel = await projectDac.GetProjectBasic(projectNo) ?? new();

            #region 取得收件人資訊
            // 調整撤銷收件人組合有兩種
            // 1. 正本: 智發會窗口，副本: 機關窗口、計畫實際承辦人
            // 2. 正本: 機關窗口、計畫實際承辦人，副本: 智發會窗口
            // 故用參數 isAuditOrigin 區分
            List<RecipientModel> allRcvs = new();

            // 取得智發會窗口、機關窗口收件人資料
            List<DeptContactRcvQueryModel> rcvQueryModel = new()
            {
                new() { OrgId = "380220000A", MailType = isRdecOrigin ? "1" : "2" },          // 智發會窗口
                new() { OrgId = basicModel.EXEC_ORGAN_C, MailType = isRdecOrigin ? "2" : "1" }// 機關窗口
            };
            List<RecipientModel> rdecRcvs = await projectCommonService.GetDeptContactRcvData(rcvQueryModel);

            // 計畫實際承辦人
            ProjectFillCkptComModel projectFillCkptComModel = await projectExecuteDac.GetProjectFillCkptCom(projectNo);
            RecipientModel realHostRcv = new();
            if (!string.IsNullOrEmpty(projectFillCkptComModel.REAL_EMAIL))
            {
                realHostRcv = new RecipientModel
                {
                    MAIL_TITLE = projectFillCkptComModel.REAL_CONTACT,
                    MAIL_ADDRESS = projectFillCkptComModel.REAL_EMAIL,
                    MAIL_TYPE = isRdecOrigin ? "2" : "1"
                };
            }
            else
            {
                // 計畫實際承辦人信箱沒有設定時，改用執行機關承辦人
                realHostRcv = await projectCommonService.GetSCContactRcvData(basicModel.EXEC_UNDERTAKER_C, isRdecOrigin ? "2" : "1");
            }

            allRcvs.AddRange(rdecRcvs);
            allRcvs.Add(realHostRcv);

            #endregion 取得收件人資訊

            MailTemplateParamModel mailTemplateParam = await projectCommonDac.GetMailTemplateParam(projectNo);
            mailTemplateParam.REVIEW_COMMENTS = REVIEW_COMMENTS;
            if (mailTemplateParam != null)
            {
                MailTemplateSendModel<MailTemplateParamModel> mailModel = new()
                {
                    TemplateId = TemplateId,
                    MailAddrs = allRcvs,
                    TemplatePara = mailTemplateParam
                };
                return await mailSetService.SetTemplateSend(mailModel);
            }
            return false;
        }

        /// <summary>
        /// 檢查是否在填報周期內且當期執行情形已送出
        /// </summary>
        /// <param name="projectNo"></param>
        /// <returns></returns>
        private bool CheckIsSend(string PROJECT_NO)
        {
            // 檢查是否在填報周期內
            bool isInTheFillCycle = projectDac.IsInTheFillCycle();
            if (projectDac.IsInTheFillCycle())
            {
                //檢查當期執行情形是否已送出
                Task<ProjectFillCkptComModel> getProjIsSendTask = Task.Run(async () => await projectExecuteDac.GetProjectFillCkptCom(PROJECT_NO));
                Task.Run(() => Task.WaitAll(getProjIsSendTask)).Wait();
                var projFillCkptData = getProjIsSendTask.Result;
                if (projFillCkptData != null)
                    return projFillCkptData.IS_SEND;
            }

            return false;
        }
    }
}
