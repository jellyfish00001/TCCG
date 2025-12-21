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
    public class RPTProjectEntOrg : XlsBuilder
    {
        private IPWSRPTDac pwsRptDac;
        private PWSReportModel pwsRptModel;
        private List<RPTBudgeReviewModel> data;

        /// <summary>
        /// 取TempFile檔案
        /// </summary>
        /// <param name="coms"></param>
        public RPTProjectEntOrg(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "ProjectEntOrg.xlsx";
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
            //pwsRptModel.BUDGETTYPE = "2";
            data = await pwsRptDac.GetEntPlanOrg(pwsRptModel);
        }


        /// <summary>
        /// 製作表單內容
        /// </summary>
        protected override void MakeContent()
        {
            // 設置工作表
            Cells cells = Sheet.Cells;
            Sheet.Name = "委託研究計畫審查結果彙整表(機關)";

            // 替換Eexcel內容
            CellReplaceByExcel(new
            {
                YEAR = pwsRptModel.PWS_YEAR,
                LASTYEAR = Convert.ToInt16(pwsRptModel.PWS_YEAR) - 1
            });

            int startRow = 7;
            int row = startRow;
            foreach (RPTBudgeReviewModel item in data)
            {
                // 插入一行
                cells.InsertRow(row - 1);

                SetColumn(cells[$"A{row}"], item.PLANORDERNUMBER);                          // 優先順序
                SetColumn(cells[$"B{row}"], item.ORGOUNAME);                                // 提報機關名稱
                SetColumn(cells[$"C{row}"], item.PLANNAME);                                 // 計畫名稱
                SetColumn(cells[$"D{row}"], item.CROSS_FUNDMONEY);                          // 跨年度法定預算
                SetColumn(cells[$"E{row}"], item.FUNDMONEY);                                // 需求數

                row++;
            }

            // 在資料結束後，下一行會加入Excel公式來計算總和
            // 確保至少有一行資料被計算
            if (row > startRow)
            {
                cells[$"C{row}"].Formula = $"=COUNTA(C6:C{row - 1})";
                cells[$"D{row}"].Formula = $"=SUM(D6:D{row - 1})";
                cells[$"E{row}"].Formula = $"=SUM(E6:E{row - 1})";
            }
            // 增加行號以避免覆蓋總計行
            row++;
            // 删除第1行
            cells.DeleteRow(5);
        }

    }
}
