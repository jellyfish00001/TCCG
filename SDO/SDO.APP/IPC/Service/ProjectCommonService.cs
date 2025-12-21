using SDO.Base.Utils;
using SDO.Base.Utils.Enum;
using SDO.Base.Utils.Models;
using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class ProjectCommonService : Service, IProjectCommonService
    {
        private readonly IProjectCommonDac dac;
        private readonly IFTPService ftpService;
        private readonly IUploadFileService uploadFileService;
        private readonly IProjectAdjustDac projectAdjustDac;
        private readonly IZipService zipService;

        public ProjectCommonService(IProjectCommonDac dac,
            IFTPService ftpService,
            IUploadFileService uploadFileService,
            IProjectAdjustDac projectAdjustDac,
            IZipService zipService)
        {
            this.dac = dac;
            this.ftpService = ftpService;
            this.uploadFileService = uploadFileService;
            this.projectAdjustDac = projectAdjustDac;
            this.zipService = zipService;
        }

        #region 相關檔案上傳
        /// <summary>
        /// 取得計畫檔案資料
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="fileUpSource">01:相關檔案上傳、02:其他地方上傳</param>
        /// <param name="fileKind">SET_PARAM.SET_ITEM ='FILE_KIND'</param>
        /// <returns></returns>
        public async Task<List<ProjectAttachmentModel>> GetProjectAttachment(UpLoadModel model)
        {
            return await dac.GetProjectAttachmentList(new ProjectAttachmentQueryModel
            {
                PROJECT_NO = model.PROJECT_NO,
                DB = model.DBKey,
                FILE_UP_SOURCE = model.FILE_UP_SOURCE,
                FILE_KIND = !string.IsNullOrEmpty(model.FILE_KIND) ? new List<string> { model.FILE_KIND } : new List<string>()
            });
        }

        /// <summary>
        /// 取得計畫檔案資料
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="fileUpSource">01:相關檔案上傳、02:其他地方上傳</param>
        /// <param name="fileKinds">SET_PARAM.SET_ITEM ='FILE_KIND'</param>
        /// <returns></returns>
        public async Task<List<ProjectAttachmentModel>> GetProjectAttachment(string projectNo, string fileUpSource, List<string> fileKinds)
        {
            return await dac.GetProjectAttachmentList(new ProjectAttachmentQueryModel
            {
                PROJECT_NO = projectNo,
                FILE_UP_SOURCE = fileUpSource,
                FILE_KIND = fileKinds ?? new List<string>()
            });
        }

        /// <summary>
        /// 取得非相關檔案上傳的檔案資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectFillFileUpModel> GetProjectOtherAttachmentList(string PROJECT_NO)
        {
            ProjectFillFileUpModel result = new ProjectFillFileUpModel()
            {
                ProjectOtherAttachmentModels = await dac.GetOtherProjectAttachmentList(PROJECT_NO),
                AdjustScheHistoryModels = await projectAdjustDac.GetProjAdjScheHistory(new List<string> { PROJECT_NO })
            };
            return result;
        }

        /// <summary>
        /// 儲存計畫檔案資料
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public RtnResultModel SaveProjectAttachment(List<ProjectAttachmentModel> models, int datafrom = 0)
        {
            #region 檢查所有檔案
            //將ProjectAttachmentModel額外存起來給檔案檢核使用
            List<ProjectAttachmentModel> newProjectAttachment = CopyProjectAttachmentModel(models);
            foreach (ProjectAttachmentModel item in newProjectAttachment)
            {
                if ((editTypeEnum)item.editType == editTypeEnum.Modify && item.EditFiles == null)
                {
                    List<UploadTempFileModel> newFile = new();
                    newFile.Add(new UploadTempFileModel
                    {
                        FileName = item.FILE_NAME,
                        EditType = 1,
                    });
                    newFile.Add(new UploadTempFileModel
                    {
                        FileId = item.IDENTITY_FIELD,
                        EditType = 2,
                    });
                    item.EditFiles = newFile;
                }
            }
            List<string> result = CheckFileName(newProjectAttachment, "", null, datafrom);
            if (result.Any())
            {
                return ChangeResult(false, $"存檔失敗，檔名 {string.Join("、", result)} 重複");
            }
            #endregion

            dac.BeginTransaction();
            foreach (ProjectAttachmentModel item in models)
            {
                switch ((editTypeEnum)item.editType)
                {
                    case editTypeEnum.Add:
                        //檔案路徑：<檔案伺服端目錄>/{PROJECT_NO}/
                        item.FILE_PATH = $"/{item.PROJECT_NO}";
                        item.IDENTITY_FIELD = dac.InsertProjectAttachment(item); break;
                    case editTypeEnum.Modify:
                        dac.UpdateProjectAttachment(item); break;
                    case editTypeEnum.Delete:
                        dac.DeleteProjectAttachment(item.IDENTITY_FIELD, item.DB); break;
                }
                // 刪除資料 or 有異動檔案
                SaveFile(item);
            }
            dac.Commit();
            return ChangeResult(true, "存檔成功");
        }

        /// <summary>
        /// 複製全新的ProjectAttachment
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static List<ProjectAttachmentModel> CopyProjectAttachmentModel(List<ProjectAttachmentModel> model)
        {
            List<ProjectAttachmentModel> result = new List<ProjectAttachmentModel>();
            foreach (ProjectAttachmentModel item in model)
            {
                AutoMapper.MapperConfiguration config = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<ProjectAttachmentModel, ProjectAttachmentModel>(); });
                AutoMapper.IMapper mapper = config.CreateMapper();
                ProjectAttachmentModel attachment = new ProjectAttachmentModel();
                mapper.Map(item, attachment);
                attachment.EditFiles = attachment.EditFiles.Any() ? attachment.EditFiles : null;
                result.Add(attachment);
            }
            return result;
        }

        /// <summary>
        /// 儲存計畫檔案資料
        /// </summary>
        /// <param name="model"></param>
        /// 
        public void MdfProjectAttachment(ProjectAttachmentModel model)
        {
            //檔案路徑：<檔案伺服端目錄>/{PROJECT_NO}/
            model.FILE_PATH = $"/{model.PROJECT_NO}";

            //只會有新增 or 刪除
            if ((editTypeEnum)model.editType == editTypeEnum.Add)
            {
                //刪除再新增計畫檔案資料
                if (!model.IsMultiple)
                {
                    dac.DeleteProjectAttachmentAll(model);
                }
                model.IDENTITY_FIELD = dac.MdfProjectAttachment(model);
            }
            else if ((editTypeEnum)model.editType == editTypeEnum.Delete)
            {
                dac.DeleteProjectAttachment(model.IDENTITY_FIELD);
            }

            // 刪除資料 or 有異動檔案
            SaveFile(model);
        }

        /// <summary>
        /// 儲存相關檔案
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void SaveFile(ProjectAttachmentModel model)
        {
            #region 新增、修改
            // 新增 - 直接新增
            // 修改 - 先刪除再新增
            if (model.EditFiles != null && model.EditFiles.Any())
            {
                List<SaveTempFileModel> saveTempFiles = new List<SaveTempFileModel>();
                foreach (UploadTempFileModel fileModel in model.EditFiles)
                {
                    switch ((FileEditTypeEnum)fileModel.EditType)
                    {
                        // 新增
                        case FileEditTypeEnum.Add:
                            saveTempFiles.Add(new SaveTempFileModel
                            {
                                TempFileName = $"{fileModel.Uid}{fileModel.Extension}",
                                FormalPath = model.FILE_PATH,
                                FormalFileName = $"{model.IDENTITY_FIELD}{fileModel.Extension}"
                            });
                            //儲存暫存檔
                            uploadFileService.SaveFile(saveTempFiles);
                            break;
                        // 刪除
                        case FileEditTypeEnum.Delete:
                            // 舊檔案路徑
                            ftpService.DeleteFile($"{model.FILE_PATH}/{fileModel.FileId}{fileModel.Extension}");
                            break;
                    }
                }
            }
            #endregion
            #region 刪除
            // 純刪除不會寫入editedFiles，故另外處理
            if ((editTypeEnum)model.editType == editTypeEnum.Delete)
            {
                ftpService.DeleteFile($"{model.FILE_PATH}/{model.IDENTITY_FIELD}{Path.GetExtension(model.FILE_NAME)}");
            }
            #endregion
        }

        /// <summary>
        /// 下載相關檔案
        /// </summary>
        /// <param name="IDENTITY_FIELD"></param>
        /// <returns></returns>
        public async Task<(byte[] bytes, string fileName, string contentType)> DownProjectAttachment(int IDENTITY_FIELD, int DBKEY)
        {
            // 取得檔案資訊
            ProjectAttachmentModel fileData = await dac.GetProjectAttachment(IDENTITY_FIELD, DBKEY);
            // 取得副檔名
            string extension = Path.GetExtension(fileData.FILE_NAME);
            // 組成下載路徑
            string downPath = $"{fileData.FILE_PATH}/{IDENTITY_FIELD}{extension}";
            return await uploadFileService.GetDownloadFile(downPath, fileData.FILE_NAME);
        }

        /// <summary>
        /// 下載附件壓縮檔
        /// </summary>
        /// <param name="IDENTITY_FIELDs"></param>
        /// <param name="title">壓縮檔檔名</param>
        /// <returns></returns>
        public async Task<(byte[] bytes, string contentType, string fileName)> DownProjectAttachmentZip(List<int> IDENTITY_FIELDs, string title)
        {
            List<ProjectAttachmentModel> models = await dac.GetProjectAttachment(IDENTITY_FIELDs);

            Dictionary<string, int> indexDict = models.OrderBy(x => x.SORT_ORDER).Select(x => x.NAME).Distinct().Select((name, idx) => new { idx, name }).ToDictionary(x => x.name, y => y.idx + 1);
            models.ForEach(x =>
            {
                x.NAME = $"{(indexDict.ContainsKey(x.NAME) ? indexDict[x.NAME] : 0)}.{x.NAME}";
            });

            return zipService.MakeZip(models, title);
        }
        #endregion

        #region 參考資料
        /// <summary>
        /// 取得參考資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProjectAttachmentModel>> GetRefFile()
        {
            return await dac.GetRefFile();
        }

        /// <summary>
        /// 儲存參考資料
        /// </summary>
        /// <param name="EditFiles"></param>
        /// <returns></returns>
        public RtnResultModel SaveRefFile(List<UploadTempFileModel> EditFiles)
        {
            dac.BeginTransaction();

            SaveProjectFiles(new ProjectAttachmentModel
            {
                PROJECT_NO = "0", // 參考資料計畫編號固定存0
                FILE_UP_SOURCE = "02",
                EditFiles = EditFiles
            });

            dac.Commit();
            return ChangeResult(true, "存檔成功");
        }
        #endregion

        /// <summary>
        /// 儲存計畫附加檔案(針對單一計畫多檔上傳)
        /// </summary>
        /// <param name="model"></param>
        public void SaveProjectFiles(ProjectAttachmentModel model)
        {
            // 檔案路徑：<檔案伺服端目錄>
            // IPC => /{PROJECT_NO}/
            // 其餘子系統 => /{FOLDER_NAME}/{PROJECT_NO}/
            if (string.IsNullOrEmpty(model.FOLDER_NAME))
            {
                model.FILE_PATH = $"/{model.PROJECT_NO}";
            }
            else
            {
                model.FILE_PATH = $"/{model.FOLDER_NAME}/{model.PROJECT_NO}";
            }

            if (model.EditFiles != null && model.EditFiles.Any())
            {
                foreach (var item in model.EditFiles)
                {
                    //只會有新增 or 刪除
                    List<SaveTempFileModel> saveTempFiles = new List<SaveTempFileModel>();
                    if ((editTypeEnum)item.EditType == editTypeEnum.Add)
                    {
                        // 新增 DB : PROJECT_ATTACHMENT
                        model.FILE_NAME = item.FileName;
                        model.IDENTITY_FIELD = dac.InsertProjectAttachment(model);

                        saveTempFiles.Add(new SaveTempFileModel
                        {
                            TempFileName = $"{item.Uid}{item.Extension}",
                            FormalPath = model.FILE_PATH,
                            FormalFileName = $"{model.IDENTITY_FIELD}{item.Extension}"
                        });

                        // 新增 ftp : 儲存暫存檔
                        uploadFileService.SaveFile(saveTempFiles);
                    }
                    else
                    {
                        // 刪除 DB : PROJECT_ATTACHMENT
                        dac.DeleteProjectAttachment(item.FileId, model.DB);

                        // 刪除 ftp : 刪除舊檔案路徑
                        ftpService.DeleteFile($"{model.FILE_PATH}/{item.FileId}{item.Extension}");
                    }
                }
            }
        }

        #region 共用
        /// <summary>
        /// 取得當期填報周期資料
        /// </summary>
        /// <returns></returns>
        public async Task<ProjectFillCycleModel> GetCurrentCycleData()
        {
            return await dac.GetCurrentCycleData();
        }


        /// <summary>
        /// 驗證Model必填欄位
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataModel">需驗證Model資料</param>
        /// <returns></returns>
        public List<string> CheckModelRequiredField<T>(T dataModel)
        {
            if (dataModel == null)
                dataModel = (T)Activator.CreateInstance(typeof(T));

            return ChkFields(dataModel);
        }

        /// <summary>
        /// 驗證Model必填欄位(List Model)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataModels"></param>
        /// <returns></returns>
        public List<List<string>> CheckModelRequiredField<T>(List<T> dataModels)
        {
            List<List<string>> errList = new();
            if (dataModels.Any())
            {
                foreach (var model in dataModels)
                {
                    List<string> result = ChkFields(model);
                    errList.Add(result);
                }
            }
            return errList;
        }

        /// <summary>
        /// Validation 套件驗證Model Require屬性欄位，並回傳錯誤訊息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        private static List<string> ChkFields<T>(T dataModel)
        {
            List<ValidationResult> results = new();
            bool valid = Validator.TryValidateObject(dataModel, new ValidationContext(dataModel), results, true);
            return results.Select(x => x.ErrorMessage).ToList();
        }
        #endregion

        /// <summary>
        /// 檢查同一計畫編號相同FILE_KIND的PROJECT_ATTACHMENT檔名不能重複
        /// </summary>
        /// <param name="models"></param>
        /// <param name="PROJECT_NO"></param>
        /// <param name="PROJ_ADJ_ID">調整撤銷流水號</param>
        /// <returns></returns>
        public List<string> CheckFileName(List<ProjectAttachmentModel> models, string PROJECT_NO = "", int? PROJ_ADJ_ID = null, int DBKey = 0)
        {
            if (!models.Any())
            {
                return new List<string>();
            }
            string projectNo = string.IsNullOrEmpty(PROJECT_NO) ? models.FirstOrDefault().PROJECT_NO : PROJECT_NO;
            List<int> DeleteFileId = models.Where(x => x.EditFiles != null).SelectMany(x =>
                x.EditFiles.Where(y => (FileEditTypeEnum)y.EditType == FileEditTypeEnum.Delete).Select(y => y.FileId)).ToList();
            DeleteFileId.AddRange(models.Where(x => (editTypeEnum)x.editType == editTypeEnum.Delete).Select(x => x.IDENTITY_FIELD).ToList());
            //刪除的檔案ID
            List<int> deleteFileIds = DeleteFileId;
            //所有要新增的檔案
            List<(string fileName, string fileKind)> fileInfo = models.Where(x => x.EditFiles != null).SelectMany(x =>
                x.EditFiles.Where(y => (FileEditTypeEnum)y.EditType == FileEditTypeEnum.Add).Select(y => (y.FileName, x.FILE_KIND))).ToList();
            //要新增的檔案本身同FILE_KIND重複的檔名
            List<string> duplicate = fileInfo.GroupBy(x => new { x.fileName, x.fileKind }).Where(x => x.Count() > 1).Select(x => x.Key.fileName).ToList();
            //已在資料庫的檔案(排除要刪除的)
            List<(string fileName, string fileKind)> dbFiles = new();
            if (fileInfo.Any())
            {
                dbFiles = dac.GetFileNameByFileKind(projectNo, fileInfo.Select(x => x.fileKind).Distinct().ToList(), deleteFileIds, PROJ_ADJ_ID, DBKey);
            }
            //跟資料庫重複檔名
            List<string> result = dbFiles.Intersect(fileInfo).Select(x => x.fileName).ToList();
            //回傳所有重複檔名
            return result.Union(duplicate).ToList();
        }

        /// <summary>
        /// 取得已使用的代碼清單
        /// </summary>
        /// <param name="SET_ITEM">代碼類別</param>
        /// <returns></returns>
        public async Task<List<string>> GetUsedCode(string SET_ITEM)
        {
            return await dac.GetUsedCode(SET_ITEM);
        }

        /// <summary>
        /// 取得計畫落後原因的落後項目代碼清單
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetDelayClasses()
        {
            return await dac.GetDelayClasses();
        }

        /// <summary>
        /// 取得計畫落後原因的落後項目清單
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetDelaySubClasses()
        {
            return await dac.GetDelaySubClasses();
        }

        /// <summary>
        /// 取得計畫經費來源的預算編號清單
        /// </summary>
        /// <param name="LEVEL_MARK">預算來源類別</param>
        /// <returns></returns>
        public async Task<List<string>> GetPlanItems(string LEVEL_MARK)
        {
            return await dac.GetPlanItems(LEVEL_MARK);
        }

        /// <summary>
        /// 透過機關取得相關人員信箱資料
        /// </summary>
        /// <param name="orgModel"></param>
        /// <returns></returns>
        public async Task<List<OrgContactModel>> GetDeptContactDatas(List<ProjectOrgContactModel> orgModel)
        {
            List<ProjectOrgContactModel> orgContactData = new();
            // 取得機關窗口資料
            List<DeptContactModel> deptContactModels = await dac.GetDeptContactByOrg(orgModel.Select(x => x.EXEC_ORGAN_C).Distinct().ToList());
            if (deptContactModels.Any())
            {
                // 加入自訂窗口人員資料
                orgContactData.AddRange(deptContactModels.Where(x => x.SOURCE == 2).Select(x =>
                    new ProjectOrgContactModel() { Contact = x.CONTACT, Email = x.EMAIL, EXEC_ORGAN_C = x.ORGAN }));
                // 找到窗口中，來源是SC的 UserId
                List<DeptContactModel> scUsers = deptContactModels.Where(x => x.SOURCE == 1).ToList();
                if (scUsers.Any())
                {
                    // 透過 UserId 取得SC人員資料
                    List<SCContactModel> scContactModels = await dac.GetSCContactData(scUsers.Select(x => x.CONTACT).ToList());
                    orgContactData.AddRange(scContactModels.Where(x => x.USR_EMAIL != null).Select(x =>
                        new ProjectOrgContactModel()
                        {
                            Contact = x.USR_NAME,
                            Email = x.USR_EMAIL,
                            EXEC_ORGAN_C = scUsers.Where(y => y.CONTACT == x.USR_ID).Select(y => y.ORGAN).FirstOrDefault()
                        }));
                }
                // 有設定機關窗口資料的Org
                List<string> hasContactDataOrgs = orgContactData.Select(x => x.EXEC_ORGAN_C).Distinct().ToList();
                List<OrgContactModel> result = new();
                foreach (string org in hasContactDataOrgs)
                {
                    result.Add(new OrgContactModel()
                    {
                        PROJECT_NO = orgModel.Where(x => x.EXEC_ORGAN_C == org).Select(x => x.PROJECT_NO).ToList(),
                        EXEC_ORGAN_C = org,
                        ContactData = orgContactData.Where(x => x.EXEC_ORGAN_C == org)
                            .Select(x => new ContactData() { Contact = x.Contact, Email = x.Email }).ToList()
                    });
                }
                return result;
            }
            return null;
        }

        /// <summary>
        /// 透過機關取得相關人員信箱資料
        /// </summary>
        /// <param name="queryData"></param>
        /// <returns></returns>
        public async Task<List<RecipientModel>> GetDeptContactRcvData(List<DeptContactRcvQueryModel> queryData)
        {
            List<RecipientModel> resultData = new();
            foreach (DeptContactRcvQueryModel data in queryData)
            {
                List<ProjectOrgContactModel> orgContactData = new();
                // 取得機關窗口資料
                List<DeptContactModel> deptContactModels = await dac.GetDeptContactByOrg(new List<string>() { data.OrgId });
                if (deptContactModels.Any())
                {
                    // 加入自訂窗口人員資料
                    orgContactData.AddRange(deptContactModels.Where(x => x.SOURCE == 2).Select(x =>
                        new ProjectOrgContactModel() { Contact = x.CONTACT, Email = x.EMAIL, EXEC_ORGAN_C = x.ORGAN }));
                    // 找到窗口中，來源是SC的 UserId
                    List<DeptContactModel> scUsers = deptContactModels.Where(x => x.SOURCE == 1).ToList();
                    if (scUsers.Any())
                    {
                        // 透過 UserId 取得SC人員資料
                        List<SCContactModel> scContactModels = await dac.GetSCContactData(scUsers.Select(x => x.CONTACT).ToList());
                        orgContactData.AddRange(scContactModels.Where(x => x.USR_EMAIL != null).Select(x =>
                            new ProjectOrgContactModel()
                            {
                                Contact = x.USR_NAME,
                                Email = x.USR_EMAIL,
                                EXEC_ORGAN_C = scUsers.Where(y => y.CONTACT == x.USR_ID).Select(y => y.ORGAN).FirstOrDefault()
                            }));
                    }

                    resultData.AddRange(orgContactData.Select(x => new RecipientModel()
                    {
                        MAIL_TITLE = x.Contact,
                        MAIL_ADDRESS = x.Email,
                        MAIL_TYPE = data.MailType
                    }).ToList());
                }
            }
            return resultData;
        }

        /// <summary>
        /// 取得郵件範本替換參數資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<MailTemplateParamModel> GetMailTemplateParam(string PROJECT_NO)
        {
            return await dac.GetMailTemplateParam(PROJECT_NO);
        }

        /// <summary>
        /// 取得SCUser收件者資料
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="mailType">郵件類型</param>
        /// <returns></returns>
        public async Task<RecipientModel> GetSCContactRcvData(string userId, string mailType = "1")
        {
            SCContactModel scData = (await dac.GetSCContactData(new List<string>() { userId })).FirstOrDefault();
            if (scData != null)
            {
                return new RecipientModel
                {
                    MAIL_TITLE = scData.USR_NAME,
                    MAIL_ADDRESS = scData.USR_EMAIL,
                    MAIL_TYPE = mailType
                };
            }

            return null;
        }

        /// <summary>
        /// 檢核點設定檢核 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string CheckProjectCheckpointValid(ProjectCheckpointModel model)
        {
            List<string> errMsgs = new();

            // 驗證計畫開始日期必填
            if (!model.CONTROL_DATE1.HasValue)
            {
                errMsgs.Add("計畫開始日期必填");
            }


            // 驗證檢核點是否均填
            bool isChkpointInvalid = model.CusCheckpointModels.Where(x => string.IsNullOrEmpty(x.CHECKITEM_NAME) || x.PROGRESS == 0 || !x.ESTIMATED_ENDDATE.HasValue).Any();

            if (!model.CusCheckpointModels.Any() || isChkpointInvalid)
            {
                errMsgs.Add("檢核點資料尚未填妥");
            }
            else
            {

                List<ProjectCusCheckpointModel> sortedCheckpoints = model.CusCheckpointModels.OrderBy(x => x.PROGRESS).ToList();
                // 若檢核點資料填畢
                // 計畫開始日期需在第一項檢核點預定完成日期之前
                if (model.CONTROL_DATE1.HasValue && (model.CONTROL_DATE1.Value.Date > sortedCheckpoints.First().ESTIMATED_ENDDATE.Value.Date))
                {
                    errMsgs.Add("計畫開始日期需在第一項檢核點預定完成日期之前");
                }

                // 管考進度不得重複
                List<decimal> chkpointsProgress = sortedCheckpoints.Select(x => x.PROGRESS).ToList();
                if (chkpointsProgress.Count != chkpointsProgress.Distinct().Count())
                {
                    errMsgs.Add("管考進度不得重複");
                }

                // 預定完成日期須依據管制進度遞增
                foreach (bool isChkpointEstimatedDateValid in CheckChkpointEstimateDateIsIncrement(sortedCheckpoints))
                {
                    if (!isChkpointEstimatedDateValid)
                    {
                        errMsgs.Add("預定完成日期需遞增輸入");
                    }
                }
            }

            return string.Join('、', errMsgs);
        }

        /// <summary>
        /// 檢查檢核點預定完成日期須依據管制進度遞增
        /// </summary>
        /// <param name="sortedCheckpoints"></param>
        /// <returns></returns>
        private static IEnumerable<bool> CheckChkpointEstimateDateIsIncrement(List<ProjectCusCheckpointModel> sortedCheckpoints)
        {
            // 迴圈內上一筆檢核點預定完成日期
            DateTime preDate = sortedCheckpoints.First().ESTIMATED_ENDDATE.Value.Date;
            DateTime currDate;
            foreach (ProjectCusCheckpointModel chkpointData in sortedCheckpoints)
            {
                // 第一項日期不檢核
                if (chkpointData.PROGRESS != sortedCheckpoints.First().PROGRESS)
                {
                    // 迴圈當前檢核點預定完成日期
                    currDate = chkpointData.ESTIMATED_ENDDATE.Value.Date;
                    if (currDate < preDate)
                    {
                        yield return false;
                        yield break;
                    }
                    else
                        preDate = currDate;
                }

                yield return true;
            }
        }

    }
}
