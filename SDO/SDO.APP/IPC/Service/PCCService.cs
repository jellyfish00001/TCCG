using Aspose.Cells;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SDO.APP.IPC.Enum;
using SDO.Base.Utils.Models;
using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using SDO.XlsReader.Interface;
using SDO.XlsReader.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class PCCService : Service, IPCCService
    {
        private readonly IPCCDac dac;
        private readonly IProjectService projectService;
        private readonly IProjectCommonService projectCommonService;
        private readonly ISetParamService setParamService;
        private readonly IXlsService xlsService;
        private readonly IConfiguration configuration;
        private readonly ILogger<PCCService> logger;
        private readonly IUploadFileService uploadFileService;

        public PCCService(
            IPCCDac dac,
            IProjectService projectService,
            IProjectCommonService projectCommonService,
            ISetParamService setParamService,
            IXlsService xlsService,
            IConfiguration configuration,
            ILogger<PCCService> logger,
            IUploadFileService uploadFileService
            )
        {
            this.dac = dac;
            this.projectService = projectService;
            this.projectCommonService = projectCommonService;
            this.setParamService = setParamService;
            this.xlsService = xlsService;
            this.configuration = configuration;
            this.logger = logger;
            this.uploadFileService = uploadFileService;
        }

        /// <summary>
        /// 取得 Pcc 資料
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">資料表名稱</param>
        /// <param name="pccFilter">篩選</param>
        /// <returns></returns>
        private async Task<List<T>> GetPccmData<T>(PccmNameEnum tableName, PccFilterModel pccFilter)
        {
            List<string> pccColumns = await dac.GetPccmColumns(tableName.ToString());
            if (!pccColumns.Any())
            {
                return new List<T>();
            }

            List<string> modelColumns = Activator.CreateInstance(typeof(T)).GetType().GetProperties().Where(x => x.CanWrite).Select(x => x.Name).ToList();

            return await dac.GetPccmData<T>(new PccModel
            {
                TableName = tableName,
                PccFilter = pccFilter,
                Columns = pccColumns.Intersect(modelColumns).ToList()
            });
        }

        /// <summary>
        /// 取得 Pcc 資料 (全部欄位)
        /// </summary>
        /// <param name="tableName">資料表名稱</param>
        /// <param name="pccFilter">篩選</param>
        /// <returns></returns>
        private async Task<List<List<PccmDataModel>>> GetPccmAllData(PccmNameEnum tableName, PccFilterModel pccFilter, Dictionary<string, PccmOrderByEnum> orderby = null)
        {
            List<string> pccColumns = await dac.GetPccmColumns(tableName.ToString());
            if (!pccColumns.Any())
            {
                return new List<List<PccmDataModel>>();
            }

            if (orderby != null)
            {
                orderby = orderby.Where(x => pccColumns.Contains(x.Key)).ToDictionary(x => x.Key, y => y.Value);
            }

            List<Dictionary<string, object>> data = await dac.GetPccmDataToDict(new PccModel
            {
                TableName = tableName,
                PccFilter = pccFilter,
                Columns = pccColumns,
                Orderby = orderby
            });
            Dictionary<string, string> schemaInfoDict = await dac.GetPccmSchemaInfoDict(tableName.ToString());

            List<List<PccmDataModel>> result = new List<List<PccmDataModel>>();
            if (!data.Any())
            {
                result.Add(schemaInfoDict.Select(x => new PccmDataModel()
                {
                    PCCM_EN_NAME = x.Key,
                    PCCM_CN_NAME = x.Value
                }).ToList());
                return result;
            }

            result = data.Select(x => x.Select(y => new PccmDataModel
            {
                PCCM_EN_NAME = y.Key,
                PCCM_CN_NAME = schemaInfoDict.ContainsKey(y.Key) ? schemaInfoDict[y.Key] : string.Empty,
                VALUE = y.Value
            }).ToList()).ToList();
            return result;
        }

        private string GetValueStr(object value)
        {
            string result = null;
            if (!(value is null))
            {
                result = value.ToString();
                switch (value)
                {
                    case DateTime:
                        result = result.ToTwDateString(); break;
                    case decimal:
                        result = decimal.Parse(result).ToString("N2"); break;
                }
            }
            return result;
        }

        /// <summary>
        /// 取得關聯工程會標案資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectMapPCCGridModel>> GetProjectMapPCC(PccFilterModel model)
        {
            return await GetPccmData<ProjectMapPCCGridModel>(PccmNameEnum.PCCM_DS01, model);
        }

        /// <summary>
        /// 取得工程會標案基本資料(單筆)
        /// </summary>
        /// <param name="PCC_PROJECT_UID"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetPccmDs01(string PCC_PROJECT_UID)
        {
            List<PccmDataModel> data = (await GetPccmAllData(PccmNameEnum.PCCM_DS01, new PccFilterModel { PCC_PROJECT_UID = PCC_PROJECT_UID })).First();

            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach(var item in data)
            {
                result.Add(item.PCCM_CN_NAME, GetValueStr(item.VALUE));
            }
            return result;
        }

        /// <summary>
        /// 取得標案系統執行進度資料
        /// </summary>
        /// <param name="PCC_PROJECT_UID"></param>
        /// <returns></returns>
        public async Task<PCCExeProgressGridModel> GetPCCExeProgress(string PCC_PROJECT_UID)
        {
            PccFilterModel pccFilter = new PccFilterModel { PCC_PROJECT_UID = PCC_PROJECT_UID };

            Dictionary<string, PccmOrderByEnum> ds04Orderby = new Dictionary<string, PccmOrderByEnum>();
            ds04Orderby.Add("plnprj_year", PccmOrderByEnum.DESC);
            ds04Orderby.Add("plnprj_month", PccmOrderByEnum.DESC);

            List<List<PccmDataModel>> pccmDs04s = (await GetPccmAllData(PccmNameEnum.PCCM_DS04, pccFilter, orderby: ds04Orderby));
            List<List<PccmDataModel>> pccmDs15s = (await GetPccmAllData(PccmNameEnum.PCCM_DS15, pccFilter));

            PCCExeProgressGridModel result = new PCCExeProgressGridModel();

            if (pccmDs04s.Any())
            {
                result.ProgressDatas = pccmDs04s.Where(x => x.Any(y => !(y.VALUE is null))).Select(x => x.ToDictionary(y => y.PCCM_EN_NAME, z => GetValueStr(z.VALUE))).ToList();
                result.ProgressHeaders = pccmDs04s.First().ToDictionary(x => x.PCCM_EN_NAME, y => y.PCCM_CN_NAME);
            }

            if (pccmDs15s.Any())
            {
                result.DelayDatas = pccmDs15s.Where(x => x.Any(y => !(y.VALUE is null))).Select(x => x.ToDictionary(y => y.PCCM_EN_NAME, z => GetValueStr(z.VALUE))).ToList();
                result.DelayHeaders = pccmDs15s.First().ToDictionary(x => x.PCCM_EN_NAME, y => y.PCCM_CN_NAME);
            }

            return result;
        }

        /// <summary>
        /// 工程標案工程概要資料
        /// </summary>
        /// <param name="PCC_PROJECT_UID"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetPCCDs07(string PCC_PROJECT_UID)
        {
            List<PccmDataModel> data = (await GetPccmAllData(PccmNameEnum.PCCM_DS07, new PccFilterModel { PCC_PROJECT_UID = PCC_PROJECT_UID })).First();

            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var item in data)
            {
                result.Add(item.PCCM_CN_NAME, GetValueStr(item.VALUE));
            }
            return result;
        }

        /// <summary>
        /// 工程標案決標資料
        /// </summary>
        /// <param name="PCC_PROJECT_UID"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetPCCDs09(string PCC_PROJECT_UID)
        {
            List<PccmDataModel> data = (await GetPccmAllData(PccmNameEnum.PCCM_DS09, new PccFilterModel { PCC_PROJECT_UID = PCC_PROJECT_UID })).First();

            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var item in data)
            {
                result.Add(item.PCCM_CN_NAME, GetValueStr(item.VALUE));
            }
            return result;
        }

        /// <summary>
        /// 關聯工程會標案資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="PCC_PROJECT_UID"></param>
        /// <param name="START_WORK"></param>
        /// <returns></returns>
        public RtnResultModel SaveProjectMapPCC(string PROJECT_NO, string PCC_PROJECT_UID, DateTime? START_WORK)
        {
            START_WORK = START_WORK.HasValue
                ? new DateTime(START_WORK.Value.Year + 1911, START_WORK.Value.Month, START_WORK.Value.Day)
                : null;

            PccFilterModel pccFilter = new PccFilterModel { PCC_PROJECT_UID = PCC_PROJECT_UID };

            // 取得關聯工程會標案資料 pccm_ds01
            var getPccmDs01Task = Task.Run(async () => await GetPccmData<PccmDs01Model>(PccmNameEnum.PCCM_DS01, pccFilter));
            Task.Run(() => Task.WaitAll(getPccmDs01Task)).Wait();
            PccmDs01Model pccmDs01 = getPccmDs01Task.Result.FirstOrDefault();

            // 取得標案工程概要資料 pccm_ds07
            var getPccmDs07Task = Task.Run(async () => await GetPccmData<PccmDs07Model>(PccmNameEnum.PCCM_DS07, pccFilter));
            Task.Run(() => Task.WaitAll(getPccmDs07Task)).Wait();
            PccmDs07Model pccmDs07 = getPccmDs07Task.Result.LastOrDefault();

            // 取得工程標案驗收資料 pccm_ds14
            var getPccmDs14Task = Task.Run(async () => await GetPccmData<PccmDs14Model>(PccmNameEnum.PCCM_DS14, pccFilter));
            Task.Run(() => Task.WaitAll(getPccmDs14Task)).Wait();
            PccmDs14Model pccmDs14 = getPccmDs14Task.Result.OrderByDescending(x=>x.idate).LastOrDefault();

            // 轉換存檔DB欄位
            ProjectMapPCCModel pccBasic = ChangePCCBasicModel(PROJECT_NO, pccmDs01, pccmDs07);

            #region 調整開工 & 竣工 & 驗收 的工程會檢核點 預定/實際 完成日期
            List<PCCProjChkItemModel> pccProjChkItemSaveModels = new()
            {
                new()
                {
                    PROJECT_NO = PROJECT_NO,
                    CTRL_POINT = "A", // 開工
                    PCC_ESTIMATED_ENDDATE = pccmDs01.scheduledstartdate,
                    PCC_ACTUAL_ENDDATE = pccmDs01.actualstartdate,
                    ACTUAL_ENDDATE = START_WORK
                },
                new()
                {
                    PROJECT_NO = PROJECT_NO,
                    CTRL_POINT = "B", // 竣工
                    PCC_ESTIMATED_ENDDATE = pccmDs01.scheduledenddate,
                    PCC_ACTUAL_ENDDATE = pccmDs01.actualenddate
                },
                new()
                {
                    PROJECT_NO = PROJECT_NO,
                    CTRL_POINT = "C", // 驗收
                    PCC_ESTIMATED_ENDDATE = pccmDs14?.idate,
                    PCC_ACTUAL_ENDDATE = pccmDs14?.aokdat
                }
            };
            #endregion

            dac.BeginTransaction();
            // 更新計畫基本資料
            dac.UpdateProjectBasicByPCC(pccBasic);
            // 更新計畫實際期程
            dac.UpdateProjCtrlExeByPCC(pccBasic);
            // 更新檢核點資料
            dac.UpdateProjChkItemByPCC(pccProjChkItemSaveModels);
            dac.Commit();

            return ChangeResult(true, "關聯成功");
        }

        /// <summary>
        /// 工程標案基本資料Models轉換至存檔DB欄位
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="pccmDs01"></param>
        /// <param name="pccmDs07"></param>
        /// <returns></returns>
        private static ProjectMapPCCModel ChangePCCBasicModel(string PROJECT_NO, PccmDs01Model pccmDs01, PccmDs07Model pccmDs07)
        {
            return new ProjectMapPCCModel
            {
                PROJECT_NO = PROJECT_NO,
                PCC_PROJECT_UID = pccmDs01.plnprj_uid,
                PCC_PROJECT_NO = pccmDs01.plnprj_id,
                PCC_PROJECT_NAME = pccmDs01.plnprj_name,
                FACTORY_CONTACT = pccmDs01.contact_name,
                FACTORY_TEL = pccmDs01.contact_tel,
                TENDER_AWARDING_AMT = (pccmDs01.budget ?? 0) * 1000,
                PROCUREMENT_AMT = pccmDs07 == null ? 0 : (pccmDs07.bdgt1 ?? 0) * 1000
            };
        }

        /// <summary>
        /// 修改使用國發會介接資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="IS_USER_FTY_DATA"></param>
        /// <returns></returns>
        public RtnResultModel SaveProjectUsePCC(string PROJECT_NO, bool IS_USER_FTY_DATA)
        {
            bool success = dac.SaveProjectUsePCC(PROJECT_NO, IS_USER_FTY_DATA);
            if (success)
            {
                projectService.InsertProjectBasicLog(PROJECT_NO, "S2", IS_USER_FTY_DATA ? "12" : "13");
            }
            return ChangeResult(success, success ? "存檔成功" : "存檔失敗");
        }

        /// <summary>
        /// 工程標案同步
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> SyncPCCData(string PROJECT_NO)
        {
            dac.DeleteReloadProjectNo();
            List<string> projNos = PROJECT_NO.Trim().Split(" ").Distinct().ToList();
            await dac.InsertProjBasicReloads(projNos);
            logger.LogInformation($"同步計畫編號:{PROJECT_NO}");
            ProjectFillCycleModel cycleModel = await projectCommonService.GetCurrentCycleData();
            List<AssociatePccProjectModel> projectPccModels = await dac.GetReloadProjectNos(cycleModel);

            var dataDict = new Dictionary<bool, List<AssociatePccProjectModel>>
            {
                { true, projectPccModels.Where(x => x.IS_USER_FTY_DATA).ToList() },
                { false, projectPccModels.Where(x => !x.IS_USER_FTY_DATA).ToList() }
            };

            foreach (var dict in dataDict)
            {
                bool isUseFtyData = dict.Key;
                List<AssociatePccProjectModel> projectPccs = dict.Value;

                if (projectPccs.Any())
                {
                    #region pccm_ds01、pccm_ds14
                    // 同步工程會檢核點完成日期資料 
                    List<PCCProjChkItemModel> pccProjChkPointItems = await SyncPccCheckPoint(projectPccs, isUseFtyData);
                    // 只取 CTRL_POINT = C 且實際完成日期有值 的資料, 修改使用國發會介接資料 IS_USER_FTY_DATA = 0
                    List<PCCProjChkItemModel> acceptances = pccProjChkPointItems.Where(x => x.CTRL_POINT == "C" && x.PCC_ACTUAL_ENDDATE.HasValue).ToList();
                    // 修改使用國發會介接資料 IS_USER_FTY_DATA = 0
                    dac.UpdateProjIsUserFtyData(acceptances.Select(x => x.PROJECT_NO).ToList(), false);
                    #endregion pccm_ds01、pccm_ds14

                    #region pccm_ds04
                    // 同步工程會工程進度資料，並更新 IsSend=1
                    List<string> syncDs04ProjectNos = await SyncPccEngineeringProgress(projectPccs, cycleModel, isUseFtyData);
                    #endregion pccm_ds04

                    
                    // 有使用標案系統資料的計畫
                    if (isUseFtyData)
                    {
                        #region pccm_ds15
                        // 同步工程會落後原因資料 並回傳 沒有落後的計畫或有落後且成功同步落後原因計畫
                        List<string> syncDs15ProjectNos = await SyncPccDelayCausal(projectPccs, cycleModel);
                        #endregion pccm_ds15

                        // 更新同步成功計畫 IS_SEND , SEND_DATE
                        var commonProjectList = syncDs04ProjectNos.Intersect(syncDs15ProjectNos).ToList();
                        if(commonProjectList.Any())
                            dac.UpdateSyncFlag(commonProjectList, cycleModel);
                    }
                }
            }

            // 刪除已重新執行的計畫編號
            dac.DeleteReloadProjectNo();
            return ChangeResult(true, "執行完成");
        }

        /// <summary>
        /// 同步工程會檢核點完成日期資料
        /// </summary>
        /// <param name="projectPccModels"></param>
        /// <param name="isUseFtyData">是否使用國發會介接資料</param>
        /// <returns></returns>
        private async Task<List<PCCProjChkItemModel>> SyncPccCheckPoint(List<AssociatePccProjectModel> projectPccModels, bool isUseFtyData)
        {
            List<PCCProjChkItemModel> pccProjChkPointItems = new();
            foreach (AssociatePccProjectModel model in projectPccModels)
            {
                PccmDs01Model pccmDs01 = (await GetPccmData<PccmDs01Model>(PccmNameEnum.PCCM_DS01, new PccFilterModel
                {
                    PCC_PROJECT_UID = model.PCC_PROJECT_UID
                })).FirstOrDefault();

                
                // 辦理開工
                pccProjChkPointItems.Add(new PCCProjChkItemModel
                {
                    PROJECT_NO = model.PROJECT_NO,
                    PROJECT_NAME = model.PROJECT_NAME,
                    PROJECT_YEAR = model.PROJECT_YEAR,
                    CTRL_POINT = "A",
                    PCC_ESTIMATED_ENDDATE = pccmDs01?.scheduledstartdate,
                    PCC_ACTUAL_ENDDATE = pccmDs01?.actualstartdate,
                    MDF_DATE = pccmDs01?.updatetime ?? DateTime.Now
                });
                // 辦理竣工
                pccProjChkPointItems.Add(new PCCProjChkItemModel
                {
                    PROJECT_NO = model.PROJECT_NO,
                    PROJECT_NAME = model.PROJECT_NAME,
                    PROJECT_YEAR = model.PROJECT_YEAR,
                    CTRL_POINT = "B",
                    PCC_ESTIMATED_ENDDATE = pccmDs01 == null? null:
                        pccmDs01.scheduledenddate_f.HasValue
                        ? pccmDs01.scheduledenddate_f
                        : pccmDs01.scheduledenddate,
                    PCC_ACTUAL_ENDDATE = pccmDs01?.actualenddate,
                    MDF_DATE = pccmDs01?.updatetime ?? DateTime.Now
                });
                

                PccmDs14Model pccmDs14 = (await GetPccmData<PccmDs14Model>(PccmNameEnum.PCCM_DS14, new PccFilterModel
                {
                    PCC_PROJECT_UID = model.PCC_PROJECT_UID
                })).OrderByDescending(x=>x.idate).LastOrDefault();


                // 辦理驗收
                pccProjChkPointItems.Add(new PCCProjChkItemModel
                {
                    PROJECT_NO = model.PROJECT_NO,
                    PROJECT_NAME = model.PROJECT_NAME,
                    PROJECT_YEAR = model.PROJECT_YEAR,
                    CTRL_POINT = "C",
                    PCC_ESTIMATED_ENDDATE = pccmDs14?.idate,
                    PCC_ACTUAL_ENDDATE = pccmDs14?.aokdat,
                    MDF_DATE = pccmDs14?.updatetime ?? DateTime.Now
                });
            }
            

            if (isUseFtyData)
            {
                // isUseFtyData 有使用 更新 ACTUAL_ENDDATE、PCC_ESTIMATED_ENDDATE、PCC_ACTUAL_ENDDATE、MDF_DATE
                dac.UpdatePccProjChkItemsByPcc(pccProjChkPointItems);
            }
            else
            {
                // isUseFtyData 不使用 更新 PCC_ESTIMATED_ENDDATE、PCC_ACTUAL_ENDDATE
                dac.UpdatePccProjChkItems(pccProjChkPointItems);
            }
            return pccProjChkPointItems;
        }

        /// <summary>
        /// 同步工程會工程進度資料，並更新 IsSend=1
        /// </summary>
        /// <param name="projectPccModels"></param>
        /// <param name="cycleModel"></param>
        /// <param name="isUseFtyData">是否使用國發會介接資料</param>
        /// <returns></returns>
        private async Task<List<string>> SyncPccEngineeringProgress(List<AssociatePccProjectModel> projectPccModels, ProjectFillCycleModel cycleModel, bool isUseFtyData)
        {
            Dictionary<string, PccmDs04Model> pccmDs04Dict = new Dictionary<string, PccmDs04Model>();
            foreach (AssociatePccProjectModel model in projectPccModels)
            {
                // 取得工程會每月辦理資料
                PccmDs04Model pccmDs04 = (await GetPccmData<PccmDs04Model>(PccmNameEnum.PCCM_DS04, new PccFilterModel
                {
                    PCC_PROJECT_UID = model.PCC_PROJECT_UID,
                    PCC_PROJECT_YEAR = cycleModel.PROJECT_YEAR_INT + 1911,
                    PCC_PROJECT_MONTH = cycleModel.PROJECT_MONTH
                })).LastOrDefault();
                if (pccmDs04 != null)
                {
                    pccmDs04Dict.Add(model.PROJECT_NO, pccmDs04);
                }
            }

            // 工程會API欄位 & 資料庫欄位轉換
            List<ProjectEngProgressInsertModel> models = pccmDs04Dict.Select(pccmDs04 => new ProjectEngProgressInsertModel
            {
                PROJECT_NO = pccmDs04.Key,
                IPC_RES_PRG = pccmDs04.Value.monthscheduledprogress,
                IPC_ACT_PRG = pccmDs04.Value.monthactualprogress,
                TEN_RES_PRG = pccmDs04.Value.monthscheduledprogress,
                TEN_ACT_PRG = pccmDs04.Value.monthactualprogress,
                EXECUTE_CONDITION = pccmDs04.Value.remark,
                YEAR = cycleModel.PROJECT_YEAR,
                MONTH = cycleModel.PROJECT_MONTH
            }).ToList();

            if (isUseFtyData)
            {
                // 使用標案系統資料的計畫
                // 更新 TEN_RES_PRG、TEN_ACT_PRG、IPC_RES_PRG、IPC_ACT_PRG、EXECUTE_CONDITION、IS_SYNC_PCC
                dac.UpdateEngProgressUseFtyDataByPcc(models);
            }
            else
            {
                // 更新 TEN_RES_PRG、TEN_ACT_PRG
                dac.UpdateEngProgressUseFtyData(models);
            }
            return pccmDs04Dict.Select(x => x.Key).ToList();
        }

        /// <summary>
        /// 同步工程會落後原因資料
        /// </summary>
        /// <param name="projectPccModels"></param>
        /// <param name="cycleModel"></param>
        /// <returns>沒有落後的計畫 或 有落後且成功同步落後原因計畫</returns>
        private async Task<List<string>> SyncPccDelayCausal(List<AssociatePccProjectModel> projectPccModels, ProjectFillCycleModel cycleModel)
        {
            // 取得在重大系統有使用標案資料並有落後事實的計畫
            Dictionary<string, string> delayProjDict = await GetDelayProject(projectPccModels, cycleModel);
            List<SetParamModel> delayRespons = (await setParamService.GetSysParams("DELAY_RESPON")).ToList();
            List<CodeDelayClassModel> codeDelayClasses = await dac.GetCodeDelayClass();

            List<ProjectDelayCausalModel> delayCausals = new();
            foreach (AssociatePccProjectModel model in projectPccModels)
            {
                // 呼叫 ds15
                PccmDs15Model pccmDs15 = (await GetPccmData<PccmDs15Model>(PccmNameEnum.PCCM_DS15, new PccFilterModel
                {
                    PCC_PROJECT_UID = model.PCC_PROJECT_UID,
                    PCC_PROJECT_YEAR = cycleModel.PROJECT_YEAR_INT + 1911,
                    PCC_PROJECT_MONTH = cycleModel.PROJECT_MONTH
                })).LastOrDefault();
                if (pccmDs15 != null)
                {
                    // 轉換落後原因工程會欄位 to DB欄位 
                    ProjectDelayCausalModel delayCausal = ChangeDelayCausal(model.PROJECT_NO, delayProjDict, pccmDs15, cycleModel, delayRespons, codeDelayClasses);
                    if (delayCausal != null)
                    {
                        delayCausals.Add(delayCausal);
                    }
                }
            }

            dac.AddMdfDelayCausal(delayCausals);

            // 沒有落後的計畫
            List<string> result = projectPccModels.Select(x => x.PROJECT_NO).ToList().Except(delayProjDict.Select(x => x.Key)).ToList();
            // 加上有落後且成功同步落後原因計畫
            result.AddRange(delayCausals.Select(x => x.PROJECT_NO));
            return result;
        }

        /// <summary>
        /// 取得在重大系統有使用標案資料並有落後事實的計畫
        /// </summary>
        /// <param name="projectPccModels"></param>
        /// <param name="cycleModel"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, string>> GetDelayProject(List<AssociatePccProjectModel> projectPccModels, ProjectFillCycleModel cycleModel)
        {
            List<string> projectNos = projectPccModels.Select(x => x.PROJECT_NO).ToList();
            // 取得工程進度資料
            List<ProjectEngineeringProgressModel> progressModels = await dac.GetProjEngProgresses(projectNos, cycleModel.PROJECT_YEAR_INT, cycleModel.PROJECT_MONTH);

            // 取得落後原因類型，並只回傳有落後原因類型的計畫
            DateTime endDT = new DateTime(cycleModel.PROJECT_YEAR_INT + 1911, cycleModel.PROJECT_MONTH_INT, 1).AddMonths(1).AddDays(-1);
            Dictionary<string, string> result = progressModels
                .Select(x => new { PROJECT_NO = x.PROJECT_NO, DelayKind = dac.GetDelayKind(x.PROJECT_NO, endDT) })
                .Where(x => !string.IsNullOrEmpty(x.DelayKind))
                .ToDictionary(x => x.PROJECT_NO, y => y.DelayKind);
            return result;
        }

        /// <summary>
        /// 轉換落後原因工程會欄位 to DB欄位
        /// </summary>
        /// <param name="PROJECT_NO">計畫編號</param>
        /// <param name="delayProjectDict">落後原因 Key:計畫編號 Value:落後原因類型</param>
        /// <param name="pccmDs15"></param>
        /// <param name="cycleModel"></param>
        /// <param name="delayRespons"></param>
        /// <param name="codeDelayClasses"></param>
        /// <returns></returns>
        private ProjectDelayCausalModel ChangeDelayCausal(string PROJECT_NO, Dictionary<string, string> delayProjectDict, PccmDs15Model pccmDs15, ProjectFillCycleModel cycleModel, List<SetParamModel> delayRespons, List<CodeDelayClassModel> codeDelayClasses)
        {
            bool isDelayProj = delayProjectDict.ContainsKey(PROJECT_NO);

            CodeDelayClassModel codeDelayClass = pccmDs15 == null ? null : codeDelayClasses.FirstOrDefault(x => x.DELAY_CLASS_SUB_ITEM == pccmDs15.mft);
            SetParamModel delayRespon = pccmDs15 == null ? null : delayRespons.FirstOrDefault(x => x.SET_VALUE == pccmDs15.respons);

            ProjectDelayCausalModel result = new ProjectDelayCausalModel()
            {
                PROJECT_NO = PROJECT_NO,
                YEAR = cycleModel.PROJECT_YEAR_INT,
                MONTH = cycleModel.PROJECT_MONTH
            };

            // 計畫有落後事實，且工程會有落後原因資料
            if (isDelayProj && pccmDs15 != null)
            {
                result.DELAY_KIND = delayProjectDict[PROJECT_NO];
                result.DELAY_CLASS_C = codeDelayClass != null ? codeDelayClass.DELAY_CLASS_ID : null;
                result.DELAY_SUBCLASS_C = codeDelayClass != null ? codeDelayClass.DELAY_CLASS_SUB_ID : null;
                result.DELAY_RESPON = delayRespon != null ? delayRespon.SET_TYPE : null;
                result.DELAY_CAUSAL = pccmDs15.mfas;
                result.SOLUTION = pccmDs15.mfrt;
                result.COORDINATION = pccmDs15.mfru;
                result.DEADLINES = GetEndOfMonth(pccmDs15.okdt);
                return result;
            }
            // 計畫沒有落後事實，但工程會有落後原因資料
            else if (!isDelayProj && pccmDs15 != null)
            {
                result.DELAY_KIND = "D2";
                result.DELAY_CLASS_C = codeDelayClass != null ? codeDelayClass.DELAY_CLASS_ID : null;
                result.DELAY_SUBCLASS_C = codeDelayClass != null ? codeDelayClass.DELAY_CLASS_SUB_ID : null;
                result.DELAY_RESPON = delayRespon != null ? delayRespon.SET_TYPE : null;
                result.DELAY_CAUSAL = pccmDs15.mfas;
                result.SOLUTION = pccmDs15.mfrt;
                result.COORDINATION = pccmDs15.mfru;
                result.DEADLINES = GetEndOfMonth(pccmDs15.okdt);
                return result;
            }
            // 計畫有落後事實，但工程會沒有落後原因資料
            else if (isDelayProj && pccmDs15 == null)
            {
                result.DELAY_KIND = delayProjectDict[PROJECT_NO];
                result.DELAY_CAUSAL = "本系統檢核點未完成，進度已落後，但工程會標案系統未落後";
                return result;
            }
            return null;
        }

        /// <summary>
        /// yyyyMM to 該月最後一天 DateTime
        /// </summary>
        /// <param name="yyyyMM"></param>
        /// <returns></returns>
        private DateTime? GetEndOfMonth(string yyyyMM)
        {
            DateTime result;
            if (!DateTime.TryParseExact(yyyyMM, "yyyyMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return null;
            }

            return result.AddMonths(1).AddDays(-1);
        }

        /// <summary>
        /// 工程標案 excel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ObjectResultModel<PccmXlsGridModel>> GetPccmXls(PccXlsFilterModel model)
        {
            string pccmXlsPath = configuration.GetValue<string>("pccmXlsPath");
            pccmXlsPath = Path.Combine(pccmXlsPath, model.SYNC_DATE.Value.ToString("yyyyMMdd"), $"{model.DSNO}.xlsx");
            logger.LogInformation($"GetPccmXls pccmXlsPath: {pccmXlsPath}");
            if (!File.Exists(pccmXlsPath))
            {
                return new ObjectResultModel<PccmXlsGridModel>() { success = false, message = "無檔案" };
            }

            var xlsResult = new RtnXlsResultModel()
            {
                Headers = new List<string>(),
                Contents = new List<Dictionary<string, object>>()
            };

            // 取得檔案
            Workbook excel = new Workbook(pccmXlsPath);
            foreach (Worksheet sheet in excel.Worksheets)
            {
                if (!sheet.Name.StartsWith("Dataset_"))
                {
                    continue;
                }

                // 讀取 excel檔案
                XlsParameter parameter = new XlsParameter
                {
                    WorksheetName = sheet.Name,
                    HeaderStrRow = 0,
                    HeaderStrColumn = 0,
                    ContentStrRow = 1
                };

                RtnXlsResultModel xlsData = xlsService.GetExcelData(excel, parameter);
                if (!string.IsNullOrEmpty(xlsData.ErrMsg))
                {
                    return new ObjectResultModel<PccmXlsGridModel>() { success = false, message = xlsData.ErrMsg };
                }

                xlsResult.Headers = xlsData.Headers;
                xlsResult.Contents.AddRange(xlsData.Contents);
            }

            // 篩選標案編號
            if (!string.IsNullOrEmpty(model.PCC_PROJECT_NO))
            {
                xlsResult.Contents = xlsResult.Contents.Where(dict => dict.ContainsKey("plnprj_id") 
                    && dict["plnprj_id"].ToString().Contains(model.PCC_PROJECT_NO)).ToList();
            }

            // 篩選標案名稱
            if(!string.IsNullOrEmpty(model.PCC_PROJECT_NAME))
            {
                xlsResult.Contents = xlsResult.Contents.Where(dict => dict.ContainsKey("plnprj_name")
                    && dict["plnprj_name"].ToString().Contains(model.PCC_PROJECT_NAME)).ToList();
            }

            PccmXlsGridModel result = new PccmXlsGridModel
            {
                Headers = await dac.GetPccmSchemaInfoDict(model.DSNO.ToUpper()),
                Contents = xlsResult.Contents.Select(x => x.ToDictionary(y => y.Key, z => GetValueStr(z.Value))).ToList()
            };
            
            logger.LogInformation($"GetPccmXls result.Headers.Cnt: {result.Headers.Count} ; result.Contents.Cnt: {result.Contents.Count}");
            return new ObjectResultModel<PccmXlsGridModel>() { success = true, data = result };
        }

        /// <summary>
        /// 下載工程標案 excel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<(byte[] bytes, string fileName, string contentType)> DownloadPccmXls(PccXlsFilterModel model)
        {
            // 取路徑
            string pccmXlsPath = configuration.GetValue<string>("pccmXlsPath");
            pccmXlsPath = Path.Combine(pccmXlsPath, model.SYNC_DATE.Value.ToString("yyyyMMdd"), $"{model.DSNO}.xlsx");
            // 加上副檔名
            string ExcelName = model.DSNO + ".xlsx";
            // Excel文件類型編碼
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; 
            // 確認檔案是否存在
            if (!System.IO.File.Exists(pccmXlsPath))
            {
                throw new FileNotFoundException("File not found.", pccmXlsPath);
            }
            // 異步讀取檔案
            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(pccmXlsPath); 
            return (fileBytes, ExcelName, contentType);
        }

        /// <summary>
        /// 取得工程標案資料集
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetPccSrcTables()
        {
            return await dac.GetPccSrcTables();
        }

    }
}

