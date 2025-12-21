using Aspose.Cells;
using Autofac;
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

namespace SDO.APP.RPT.Excel
{
    /// <summary>
    /// 各年度提案數統計表
    /// </summary>
    public class RPTPolicyPublicBudgetReview : XlsBuilder

    {
        private IPWSRPTDac pwsRptDac;
        private PWSReportModel pwsRptModel;
        private List<RPTBudgeReviewModel> data;

        /// <summary>
        /// 取TempFile檔案
        /// </summary>
        /// <param name="coms"></param>
        public RPTPolicyPublicBudgetReview(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "PolicyPublicBudgetReview.xlsx";
            pwsRptDac = coms.Resolve<IPWSRPTDac>();
        }

        /// <summary>
        /// 取得表單資料
        /// </summary>
        /// <returns></returns>
        protected override async Task GetData()
        {
            PWSReportModel model = (PWSReportModel)Parameter.ObjectModel;
            pwsRptModel = model;
            pwsRptModel.BUDGETTYPE = "1";
            data = await pwsRptDac.GetPlanReview(pwsRptModel);
        }

        /// <summary>
        /// 製作表單內容
        /// </summary>
        protected override void MakeContent()
        {
            // 設置工作表
            Cells cells = Sheet.Cells;
            Sheet.Name = "公務預算審查結果彙整表";

            // 替換Eexcel內容
            CellReplaceByExcel(new
            {
                YEAR = pwsRptModel.PWS_YEAR,
                LASTYEAR = Convert.ToInt16(pwsRptModel.PWS_YEAR)-1,
                OU_NAME = pwsRptModel.OU_NAME == null ? "" : pwsRptModel.OU_NAME
            });

            int startRow = 12;
            int row = startRow;
            foreach (RPTBudgeReviewModel item in data)
            {
                // 插入一行
                cells.InsertRow(row - 1);

                SetColumn(cells[$"A{row}"], item.PLANORDERNUMBER);                          // 優先順序
                SetColumn(cells[$"B{row}"], $"{item.ORGOUNAME} ({item.UNITOUNAME})");       // 提報機關(單位)
                SetColumn(cells[$"C{row}"], item.PLANNAME);                                 // 計畫名稱
                SetColumn(cells[$"D{row}"], item.CROSS_PUBLICMONEY);                        // 跨年度法定公務預算
                SetColumn(cells[$"E{row}"], item.CROSS_FUNDMONEY);                          // 跨年度法定基金預算
                SetColumn(cells[$"F{row}"], item.PUBLICMONEY);                              // 公務預算
                SetColumn(cells[$"G{row}"], item.FUNDMONEY);                                // 基金預算

                row++;
            }

            // 在資料結束後，下一行會加入Excel公式來計算總和
            // 確保至少有一行資料被計算
            if (row > startRow)
            {
                cells[$"C{row}"].Formula = $"=COUNTA(C11:C{row - 1})";
                cells[$"D{row}"].Formula = $"=SUM(D11:D{row - 1})";
                cells[$"E{row}"].Formula = $"=SUM(E11:E{row - 1})";
                cells[$"F{row}"].Formula = $"=SUM(F11:F{row - 1})";
                cells[$"G{row}"].Formula = $"=SUM(G11:G{row - 1})";
            }
            // 增加行號以避免覆蓋總計行
            row++;
            // 删除第11行
            cells.DeleteRow(10);
        }
    }
}
