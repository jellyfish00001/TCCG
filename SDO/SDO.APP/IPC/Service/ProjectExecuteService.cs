using SDO.Base.Utils;
using Microsoft.AspNetCore.Http;
using SDO.Base.Utils.Models;
using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SDO.APP.IPC.Enum;
using Microsoft.CodeAnalysis;

namespace SDO.Services
{
    public class ProjectExecuteService : Service, IProjectExecuteService
    {
        private readonly IProjectExecuteDac dac;
        private readonly IProjectDac projectDac;
        private readonly IProjectCommonDac projectCommonDac;
        private readonly ISetParamService setParamService;
        private readonly IUploadFileService uploadFileService;
        private readonly IProjectCommonService projectCommonService;
        private readonly IProjectService projectService;
        private readonly IProjectClosedDac projectCloseDac;
        private readonly IPCCDac pccDac;
        private readonly IMailSetService mailService;
        private readonly IProjectAdjustDac projectAdjustDac;


        public ProjectExecuteService(IProjectExecuteDac dac,
            IProjectDac projectDac,
            IProjectCommonDac projectCommonDac,
            ISetParamService setParamService,
            IUploadFileService uploadFileService,
            IProjectCommonService projectCommonService,
            IProjectService projectService,
            IProjectClosedDac projectCloseDac,
            IPCCDac pccDac,
            IMailSetService mailService,
            IProjectAdjustDac projectAdjustDac)
        {
            this.dac = dac;
            this.projectDac = projectDac;
            this.projectCommonDac = projectCommonDac;
            this.setParamService = setParamService;
            this.uploadFileService = uploadFileService;
            this.projectCommonService = projectCommonService;
            this.projectService = projectService;
            this.projectCloseDac = projectCloseDac;
            this.pccDac = pccDac;
            this.mailService = mailService;
            this.projectAdjustDac = projectAdjustDac;
        }

        #region 每月辦理情形
        /// <summary>
        /// 取得計畫每月辦理情形(單筆)
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SEQ"></param>
        /// <returns></returns>
        public async Task<ProjectEngineeringProgressTableModel> GetProjecFillExecute(string PROJECT_NO, string SEQ)
        {
            ProjectEngineeringProgressTableModel data = await dac.GetProjecFillExecute(PROJECT_NO, SEQ);
            // 是否為 施工方式為"工程類"且辦理開工的實際完成日期已填寫
            data.IsEngStartWork = await IsEngStartWork(PROJECT_NO);
            // 是否可存檔
            data.CanSave = await CheckProjFillExeDataCanSave(PROJECT_NO);
            // 是否辦理竣工的實際完成日期已填寫
            data.IsCompletedWork = await dac.CheckIsCompletedWork(PROJECT_NO);
            return data;
        }

        /// <summary>
        /// 取得計畫每月辦理情形清單
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="DATA_TYPE">資料種類 0:預設近5個月 1:全部</param>
        /// <returns></returns>
        public async Task<List<ProjectEngineeringProgressGridModel>> GetProjecFillExecuteList(string PROJECT_NO, string DATA_TYPE)
        {
            return await dac.GetProjecFillExecuteList(PROJECT_NO, DATA_TYPE);
        }

        /// <summary>
        /// 儲存計劃每月辦理情形
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ObjectResultModel<float>> SaveProjecFillExecute(ProjectEngineeringProgressModel model)
        {
            dac.BeginTransaction();
            if (model.SEQ > 0)
            {
                // 修改計畫工程進度
                dac.UpdateProjectEngineeringProgress(model);
            }
            else
            {
                // 新增計畫工程進度
                model.SEQ = dac.InsertProjectEngineeringProgress(model);
            }

            // 取消執行情形送出
            if (model.CancelSend)
            {
                dac.CancelSend(model.PROJECT_NO);
                projectService.InsertProjectBasicLog(model.PROJECT_NO, "S2", "11");
            }

            dac.Commit();

            // 檢查落後原因類型
            var newDelayKindAsync = Task.Run(async () => await CheckDelayKind(model));
            string newDelayKind = newDelayKindAsync.Result;
            model.IS_DELAY = !string.IsNullOrEmpty(newDelayKind);

            ProjectDelayCausalInsertModel data = new ProjectDelayCausalInsertModel()
            {
                PROJECT_NO = model.PROJECT_NO,
                DATA_YEAR = model.YEAR,
                DATA_MONTH = model.MONTH,
                DELAY_KIND = newDelayKind
            };

            // 取得計畫落後原因
            var projectDelayCausalAsync = Task.Run(async () => await dac.GetProjectDelayCausal(model.PROJECT_NO));
            ProjectDelayCausalModel projectDelayCausal = projectDelayCausalAsync.Result;

            // 增刪修 計畫落後原因
            CUDProjectDelayCausal(projectDelayCausal, data, model.IS_DELAY);

            // 若有實際施工進度 = 100，需檢查是否辦理竣工的實際完成日期已填寫
            bool isCompleteWork = true;
            if (model.IPC_ACT_PRG == 100)
            {
                var isCompleteWorkAsync = Task.Run(async () => await dac.CheckIsCompletedWork(model.PROJECT_NO));
                isCompleteWork = isCompleteWorkAsync.Result;
            }
            //取上個月執行情形欄位一起放入model
            ProjectEngineeringProgressModel model2 = await dac.GetProjecFillExecute(model.PROJECT_NO, "");
            ObjectResultModel<float> result = new()
            {
                success = true,
                message = isCompleteWork.ToString(),
                data = await dac.ExecutionProgress(model2)
            };

            return result;
        }

        /// <summary>
        /// 檢查落後原因類型
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<string> CheckDelayKind(ProjectEngineeringProgressModel model, string PROJECT_NO = "")
        {
            PROJECT_NO = model == null ? PROJECT_NO : model.PROJECT_NO;

            // 取得填報週期月份最後一天 (ex : 111_06 => 2022/06/30)
            ProjectFillCycleModel cycleData = await projectCommonDac.GetCurrentCycleData() ?? new();
            DateTime fsd = cycleData.FILL_START_DATE; // 填報開始日
            DateTime lastDay = new(fsd.Year, fsd.Month, DateTime.DaysInMonth(fsd.Year, fsd.Month));

            return await projectCommonDac.GetDelayKind(PROJECT_NO, lastDay);
        }
        #endregion

        #region 落後原因分析
        /// <summary>
        /// 取得計畫落後原因分析(單筆)
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SEQ"></param>
        /// <param name="isRdecFun"></param>
        /// <returns></returns>
        public async Task<ProjectDelayCausalModel> GetProjectFillDelay(string PROJECT_NO, string SEQ, bool isRdecFun)
        {
            // 管考進來不用檢驗
            if (!isRdecFun)
            {
                // 檢查落後項目
                CheckDelayKind(PROJECT_NO);
            }

            ProjectDelayCausalModel model = await dac.GetProjectDelayCausal(PROJECT_NO, SEQ) ?? new();
            // 檢查可否存檔
            model.CanSave = await CheckProjFillExeDataCanSave(PROJECT_NO);
            model.IS_SEND = await dac.GetIsSend(PROJECT_NO);
            return model;
        }

        /// <summary>
        /// 取得計畫落後原因分析
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="DATA_TYPE">資料種類 0:預設近5個月 1:全部</param>
        /// <returns></returns>
        public async Task<List<ProjectDelayCausalModel>> GetProjectFillDelayList(string PROJECT_NO, string DATA_TYPE)
        {
            return await dac.GetProjectDelayCausalList(PROJECT_NO, DATA_TYPE);
        }

        /// <summary>
        /// 儲存計畫落後原因
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SaveProjectFillDelay(ProjectDelayCausalModel model)
        {
            dac.BeginTransaction();
            dac.UpdateProjectDelayCausal(model);
            // 取消執行情形送出
            if (model.CancelSend)
            {
                dac.CancelSend(model.PROJECT_NO);
                projectService.InsertProjectBasicLog(model.PROJECT_NO, "S2", "11");
            }
            dac.Commit();
            return ChangeResult(true, model.SEQ.ToString());
        }

        /// <summary>
        /// 刪除計畫落後原因
        /// </summary>
        /// <param name="SEQ"></param>
        /// <returns></returns>
        public RtnResultModel DeleteProjectDelayCausal(string SEQ)
        {
            dac.DeleteProjectDelayCausal(SEQ);
            return ChangeResult(true, "刪除成功");
        }
        #endregion

