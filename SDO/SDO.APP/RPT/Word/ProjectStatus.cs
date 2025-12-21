using Aspose.Words;
using Aspose.Words.Drawing.Charts;
using Aspose.Words.Tables;
using Autofac;
using SDO.Base.RPT.Enums;
using SDO.Models;
using SDO.ReportBuilder.Models;
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
    /// 表1 案件狀態表、落後案件表
    /// </summary>
    public class ProjectStatus : ProjectStatistics
    {
        public ProjectStatus(IComponentContext coms) : base(coms)
        {
        }

        protected override void Title()
        {
            SetTitle(Parameter.FileName, fontSize: 14, bold: true, alignment: ParagraphAlignment.Center);
            SetSubTitle($"統計日期：{DateTime.Now.ToTwDateString()}", fontSize: 10, alignment: ParagraphAlignment.Right);
        }

        protected override void Content()
        {
            int CLOSE_NUM = 0; // 結案件數
            int CONFORM_NUM = 0; // 進度符合或超前件數
            
            // 狀態表才顯示
            if (Parameter.Type == "Status")
            {
                //已結案
                CLOSE_NUM = execOrganData.Sum(x => x.EXEC_DEPT_B);
                SetSubTitle($"【已結案】計{CLOSE_NUM}案。", fontSize: 14);
                if (CLOSE_NUM > 0)
                {
                    SetHeader("B");
                    SetData(execOrganData.Where(x => x.EXEC_DEPT_B > 0), "B");
                }

                Builder.Writeln(string.Empty);
                //進度符合或超前
                CONFORM_NUM = execOrganData.Sum(x => x.EXEC_DEPT_C);
                SetSubTitle($"【進度符合或超前】計{CONFORM_NUM}案。", fontSize: 14);
                if (CONFORM_NUM > 0)
                {
                    SetHeader("C");
                    SetData(execOrganData.Where(x => x.EXEC_DEPT_C > 0), "C");
                }

                Builder.Writeln(string.Empty);
            }

            //進度落後
            int DELAY_NUM = execOrganData.Sum(x => x.EXEC_DEPT_D); // 落後件數
            SetSubTitle($"【進度落後】計{DELAY_NUM}案。", fontSize: 14);
            SetRemark("代號說明：D1為開工前檢核點進度落後（含非工程類工作檢核點進度落後）；D2為施工進度落後；D3為竣工後檢核點進度落後。", fontSize: 12);
            if (DELAY_NUM > 0)
            {
                SetHeader("D");
                SetData(execOrganData.Where(x => x.EXEC_DEPT_D > 0), "D");
            }
            Builder.Writeln(string.Empty);


            // 狀態表才顯示
            if (Parameter.Type == "Status")
            {
                //開工前預定檢核點進度落後
                int DELAY_D1_NUM = execOrganData.Sum(x => x.EXEC_DEPT_D1);
                SetSubTitle($"【開工前預定檢核點進度落後】計{DELAY_D1_NUM}案。", fontSize: 14);
                if (DELAY_D1_NUM > 0)
                {
                    SetHeader("D1");
                    SetData(execOrganData.Where(x => x.EXEC_DEPT_D1 > 0), "D1");
                }

                Builder.Writeln(string.Empty);
                //施工進度落後
                int DELAY_D2_NUM = execOrganData.Sum(x => x.EXEC_DEPT_D2);
                SetSubTitle($"【施工進度落後】計{DELAY_D2_NUM}案。", fontSize: 14);
                if (DELAY_D2_NUM > 0)
                {
                    SetHeader("D2");
                    SetData(execOrganData.Where(x => x.EXEC_DEPT_D2 > 0), "D2");
                }

                Builder.Writeln(string.Empty);
                //竣工後預定檢核點進度落後
                int DELAY_D3_NUM = execOrganData.Sum(x => x.EXEC_DEPT_D3);
                SetSubTitle($"【竣工後預定檢核點進度落後】計{DELAY_D3_NUM}案。", fontSize: 14);
                if (DELAY_D3_NUM > 0)
                {
                    SetHeader("D3");
                    SetData(execOrganData.Where(x => x.EXEC_DEPT_D3 > 0), "D3");
                }

                Builder.Writeln(string.Empty);
                //撤銷列管件數
                int REVOKE_NUM = execOrganData.Sum(x => x.EXEC_DEPT_E);
                SetSubTitle($"【撤銷列管件數】計{REVOKE_NUM}案。", fontSize: 14);
                if (REVOKE_NUM > 0)
                {
                    SetHeader("E");
                    SetData(execOrganData.Where(x => x.EXEC_DEPT_E > 0), "E");
                }

                Builder.Writeln(string.Empty);
                //圖表
                if (DELAY_NUM > 0)
                {
                    SetColumnChart();
                }
                if (CLOSE_NUM > 0 || CONFORM_NUM > 0 || DELAY_NUM > 0)
                {
                    SetPieChart();
                }
            }

        }

        /// <summary>
        /// 設定表格資料
        /// </summary>
        private void SetData(IEnumerable<ProjectStatisticsModel> data, string status)
        {
            foreach (ProjectStatisticsModel item in data)
            {
                //找出該機關符合狀態的計畫，以計畫筆畫小到大排序
                List<ProjectStatisticsModel> orders = statisticsData.Where(x => x.EXEC_DEPT == item.EXEC_DEPT &&
                (status == "D" ? x.STATUS.StartsWith("D") : x.STATUS == status))
                    .OrderBy(x => x.PROJECT_NAME, StringComparer.Create(
                            new System.Globalization.CultureInfo("zh-TW"),
                            false //是否區分大小寫
                        )).ToList();
                foreach (ProjectStatisticsModel order in orders)
                {
                    if (order.Equals(orders.First()))
                    {
                        SetTdColumn(item.EXEC_DEPT, alignment: AlignmentEnum.Center, vMerge: CellMerge.First);
                        SetTdColumn(orders.Count, alignment: AlignmentEnum.Center, vMerge: CellMerge.First);
                        SetTdColumn(order.PROJECT_NAME, alignment: AlignmentEnum.Left);
                        if (status == "D")
                        {
                            SetTdColumn(order.STATUS, alignment: AlignmentEnum.Center);
                        }
                        SetTdColumn(orders.Sum(x => x.BUDGET_TOTAL), alignment: AlignmentEnum.Right, vMerge: CellMerge.First);
                    }
                    else
                    {
                        SetTdColumn("", vMerge: CellMerge.Previous);
                        SetTdColumn("", vMerge: CellMerge.Previous);
                        SetTdColumn(order.PROJECT_NAME, alignment: AlignmentEnum.Left);
                        if (status == "D")
                        {
                            SetTdColumn(order.STATUS, alignment: AlignmentEnum.Center);
                        }
                        SetTdColumn("", vMerge: CellMerge.Previous);
                    }
                    Builder.EndRow();
                }
            }

            Builder.EndTable();
            List<double> pageWidth = status == "D" ? new() { 16, 5, 48, 7, 24 } : new() { 16, 5, 55, 24 };
            InitTable(Table, pageWidth);

            SetRepeatHeader(new List<int> { 0 });
        }

        /// <summary>
        /// 設定表格標題
        /// </summary>
        private void SetHeader(string status)
        {
            Color backGroundColor = Color.FromArgb(198, 217, 241);
            Table = Builder.StartTable();
            SetThColumn("執行機關", backGroundColor: backGroundColor, isBold: true);
            SetThColumn("案件數", backGroundColor: backGroundColor, isBold: true);
            SetThColumn("案件名稱", backGroundColor: backGroundColor, isBold: true);
            if (status == "D")
            {
                SetThColumn("落後類型", backGroundColor: backGroundColor, isBold: true);
            }
            SetThColumn("總經費", backGroundColor: backGroundColor, isBold: true);
            Builder.EndRow();
        }

        /// <summary>
        /// 設定柱狀圖
        /// </summary>
        private void SetColumnChart()
        {
            InsertChart(new WordChartModel
            {
                ChartType = ChartType.Column,
                Width = 432,
                Height = 252,
                TitleText = "執行進度落後情形",
                Categories = execOrganData.Select(x => x.EXEC_DEPT).ToArray(),
                SeriesColls = new List<WordSeriesCollModel>
                {
                    new WordSeriesCollModel
                    {
                        SeriesName = "進度落後案件",
                        Values = execOrganData.Select(x => (double)x.EXEC_DEPT_D).ToArray(),
                        Format = "#,##0",
                        ShowValue = true
                    }
                },
                LegendPosition = LegendPosition.Bottom
            });
        }

        /// <summary>
        /// 設定圓餅圖
        /// </summary>
        private void SetPieChart()
        {
            // 所有案件類別加總
            double total = execOrganData.Sum(x => x.EXEC_DEPT_B + x.EXEC_DEPT_C + x.EXEC_DEPT_D + x.EXEC_DEPT_E);

            // 各案件類別加總
            double closeSum = execOrganData.Sum(x => x.EXEC_DEPT_B);
            double confirmSum = execOrganData.Sum(x => x.EXEC_DEPT_C);
            double delaySum = execOrganData.Sum(x => x.EXEC_DEPT_D);
            double revokeSum = execOrganData.Sum(x => x.EXEC_DEPT_E);

            InsertChart(new WordChartModel
            {
                ChartType = ChartType.Pie,
                Width = 432,
                Height = 252,
                TitleText = "列管案件狀態",
                Categories = new string[] { $"已結案({closeSum/total:#0.0%})", $"符合或超前({confirmSum / total:#0.0%})",
                    $"進度落後({delaySum/total:#0.0%})", $"撤銷列管({revokeSum/total:#0.0%})" },
                SeriesColls = new List<WordSeriesCollModel>
                {
                    new WordSeriesCollModel
                    {
                        Values = new double[] {
                            execOrganData.Sum(x => x.EXEC_DEPT_B),
                            execOrganData.Sum(x => x.EXEC_DEPT_C),
                            execOrganData.Sum(x => x.EXEC_DEPT_D),
                            execOrganData.Sum(x => x.EXEC_DEPT_E)
                        },
                        Format = "0.0%",
                        ShowPercentage = true
                    }
                },
                LegendPosition = LegendPosition.TopRight,
                LegendOverlay = true
            });
        }
    }
}
