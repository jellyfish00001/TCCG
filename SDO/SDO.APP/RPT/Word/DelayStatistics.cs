using Aspose.Words;
using Aspose.Words.Tables;
using Autofac;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    /// <summary>
    /// 報表4 連續落後統計表
    /// </summary>
    public class DelayStatistics : WContentBuilder
    {
        private IStatisticsDac statisticsDac;

        // 需合併清單
        private List<string> mergeData = new();

        public DelayStatistics(IComponentContext coms) : base(coms)
        {
            statisticsDac = coms.Resolve<IStatisticsDac>();
            TemplateFileName = "DelayStatisticsRPT.doc";
        }

        protected override async Task GetData()
        {
            StatisticsModel queryModel = (StatisticsModel)Parameter.ObjectModel;
            // 近三期年月
            var currYM = new { YEAR = queryModel.STATISTICS_YEAR, MONTH = queryModel.STATISTICS_MONTH };
            var lastYM = new
            {
                YEAR = queryModel.STATISTICS_MONTH == 1 ? (Convert.ToInt32(queryModel.STATISTICS_YEAR) - 1).ToString() : queryModel.STATISTICS_YEAR,
                MONTH = queryModel.STATISTICS_MONTH == 1 ? 12 : queryModel.STATISTICS_MONTH - 1
            };
            var YMBeforeLast = new
            {
                YEAR = queryModel.STATISTICS_MONTH <= 2 ? (Convert.ToInt32(queryModel.STATISTICS_YEAR) - 1).ToString() : queryModel.STATISTICS_YEAR,
                MONTH = queryModel.STATISTICS_MONTH == 2 ? 12 : queryModel.STATISTICS_MONTH == 1 ? 11 : queryModel.STATISTICS_MONTH - 2
            };

            // 取得資料
            List<DelayStatisticsModel> models = await statisticsDac.GetDelayStatistics(queryModel);
            // 取得機關列管計畫件數
            List<OrgProjectCntModel> orgProjectCntModels = await statisticsDac.GetExecOrgProjectCnt(queryModel);

            #region 加總資料宣告
            decimal T1OrgTt = 0; // 列管件數加總
            decimal T1DlyTt = 0; // 落後件數加總

            decimal T1Dly3MnTt = 0; // 連續落後三個月以上加總
            decimal T1Dly2MnTt = 0; // 落後兩個月加總
            decimal T1Dly1MnTt = 0; // 落後一個月加總
            #endregion

            if (models.Any())
            {
                #region 計算落後資料
                List<string> projStatuses = new List<string> { null, "1", "2", "3" };
                // 落後1個月資料
                List<DelayStatisticsModel> delay1MnDatas = models.Where(x => x.DATA_YEAR.ToString() == currYM.YEAR
                    && (Convert.ToInt32(x.DATA_MONTH) == currYM.MONTH)
                    && !projStatuses.Contains(x.PROJECT_STATUS)).ToList();
                // 連續落後兩個月資料
                List<DelayStatisticsModel> delay2MnDatas = delay1MnDatas.Any()
                    ? models.Where(x => x.DATA_YEAR.ToString() == lastYM.YEAR
                        && Convert.ToInt32(x.DATA_MONTH) == lastYM.MONTH
                        && delay1MnDatas.Select(y => y.PROJECT_NO).Contains(x.PROJECT_NO)).ToList()
                    : new();
                // 連續落後三個月的資料
                List<DelayStatisticsModel> delay3MnDatas = delay2MnDatas.Any()
                    ? models.Where(x => x.DATA_YEAR.ToString() == YMBeforeLast.YEAR
                        && Convert.ToInt32(x.DATA_MONTH) == YMBeforeLast.MONTH
                        && delay2MnDatas.Select(y => y.PROJECT_NO).Contains(x.PROJECT_NO)).ToList()
                    : new();

                // 連續落後兩個月和三個月的資料，落後類型要取落後一個月的資料
                delay3MnDatas.ForEach(x =>
                {
                    x.DELAY_KIND = delay1MnDatas.Where(y => y.PROJECT_NO == x.PROJECT_NO).First().DELAY_KIND;
                });
                delay2MnDatas.ForEach(x =>
                {
                    x.DELAY_KIND = delay1MnDatas.Where(y => y.PROJECT_NO == x.PROJECT_NO).First().DELAY_KIND;
                });

                // 落後一個月的資料中 排除連續落後兩個月
                if (delay2MnDatas.Any())
                {
                    delay1MnDatas = delay1MnDatas.Where(x => !delay2MnDatas.Select(x => x.PROJECT_NO).Contains(x.PROJECT_NO)).ToList();
                }
                // 落後兩個月的資料中，排除連續落後三個月的資料
                if (delay3MnDatas.Any())
                {
                    delay2MnDatas = delay2MnDatas.Where(x => !delay3MnDatas.Select(x => x.PROJECT_NO).Contains(x.PROJECT_NO)).ToList();
                }
                #endregion

                // 表1資料
                List<object> tb1 = new();
                foreach (OrgProjectCntModel model in orgProjectCntModels)
                {
                    string org = model.OrgId;
                    // 機關列管件數
                    decimal orgCnt = model.ProjectCnt;
                    T1OrgTt += orgCnt;
                    // 落後一個月件數
                    decimal delay1MnCnt = delay1MnDatas.Where(x => x.EXEC_ORGAN_C == org).Count();
                    T1Dly1MnTt += delay1MnCnt;
                    // 連續落後兩個月件數
                    decimal delay2MnCnt = delay2MnDatas.Where(x => x.EXEC_ORGAN_C == org).Count();
                    T1Dly2MnTt += delay2MnCnt;
                    // 連續落後三個月件數
                    decimal delay3MnCnt = delay3MnDatas.Where(x => x.EXEC_ORGAN_C == org).Count();
                    T1Dly3MnTt += delay3MnCnt;
                    // 落後件數
                    decimal totalDelayCnt = delay1MnCnt + delay2MnCnt + delay3MnCnt;
                    T1DlyTt += totalDelayCnt;
                    tb1.Add(new
                    {
                        T1Organ = model.OrgName,
                        T1OrgCnt = orgCnt,
                        T1Dly3Mn = delay3MnCnt,
                        T1Dly2Mn = delay2MnCnt,
                        T1Dly1Mn = delay1MnCnt,
                        T1DlyCnt = totalDelayCnt,
                        T1DlyPCT = Math.Round(totalDelayCnt / orgCnt * 100, 1)
                    });
                }

                // 表2 落後一個月資料
                List<Dictionary<string, object>> tb2For1Mn = GetListData(delay1MnDatas, 1);
                // 表2 連續落後兩個月資料
                List<Dictionary<string, object>> tb2For2Mn = GetListData(delay2MnDatas, 2);
                // 表2 連續落後三個月資料
                List<Dictionary<string, object>> tb2For3Mn = GetListData(delay3MnDatas, 3);

                if (tb1.Any())
                    ListData.Add(new WordTableData { LIST_DATA = tb1, TABLE_INDEX = 0 });
                if (tb2For3Mn.Any())
                    ListData.Add(new WordTableData { LIST_DATA = tb2For3Mn, TABLE_INDEX = 1 });
                if (tb2For2Mn.Any())
                    ListData.Add(new WordTableData { LIST_DATA = tb2For2Mn, TABLE_INDEX = 1 });
                if (tb2For1Mn.Any())
                    ListData.Add(new WordTableData { LIST_DATA = tb2For1Mn, TABLE_INDEX = 1 });
            }

            string title = Parameter.FileName;
            int lineWrapIndex = title.IndexOf("連續落後統計表");
            title = $"{title.Substring(0, lineWrapIndex)}\v{title.Substring(lineWrapIndex)}";

            BasicData = new
            {
                YEAR_MONTH = $"{queryModel.STATISTICS_YEAR}年{queryModel.STATISTICS_MONTH}月",
                TITLE = title,
                DATE = DateTime.Now.ToTwDateString(),
                // 表一加總
                T1OrgTt,
                T1DlyTt,
                T1DlyTtPCT = Math.Round(T1DlyTt / T1OrgTt * 100, 1),
                T1Dly3MnTt,
                T1Dly2MnTt,
                T1Dly1MnTt
            };
        }

        /// <summary>
        /// 取得表2替換資料
        /// </summary>
        /// <param name="models"></param>
        /// <param name="delayMonths">落後月數</param>
        /// <returns></returns>
        private List<Dictionary<string, object>> GetListData(List<DelayStatisticsModel> models, int delayMonths)
        {
            models = models
                .OrderBy(x => x.EXEC_ORGAN_NAME)
                .ThenBy(x => x.DELAY_KIND == "D2" ? 0
                    : x.DELAY_KIND == "D1" ? 1
                    : x.DELAY_KIND == "D3" ? 2
                    : 3)
                .ToList();

            List<Dictionary<string, object>> result = new();
            foreach (DelayStatisticsModel model in models)
            {
                mergeData.Add($"{model.EXEC_ORGAN_NAME},{delayMonths}");

                result.Add(new Dictionary<string, object>()
                {
                    {$"T2Org{delayMonths}Mn",$"{model.EXEC_ORGAN_NAME},{delayMonths}"},
                    {$"T2ProjectName{delayMonths}Mn",$"{model.PROJECT_NAME}" },
                    {$"T2DlyKind{delayMonths}Mn", model.DELAY_KIND }
                });
            }

            return result;
        }

        protected override void Other()
        {
            Table targetTable = (Table)Doc.GetChildNodes(NodeType.Table, true)[1];
            foreach (string data in mergeData.Distinct().ToList())
            {
                var dataSet = data.Split(',');
                List<Cell> targetCell = FindCell(targetTable, data);
                // 將 "執行機關,[落後月數]" 替換成執行機關 
                VerticalMergeCells(targetCell, dataSet[0]);
            }
        }
    }
}