        #region 檢核點完成日期
        /// <summary>
        /// 取得檢核點完成日期
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectFillCkptComModel> GetProjectFillCkptCom(string PROJECT_NO)
        {
            // 取得計畫聯絡資訊、工程發包資訊
            ProjectFillCkptComModel model = await dac.GetProjectFillCkptCom(PROJECT_NO) ?? new();
            // 取得期程調整歷程資料
            model.AdjustScheHistoryModels = await projectAdjustDac.GetProjAdjScheHistory(new List<string> { PROJECT_NO });
            // 取得檢核點資料
            model.CustomChkItemModels = await projectDac.GetProjectCusCheckpoint(PROJECT_NO);
            // 取得檔案資訊 (工程進度網圖)
            model.FileModels = await projectCommonDac.GetProjectAttachmentList(new ProjectAttachmentQueryModel
            {
                PROJECT_NO = PROJECT_NO,
                FILE_UP_SOURCE = "02",
                FILE_KIND = new List<string> { "02" }
            });

            model.CanSave = await CheckProjFillExeDataCanSave(PROJECT_NO);

            return model;
        }

        /// <summary>
        /// 儲存檢核點完成日期
        /// </summary>
        /// <param name="model"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public RtnResultModel SaveProjectFillCkptCom(ProjectFillCkptComModel model, IFormFile file)
        {
            #region 檢查所有檔案
            if (file.Length > 0)
            {
                List<ProjectAttachmentModel> fileList = new()
                {
                    new ProjectAttachmentModel()
                    {
                        PROJECT_NO = model.PROJECT_NO,
                        FILE_KIND = "02",
                        EditFiles = new()
                        {
                            new UploadTempFileModel
                            {
                                FileName = file.FileName,
                                EditType = 1,
                            }
                        }
                    }
                };
                List<string> result = projectCommonService.CheckFileName(fileList);
                if (result.Any())
                {
                    return ChangeResult(false, $"存檔失敗，檔名 {string.Join("、", result)} 重複");
                }
            }
            #endregion

            if (!string.IsNullOrEmpty(model.CONTRACT_FINISH_DATE_FOR_SAVE))
            {
                model.CONTRACT_FINISH_DATE = DateTime.ParseExact(model.CONTRACT_FINISH_DATE_FOR_SAVE, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            }

            model.PCC_PROJECT_NO ??= string.Empty;

            dac.BeginTransaction();

            /// 移除開工實際完成日期，澤清空竣工相關資料及移除已上傳工程進度檔案
            if (model.DeleteEngData)
            {
                model.CONTRACT_FINISH_DATE = null;
                model.TENDER_AWARDING_AMT = 0;
                model.PROCUREMENT_AMT = 0;
                RemoveEngProgressFile(model.PROJECT_NO);
            }
            // 更新計畫基本資料
            dac.UpdateProjectBasicCkptCom(model);

            // 儲存聯繫資訊
            dac.UpdateProjectFillContact(model);

            // 儲存檢核點 - 實際完成日期
            if (model.CustomChkItemModels != null && model.CustomChkItemModels.Any())
            {
                dac.UpdateActualEndDate(model.CustomChkItemModels);

                // 如果 CTRL_POINT=C 有異動，要同步比這筆CHECKITEM_SEQ大的實際完成日
                if (model.CustomChkItemModels.Any(x => x.CTRL_POINT == "C"))
                {
                    ProjectCusCheckpointModel endWork = model.CustomChkItemModels.Where(x => x.CTRL_POINT == "C").First();
                    dac.UpdateActualEndDateSync(endWork);
                }

                // 竣工日期有填 每月辦理情形-累計預定施工進度% & 累計實際施工進度% 設為100
                var completedWork = model.CustomChkItemModels.Where(x => x.CTRL_POINT == "B").FirstOrDefault();
                if (completedWork != null && completedWork.ACTUAL_ENDDATE_FOR_SAVE.HasValue)
                {
                    dac.UpdateFillCycleIpcPrg(model.PROJECT_NO, 100);
                }
            }

            // 取消執行情形送出
            if (model.CancelSend)
            {
                dac.CancelSend(model.PROJECT_NO);
                projectService.InsertProjectBasicLog(model.PROJECT_NO, "S2", "11");
            }

            #region 檔案異動
            // 儲存檔案    
            if (file.Length > 0 && !model.DeleteEngData)
            {
                int insertedId = projectCommonDac.InsertProjectAttachment(new ProjectAttachmentModel()
                {
                    PROJECT_NO = model.PROJECT_NO,
                    FILE_KIND = "02",
                    FILE_NAME = file.FileName,
                    FILE_PATH = $"/{model.PROJECT_NO}",
                    FILE_UP_SOURCE = "02"
                });

                // 上傳檔案
                SaveFileModel fileModel = new()
                {
                    SavePath = $"/{model.PROJECT_NO}",
                    NewFileName = $"{insertedId}{Path.GetExtension(file.FileName)}",
                };
                uploadFileService.SaveFile(file, fileModel);
            }
            // 刪除特定檔案
            if (model.RemovedFileIds != null && model.RemovedFileIds.Count > 0)
            {
                RemoveEngProgressFile(model.PROJECT_NO, model.RemovedFileIds);
            }
            #endregion
            dac.Commit();

            // 檢查落後項目
            CheckDelayKind(model.PROJECT_NO);
            return ChangeResult(true, "存檔成功");
        }

        /// <summary>
        /// 移除工程進度檔案
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="removedFileIds">移除特定檔案識別碼</param>
        private void RemoveEngProgressFile(string PROJECT_NO, List<int> removedFileIds = null)
        {
            // 需移除檔案資料
            List<ProjectAttachmentModel> removeFileDatas = new();

            // 若無指定移除的檔案識別碼，則全部移除
            if (removedFileIds == null || removedFileIds.Count == 0)
            {
                ProjectAttachmentQueryModel fileQueryModel = new()
                {
                    PROJECT_NO = PROJECT_NO,
                    FILE_KIND = new List<string>() { "02" },
                    FILE_UP_SOURCE = "02"
                };
                // 取得該計畫所有已上傳工程進度檔案
                var getFileTask = Task.Run(async () => await projectCommonDac.GetProjectAttachmentList(fileQueryModel));
                Task.Run(() => Task.WaitAll(getFileTask)).Wait();
                removeFileDatas = getFileTask.Result;
            }
            else
            {
                // 取得特定檔案資料
                var getFileTask = Task.Run(async () => await projectCommonDac.GetProjectAttachment(removedFileIds));
                Task.Run(() => Task.WaitAll(getFileTask)).Wait();
                removeFileDatas = getFileTask.Result;
            }


            // 移除Ftp檔案 & DB
            if (removeFileDatas != null && removeFileDatas.Any())
            {
                foreach (ProjectAttachmentModel fileData in removeFileDatas)
                {
                    uploadFileService.DeleteFile(Path.Combine($"/{PROJECT_NO}",
                        fileData.IDENTITY_FIELD + Path.GetExtension(fileData.FILE_NAME)));
                    projectCommonDac.DeleteProjectAttachment(fileData.IDENTITY_FIELD);
                }
            }
        }

        /// <summary>
        /// 取得 落後原因類型
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        public async Task<string> GetDelayKind(string PROJECT_NO)
        {
            // 取得當期辦理情形資料
            ProjectEngineeringProgressModel data = await dac.GetCurrentProjectEngineeringProgress(PROJECT_NO);

            // 檢查落後原因類型
            string newDelayKind = await CheckDelayKind(data, PROJECT_NO);

            return newDelayKind;
        }

        /// <summary>
        /// 檢核落後項目-增刪修計畫落後原因
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        public void CheckDelayKind(string PROJECT_NO)
        {
            var delayKindTask = Task.Run<string>(async () => await GetDelayKind(PROJECT_NO));
            string newDelayKind = delayKindTask.Result;

            // 取得填報週期資料
            var getCurrentCycleDataTask = Task.Run(async () => await projectCommonDac.GetCurrentCycleData());
            ProjectFillCycleModel cycleData = getCurrentCycleDataTask.Result;

            ProjectDelayCausalInsertModel data = new()
            {
                PROJECT_NO = PROJECT_NO,
                DATA_YEAR = cycleData.PROJECT_YEAR,
                DATA_MONTH = cycleData.PROJECT_MONTH,
                DELAY_KIND = newDelayKind
            };

            // 取得計畫落後原因
            var projectDelayCausalAsync = Task.Run<ProjectDelayCausalModel>(async () => await dac.GetProjectDelayCausal(PROJECT_NO));
            ProjectDelayCausalModel projectDelayCausal = projectDelayCausalAsync.Result;

            // 增刪修計畫落後原因
            CUDProjectDelayCausal(projectDelayCausal, data, !string.IsNullOrEmpty(newDelayKind));
        }

        /// <summary>
        /// 清空當次週期已填報的檢核點完成日期、辦理情形及落後原因
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public RtnResultModel ClearCycleData(string PROJECT_NO)
        {
            var projFillCycleAsync = Task.Run(async () => await dac.GetProjFillCycle());
            ProjectFillCycleModel projFillCycle = projFillCycleAsync.Result;

            // 檢核點完成日期 ctrlPoint=A(開工) 以後的"實際完成日期"要清除，且在填報周期內
            // 每日辦理情形 刪除填報周期內的那1筆資料
            // 落後原因分析 刪除填報周期內的那1筆資料
            dac.ClearCycleData(PROJECT_NO, projFillCycle);
            return ChangeResult(true);
        }
        #endregion

        #region 其他資料
        /// <summary>
        /// 取得其他資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectFillOtherModel> GetProjectFillOther(string PROJECT_NO)
        {
            ProjectFillOtherModel result = await GetProjectBid(PROJECT_NO);
            result.ProjectActivity = await GetProjectActivity(PROJECT_NO);
            ProjectFillOtherModel reviewData = await GetProjectReview(PROJECT_NO);
            result.SetParam = reviewData.SetParam;
            result.ProjectReview = reviewData.ProjectReview;
            result.ProjectTender = await GetProjectTender(PROJECT_NO);
            return result;
        }

        /// <summary>
        /// 儲存其他資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SaveProjectFillOther(ProjectFillOtherModel model)
        {
            dac.BeginTransaction();
            SaveProjectBid(model);
            SaveProjectActivity(model.ProjectActivity);
            SaveProjectReview(model.ProjectReview);
            SaveProjectTender(model.ProjectTender);
            dac.Commit();
            return ChangeResult(true, "存檔成功");
        }

        /// <summary>
        /// 取得其他資料招標情形
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectFillOtherModel> GetProjectBid(string PROJECT_NO)
        {
            ProjectFillOtherModel result = new ProjectFillOtherModel();
            List<ProjectBidModel> ProjectBid = new List<ProjectBidModel>();
            List<ProjectBidModel> projectBidData = await dac.GetProjectBid(PROJECT_NO);
            //若projectBid無資料，撈取SET_PARAM.SET_TYPE=BID_KIND
            if (projectBidData.Any())
            {
                ProjectBid = projectBidData;
            }
            else
            {
                IList<SetParamModel> paramData = await setParamService.GetSysParams("BID_KIND");
                foreach (SetParamModel item in paramData)
                {
                    ProjectBid.Add(new ProjectBidModel
                    {
                        PROJECT_NO = PROJECT_NO,
                        BID_KIND = item.SET_TYPE,
                        BID_NAME = item.SET_VALUE,
                        editType = 1,
                    });
                }
            }
            result.ProjectBid = ProjectBid;
            result.ProjectBidDetail = await dac.GetProjectBidDetail(PROJECT_NO) ?? new List<ProjectBidDetailModel>();
            return result;
        }

        /// <summary>
        /// 儲存其他資料招標情形
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void SaveProjectBid(ProjectFillOtherModel model)
        {
            //招標情形
            List<ProjectBidModel> insertProjectBidList = model.ProjectBid.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
            List<ProjectBidModel> updateProjectBidList = model.ProjectBid.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
            //招標情形歷程
            List<ProjectBidDetailModel> insertProjectBidDetailList = model.ProjectBidDetail.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
            List<ProjectBidDetailModel> updateProjectBidDetailList = model.ProjectBidDetail.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
            List<ProjectBidDetailModel> deleteProjectBidDetailList = model.ProjectBidDetail.Where(x => (editTypeEnum)x.editType == editTypeEnum.Delete).ToList();

            //招標情形
            dac.InsertProjectBid(insertProjectBidList);
            dac.UpdateProjectBid(updateProjectBidList);
            //招標情形歷程
            dac.InsertProjectBidDetail(insertProjectBidDetailList);
            dac.UpdateProjectBidDetail(updateProjectBidDetailList);
            dac.DeleteProjectBidDetail(deleteProjectBidDetailList);
        }

        /// <summary>
        /// 取得其他資料相關活動
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectActivityModel>> GetProjectActivity(string PROJECT_NO)
        {
            IList<SetParamModel> paramData = await setParamService.GetSysParams("ACTIVITY_KIND");
            List<ProjectActivityModel> result = new List<ProjectActivityModel>();
            List<ProjectActivityModel> activityData = await dac.GetProjectActivity(PROJECT_NO);
            //若projectActivity無資料，撈取SET_PARAM.SET_TYPE=ACTIVITY_KIND
            if (activityData.Any())
            {
                result = activityData;
                if (!activityData.Where(x => x.ACTIVITY_KIND == "04").Any())
                {
                    result.Add(new ProjectActivityModel
                    {
                        PROJECT_NO = PROJECT_NO,
                        ACTIVITY_KIND = "04",
                        ACTIVITY_KIND_NAME = paramData.Where(x => x.SET_TYPE == "04").Select(x => x.SET_VALUE).FirstOrDefault(),
                        editType = 0,
                    });
                }
            }
            else
            {
                foreach (SetParamModel item in paramData)
                {
                    result.Add(new ProjectActivityModel
                    {
                        PROJECT_NO = PROJECT_NO,
                        ACTIVITY_KIND = item.SET_TYPE,
                        ACTIVITY_KIND_NAME = item.SET_VALUE,
                        editType = item.SET_TYPE == "04" ? 0 : 1,
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// 儲存其他資料相關活動
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void SaveProjectActivity(List<ProjectActivityModel> model)
        {
            List<ProjectActivityModel> insertList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
            List<ProjectActivityModel> updateList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
            List<ProjectActivityModel> deleteList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Delete).ToList();

            dac.InsertProjectActivity(insertList);
            dac.UpdateProjectActivity(updateList);
            dac.DeleteProjectActivity(deleteList);
        }

        /// <summary>
        /// 取得其他資料相關審查
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectFillOtherModel> GetProjectReview(string PROJECT_NO)
        {
            ProjectFillOtherModel result = new ProjectFillOtherModel();
            result.SetParam = await setParamService.GetSysParams("REVIEW_KIND");
            List<ProjectReviewModel> ProjectReviewList = new List<ProjectReviewModel>();
            List<ProjectReviewModel> reviewData = await dac.GetProjectReview(PROJECT_NO);
            //若projectReview無資料，撈取SET_PARAM.SET_TYPE=REVIEW_KIND
            if (reviewData.Any())
            {
                ProjectReviewList = reviewData;
            }
            else
            {
                foreach (SetParamModel item in result.SetParam)
                {
                    if (item.SET_TYPE != "04")
                    {
                        ProjectReviewList.Add(new ProjectReviewModel
                        {
                            PROJECT_NO = PROJECT_NO,
                            REVIEW_KIND = item.SET_TYPE,
                            REVIEW_NAME = item.SET_VALUE,
                            editType = 1,
                        });
                    }
                }
            }
            result.ProjectReview = ProjectReviewList;
            return result;
        }

        /// <summary>
        /// 儲存其他資料相關審查
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void SaveProjectReview(List<ProjectReviewModel> model)
        {
            List<ProjectReviewModel> insertList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
            List<ProjectReviewModel> updateList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
            List<ProjectReviewModel> deleteList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Delete).ToList();

            dac.InsertProjectReview(insertList);
            dac.UpdateProjectReview(updateList);
            dac.DeleteProjectReview(deleteList);
        }

        /// <summary>
        /// 取得其他資料廠商資訊
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectTenderModel>> GetProjectTender(string PROJECT_NO)
        {
            return await dac.GetProjectTender(PROJECT_NO);
        }

        /// <summary>
        /// 儲存其他資料廠商資訊
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void SaveProjectTender(List<ProjectTenderModel> model)
        {
            List<ProjectTenderModel> insertList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
            List<ProjectTenderModel> updateList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
            List<ProjectTenderModel> deleteList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Delete).ToList();

            dac.InsertProjectTender(insertList);
            dac.UpdateProjectTender(updateList);
            dac.DeleteProjectTender(deleteList);
        }
        #endregion

        #region 執行情形送出
        /// <summary>
        /// 執行情形送出檢核
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectExecuteSubmitModel> CheckProjectFillExecuteSubmit(string PROJECT_NO)
        {
            ProjectExecuteSubmitModel submitResultModel = new();
            List<ProjectSubmitErrorModel> errModels = new();

            // 檢查當期執行情形是否已送出
            submitResultModel.IsSubmitted = await dac.CheckProjectFillExecuteIsSend(PROJECT_NO);
            // 未送出才需檢驗資料
            if (!submitResultModel.IsSubmitted)
            {
                // 驗證檢核點完成日期必填欄位
                await ChkProjFillCkptComValid(PROJECT_NO, errModels);
                // 驗證每月辦理情形必填欄位
                await ChkProjEngProgressValid(PROJECT_NO, errModels);
                // 驗證落後原因分析必填欄位
                await CheckProjDelayCausalValid(PROJECT_NO, errModels);
            }

            #region 結案申請
            // 驗證是否可提出結案申請
            bool closeApplyValid = await CheckProjCloseValid(PROJECT_NO, errModels);
            submitResultModel.projectCloseApplyValid = closeApplyValid;
            #endregion

            submitResultModel.ErrorModels = errModels;
            return submitResultModel;
        }

        /// <summary>
        /// 驗證檢核點完成日期必填欄位
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="errModels"></param>
        /// <returns></returns>
        private async Task ChkProjFillCkptComValid(string PROJECT_NO, List<ProjectSubmitErrorModel> errModels)
        {
            ProjectFillCkptComModel model = await GetProjectFillCkptCom(PROJECT_NO);
            List<string> errMsgs = new();
            if (string.IsNullOrEmpty(model.REAL_CONTACT))
            {
                errMsgs.Add("「計畫實際承辦人」");
            }

            if (string.IsNullOrEmpty(model.REAL_TEL))
            {
                errMsgs.Add("「電話」");
            }

            if (string.IsNullOrEmpty(model.REAL_EMAIL))
            {
                errMsgs.Add("「信箱」");
            }

            if (model.CustomChkItemModels != null)
            {
                ProjectCusCheckpointModel startWork = model.CustomChkItemModels.Where(x => x.CTRL_POINT == "A").FirstOrDefault();
                if (startWork != null && startWork.ACTUAL_ENDDATE.HasValue)
                {
                    if (!model.CONTRACT_FINISH_DATE.HasValue)
                    {
                        errMsgs.Add("「契約預定竣工日」");
                    }

                    if (model.FileModels != null && !model.FileModels.Any())
                    {
                        errMsgs.Add("「工程預定進度表」");
                    }
                }
            }

            // 檢查是否為工程類 且 檢查辦理開工實踐完成日期是否有填
            bool isEngStartWork = await IsEngStartWork(PROJECT_NO);
            if (isEngStartWork)
            {
                // 檢查是否關聯工程會
                bool isAssociatePCC = await pccDac.CheckProjIsAssociatePCC(PROJECT_NO);
                if (!isAssociatePCC)
                {
                    errMsgs.Add("尚未關聯工程會標案系統");
                }
            }

            // 若「每月辦理情形」的實際施工進度 = 100，檢查「檢核點完成日期」的竣工實際完成日期需填寫
            var isCompleteWorkAsync = Task.Run(async () => await IsEngCompleteWork(PROJECT_NO));
            bool isCompleteWork = isCompleteWorkAsync.Result;
            if (!isCompleteWork)
            {
                errMsgs.Add("「竣工完成日期」");
            }

            if (errMsgs.Any())
            {
                string chapterId = "ProjectFillCkptCom";
                errModels.Add(new ProjectSubmitErrorModel
                {
                    Chapter = "檢核點完成日期",
                    ChapterId = chapterId,
                    ChapterUrl = await projectCommonDac.GetChapterPath(chapterId),
                    ErrMsg = $"{string.Join('、', errMsgs)} 尚未填報"
                });
            }
        }

        /// <summary>
        /// 驗證每月辦理情形必填欄位
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="errModels"></param>
        /// <returns></returns>
        private async Task ChkProjEngProgressValid(string PROJECT_NO, List<ProjectSubmitErrorModel> errModels)
        {
            // 取得需驗證資料
            ProjectEngineeringProgressTableModel model = await dac.GetProjecFillExecute(PROJECT_NO, "");
            // 是否為 施工方式為"工程類"且辦理開工的實際完成日期已填寫
            model.IsEngStartWork = await IsEngStartWork(PROJECT_NO);
            List<string> errMsgs = projectCommonService.CheckModelRequiredField(model);

            if (errMsgs.Any())
            {
                string chapterId = "ProjectFillExecute";
                errModels.Add(new ProjectSubmitErrorModel()
                {
                    Chapter = "每月辦理情形",
                    ChapterId = chapterId,
                    ChapterUrl = await projectCommonDac.GetChapterPath(chapterId),
                    ErrMsg = $"{string.Join('、', errMsgs)} 尚未填報"
                });
            }
        }

        /// <summary>
        /// 驗證落後原因分析必填欄位
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="errModels"></param>
        /// <returns></returns>
        private async Task CheckProjDelayCausalValid(string PROJECT_NO, List<ProjectSubmitErrorModel> errModels)
        {
            ProjectDelayCausalModel projectDelayCausal = await dac.GetProjectDelayCausal(PROJECT_NO);
            if (projectDelayCausal == null)
            {
                return;
            }

            List<string> errMsgs = new();
            if (string.IsNullOrEmpty(projectDelayCausal.DELAY_CLASS_C))
            {
                errMsgs.Add("「落後類別」");
            }
            if (string.IsNullOrEmpty(projectDelayCausal.DELAY_RESPON))
            {
                errMsgs.Add("「責任歸屬」");
            }
            if (string.IsNullOrEmpty(projectDelayCausal.DELAY_CAUSAL))
            {
                errMsgs.Add("「落後原因」");
            }
            if (string.IsNullOrEmpty(projectDelayCausal.SOLUTION))
            {
                errMsgs.Add("「解決對策」");
            }
            if (string.IsNullOrEmpty(projectDelayCausal.COORDINATION))
            {
                errMsgs.Add("「需協辦事項」");
            }
            if (!projectDelayCausal.DEADLINES.HasValue)
            {
                errMsgs.Add("「改進完成期限」");
            }

            if (errMsgs.Any())
            {
                string chapterId = "ProjectFillDelay";
                errModels.Add(new ProjectSubmitErrorModel
                {
                    Chapter = "落後原因分析",
                    ChapterId = chapterId,
                    ChapterUrl = await projectCommonDac.GetChapterPath(chapterId),
                    ErrMsg = $"{string.Join('、', errMsgs)} 尚未填報"
                });
            }
        }

        /// <summary>
        /// 驗證是否可提出結案申請
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="errModels"></param>
        /// <returns></returns>
        private async Task<bool> CheckProjCloseValid(string PROJECT_NO, List<ProjectSubmitErrorModel> errModels)
        {
            // 取得計畫狀態
            string projectStatus = await projectDac.GetProjectStatus(PROJECT_NO);

            // 1. 計畫狀態須為執行情形才可提出
            if (string.IsNullOrEmpty(projectStatus) || (projectStatus != "6" && projectStatus != "4"))
            {
                return false;
            }

            // 2. 最後一筆檢核點項目的實際完成日期是否有填
            bool lastChkptIsFilled = await dac.ChkLastChkptActualEndDateIsFilled(PROJECT_NO);
            if (!lastChkptIsFilled)
            {
                return false;
            }

            // 3. 結案資料必填欄位是否有填
            ProjectFillCloseModel closeModel = await projectCloseDac.GetProjectFillClose(PROJECT_NO);
            List<string> errMsgs = projectCommonService.CheckModelRequiredField(closeModel);
            if (!closeModel.ProjAttachments.Any())
                errMsgs.Add("「佐證資料」");

            // 4. 累計經費執行率需等於100%
            decimal expense = (closeModel.ACTUAL_PAY ?? 0) + (closeModel.UNPAY ?? 0) + (closeModel.BALANCE ?? 0);
            decimal totalBudget = closeModel.TOTAL_BUDGET;
            if (totalBudget == 0 || (expense / totalBudget) * 100 != 100)
            {
                errMsgs.Add("累計經費執行率需為100%");
            }

            if (errMsgs.Any())
            {
                string chapterId = "ProjectFillClose";
                errModels.Add(new ProjectSubmitErrorModel
                {
                    Chapter = "結案資料",
                    ChapterId = chapterId,
                    ChapterUrl = await projectCommonDac.GetChapterPath(chapterId),
                    ErrMsg = $"{string.Join('、', errMsgs)} 尚未填報"
                });
                return false;
            }

            return true;
        }

        /// <summary>
        /// 執行情形送出 送出/結案申請
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SaveType"></param>
        /// <returns></returns>
        public RtnResultModel SaveProjectFillExecuteSubmit(string PROJECT_NO, int SaveType)
        {
            #region get required data
            // 取得當期填報周期資料

            var getCurrentProjectFillCycleTask = Task.Run(async () => await projectCommonDac.GetCurrentCycleData());
            Task.Run(() => Task.WaitAll(getCurrentProjectFillCycleTask)).Wait();
            ProjectFillCycleModel projCycleModel = getCurrentProjectFillCycleTask.Result;

            // 檢查當期執行情形是否已送出
            var getProjectFillExecuteIsSendTask = Task.Run(async () => await dac.CheckProjectFillExecuteIsSend(PROJECT_NO));
            Task.Run(() => Task.WaitAll(getProjectFillExecuteIsSendTask)).Wait();
            bool isProjSend = getProjectFillExecuteIsSendTask.Result;
            #endregion

            dac.BeginTransaction();
            // 送出計畫執行情形
            if (!isProjSend)
                dac.SendProjectFillExecute(PROJECT_NO, projCycleModel.PROJECT_YEAR, projCycleModel.PROJECT_MONTH);

            // 新增計畫異動記錄檔
            // 異動狀態：執行情形送出 => 9 / 結案送審 => 5
            projectService.InsertProjectBasicLog(PROJECT_NO, "S2", SaveType == 1 ? "9" : "5");

            // 結案申請
            if (SaveType == 2)
            {
                // 更新ProjectBasic計畫狀態為結案審核階段
                projectDac.UpdateProjectStatusAndMemo(PROJECT_NO, "5");
                var hasAuditData = projectDac.GetProjectAudit(PROJECT_NO, "P2");
                // 新增計畫審查檔
                projectDac.InsertProjectAudit(new ProjectAuditModel()
                {
                    LOG_ID = 0,
                    PROJECT_NO = PROJECT_NO,
                    PLAN_REVIEW_TYPE = "P2",// 結案
                    REVIEW_RESULT = string.Empty,
                    REVIEW_COMMENTS = string.Empty
                });
            }
            dac.Commit();
            if (SaveType == 2)
            {
                // 結案送審寄信
                Task<bool> sendMailTask = Task.Run(async () => await SendProjectClosedApplyMail(PROJECT_NO));
                Task.Run(() => Task.WaitAll(sendMailTask)).Wait();
            }

            return ChangeResult(true, SaveType == 1 ? "送出成功" : "已成功提出結案申請");
        }

        /// <summary>
        /// 結案申請寄信
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="rvwResult"></param>
        /// <returns></returns>
        private async Task<bool> SendProjectClosedApplyMail(string PROJECT_NO)
        {
            ProjectBasicModel basicModel = await projectDac.GetProjectBasic(PROJECT_NO) ?? new();
            // 取得計畫實際承辦人
            ProjectFillCkptComModel projContactModel = await dac.GetProjectFillCkptCom(PROJECT_NO);
            #region 取得收件人資訊
            List<RecipientModel> allRcvs = new();
            List<DeptContactRcvQueryModel> rcvQueryModel = new()
            {
                new() { OrgId = "380220000A", MailType = "1" },           // 正本智發會窗口
                new() { OrgId = basicModel.EXEC_ORGAN_C, MailType = "2" },// 副本機關窗口
            };
            // 取得收件人資料
            List<RecipientModel> rdecRcvs = await projectCommonService.GetDeptContactRcvData(rcvQueryModel);
            allRcvs.AddRange(rdecRcvs);

            if (projContactModel != null)
            {
                // 副本 計畫實際承辦人收件資訊
                RecipientModel projRelContactRcv = new()
                {
                    MAIL_TITLE = projContactModel.REAL_CONTACT,
                    MAIL_ADDRESS = projContactModel.REAL_EMAIL,
                    MAIL_TYPE = "2"
                };
                allRcvs.Add(projRelContactRcv);
            }

            #endregion 取得收件人資訊
            MailTemplateParamModel mailTemplateParam = await projectCommonDac.GetMailTemplateParam(PROJECT_NO);
            if (mailTemplateParam != null)
            {
                MailTemplateSendModel<MailTemplateParamModel> mailModel = new()
                {
                    TemplateId = "PRJ_CLOSE_SUBMIT",
                    MailAddrs = allRcvs,
                    TemplatePara = mailTemplateParam
                };
                return await mailService.SetTemplateSend(mailModel);
            }
            return false;
        }
        #endregion

        #region 管考備註
        /// <summary>
        /// 管考意見寄信
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> SendProjectAuditOpinionMail(ProjectEngineeringAuditOpinionModel model)
        {
            //正本：計畫實際承辦人、機關窗口
            //副本：智發會窗口

            // 取得執行機關
            ProjectBasicModel basicModel = await projectDac.GetProjectBasic(model.PROJECT_NO) ?? new();
            // 取得計畫實際承辦人
            ProjectFillCkptComModel projContactModel = await dac.GetProjectFillCkptCom(model.PROJECT_NO);

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

            MailTemplateParamModel mailTemplateParam = await projectCommonDac.GetMailTemplateParam(model.PROJECT_NO);
            mailTemplateParam.FILL_MONTH = $"{model.YEAR}_{(int.TryParse(model.MONTH, out int month) ? $"{month}" : string.Empty)}";
            mailTemplateParam.AUDIT_OPINION = model.AUDIT_OPINION;
            mailTemplateParam.AUDIT_MEMO = ShowMemo(model.ComIPCMemoMappingData);

            bool sendResult = false;
            if (mailTemplateParam != null)
            {
                MailTemplateSendModel<MailTemplateParamModel> mailModel = new()
                {
                    TemplateId = "RVW_AUDIT_SEND",
                    MailAddrs = allRcvs,
                    TemplatePara = mailTemplateParam
                };
                sendResult = await mailService.SetTemplateSend(mailModel);
            }
            if (sendResult)
            {
                return ChangeResult(true, "信件寄發成功");
            }
            else
            {
                return ChangeResult(false, "信件寄發失敗");
            }
        }

        /// <summary>
        /// 信件通知組管考備註
        /// </summary>
        /// <param name="mappingData"></param>
        /// <returns></returns>
        private string ShowMemo(List<ProjectMappingDataModel> mappingData)
        {
            var getComIPCMemoTask = Task.Run(async () => await setParamService.GetSysParams("COM_IPCMEMO"));
            Task.Run(() => Task.WaitAll(getComIPCMemoTask)).Wait();
            IList<SetParamModel> auditMemo = getComIPCMemoTask.Result;

            int count = 1;
            string result = "";
            foreach (ProjectMappingDataModel item in mappingData)
            {
                result += $"{count}.{auditMemo.Where(x => x.SET_TYPE == item.SET_TYPE).Select(x => x.SET_VALUE).FirstOrDefault()}";
                count++;
                if (!item.Equals(mappingData.LastOrDefault()))
                {
                    result += "\n";
                }
            }
            return result;
        }

        /// <summary>
        /// 取得平時管考意見
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectFillAuditModel> GetProjectFillAudit(string PROJECT_NO)
        {
            ProjectFillAuditModel result = new ProjectFillAuditModel()
            {
                ProjectBasic = await dac.GetProjectBasic(PROJECT_NO) ?? new ProjectBasicForProjectFillAuditModel(),
                ProjectEngineeringAuditOpinion = await GetProjectEngineeringAuditOpinion(PROJECT_NO),
                ProjectConference = await dac.GetProjectConference(PROJECT_NO),
                ProjectDelayfill = await dac.GetProjectDelayfill(PROJECT_NO),
                SpecNoteMappingData = await projectCommonDac.GetProjectMappingData(PROJECT_NO, "SPEC_NOTE", PROJECT_NO),
                ProjectMergeLog = await GetProjectMergeLog(PROJECT_NO),
                ProjectFactFinding = await GetProjectFactFinding(PROJECT_NO),
                ProjectCloseDetails = await dac.GetProjectCloseDetails(PROJECT_NO),
                ProjectCloseMemo = await dac.GetProjectCloseMemo(PROJECT_NO) ?? new ProjectCloseMemoModel(),
                DelayApply = await dac.GetDelayApply(PROJECT_NO),
                RevokeData = await dac.GetRevokeData(PROJECT_NO),
            };
            return result;
        }

        /// <summary>
        /// 取得管考審核意見
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        private async Task<List<ProjectEngineeringAuditOpinionModel>> GetProjectEngineeringAuditOpinion(string PROJECT_NO)
        {
            List<ProjectEngineeringAuditOpinionModel> result = await dac.GetProjectEngineeringAuditOpinion(PROJECT_NO);
            List<ProjectMappingDataModel> mappingData = await projectCommonDac.GetProjectMappingData(PROJECT_NO, "COM_IPCMEMO");
            foreach (ProjectEngineeringAuditOpinionModel item in result)
            {
                item.ComIPCMemoMappingData = mappingData.Where(x => x.SOURCE_ID == item.SEQ.ToString()).ToList();
            }
            return result;
        }

        /// <summary>
        /// 取得計畫分併案記錄檔
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        private async Task<List<ProjectMergeLogModel>> GetProjectMergeLog(string PROJECT_NO)
        {
            List<ProjectMergeLogModel> result = await dac.GetProjectMergeLog(PROJECT_NO);

            //分案併案檔案
            List<ProjectAttachmentModel> statusFiles = await projectCommonDac.GetProjectAttachmentList(new ProjectAttachmentQueryModel
            {
                PROJECT_NO = PROJECT_NO,
                FILE_UP_SOURCE = "02",
                FILE_KIND = new List<string> { "17", "18" }
            });
            foreach (ProjectMergeLogModel item in result)
            {
                //MERGE_STATUS = 01(分案) FILE_KIND = 17(分案核定公文)
                //MERGE_STATUS = 02(併案) FILE_KIND = 18(併案核定公文)
                string fileKind = item.MERGE_STATUS == "01" ? "17" : "18";
                item.File = statusFiles.Where(x => x.FILE_KIND == fileKind && x.SOURCE_ID == item.SEQ).ToList();
            }

            return result;
        }

        /// <summary>
        /// 取得實地查證
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectFactFindingModel>> GetProjectFactFinding(string PROJECT_NO)
        {
            List<ProjectFactFindingModel> result = await dac.GetProjectFactFinding(PROJECT_NO);
            List<ProjectAttachmentModel> files = await projectCommonDac.GetProjectAttachmentList(new ProjectAttachmentQueryModel
            {
                PROJECT_NO = PROJECT_NO,
                FILE_UP_SOURCE = "02",
                FILE_KIND = new List<string> { "07", "08" }
            });
            result.ForEach(x =>
            {
                //管考 實地查證紀錄檔案(02:其他地方上傳 FileKind：07)
                //主辦 實地查證參採資料檔案(02:其他地方上傳 FileKind：08)
                x.RdecFile = files.Where(y => y.FILE_KIND == "07" && x.SEQ == y.SOURCE_ID).ToList();
                x.HandFile = files.Where(y => y.FILE_KIND == "08" && x.SEQ == y.SOURCE_ID).ToList();
            });

            return result;
        }

        /// <summary>
        /// 儲存平時管考意見
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SaveProjectFillAudit(ProjectFillAuditModel model)
        {
            string projectNo = model.ProjectBasic.PROJECT_NO;
            #region 檢查管考審核意見期間
            //管考審核意見有新增或修改資料 需先檢核期間有無重複
            List<ProjectEngineeringAuditOpinionModel> AuditOpinion = model.ProjectEngineeringAuditOpinion.Where(x => (editTypeEnum)x.editType != editTypeEnum.Delete).ToList();
            if (AuditOpinion.Any())
            {
                //抓取已存在資料
                var getAuditOpinionTask = Task.Run(async () => await dac.GetProjectEngineeringAuditOpinion(projectNo));
                Task.Run(() => Task.WaitAll(getAuditOpinionTask)).Wait();
                List<ProjectEngineeringAuditOpinionModel> AuditOpinionData = getAuditOpinionTask.Result;
                //找出需排除資料的SEQ(刪除與修改)
                List<int> seqs = model.ProjectEngineeringAuditOpinion.Where(x => (editTypeEnum)x.editType != editTypeEnum.Add).Select(x => x.SEQ).ToList();
                //取得資料庫的期間(已排掉刪除修改)
                List<string> sqlData = AuditOpinionData.Where(x => !seqs.Contains(x.SEQ)).Select(x => $"{x.YEAR}-{x.MONTH}").ToList();
                //取得異動資料期間(新增修改)
                List<string> editData = model.ProjectEngineeringAuditOpinion.Where(x => (editTypeEnum)x.editType != editTypeEnum.Delete).Select(x => $"{x.YEAR}-{x.MONTH}").ToList();

                //與資料庫期間重複 或 異動資料本身期間重複
                if (editData.Intersect(sqlData).Any() || editData.GroupBy(x => x).Where(x => x.Count() > 1).Any())
                {
                    return ChangeResult(false, "存檔失敗，管考審核意見期間重複");
                }
            }
            #endregion

            #region 檢查所有檔案
            List<ProjectAttachmentModel> fileList = new();
            if (model.ProjectMergeLog.Any())
            {
                //將model.ProjectMergeLog額外存起來給檔案檢核使用
                List<ProjectMergeLogModel> newProjectMergeLog = CopyProjectMergeLogModel(model.ProjectMergeLog);
                foreach (ProjectMergeLogModel item in newProjectMergeLog)
                {
                    foreach (var file in item.File)
                    {
                        file.FILE_KIND = item.MERGE_STATUS == "01" ? "17" : "18";
                        if ((editTypeEnum)file.editType == editTypeEnum.Modify && file.EditFiles == null)
                        {
                            List<UploadTempFileModel> newFile = new();
                            newFile.Add(new UploadTempFileModel
                            {
                                FileName = file.FILE_NAME,
                                EditType = 1,
                            });
                            newFile.Add(new UploadTempFileModel
                            {
                                FileId = file.IDENTITY_FIELD,
                                EditType = 2,
                            });
                            file.EditFiles = newFile;
                        }
                    }

                }
                fileList.AddRange(newProjectMergeLog.Where(x => x.File != null).SelectMany(x => x.File).ToList());
            }
            if (model.ProjectFactFinding.Any())
            {
                foreach (ProjectFactFindingModel item in model.ProjectFactFinding)
                {

                    if (item.RdecFile != null)
                    {
                        foreach (var file in item.RdecFile)
                            file.FILE_KIND = item.FileKind;
                    }
                }
                fileList.AddRange(model.ProjectFactFinding.Where(x => x.RdecFile != null).SelectMany(x => x.RdecFile).ToList());
            }
            List<string> result = projectCommonService.CheckFileName(fileList, projectNo);
            if (result.Any())
            {
                return ChangeResult(false, $"存檔失敗，檔名 {string.Join("、", result)} 重複");
            }
            #endregion

            dac.BeginTransaction();
            //儲存特殊加註 沒資料全刪除、有資料刪除後新增
            projectCommonDac.DeleteProjectMappingData(projectNo, "SPEC_NOTE", projectNo);
            if (model.SpecNoteMappingData.Any())
            {
                foreach (ProjectMappingDataModel item in model.SpecNoteMappingData)
                {
                    item.PROJECT_NO = projectNo;
                    item.SOURCE_ID = projectNo;
                }
                projectCommonDac.InsertProjectMappingData(model.SpecNoteMappingData);
            }

            //儲存管考審核意見
            SaveProjectAuditOpinion(model.ProjectEngineeringAuditOpinion);
            //儲存列管會議
            SaveProjectConference(model.ProjectConference);
            //儲存資料逾期繳交或填報
            SaveProjectDelayfill(model.ProjectDelayfill);
            //儲存分案併案
            SaveProjectMergeLog(model.ProjectMergeLog);
            //儲存實地查證
            SaveProjectFactFindingCommon(model.ProjectFactFinding);
            //儲存年終考核意見
            SaveProjectCloseDetails(model.ProjectCloseDetails, model.ProjectCloseMemo, projectNo);
            //存主檔
            dac.UpdateProjectBasic(model.ProjectBasic);
            //儲存結案意見
            CalculateScore(projectNo);
            dac.Commit();
            return ChangeResult(true, "存檔成功");
        }

        /// <summary>
        /// 複製全新的ProjectMergeLog
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static List<ProjectMergeLogModel> CopyProjectMergeLogModel(List<ProjectMergeLogModel> model)
        {
            List<ProjectMergeLogModel> result = new List<ProjectMergeLogModel>();
            foreach (ProjectMergeLogModel item in model)
            {
                MapperConfiguration config = new MapperConfiguration(cfg => { cfg.CreateMap<ProjectMergeLogModel, ProjectMergeLogModel>(); });
                IMapper mapper = config.CreateMapper();
                ProjectMergeLogModel mergeLog = new ProjectMergeLogModel();
                mapper.Map(item, mergeLog);
                result.Add(mergeLog);
            }

            return result;
        }

        /// <summary>
        /// 儲存列管會議
        /// </summary>
        /// <param name="model"></param>
        private void SaveProjectConference(List<ProjectConferenceModel> model)
        {
            List<ProjectConferenceModel> insertList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
            List<ProjectConferenceModel> updateList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
            List<ProjectConferenceModel> deleteList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Delete).ToList();
            dac.InsertProjectConference(insertList);
            dac.UpdateProjectConference(updateList);
            dac.DeleteProjectConference(deleteList);
        }

        /// <summary>
        /// 儲存資料逾期繳交或填報
        /// </summary>
        /// <param name="model"></param>
        private void SaveProjectDelayfill(List<ProjectDelayfillModel> model)
        {
            List<ProjectDelayfillModel> insertList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
            List<ProjectDelayfillModel> updateList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
            List<ProjectDelayfillModel> deleteList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Delete).ToList();
            dac.InsertProjectDelayfill(insertList);
            dac.UpdateProjectDelayfill(updateList);
            dac.DeleteProjectDelayfill(deleteList);
        }

        /// <summary>
        /// 儲存分案併案
        /// </summary>
        /// <param name="model"></param>
        private void SaveProjectMergeLog(List<ProjectMergeLogModel> model)
        {
            if (!model.Any())
            {
                return;
            }

            foreach (ProjectMergeLogModel item in model)
            {
                switch ((editTypeEnum)item.editType)
                {
                    case editTypeEnum.Add:
                        item.SEQ = dac.InsertProjectMergeLog(item);
                        break;
                    case editTypeEnum.Modify:
                        dac.UpdateProjectMergeLog(item);
                        foreach (var file in item.File)
                        {
                            file.FILE_KIND = item.MERGE_STATUS == "01" ? "17" : "18";
                            projectCommonDac.UpdateProjectAttachment(file);
                        }
                        break;
                    case editTypeEnum.Delete:
                        dac.DeleteProjectMergeLog(item.SEQ);
                        break;
                }
                List<ProjectAttachmentModel> files = item.File;
                //處理檔案
                if (item.File != null)
                {
                    foreach (var file in item.File)
                    {
                        file.PROJECT_NO = model[0].PROJECT_NO;
                        if ((editTypeEnum)item.editType == editTypeEnum.Add)
                        {
                            //01(分案)：17、02(併案)：18
                            file.FILE_KIND = item.MERGE_STATUS == "01" ? "17" : "18";
                        }
                        file.FILE_UP_SOURCE = "02";
                        file.SOURCE_ID = item.SEQ;
                        projectCommonService.MdfProjectAttachment(file);
                    }

                }
            }
        }

        /// <summary>
        /// 儲存實地查證(主辦管考共用)
        /// </summary>
        private void SaveProjectFactFindingCommon(List<ProjectFactFindingModel> model)
        {
            foreach (ProjectFactFindingModel item in model)
            {
                switch ((editTypeEnum)item.editType)
                {
                    case editTypeEnum.Add:
                        item.SEQ = dac.InsertProjectFactFinding(item);
                        break;
                    case editTypeEnum.Modify:
                        dac.UpdateProjectFactFinding(item);
                        break;
                    case editTypeEnum.Delete:
                        dac.DeleteProjectFactFinding(item.SEQ);
                        break;
                }
                List<ProjectAttachmentModel> files = item.FileKind == "07" ? item.RdecFile : item.HandFile;
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        file.PROJECT_NO = model.FirstOrDefault().PROJECT_NO;
                        file.FILE_KIND = item.FileKind;
                        file.SOURCE_ID = item.SEQ;
                        file.FILE_UP_SOURCE = "02";
                        projectCommonService.MdfProjectAttachment(file);
                    }
                }
                
                //管考頁面grid刪除要連同主辦的檔案也刪除
                if ((editTypeEnum)item.editType == editTypeEnum.Delete && item.HandFile != null)
                {
                    foreach (var file in item.HandFile)
                    {
                        file.editType = 3;
                        projectCommonService.MdfProjectAttachment(file);
                    }
                }
            }
        }

