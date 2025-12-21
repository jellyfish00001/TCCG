using System.Collections.Generic;
using System.Threading.Tasks;
using SDO.Dac;
using SDO.Base.Utils.Models;
using System;
using SDO.Models;
using Microsoft.AspNetCore.Http;
using Aspose.Cells;
using SDO.XlsReader.Models;
using SDO.XlsReader.Interface;
using System.Linq;
using System.Globalization;

namespace SDO.Services
{
    public class ImportProjectService : Service, IImportProjectService
    {
        private readonly IImportProjectDac dac;
        private readonly ISCOrgDac scOrgDac;
        private readonly IXlsService xlsService;
        private readonly IProjectService projectService;


        public ImportProjectService(IImportProjectDac dac,ISCOrgDac scOrgDac,IXlsService xlsService, IProjectService projectService)
        {
            this.dac = dac;
            this.scOrgDac = scOrgDac;
            this.xlsService = xlsService;
            this.projectService = projectService;
        }
        
        /// <summary>
        /// 取得先期計畫資料列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<PWSSDPlanGridModel>> QueryPWSSDPlanList(ImportProjectQueryModel model)
        {
            return await dac.QueryPWSSDPlanList(model);
        }

        /// <summary>
        /// 匯入先期計畫
        /// </summary>
        /// <param name="PlanIds"></param>
        /// <returns></returns>
        public RtnResultModel ImportPWSSDPlans(List<int> PlanIds)
        {
            dac.BeginTransaction();
            foreach(int PlanId in PlanIds)
            {
                // 取得先期計畫基本資料
                var getPWSSDPlanTask = Task.Run(async () => await dac.GetPWSSDPlan(PlanId));
                Task.Run(() => Task.WaitAll(getPWSSDPlanTask)).Wait();
                var PWSSDPlan = getPWSSDPlanTask.Result;

                if (PWSSDPlan != null)
                {
                    #region 產生ProjectNo
                    string ProjectNo = projectService.GenProjectNo(PWSSDPlan.PlanYear.PadLeft(3, '0'), PWSSDPlan.OU_ID);
                    #endregion
                    // 計畫性質
                    PWSSDPlan.PlanDateType = PWSSDPlan.PlanDateType == "1"? "新興計畫(以前年度從未提報之計畫)":
                        PWSSDPlan.PlanDateType == "2" ? "延續性計畫(既有例行單一年度計畫或前一年度已編列預算之跨年度計畫)":"";

                    // 預定完成期限
                    var getLastDateTask = Task.Run(async () => await dac.GetProjectLastDate(PWSSDPlan.PlanId));
                    Task.Run(() => Task.WaitAll(getLastDateTask)).Wait();
                    DateTime? projectLastDate = getLastDateTask.Result;

                    // 匯入先期計畫查核點日期
                    dac.InsertCustomCheckItemDateByPlanId(PlanId, ProjectNo);
                    // 新增基本資料
                    dac.InsertProjectBasicByPWSSD(ProjectNo, projectLastDate, PWSSDPlan);
                    // 新增計畫經費來源
                    dac.InsertBudgetSourceGByPWSSD(ProjectNo, PlanId);
                    // 寫入異動記錄檔 (PROJECT_BASIC_LOG)
                    projectService.InsertProjectBasicLog(ProjectNo, "S1", "0");
                }
            }
            dac.Commit();

            return ChangeResult(true, "匯入成功");
        }
        #region 匯入計畫基本資料
        /// <summary>
        /// 匯入計畫基本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel ImportGeneralProject(string PlanYear,IFormFile file)
        {
            // 取得檔案
            Workbook excel = new Workbook(file.OpenReadStream());
            // 讀取 excel檔案
            XlsParameter parameter = new XlsParameter
            {
                WorksheetIndex = 0,
                HeaderStrRow = 0,
                HeaderStrColumn = 0,
                ContentStrRow = 1
            };
            RtnXlsResultModel xlsResult = xlsService.GetExcelData(excel, parameter);

            if (!string.IsNullOrEmpty(xlsResult.ErrMsg))
                return ChangeResult(false, xlsResult.ErrMsg);

            // 轉換Model
            List<ImportGenProjExcelModel> models = ToImpGenProjExcelModel(xlsResult.Contents, parameter.ContentStrRow);

            // 檢查欄位
            CheckFields(models);

            List<ImportGenProjExcelModel> successModels = models.Where(x => !x.ErrMsgs.Any()).ToList();
            List<ImportGenProjExcelModel> errModels = models.Where(x => x.ErrMsgs.Any()).ToList();

            if (successModels.Any())
            {
                List<ImportGeneralProjectModel> importModels = new List<ImportGeneralProjectModel>();
                // 將驗證用Model 轉換為Insert Model
                ChangeModel(successModels, importModels,PlanYear);

                dac.BeginTransaction();
                int importModelIndex = 0;
                foreach(var importModel in importModels)
                {
                    importModel.PROJECT_NO = projectService.GenProjectNo(PlanYear.PadLeft(3, '0'), importModel.EXEC_ORGAN_C);
                    // 寫入 PROJECT_BASIC
                    dac.InsertProjectBasicByExcel(importModel);
                    // 寫入 PROJECT_BUDGET_SOURCE_G
                    dac.InsertProjecBudgetSourceGByExcel(importModel);
                    // 寫入異動記錄檔 (PROJECT_BASIC_LOG)
                    projectService.InsertProjectBasicLog(importModel.PROJECT_NO, "S1", "0");
                    importModelIndex++;
                }
                
                dac.Commit();
            }

            List<string> errMsgs = errModels.Select(x => $"第{x.Index}列資料 " + string.Join("、", x.ErrMsgs)).ToList();

            if (errModels.Any())
                return ChangeResult(false, string.Join("\n", errMsgs));
            else
                return ChangeResult(true, "匯入成功");
        }

