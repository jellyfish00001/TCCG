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
    public class RPTAllProjectOrgResultList : XlsBuilder
    {
        private IPWSRPTDac pwsRptDac;
        private PWSReportModel pwsRptModel;
        private List<RPTBudgeReviewModel> data;

        /// <summary>
        /// 取TempFile檔案
        /// </summary>
        /// <param name="coms"></param>
        public RPTAllProjectOrgResultList(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "AllProjectOrgResultList.xlsx";
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
            data = await pwsRptDac.GetALLORGPlan(pwsRptModel);
        }

        /// <summary>
        /// 製作表單內容
        /// </summary>
        protected override void MakeContent()
        {
            // 設置工作表
            Cells cells = Sheet.Cells;
            Sheet.Name = "全部局處審查結果彙整表";

            // 替換Eexcel內容
            CellReplaceByExcel(new
            {
                YEAR = pwsRptModel.PWS_YEAR,
                LASTYEAR = Convert.ToInt16(pwsRptModel.PWS_YEAR) - 1
            });

            int startRow = 11;
            int index = 1;
            int row = startRow;
            foreach (RPTBudgeReviewModel item in data)
            {
                // 插入一行
                cells.InsertRow(row - 1);

                SetColumn(cells[$"A{row}"], index);                                         // 項次
                SetColumn(cells[$"B{row}"], item.OU_NAME);                                  // 辦理機關
                SetColumn(cells[$"C{row}"], item.PLANCOUNT);                                // 計畫數
                SetColumn(cells[$"D{row}"], item.CROSS_PUBLICMONEY);                        // 跨年度法定公務預算
                SetColumn(cells[$"E{row}"], item.CROSS_FUNDMONEY);                          // 跨年度法定基金預算
                SetColumn(cells[$"F{row}"], item.PUBLICMONEY);                              // 公務預算
                SetColumn(cells[$"G{row}"], item.FUNDMONEY);                                // 基金預算
                SetColumn(cells[$"H{row}"], item.PUBLIC1);                                  // 建議公務預算
                SetColumn(cells[$"I{row}"], item.FUND1);                                    // 建議基金預算
                SetColumn(cells[$"J{row}"], item.PUBLIC2);                                  // 不建議公務預算
                SetColumn(cells[$"K{row}"], item.FUND2);                                    // 不建議基金預算

                row++;
                index++;
            }

            // 在資料結束後，下一行會加入Excel公式來計算總和
            // 確保至少有一行資料被計算
            if (row > startRow)
            {
                cells[$"C{row}"].Formula = $"=SUM(C11:C{row - 1})";
                cells[$"D{row}"].Formula = $"=SUM(D11:D{row - 1})";
                cells[$"E{row}"].Formula = $"=SUM(E11:E{row - 1})";
                cells[$"F{row}"].Formula = $"=SUM(F11:F{row - 1})";
                cells[$"G{row}"].Formula = $"=SUM(G11:G{row - 1})";
                cells[$"H{row}"].Formula = $"=SUM(H11:H{row - 1})";
                cells[$"I{row}"].Formula = $"=SUM(I11:I{row - 1})";
                cells[$"J{row}"].Formula = $"=SUM(J11:J{row - 1})";
                cells[$"K{row}"].Formula = $"=SUM(K11:K{row - 1})";
            }
            // 增加行號以避免覆蓋總計行
            row++;
            // 删除第11行
            cells.DeleteRow(9);
        }

    }
}