        /// <summary>
        /// 儲存年終管考意見
        /// </summary>
        /// <param name="model"></param>
        private void SaveProjectCloseDetails(List<ProjectCloseDetailsModel> model, ProjectCloseMemoModel memoModel, string PROJECT_NO)
        {
            if (!model.Any())
            {
                return;
            }
            List<ProjectCloseDetailsModel> insertList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
            List<ProjectCloseDetailsModel> updateList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
            List<ProjectCloseDetailsModel> deleteList = model.Where(x => (editTypeEnum)x.editType == editTypeEnum.Delete).ToList();
            dac.InsertProjectCloseDetails(insertList);
            dac.UpdateProjectCloseDetails(updateList);
            dac.DeleteProjectCloseDetails(deleteList);
        }

        /// <summary>
        /// 計算特殊扣分
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        public void CalculateScore(string PROJECT_NO)
        {
            // 取得計畫分併案記錄檔
            var getMergeLogTask = Task.Run(async () => await dac.GetProjectMergeLog(PROJECT_NO));
            Task.Run(() => Task.WaitAll(getMergeLogTask)).Wait();
            List<ProjectMergeLogModel> MergeLogData = getMergeLogTask.Result;

            // 取得結案明細資料
            var getCloseDetailsTask = Task.Run(async () => await dac.GetProjectCloseDetails(PROJECT_NO));
            Task.Run(() => Task.WaitAll(getCloseDetailsTask)).Wait();
            List<ProjectCloseDetailsModel> CloseDetailsData = getCloseDetailsTask.Result;

            // 取得未於期限內提出計畫調整資料
            var getDelayApplyTask = Task.Run(async () => await dac.GetDelayApply(PROJECT_NO));
            Task.Run(() => Task.WaitAll(getDelayApplyTask)).Wait();
            List<ProjectBasicAdjForDelayApply> delayApplyData = getDelayApplyTask.Result;

            // 取得結案意見資料
            var getCloseMemoTask = Task.Run(async () => await dac.GetProjectCloseMemo(PROJECT_NO));
            Task.Run(() => Task.WaitAll(getCloseMemoTask)).Wait();
            ProjectCloseMemoModel CloseMemoData = getCloseMemoTask.Result;

            // 計算分數
            int score1 = CloseDetailsData.Where(x => x.CLOSE_DETAILS_TYPE == "2").Any() ? 5 : 0;
            int score2 = delayApplyData.Count() * 5;
            int score3 = CloseDetailsData.Where(x => x.CLOSE_DETAILS_TYPE == "4").Count() * 3;
            int score4 = MergeLogData.Where(x => x.MERGE_STATUS == "01").Any() ? 3 : 0;
            int totalScore = score1 + score2 + score3 + score4;

            ProjectCloseMemoModel CloseMemoModel = new ProjectCloseMemoModel()
            {
                PROJECT_NO = PROJECT_NO,
                TOTAL_SCORE = totalScore
            };
            if (CloseMemoData == null)
            {
                dac.InsertProjectCloseMemo(CloseMemoModel);
            }
            else
            {
                dac.UpdateProjectCloseMemo(CloseMemoModel);
            }
        }

