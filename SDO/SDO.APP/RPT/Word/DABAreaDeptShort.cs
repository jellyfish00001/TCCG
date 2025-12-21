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
using System.Threading.Tasks;
using System.Drawing;
using Aspose.Pdf;

namespace SDO.APP.RPT.Word
{
    /// <summary>
    /// 儀表板表11：每月案件地區統計表 詳版
    /// </summary>
    public class DABAreaDeptShort : WContentBuilder
    {
        private readonly IStatisticsDac statisticsDac;
        private readonly IIPCSetParamDac paramDac;
        List<IPCSetParamModel> specNoteDatas;
        List<ProjectAreaDeptDetailedModel> statisticsData;
        List<ProjectAreaDeptDetailedModel> areaData = new();
        StatisticsModel model;
        // 結束時間
        DateTime Endtime = DateTime.Now;

        public DABAreaDeptShort(IComponentContext coms) : base(coms)
        {
            statisticsDac = coms.Resolve<IStatisticsDac>();
            paramDac = coms.Resolve<IIPCSetParamDac>();
        }

        /// <summary>
        /// 取得資料
        /// </summary>
        /// <returns></returns>
        protected override async Task GetData()
        {
            model = (StatisticsModel)Parameter.ObjectModel;
            // 取得統計資料
            statisticsData = await statisticsDac.GetIPCDashBoardProjectAreaDept(model);
            // 結束時間
            Endtime = new DateTime(int.Parse(model.STATISTICS_YEAR) + 1911, model.STATISTICS_MONTH, DateTime.DaysInMonth(int.Parse(model.STATISTICS_YEAR), model.STATISTICS_MONTH));

        }

        /// <summary>
        /// 頁面號資料設定
        /// </summary>
        protected override void Content()
        {
            // 設定頁面設定
            SetOrderData();

            foreach (var item in areaData)
            {
                // 新增區域資料
                AppendAreaData(item);
                Builder.InsertBreak(BreakType.PageBreak);
            }
        }

        /// <summary>
        /// 設定區域資料
        /// </summary>
        private void SetOrderData()
        {
            var areaDict = statisticsData
                .Where(x => !string.IsNullOrEmpty(x.TOWN_C))
                .Select(x => new { x.TOWN_C, x.TOWNNAME })
                .Distinct()
                .ToDictionary(x => x.TOWN_C, y => y.TOWNNAME);

            // 篩選掉areaDict中H00 全市和H01 跨區的資料
            areaDict = areaDict.Where(x => x.Key != "H00" && x.Key != "H01")
                .ToDictionary(x => x.Key, y => y.Value);

            foreach (var item in areaDict)
            {
                // 找出所有符合 TOWN_C 的資料，並且考慮跨區的情況 (TOWN_C = H01 的資料)
                var data = statisticsData.Where(x =>
                    x.TOWN_C == item.Key ||
                    (x.TOWN_C == "H01" && !string.IsNullOrEmpty(x.TOWN_M) && x.TOWN_M.Split(',').Contains(item.Key)) ||
                    x.TOWN_C == "H00"
                ).ToList();
                // 計算區域數量
                var areaNum = data.Count;
                // 計算延遲案件數量
                var areaDelayNum = data.Count(x => x.STATUS.StartsWith("D"));

                areaData.Add(new ProjectAreaDeptDetailedModel
                {
                    TOWN_C = item.Key,
                    TOWNNAME = item.Value,
                    NUM = areaNum,
                    EXS = Math.Round((double)data.Sum(x => x.PROJECT_EXS) / 100000000, 1),
                    DELAY_NUM = areaDelayNum,
                    SORT_ORDER = data.FirstOrDefault()?.SORT_ORDER ?? areaDict.Count + 1
                });
            }

            areaData = areaData.OrderBy(x => x.SORT_ORDER).ToList();
        }