        /// <summary>
        /// 轉換計劃 Excel 清單
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="contentStrRow"></param>
        /// <returns></returns>
        private List<ImportGenProjExcelModel> ToImpGenProjExcelModel(List<Dictionary<string, object>> contents, int contentStrRow)
        {
            int index = contentStrRow + 1;
            List<ImportGenProjExcelModel> models = (from content in contents
                          let idx = index++
                          select new ImportGenProjExcelModel
                          {
                              Index = idx,
                              PROJECT_NAME = ToValStr(content["計畫名稱"]),
                              ALL_JOB = ToValStr(content["辦理內容"]),
                              BUDGET_CENTRAL = ToValStr(content["中央補助款(元)"]),
                              BUDGET_LOCAL = ToValStr(content["本府預算金額"]),
                              MASTER_ORGAN_C = ToValStr(content["主管機關"]),
                              EXEC_ORGAN_C = ToValStr(content["執行機關"])
                          }).ToList();
            return models;
        }

        /// <summary>
        /// 將驗證用Model 轉換為Insert Model
        /// </summary>
        /// <param name="excelModels">Models From Excel</param>
        /// <param name="insertModels">新增計畫Model</param>
        /// <param name="planYear">年度</param>
        private void ChangeModel(List<ImportGenProjExcelModel> excelModels, List<ImportGeneralProjectModel> insertModels,string planYear)
        {
            foreach (var model in excelModels)
            {
                insertModels.Add(new ImportGeneralProjectModel
                {
                    PlanYear = planYear,
                    PROJECT_NAME = model.PROJECT_NAME,
                    ALL_JOB = model.ALL_JOB,
                    BUDGET_CENTRAL = Decimal.Parse(model.BUDGET_CENTRAL), 
                    BUDGET_LOCAL = Decimal.Parse(model.BUDGET_LOCAL),     
                    BUDGET_LOCAL_SOURCE = model.BUDGET_LOCAL_SOURCE,
                    MASTER_ORGAN_C = model.MASTER_ORGAN_C,
                    EXEC_ORGAN_C = model.EXEC_ORGAN_C
                });
            }
        }

        /// <summary>
        /// 轉成字串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private string ToValStr(object obj)
        {
            return obj is null ? string.Empty : obj.ToString().Trim();
        }

