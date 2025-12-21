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
    public class ProjectDStatus : ProjectDStatistics
    {
        public ProjectDStatus(IComponentContext coms) : base(coms)
        {
        }

        protected override void Title()
        {
            SetTitle(Parameter.FileName, fontSize: 14, bold: true, alignment: ParagraphAlignment.Center);
            SetSubTitle($"統計日期：{DateTime.Now.ToTwDateString()}", fontSize: 10, alignment: ParagraphAlignment.Right);
        }

        protected override void Content()
        {
            //進度落後
            int DELAY_NUM = DABProjectDelyData.Count; // 落後總經費
            SetSubTitle($"【進度落後】計{DELAY_NUM}案。", fontSize: 14);
            SetRemark("代號說明：D1為開工前檢核點進度落後（含非工程類工作檢核點進度落後）；D2為施工進度落後；D3為竣工後檢核點進度落後。", fontSize: 12);
            if (DELAY_NUM > 0)
            {

                // 設定表格標題
                SetHeader();

                // 設定表格資料
                SetData(DABstatisticsData.Where(x => x.DELAY_NUM > 0));

                // 結束表格
                Builder.EndTable();
            }
            Builder.Writeln(string.Empty);

        }

        /// <summary>
        /// 設定表格資料
        /// </summary>
        private void SetData(IEnumerable<DABCompositeModel> data)
        {

            // 初始化表格寬度設定和重複標題行
            List<double> pageWidth = new() { 16, 5, 48, 7, 24 };
            InitTable(Table, pageWidth);
            SetRepeatHeader(new List<int> { 0 });

            foreach (DABCompositeModel item in data)
            {
                // 以計畫筆畫小到大排序
                List<DABProjectDataModel> orders = DABProjectDelyData.Where(x => x.EXEC_DEPT == item.SET_VALUE)
                   .OrderBy(x => x.PROJECT_NAME, StringComparer.Create(
                           new System.Globalization.CultureInfo("zh-TW"),
                           false //是否區分大小寫
                       )).ToList();
                foreach (DABProjectDataModel order in orders)
                {
                    if (order.Equals(orders.First()))
                    {
                        SetTdColumn(item.SET_VALUE, alignment: AlignmentEnum.Center, vMerge: CellMerge.First);
                        SetTdColumn(orders.Count, alignment: AlignmentEnum.Center, vMerge: CellMerge.First);
                        SetTdColumn(order.PROJECT_NAME, alignment: AlignmentEnum.Left);
                        SetTdColumn(order.DELAY_TYPE, alignment: AlignmentEnum.Center);
                        SetTdColumn(orders.Sum(x => x.BUDGET_TOTAL), alignment: AlignmentEnum.Right, vMerge: CellMerge.First);
                    }
                    else
                    {
                        SetTdColumn("", vMerge: CellMerge.Previous);
                        SetTdColumn("", vMerge: CellMerge.Previous);
                        SetTdColumn(order.PROJECT_NAME, alignment: AlignmentEnum.Left);
                        SetTdColumn(order.DELAY_TYPE, alignment: AlignmentEnum.Center);
                        SetTdColumn("", vMerge: CellMerge.Previous);
                    }
                    Builder.EndRow();
                }
            }

        }

        /// <summary>
        /// 設定表格標題
        /// </summary>
        private void SetHeader()
        {
            Color backGroundColor = Color.FromArgb(198, 217, 241);
            Table = Builder.StartTable();
            SetThColumn("執行機關", backGroundColor: backGroundColor, isBold: true);
            SetThColumn("案件數", backGroundColor: backGroundColor, isBold: true);
            SetThColumn("案件名稱", backGroundColor: backGroundColor, isBold: true);
            SetThColumn("落後類型", backGroundColor: backGroundColor, isBold: true);
            SetThColumn("總經費", backGroundColor: backGroundColor, isBold: true);
            Builder.EndRow();
        }

    }
}