        private void AppendAreaData(ProjectAreaDeptDetailedModel item)
        {
            // 取得與該區域相關的資料
            var relevantData = statisticsData.Where(x =>
                x.TOWN_C == item.TOWN_C ||
                (x.TOWN_C == "H01" && !string.IsNullOrEmpty(x.TOWN_M) && x.TOWN_M.Split(',').Contains(item.TOWN_C)) ||
                x.TOWN_C == "H00"
            ).ToList();

            // (區域)重大建設資料
            Builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Normal;
            Builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            Builder.Font.Size = 20;
            Builder.Font.Bold = false;
            Builder.Writeln($"（{item.TOWNNAME}）重大建設資料");

            // 智發會製表日期
            Builder.ParagraphFormat.Alignment = ParagraphAlignment.Right;
            Builder.Font.Size = 10;
            Builder.Writeln($"智發會製表日期：{DateTime.Now.ToTwDateString("yyy年MM月dd日")}");

            // 資料來源
            Builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            Builder.Font.Size = 12;
            string reportDate = $"{model.STATISTICS_YEAR}年{model.STATISTICS_MONTH}月";
            Builder.Writeln($"資料來源：本府重大建設系統截至{reportDate}底列管資料");

            // 新增表格數據
            string previousYear = (int.Parse(model.STATISTICS_YEAR) - 1).ToString();
            string dateRange = $"{previousYear}年1月1日至{model.STATISTICS_YEAR}年{model.STATISTICS_MONTH}月{DateTime.DaysInMonth(int.Parse(model.STATISTICS_YEAR), model.STATISTICS_MONTH)}日";
            
            // 轉西元年
            int startTear = int.Parse(previousYear) + 1911;

            // 取得近1年已完工的資料
            var completedData = relevantData.Where(x => x.ACT_COM.HasValue &&
                    x.ACT_COM.Value >= new DateTime(startTear, 1, 1) &&
                    x.ACT_COM.Value <= Endtime)
                .OrderByDescending(x => x.ACT_COM).ToList(); // 實際竣工日由近到遠排序

            AppendConditionTable(
                $"一﹑近1年已完工",
                $"（{dateRange}完工）",
                item,
                completedData,
                // 客製化不同表格Title和欄寬大小
                new List<string> { "序號", "計畫名稱", "計畫總經費", "執行\n機關", "實際\n竣工日", "結案日期" },
                colWidths: new List<double> { 5, 30, 20, 10, 15, 15 }
            ) ;

            // 取得進行中的資料
            var inProgressData = relevantData.Where(x => x.ACT_START.HasValue && !x.ACT_COM.HasValue)
                .OrderBy(x => x.EST_COM).ToList(); // 預定竣工日由近到遠排序

            AppendConditionTable(
                "二﹑目前進行中",
                "（已開工尚未竣工，紅色字體為預定竣工日進度落後案件）",
                item,
                inProgressData,
                new List<string> { "序號", "計畫名稱", "計畫總經費", "執行\n機關", "實際\n開工日", "預定\n竣工日", "機關預定\n竣工日" },
                colWidths: new List<double> { 5, 20, 20, 10, 15, 15, 15 }
            );

            // 取得規劃中的資料
            var plannedData = relevantData.Where(x => !x.ACT_START.HasValue)
                .OrderBy(x => x.EST_START).ToList(); // 預定開工日由近到遠排序

            AppendConditionTable(
                "三﹑預計建設項目",
                "（規劃中尚未開工，紅色字體為預定開工日進度落後案件）",
                item,
                plannedData,
                new List<string> { "序號", "計畫名稱", "計畫總經費", "執行機關", "預定\n開工日" },
                colWidths: new List<double> { 5, 40, 20, 10, 20 }
            );
        }