        /// <summary>
        /// 儲存管考審核意見
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void SaveProjectAuditOpinion(List<ProjectEngineeringAuditOpinionModel> model)
        {
            foreach (ProjectEngineeringAuditOpinionModel item in model)
            {
                switch ((editTypeEnum)item.editType)
                {
                    case editTypeEnum.Add:
                        item.SEQ = dac.InsertProjectEngineeringAuditOpinion(item);
                        break;
                    case editTypeEnum.Modify:
                        dac.UpdateProjectEngineeringAuditOpinion(item);
                        break;
                    case editTypeEnum.Delete:
                        dac.DeleteProjectEngineeringAuditOpinion(item.SEQ);
                        break;
                }

                //備註
                projectCommonDac.DeleteProjectMappingData(item.PROJECT_NO, "COM_IPCMEMO", item.SEQ.ToString());
                if ((editTypeEnum)item.editType != editTypeEnum.Delete)
                {
                    foreach (ProjectMappingDataModel memo in item.ComIPCMemoMappingData)
                    {
                        memo.PROJECT_NO = item.PROJECT_NO;
                        memo.SET_ITEM = "COM_IPCMEMO";
                        memo.SOURCE_ID = item.SEQ.ToString();
                    }
                    projectCommonDac.InsertProjectMappingData(item.ComIPCMemoMappingData);
                }
            }
        }
        #endregion

