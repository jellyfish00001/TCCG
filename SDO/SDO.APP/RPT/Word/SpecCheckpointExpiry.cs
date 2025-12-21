using Aspose.Words;
using Aspose.Words.Tables;
using Autofac;
using SDO.Base.RPT.Enums;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    public class SpecCheckpointExpiry : WContentBuilder
    {
        private IStatisticsDac statisticsDac;
        private IIPCSetParamService sysParamDac;
        private StatisticsModel statistics;
        private List<SpecCheckpointExpiryModel> data;
        private List<OrgProjectCntModel> orgProjectCntModels;
        private Dictionary<string, string> ctrlPointDic;

        private Color backGroundColor = Color.FromArgb(198, 217, 241);

        public SpecCheckpointExpiry(IComponentContext coms) : base(coms)
        {
            statisticsDac = coms.Resolve<IStatisticsDac>();
            sysParamDac = coms.Resolve<IIPCSetParamService>();
        }

        protected override async Task GetData()
        {
            statistics = Parameter.ObjectModel is null ? new() : (StatisticsModel)(Parameter.ObjectModel);
            List<string> projStatuses = new List<string> { null, "1", "2", "3", "8" };
            data = (await statisticsDac.GetSpecCheckpointExpiry(statistics)).Where(x => !projStatuses.Contains(x.PROJECT_STATUS)).ToList();
            // 取得機關列管計畫件數
            orgProjectCntModels = await statisticsDac.GetExecOrgProjectCnt((StatisticsModel)Parameter.ObjectModel);

            ctrlPointDic = (await sysParamDac.GetSysParams("CTRL_CHK_POINT_TYPE", false))
                .Where(x => statistics.CTRL_CHK_POINT_TYPE.Contains(x.SET_TYPE))
                .OrderBy(x => x.SORT_ORDER)
                .ToDictionary(x => x.SET_TYPE, y => y.SET_VALUE);
        }

        protected override void Title()
        {
            SetTitle(
                Parameter.FileName,
                fontSize: 18,
                alignment: ParagraphAlignment.Center
            );

            SetSubTitle(
                $"統計日期：{DateTime.Now.ToTwDateString()}",
                fontSize: 12,
                alignment: ParagraphAlignment.Right
            );
        }

        protected override void Footer()
        {
            SetRemark("備註：", fontSize: 12);
            SetRemark("一、當月屆期：係指工作項目預定完成日期為當月份。", fontSize: 12, firstLineIndent: 1);
            SetRemark("二、次月屆期：係指工作項目預定完成日期為次月份。", fontSize: 12, firstLineIndent: 1);
        }

        protected override void Content()
        {
            int tableIndex = 1;

            // 設定資料統計表
            tableIndex = SetDataStat(tableIndex);

            // 設定工作項目
            foreach (KeyValuePair<string, string> ctrlPoint in ctrlPointDic)
            {
                tableIndex = SetCtrlPointData(tableIndex, ctrlPoint, true);
                tableIndex = SetCtrlPointData(tableIndex, ctrlPoint, false);
            }

            // 設定明細
            SetDetailData(tableIndex);
        }

        private Dictionary<string, string> GetExecOrganDic(List<SpecCheckpointExpiryModel> specCheckpointExpiries)
        {
             return specCheckpointExpiries
                .OrderBy(x => x.OU_SORT_ORDER)
                .ThenBy(x=>x.PROJECT_NO)
                .Select(x => new { x.EXEC_ORGAN_C, x.EXEC_ORGAN_NAME })
                .Distinct()
                .ToDictionary(x => x.EXEC_ORGAN_C, y => y.EXEC_ORGAN_NAME);
        }

        #region 統計表
        /// <summary>
        /// 設定資料統計表
        /// </summary>
        /// <param name="tableIndex">表格索引</param>
        /// <returns></returns>
        private int SetDataStat(int tableIndex)
        {
            SetSubTitle($"表{tableIndex}  特定工作項目屆期未完成件數統計表");

            Table = Builder.StartTable();
            SetStatHeader();

            Dictionary<string, int> totalDic = GetTotalDic();

            foreach (OrgProjectCntModel model in orgProjectCntModels)
            {
                // 取得此機關資料 條件: 執行機關 = 此機關
                IEnumerable<SpecCheckpointExpiryModel> details = data.Where(x => x.EXEC_ORGAN_C == model.OrgId);

                SetTdColumn(model.OrgName, alignment: AlignmentEnum.Center);
                SetTdColumn(model.ProjectCnt, alignment: AlignmentEnum.Center);
                totalDic["totalCnt"] += model.ProjectCnt;

                foreach (KeyValuePair<string, string> ctrlPoint in ctrlPointDic)
                {
                    // 取得此工作項目資料 條件: 工作項目 = 此項目 & 無實際完成日期
                    IEnumerable<SpecCheckpointExpiryModel> ctrlPointData = details.Where(x => x.CTRL_POINT == ctrlPoint.Key && !x.ACTUAL_ENDDATE.HasValue);

                    // 當月 條件: 預計完成日期
                    DateTime strDate = statistics.YEAR_MONTH_START.Value;
                    DateTime endDate = statistics.YEAR_MONTH_END.Value;
                    int currentCnt = ctrlPointData.Where(x => strDate <= x.ESTIMATED_ENDDATE && x.ESTIMATED_ENDDATE <= endDate).Count();
                    SetTdColumn(currentCnt, alignment: AlignmentEnum.Center);

                    // 次月 條件: 預計完成日期
                    DateTime nextStrDate = strDate.AddMonths(1);
                    DateTime nextEndDate = nextStrDate.AddMonths(1).AddDays(-1);
                    int nextCnt = ctrlPointData.Where(x => nextStrDate <= x.ESTIMATED_ENDDATE && x.ESTIMATED_ENDDATE <= nextEndDate).Count();
                    SetTdColumn(nextCnt, alignment: AlignmentEnum.Center);

                    totalDic[$"{ctrlPoint.Key}_current"] += currentCnt;
                    totalDic[$"{ctrlPoint.Key}_next"] += nextCnt;
                }

                Builder.EndRow();
            }

            // 合計
            SetTdColumn("合計", alignment: AlignmentEnum.Center);
            foreach (KeyValuePair<string, int> total in totalDic)
            {
                SetTdColumn(total.Value, alignment: AlignmentEnum.Center);
            }
            Builder.EndRow();

            Builder.EndTable();

            List<double> colWidths = new List<double>() { 14, 6 };
            int ctrlPointCol = (100 - colWidths.Sum(x => (int)x)) / (ctrlPointDic.Count * 2);
            for (int i = 0; i < ctrlPointDic.Count * 2; i++)
            {
                colWidths.Add(ctrlPointCol);
            }
            InitTable(Table, colWidths);

            SetRepeatHeader(new List<int> { 0, 1, 2 });

            Builder.Writeln(string.Empty);
            return tableIndex + 1;
        }

        /// <summary>
        /// 設定統計表標題
        /// </summary>
        private void SetStatHeader()
        {
            SetThColumn("執行機關", backGroundColor: backGroundColor, vMerge: CellMerge.First);
            SetThColumn("列管\n件數", backGroundColor: backGroundColor, vMerge: CellMerge.First);
            SetThColumn("工作項目類型", backGroundColor: backGroundColor, hMergeCount: ctrlPointDic.Count * 2);
            Builder.EndRow();

            SetThColumn(string.Empty, backGroundColor: backGroundColor, vMerge: CellMerge.Previous);
            SetThColumn(string.Empty, backGroundColor: backGroundColor, vMerge: CellMerge.Previous);
            foreach (KeyValuePair<string, string> ctrlPoint in ctrlPointDic)
            {
                SetThColumn(ctrlPoint.Value, backGroundColor: backGroundColor, hMergeCount: 2);
            }
            Builder.EndRow();

            SetThColumn(string.Empty, backGroundColor: backGroundColor, vMerge: CellMerge.Previous);
            SetThColumn(string.Empty, backGroundColor: backGroundColor, vMerge: CellMerge.Previous);
            for (int i = 0; i < ctrlPointDic.Count; i++)
            {
                SetThColumn("當月\n屆期\n未完成\n件數", backGroundColor: backGroundColor);
                SetThColumn("次月\n屆期\n未完成\n件數", backGroundColor: backGroundColor);
            }
            Builder.EndRow();
        }

        /// <summary>
        /// 取得合計 DIC
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, int> GetTotalDic()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            result.Add("totalCnt", 0);
            foreach (KeyValuePair<string, string> ctrlPoint in ctrlPointDic)
            {
                result.Add($"{ctrlPoint.Key}_current", 0);
                result.Add($"{ctrlPoint.Key}_next", 0);
            }
            return result;
        }
        #endregion 統計表

        /// <summary>
        /// 設定工作項目資料
        /// </summary>
        /// <param name="tableIndex">表格索引</param>
        /// <param name="ctrlPoint">工作項目</param>
        /// <param name="isCurrent">是否當月</param>
        /// <returns></returns>
        private int SetCtrlPointData(int tableIndex, KeyValuePair<string, string> ctrlPoint, bool isCurrent)
        {
            // 取得此工作項目清單 條件: 工作項目 = 此項目 & 無實際完成日期 & 預計完成日期 between 統計年月(1個月)
            DateTime strDate = isCurrent ? statistics.YEAR_MONTH_START.Value : statistics.YEAR_MONTH_START.Value.AddMonths(1);
            DateTime endDate = strDate.AddMonths(1).AddDays(-1);
            List<SpecCheckpointExpiryModel> ctrlPointData = data
                .Where(x =>
                    x.CTRL_POINT == ctrlPoint.Key
                    && !x.ACTUAL_ENDDATE.HasValue
                    && strDate <= x.ESTIMATED_ENDDATE && x.ESTIMATED_ENDDATE <= endDate)
                .ToList();

            Dictionary<string, string> execOrganDic = GetExecOrganDic(ctrlPointData);
            if (!execOrganDic.Any())
            {
                return tableIndex;
            }

            SetSubTitle($"表{tableIndex}  { (isCurrent ? "當月" : "次月") }未完成「{ctrlPoint.Value}」明細表");

            Table = Builder.StartTable();
            SetThColumn("執行機關", backGroundColor: backGroundColor);
            SetThColumn("件數", backGroundColor: backGroundColor);
            SetThColumn("計畫名稱", backGroundColor: backGroundColor);
            SetThColumn("屆期檢核點名稱", backGroundColor: backGroundColor);
            SetThColumn("預定完成日期", backGroundColor: backGroundColor);
            Builder.EndRow();

            foreach (KeyValuePair<string, string> execOrgan in execOrganDic)
            {
                // 取得此機關清單 條件: 執行機關 = 此機關
                IEnumerable<SpecCheckpointExpiryModel> details = ctrlPointData.Where(x => x.EXEC_ORGAN_C == execOrgan.Key).OrderBy(x => x.PROJECT_NO);

                int detailCnt = details.Count();
                foreach (SpecCheckpointExpiryModel item in details)
                {
                    CellMerge vMerge = item.Equals(details.First()) ? CellMerge.First : CellMerge.Previous;
                    SetTdColumn(execOrgan.Value, alignment: AlignmentEnum.Center, vMerge: vMerge);
                    SetTdColumn(detailCnt, alignment: AlignmentEnum.Center, vMerge: vMerge);
                    SetTdColumn(item.PROJECT_NAME);
                    SetTdColumn(item.CHECKITEM_NAME);
                    SetTdColumn(item.ESTIMATED_ENDDATE.ToTwDateString(), alignment: AlignmentEnum.Center);
                    Builder.EndRow();
                }
            }

            Builder.EndTable();
            InitTable(Table, new List<double> { 15, 8, 37, 20, 20 });

            SetRepeatHeader(new List<int> { 0 });

            Builder.Writeln(string.Empty);
            return tableIndex + 1;
        }

        #region 明細資料
        /// <summary>
        /// 設定明細資料
        /// </summary>
        /// <param name="tableIndex">表格索引</param>
        /// <returns></returns>
        private void SetDetailData(int tableIndex)
        {
            SetSubTitle($"表{tableIndex}  計畫當月及次月屆期未完成工作項目類型明細表");

            Table = Builder.StartTable();
            SetDetailHeader();

            // 取得未完成清單 條件: 無實際完成日期
            List<SpecCheckpointExpiryModel> specCheckpointExpirys = data.Where(x => !x.ACTUAL_ENDDATE.HasValue).ToList();
            
            // 當月月初
            DateTime startDT = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            // 次月月底
            DateTime endDT = startDT.AddMonths(2).AddDays(-1);

            // 條件：當月和次月資料；有包含選擇的工作項目；沒有實際完成日期，顯示預計完成日期
            foreach (KeyValuePair<string, string> execOrgan in GetExecOrganDic(specCheckpointExpirys))
            {
                IEnumerable<SpecCheckpointExpiryModel> items = specCheckpointExpirys.
                    Where(x => x.EXEC_ORGAN_C == execOrgan.Key 
                        && startDT <= x.ESTIMATED_ENDDATE 
                        && x.ESTIMATED_ENDDATE <= endDT);

                // 取得此機關清單 條件: 執行機關 = 此機關
                List<string> projNos = items.OrderBy(x => x.ESTIMATED_ENDDATE).Select(x => x.PROJECT_NO).Distinct().ToList();

                foreach (string projNo in projNos)
                {
                    // 取得此計畫清單 條件: 計畫編號 = 此編號
                    IEnumerable<SpecCheckpointExpiryModel> details = items.Where(x => x.PROJECT_NO == projNo);

                    List<string> ctrlPointDates = new List<string>();
                    foreach (KeyValuePair<string, string> ctrlPoint in ctrlPointDic)
                    {
                        SpecCheckpointExpiryModel ctrlPointItem = details.FirstOrDefault(x => x.CTRL_POINT == ctrlPoint.Key);

                        string value = ctrlPointItem != null ? ctrlPointItem.ESTIMATED_ENDDATE.ToTwDateString() : string.Empty;
                        ctrlPointDates.Add(value);
                    }

                    if (ctrlPointDates.Any(x => !string.IsNullOrEmpty(x)))
                    {
                        SetTdColumn(execOrgan.Value, alignment: AlignmentEnum.Center, vMerge: projNo.Equals(projNos.First()) ? CellMerge.First : CellMerge.Previous);
                        SetTdColumn(details.First().PROJECT_NAME);

                        foreach (string date in ctrlPointDates)
                        {
                            SetTdColumn(date, alignment: AlignmentEnum.Center);
                        }

                        Builder.EndRow();
                    }                    
                }
            }

            Builder.EndTable();

            int ctrlPointWidth = 16;
            List<double> colWidths = new List<double>() { 14 };
            colWidths.Add(100 - colWidths[0] - ctrlPointDic.Count * ctrlPointWidth);
            for (int i = 0; i < ctrlPointDic.Count; i++)
            {
                colWidths.Add(ctrlPointWidth);
            }
            InitTable(Table, colWidths);

            SetRepeatHeader(new List<int> { 0, 1 });

            Builder.Writeln(string.Empty);
        }

        /// <summary>
        /// 設定明細資料
        /// </summary>
        private void SetDetailHeader()
        {
            SetThColumn("執行機關", backGroundColor: backGroundColor, vMerge: CellMerge.First);
            SetThColumn("計畫名稱", backGroundColor: backGroundColor, vMerge: CellMerge.First);
            SetThColumn("預定完成日期", backGroundColor: backGroundColor, hMergeCount: ctrlPointDic.Count);
            Builder.EndRow();

            SetThColumn(string.Empty, backGroundColor: backGroundColor, vMerge: CellMerge.Previous);
            SetThColumn(string.Empty, backGroundColor: backGroundColor, vMerge: CellMerge.Previous);
            foreach (KeyValuePair<string, string> ctrlPoint in ctrlPointDic)
            {
                SetThColumn(ctrlPoint.Value, backGroundColor: backGroundColor);
            }
            Builder.EndRow();
        }
        #endregion 明細資料
    }
}