        // 新增表格
        private void AppendConditionTable(string title, string subtitle, ProjectAreaDeptDetailedModel item, List<ProjectAreaDeptDetailedModel> data, List<string> headers, List<double> colWidths)
        {
            // 標題和副標題
            Builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;

            // 插入標題
            Builder.Font.Bold = true;
            Builder.Font.Size = 12;
            Builder.Write(title);

            // 插入副標題
            Builder.Font.Bold = false;
            Builder.Write(subtitle);

            // 插入新行
            Builder.Writeln();

            // 若無資料，則顯示無資料
            if (data.Count == 0)
            {
                Builder.Writeln("無資料");
                // 插入新行
                Builder.Writeln();
                return;
            }

            // 設定表格標題
            SetTableHeader(headers, colWidths);
            Builder.Font.Bold = false;

            int index = 1;
            foreach (var record in data)
            {
                // 重置背景顏色為白色
                Builder.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.White;
                Builder.InsertCell();
                // 序號 置中
                Builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
                Builder.Write(index.ToString());
                Builder.InsertCell();

                Builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
                // 若為延遲案件D開頭，將計畫名稱改為紅色
                if (record.STATUS.StartsWith("先取消此規則"))
                {
                    Builder.Font.Color = System.Drawing.Color.Red;
                }
                else
                {
                    Builder.Font.Color = System.Drawing.Color.Black;
                }
                // 「目前進行中」如果已逾「預定竣工日」，紅色呈現「計畫名稱」
                if ((title.Contains("目前進行中") && record.EST_COM.HasValue && record.EST_COM.Value < Endtime) ||
                    // 「預計建設項目」如果已逾「預定開工日」，紅色呈現「計畫名稱」
                    (title.Contains("預計建設項目") && record.EST_START.HasValue && record.EST_START.Value < Endtime))
                {
                    Builder.Font.Color = System.Drawing.Color.Red;
                }
                else
                {
                    Builder.Font.Color = System.Drawing.Color.Black;
                }
                Builder.Write(record.PROJECT_NAME);
                Builder.Font.Color = System.Drawing.Color.Black;
                Builder.InsertCell();
                Builder.ParagraphFormat.Alignment = ParagraphAlignment.Right;
                Builder.Write(record.PROJECT_EXS.ToString("N0"));
                Builder.InsertCell();
                Builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
                Builder.Write(record.EXEC_DEPT);

                // 根據不同的表格標題插入額外欄位資料
                if (headers.Contains("實際\n竣工日"))
                {
                    Builder.InsertCell();
                    Builder.Write(record.ACT_COM?.ToTwDateString() ?? string.Empty);
                }

                if (headers.Contains("結案日期"))
                {
                    Builder.InsertCell();
                    if (record.ACT_ACPT.HasValue)
                    {
                        Builder.Write(record.ACT_ACPT.ToTwDateString());
                    }
                    else
                    {
                        Builder.Write("驗收中");
                    }
                }

                if (headers.Contains("實際\n開工日"))
                {
                    Builder.InsertCell();
                    Builder.Write(record.ACT_START?.ToTwDateString() ?? string.Empty);
                }

                if (headers.Contains("預定\n竣工日"))
                {
                    Builder.InsertCell();
                    Builder.Write(record.EST_COM?.ToTwDateString() ?? string.Empty);
                }

                if (headers.Contains("機關預定\n竣工日"))
                {
                    Builder.InsertCell();
                    // 插入「機關預定竣工日」欄位，如果狀態是D，則顯示空字串，否則顯示"-"
                    if (record.EST_COM >= Endtime)
                    {
                        Builder.Write("－");
                    }
                    else
                    {
                        Builder.Write(string.Empty);
                    }
                }
                if (headers.Contains("預定\n開工日"))
                {
                    Builder.InsertCell();
                    Builder.Write(record.EST_START?.ToTwDateString() ?? string.Empty);
                }

                Builder.EndRow();
                index++;
            }
            // 結束表格
            Builder.EndTable();
            // 呼叫 InitTable 設定表格欄位寬度，並傳入指定的寬度列表
            InitTable(Table, colWidths);
            // 設置第一行為標題行，確保跨頁時標題行重複顯示
            SetRepeatHeader(new List<int> { 0 });
            // 插入新行
            Builder.Writeln();
        }

        // 設定表格標題
        private Aspose.Words.Tables.Table SetTableHeader(List<string> headers, List<double> colWidths)
        {
            // 初始化表格
            Table = Builder.StartTable();

            // 插入標題欄位
            foreach (var header in headers)
            {
                SetThColumn(header, System.Drawing.Color.FromArgb(189, 213, 237));
            }
            Builder.EndRow();

            // 設定表格寬度為95%
            Table.PreferredWidth = PreferredWidth.FromPercent(95);
            // 將表格靠右對齊
            Table.Alignment = TableAlignment.Right;
            return Table;
        }
    }
}