        #region 實地查證情形
        /// <summary>
        /// 儲存實地查證
        /// </summary>
        public RtnResultModel SaveProjectFactFinding(List<ProjectFactFindingModel> model)
        {
            //檢查所有檔案
            if (model.Any())
            {
                foreach (ProjectFactFindingModel item in model)
                {
                    foreach (var file in item.HandFile)
                    {
                        file.FILE_KIND = item.FileKind;
                    }
                        
                }
                List<string> result = projectCommonService.CheckFileName(model.Where(x => x.HandFile != null).SelectMany(x => x.HandFile).ToList());
                if (result.Any())
                {
                    return ChangeResult(false, $"存檔失敗，檔名 {string.Join("、", result)} 重複");
                }
            }

            dac.BeginTransaction();
            SaveProjectFactFindingCommon(model);
            dac.Commit();
            return ChangeResult(true, "存檔成功");
        }
        #endregion

        #region 預算執行情形
        /// <summary>
        /// 取得計畫預算執行情形
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectFillBudgetExecModel> GetProjectFillBudgetExec(string PROJECT_NO)
        {
            ProjectFillBudgetExecModel result = new();
            result.ProjectFillCycle = await projectCommonDac.GetCurrentCycleData();
            List<ProjectBudgetExecuteModel> BudgetExecData = await dac.GetProjectFillBudgetExec(PROJECT_NO);

            List<ProjectMappingDataModel> failedMappingData = await projectCommonDac.GetProjectMappingData(PROJECT_NO, "IPCBGTEXECFAILED");
            List<ProjectMappingDataModel> failedDutyMappingData = await projectCommonDac.GetProjectMappingData(PROJECT_NO, "IPCBGTEXECFAILEDDUTY");
            foreach (ProjectBudgetExecuteModel item in BudgetExecData)
            {
                item.FailedMappingData = failedMappingData.Where(x => x.SOURCE_ID == item.SEQ.ToString()).ToList();
                item.FailedDutyMappingData = failedDutyMappingData.Where(x => x.SOURCE_ID == item.SEQ.ToString()).ToList();
            }
            result.ProjectBudgetExecute = BudgetExecData;
            return result;
        }