        /// <summary>
        /// 檢查欄位
        /// </summary>
        /// <param name="models">計劃資料</param>
        private void CheckFields(List<ImportGenProjExcelModel> models)
        {
            // 取得一級單位資料
            List<string> orgIds = scOrgDac.GetOUByOUKind("1").Select(x=>x.OU_ID.Trim()).ToList();
            foreach (var model in models)
            {
                model.ErrMsgs ??= new List<string>();
                
                CheckVal(model.PROJECT_NAME, "計畫名稱", true, model.ErrMsgs, maxLength: 200);
                CheckVal(model.ALL_JOB, "辦理內容", true, model.ErrMsgs, maxLength: 600);

                if (model.BUDGET_CENTRAL == ""|| model.BUDGET_CENTRAL.Substring(0,1) == "-" ||
                    !decimal.TryParse(model.BUDGET_CENTRAL, out decimal budCentral))
                    model.ErrMsgs.Add("中央補助款(元)必須為正數且不得為空");

                if (model.BUDGET_LOCAL == ""||model.BUDGET_LOCAL.Substring(0,1) == "-" ||
                    !decimal.TryParse(model.BUDGET_LOCAL, out decimal budLocal))
                    model.ErrMsgs.Add("本府預算金額必須為正數且不得為空");

                // 檢查主辦、執行機關代碼是否存在
                if (!orgIds.Contains(model.MASTER_ORGAN_C))
                    model.ErrMsgs.Add("主辦機關有誤");
                if (!orgIds.Contains(model.EXEC_ORGAN_C))
                    model.ErrMsgs.Add("執行機關有誤");
            }
        }

        /// <summary>
        /// 檢查欄位值
        /// </summary>
        /// <param name="obj">要檢查的欄位</param>
        /// <param name="objName">欄位名稱</param>
        /// <param name="isRequired">是否必填</param>
        /// <param name="errMsgs">錯誤清單</param>
        /// <param name="maxLength">最大長度</param>
        /// <param name="minLength">最小長度</param>
        /// <param name="isDate">是否是日期</param>
        private void CheckVal(object obj, string objName, bool isRequired, List<string> errMsgs, int? maxLength = null, int? minLength = null, bool isDate = false)
        {
            if (obj is null)
            {
                if (isRequired)
                {
                    errMsgs.Add($"{objName}不能為空值");
                }
                return;
            }

            Type[] typeInts = new Type[] { typeof(int), typeof(int?) };
            if (typeInts.Contains(obj.GetType()))
            {
                int? val = (int?)obj;
                if (isRequired && !val.HasValue)
                {
                    errMsgs.Add($"{objName}不能為空值");
                }
                else if (val.Value < 0)
                {
                    errMsgs.Add($"{objName}必須為正整數");
                }
            }

            Type[] typeDoubles = new Type[] { typeof(double), typeof(double?) };
            if (typeDoubles.Contains(obj.GetType()))
            {
                double? val = (double?)obj;
                if (isRequired && !val.HasValue)
                {
                    errMsgs.Add($"{objName}不能為空值");
                }
                else if (val.Value < 0)
                {
                    errMsgs.Add($"{objName}必須為正整數");
                }
            }

            Type[] typeDecimals = new Type[] { typeof(decimal), typeof(decimal?) };
            if (typeDecimals.Contains(obj.GetType()))
            {
                decimal? val = (decimal?)obj;
                if (isRequired && !val.HasValue)
                {
                    errMsgs.Add($"{objName}不能為空值");
                }
                else if (val.Value < 0)
                {
                    errMsgs.Add($"{objName}必須為正整數");
                }
            }

            Type[] typeDateTimes = new Type[] { typeof(DateTime), typeof(DateTime?) };
            if (typeDateTimes.Contains(obj.GetType()))
            {
                DateTime? val = (DateTime?)obj;
                if (isRequired && !val.HasValue)
                {
                    errMsgs.Add($"{objName}必填");
                }
            }

            Type[] typeBools = new Type[] { typeof(bool), typeof(bool?) };
            if (typeBools.Contains(obj.GetType()))
            {
                int? intVal = (int?)obj;
                if (isRequired && !intVal.HasValue)
                {
                    errMsgs.Add($"{objName}必填");
                }

                if (intVal.HasValue && !(intVal == 0 || intVal == 1))
                {
                    errMsgs.Add($"{objName}輸入錯誤");
                }
            }

            if (obj.GetType() == typeof(string))
            {
                string strVal = (string)obj;
                if (isRequired && string.IsNullOrEmpty(strVal))
                {
                    errMsgs.Add($"{objName}必填");
                }

                if (!string.IsNullOrEmpty(strVal) && maxLength.HasValue && strVal.Length > maxLength)
                {
                    errMsgs.Add($"{objName}限{maxLength}字");
                }

                if (!string.IsNullOrEmpty(strVal) && minLength.HasValue && strVal.Length < minLength)
                {
                    errMsgs.Add($"{objName}至少{minLength}字");
                }

                if (isDate && !DateTime.TryParseExact(strVal, "yyyyMMdd", null, DateTimeStyles.None, out DateTime dateTimeVal))
                {
                    errMsgs.Add($"{objName}非日期格式");
                }
            }
        }
        #endregion
    }
}
