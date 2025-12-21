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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    /// <summary>
    /// 報表6 檢核點屆期預告
    /// </summary>
    public class CheckpointExpirationNotice : WContentBuilder
    {
        private readonly IStatisticsDac statisticsDac;

        private Dictionary<int, List<string>> tbNeedToMergeDict = new()
        {
            { 1, new List<string>() },
            { 2, new List<string>() },
            { 3, new List<string>() },
            { 4, new List<string>() },
        };

        private Dictionary<int, List<string>> tbNeedToMergeDictForMail = new()
        {
            { 0, new List<string>() },
            { 1, new List<string>() }
        };

        public CheckpointExpirationNotice(IComponentContext coms) : base(coms)
        {
            statisticsDac = coms.Resolve<IStatisticsDac>();
        }

        protected override void SetTemplateFileName()
        {
            StatisticsModel queryModel = (StatisticsModel)Parameter.ObjectModel;
            TemplateFileName = "CheckpointExpirationNoticeRPT.doc";
            // 若為郵件報表，則替換範本
            if (queryModel.ForMail.HasValue && queryModel.ForMail.Value)
            {
                TemplateFileName = "CheckpointExpirationNoticeForMailRPT.doc";
            }
        }

        protected override async Task GetData()
        {
            StatisticsModel queryModel = (StatisticsModel)Parameter.ObjectModel;
            bool isForMail = queryModel.ForMail.HasValue;
            // 取得資料
            List<CheckpointExpiryModel> models = await statisticsDac.GetCheckItemExpiry(queryModel);
            // 取得機關列管計畫件數
            List<OrgProjectCntModel> orgProjectCntModels = await statisticsDac.GetExecOrgProjectCnt(queryModel);

            #region 表一加總資料宣告
            decimal Stat1CountSum = 0; // 列管件數加總
            decimal Stat1TotalSum = 0; // 屆期總件數加總
            #region 當月屆期資料加總
            decimal Stat1CMUSum = 0; // 未完成件數
            decimal Stat1CMCSum = 0; // 已完成件數
            #endregion
            #region 次月屆期資料加總
            decimal Stat1NMUSum = 0; // 未完成件數
            decimal Stat1NMCSum = 0; // 已完成件數
            #endregion
            #endregion

            if (models.Any())
            {
                List<object> tb1 = new(); // 表1資料集
                List<Dictionary<string, object>> tb2 = new(); // 表2資料集(當月屆期未完成)
                List<Dictionary<string, object>> tb3 = new(); // 表3資料集(當月屆期已完成)
                List<Dictionary<string, object>> tb4 = new(); // 表4資料集(次月屆期未完成)
                List<Dictionary<string, object>> tb5 = new(); // 表5資料集(次月屆期已完成)

                // 依據機關OU_SORT_ORDER排序
                List<string> orgs = models.OrderBy(o => o.OU_SORT_ORDER).Select(x => x.EXEC_ORGAN_C).Distinct().ToList();

                foreach (OrgProjectCntModel model in orgProjectCntModels)
                {
                    string org = model.OrgId;
                    List<CheckpointExpiryModel> orgDatas = models.Where(x => x.EXEC_ORGAN_C == org).ToList();
                    // 列管件數加總
                    decimal stat1Count = model.ProjectCnt;
                    Stat1CountSum += stat1Count;

                    List<string> projStatuses = new List<string> { null, "1", "2", "3" };

                    #region 機關當月屆期資料
                    List<CheckpointExpiryModel> expiryCurrMonthDatas = orgDatas
                        .Where(x =>(x.ESTIMATED_ENDDATE.Year - 1911).ToString() == queryModel.STATISTICS_YEAR
                            && x.ESTIMATED_ENDDATE.Month == queryModel.STATISTICS_MONTH
                            // 排除已撤銷的計畫
                            && x.PROJECT_STATUS != "8")
                        .OrderBy(x => x.PROJECT_NO).ThenBy(x => x.ESTIMATED_ENDDATE).ToList();

                    tb2.AddRange(GetListData(expiryCurrMonthDatas, false, true, isForMail)); // 表2資料(當月屆期未完成)
                    if (!isForMail)
                    {
                        tb3.AddRange(GetListData(expiryCurrMonthDatas, true, true));  // 表3資料(當月屆期已完成)
                    }

                    decimal stat1CMU = expiryCurrMonthDatas.Where(x => !x.ACTUAL_ENDDATE.HasValue && !projStatuses.Contains(x.PROJECT_STATUS)).Select(x => x.PROJECT_NO).Distinct().Count(); // 未完成件數
                    Stat1CMUSum += stat1CMU; // 未完成件數加總計算

                    decimal stat1CMC = expiryCurrMonthDatas.Where(x => x.ACTUAL_ENDDATE.HasValue).Select(x => x.PROJECT_NO).Distinct().Count(); // 已完成件數
                    Stat1CMCSum += stat1CMC; // 已完成件數加總計算
                    #endregion

                    #region 機關次月屆期資料
                    List<CheckpointExpiryModel> expiryNextMonthDatas = new();
                    // 若月份為12月則統計隔年1月
                    if (queryModel.STATISTICS_MONTH == 12)
                    {
                        expiryNextMonthDatas = orgDatas
                            .Where(x => x.ESTIMATED_ENDDATE.Year - 1911 == Convert.ToInt32(queryModel.STATISTICS_YEAR) + 1
                                && x.ESTIMATED_ENDDATE.Month == 1
                                // 排除已撤銷的計畫
                                && x.PROJECT_STATUS != "8")
                            .OrderBy(x => x.PROJECT_NO).ThenBy(x => x.ESTIMATED_ENDDATE).ToList();
                    }
                    else
                    {
                        expiryNextMonthDatas = orgDatas
                            .Where(x => x.ESTIMATED_ENDDATE.Year - 1911 == Convert.ToInt32(queryModel.STATISTICS_YEAR) 
                                && x.ESTIMATED_ENDDATE.Month == queryModel.STATISTICS_MONTH + 1
                                // 排除已撤銷的計畫
                                && x.PROJECT_STATUS != "8")
                            .OrderBy(x => x.PROJECT_NO).ThenBy(x => x.ESTIMATED_ENDDATE).ToList();
                    }

                    tb4.AddRange(GetListData(expiryNextMonthDatas, false, false, isForMail)); // 表4資料(次月屆期未完成)
                    if (!isForMail)
                    {
                        tb5.AddRange(GetListData(expiryNextMonthDatas, true, false));  // 表5資料(次月屆期已完成) 
                    }

                    decimal stat1NMU = expiryNextMonthDatas.Where(x => !x.ACTUAL_ENDDATE.HasValue && !projStatuses.Contains(x.PROJECT_STATUS)).Select(x => x.PROJECT_NO).Distinct().Count(); // 未完成件數
                    Stat1NMUSum += stat1NMU; // 未完成件數加總計算

                    decimal stat1NMC = expiryNextMonthDatas.Where(x => x.ACTUAL_ENDDATE.HasValue).Select(x => x.PROJECT_NO).Distinct().Count(); // 已完成件數
                    Stat1NMCSum += stat1NMC; // 已完成件數加總計算
                    #endregion

                    // 屆期總件數
                    decimal stat1Total = stat1CMU + stat1CMC + stat1NMU + stat1NMC;
                    Stat1TotalSum += stat1Total;
                    tb1.Add(new
                    {
                        Stat1Dept = orgDatas.FirstOrDefault()?.EXEC_ORGAN_NAME,
                        Stat1Count = stat1Count, // 列管件數
                        // 檢核點當月屆期
                        Stat1CMU = stat1CMU,
                        Stat1CMC = stat1CMC,
                        Stat1CMT = stat1CMU + stat1CMC,
                        Stat1CMRate = stat1CMU + stat1CMC == 0 ? 0 : Math.Round(stat1CMC / (stat1CMU + stat1CMC) * 100, 1),
                        // 檢核點次月屆期資料
                        Stat1NMU = stat1NMU,
                        Stat1NMC = stat1NMC,
                        Stat1NMT = stat1NMU + stat1NMC,
                        Stat1NMRate = stat1NMU + stat1NMC == 0 ? 0 : Math.Round(stat1NMC / (stat1NMU + stat1NMC) * 100, 1),
                        Stat1Total = stat1Total
                    });
                }

                if (tb1.Any() && !isForMail)
                    ListData.Add(new WordTableData { LIST_DATA = tb1, TABLE_INDEX = 0 });
                if (tb2.Any())
                    ListData.Add(new WordTableData { LIST_DATA = tb2, TABLE_INDEX = isForMail ? 0 : 1 });
                if (tb3.Any() && !isForMail)
                    ListData.Add(new WordTableData { LIST_DATA = tb3, TABLE_INDEX = 2 });
                if (tb4.Any())
                    ListData.Add(new WordTableData { LIST_DATA = tb4, TABLE_INDEX = isForMail ? 1 : 3 });
                if (tb5.Any() && !isForMail)
                    ListData.Add(new WordTableData { LIST_DATA = tb5, TABLE_INDEX = 4 });
            }

            BasicData = new
            {
                TITLE = Parameter.FileName,
                YEAR_MONTH = $"{queryModel.STATISTICS_YEAR}年{queryModel.STATISTICS_MONTH}月",
                DATE = DateTime.Now.ToTwDateString(),
                // 表一當月屆期總計
                Stat1CountSum,
                Stat1CMUSum,
                Stat1CMCSum,
                Stat1CMTSum = Stat1CMUSum + Stat1CMCSum,
                Stat1CMRateSum = Stat1CMUSum + Stat1CMCSum == 0
                    ? 0
                    : Math.Round(Stat1CMCSum / (Stat1CMUSum + Stat1CMCSum) * 100, 1),
                //表一次月屆期總計
                Stat1NMUSum,
                Stat1NMCSum,
                Stat1NMTSum = Stat1NMUSum + Stat1NMCSum,
                Stat1NMRateSum = Stat1NMUSum + Stat1NMCSum == 0
                    ? 0
                    : Math.Round(Stat1NMCSum / (Stat1NMUSum + Stat1NMCSum) * 100, 1),
                Stat1TotalSum
            };
        }

        /// <summary>
        /// 取得表2至表5替換資料
        /// </summary>
        /// <param name="models"></param>
        /// <param name="isFinished"></param>
        /// <param name="isCurrMonth"></param>
        /// <param name="isForMail"></param>
        /// <returns></returns>
        private List<Dictionary<string, object>> GetListData(List<CheckpointExpiryModel> models, bool isFinished, bool isCurrMonth, bool isForMail = false)
        {
            int tbIndex = 0;
            if (isFinished)
            {
                models = models.Where(x => x.ACTUAL_ENDDATE.HasValue).ToList(); // 篩選已完成資料
                tbIndex = isCurrMonth ? 3 : 5;
            }
            else
            {
                models = models.Where(x => !x.ACTUAL_ENDDATE.HasValue).ToList(); // 篩選未完成資料
                tbIndex = isCurrMonth ? 2 : 4;
            }
            List<Dictionary<string, object>> result = new();
            foreach (CheckpointExpiryModel model in models)
            {
                int projectCnt = models.Select(x => x.PROJECT_NO).Distinct().Count();
                // 加入需垂直合併清單
                AddDataNeedtoMerge(tbIndex, new List<string>(){
                    model.EXEC_ORGAN_NAME,$"{model.EXEC_ORGAN_NAME},{projectCnt}"}, isForMail);

                result.Add(new Dictionary<string, object>()
                {
                    { $"Stat{tbIndex}Dept", model.EXEC_ORGAN_NAME },
                    { $"Stat{tbIndex}Count", $"{model.EXEC_ORGAN_NAME},{projectCnt}" },
                    { $"Stat{tbIndex}Name", model.PROJECT_NAME },
                    { $"Stat{tbIndex}CheckItem", model.CHECKITEM_NAME },
                    { $"Stat{tbIndex}EndDate", model.ESTIMATED_ENDDATE.ToTwDateString() },
                    { $"Stat{tbIndex}ActDate", !isFinished ? string.Empty : model.ACTUAL_ENDDATE.ToTwDateString() }
                });
            }

            return result;
        }

        /// <summary>
        /// 加入待垂直合併資料
        /// </summary>
        /// <param name="tbIndex"></param>
        /// <param name="mergeItems"></param>
        /// <param name="isForMail"></param>
        private void AddDataNeedtoMerge(int tbIndex, List<string> mergeItems, bool isForMail)
        {
            foreach (string mergeItem in mergeItems)
            {
                if (isForMail)
                {
                    int index = tbIndex == 2 ? 0 : 1;
                    if (tbNeedToMergeDictForMail[index].IndexOf(mergeItem) == -1)
                        tbNeedToMergeDictForMail[index].Add(mergeItem);
                }
                else
                {
                    if (tbNeedToMergeDict[tbIndex - 1].IndexOf(mergeItem) == -1)
                        tbNeedToMergeDict[tbIndex - 1].Add(mergeItem);
                }
            }

        }

        protected override void Other()
        {
            StatisticsModel queryModel = (StatisticsModel)Parameter.ObjectModel;
            Dictionary<int, List<string>> targetDict = (queryModel.ForMail.HasValue && queryModel.ForMail.Value)
                ? tbNeedToMergeDictForMail
                : tbNeedToMergeDict;
            foreach (KeyValuePair<int, List<string>> dataNeedToMerge in targetDict)
            {
                int tbIndex = dataNeedToMerge.Key;
                if (dataNeedToMerge.Value.Any())
                {
                    Table targetTable = (Table)Doc.GetChildNodes(NodeType.Table, true)[tbIndex];
                    foreach (string targetData in dataNeedToMerge.Value)
                    {
                        var dataSet = targetData.Split(',');
                        List<Cell> targetCell = FindCell(targetTable, targetData);
                        VerticalMergeCells(targetCell, dataSet.Length == 2 ? dataSet[1] : targetData);
                    }
                }
            }
        }
    }
}