        /// <summary>
        /// 儲存計畫預算執行情形
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SaveProjectFillBudgetExec(ProjectBudgetExecuteModel model)
        {
            dac.BeginTransaction();
            string message = "";
            if ((editTypeEnum)model.editType == editTypeEnum.Delete)
            {
                dac.DeleteProjectFillBudgetExec(model);
                message = "刪除成功";
            }
            else
            {
                model.EXEC_RATE_FAILED_NOTE = model.YEAR_EXEC_RATE < 80 ? model.EXEC_RATE_FAILED_NOTE : "";
                if (model.SEQ == 0)
                {
                    model.SEQ = dac.InsertProjectFillBudgetExec(model);
                }
                else
                {
                    dac.UpdateProjectFillBudgetExec(model);
                }
                //原因
                projectCommonDac.DeleteProjectMappingData(model.PROJECT_NO, "IPCBGTEXECFAILED", model.SEQ.ToString());
                //責任歸屬
                projectCommonDac.DeleteProjectMappingData(model.PROJECT_NO, "IPCBGTEXECFAILEDDUTY", model.SEQ.ToString());
                if (model.YEAR_EXEC_RATE < 80)
                {
                    foreach (ProjectMappingDataModel item in model.FailedMappingData)
                    {
                        item.PROJECT_NO = model.PROJECT_NO;
                        item.SET_ITEM = "IPCBGTEXECFAILED";
                        item.SOURCE_ID = model.SEQ.ToString();
                    }
                    foreach (ProjectMappingDataModel item in model.FailedDutyMappingData)
                    {
                        item.PROJECT_NO = model.PROJECT_NO;
                        item.SET_ITEM = "IPCBGTEXECFAILEDDUTY";
                        item.SOURCE_ID = model.SEQ.ToString();
                    }
                    List<ProjectMappingDataModel> mappingDatalist = model.FailedMappingData;
                    mappingDatalist.AddRange(model.FailedDutyMappingData);
                    projectCommonDac.InsertProjectMappingData(mappingDatalist);
                }
                message = model.SEQ.ToString();
            }

            dac.Commit();
            return ChangeResult(true, message);
        }
        #endregion

        #region 共用 
        /// <summary>
        /// 增刪修計畫落後原因
        /// </summary>
        /// <param name="model"></param>
        /// <param name="data"></param>
        /// <param name="isDelay"></param>
        public void CUDProjectDelayCausal(ProjectDelayCausalModel model, ProjectDelayCausalInsertModel data, bool isDelay)
        {
            if (model == null && isDelay)
                dac.InsertProjectDelayCausal(data);
            else if (isDelay)
                dac.UpdateProjectDelayCausal(data);
            else
                dac.DeleteProjectDelayCausal(data);
        }

        /// <summary>
        /// 是否為 施工方式為"工程類"且辦理開工的實際完成日期已填寫
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> IsEngStartWork(string PROJECT_NO)
        {
            //檢查計畫是否為工程類
            bool isEngineeringType = await dac.CheckIsEngineeringType(PROJECT_NO);
            //檢查辦理開工實際完成日期是否有填
            bool isStartWork = await dac.CheckIsStartWork(PROJECT_NO);
            //檢查是否本府執行案件
            bool isTycgProject = await dac.CheckIsTycgProject(PROJECT_NO);
            return isEngineeringType && isStartWork && isTycgProject;
        }

        /// <summary>
        /// 檢查執行情形填報頁面資料可否存檔
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        private async Task<bool> CheckProjFillExeDataCanSave(string PROJECT_NO)
        {
            // 當期執行情形未送出或未超過填報週期
            return await dac.CheckProjFillExeDataCanSave(PROJECT_NO);
        }

        /// <summary>
        /// 若工程類計畫「每月辦理情形」的實際施工進度 = 100，「檢核點完成日期」的竣工實際完成日期是否填寫
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> IsEngCompleteWork(string PROJECT_NO)
        {
            //檢查計畫是否為工程類
            bool isEngineeringType = await dac.CheckIsEngineeringType(PROJECT_NO);
            //取得「每月辦理情形」的實際施工進度
            ProjectEngineeringProgressTableModel progressData = await dac.GetProjecFillExecute(PROJECT_NO, "");
            if (isEngineeringType && progressData.IPC_ACT_PRG == 100)
            {
                //檢查實際完成日期是否有填
                return await dac.CheckIsCompletedWork(PROJECT_NO);
            }

            return true;
        }
        #endregion
    }
}

