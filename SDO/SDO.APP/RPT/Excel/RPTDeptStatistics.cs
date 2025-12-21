using Aspose.Cells;
using Autofac;
using Microsoft.Data.SqlClient;
using Renci.SshNet.Messages.Connection;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Excel
{
    /// <summary>
    /// 各年度提案資料清冊
    /// </summary>
    public class RPTDeptStatistics : XlsBuilder
    {
        private IInnStatisticsDac statisticsDac;
        private IProjectTitleDac projectTitleDac;
        private InnStatisticsModel statistics;
        private List<DeptStatisticsModel> data;
        private List<ProjectTitleModel> projectTitle;

        public RPTDeptStatistics(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "RPTDeptStatistics.xlsx";
            statisticsDac = coms.Resolve<IInnStatisticsDac>();
            projectTitleDac = coms.Resolve<IProjectTitleDac>();
        }

        protected override async Task GetData()
        {
            InnStatisticsModel model = (InnStatisticsModel)Parameter.ObjectModel;
            statistics = model;
            projectTitle = await projectTitleDac.GetInnProjectTitle(model.INN_YEAR);
            data = await statisticsDac.GetDeptStatistics(model);
            
        }

        protected override void MakeContent()
        {
            Cells cells = Sheet.Cells;
            Sheet.Name = "各主題機關提案統計表";

            CellReplaceByExcel(new
            {
                INN_YEAR = statistics.INN_YEAR,
            });

            //設主題名稱欄位
            int ColumnIndex = 5;
            foreach (ProjectTitleModel item in projectTitle)
            {
                SetColumn(cells[$"{GetEnColumn(ColumnIndex)}2"], item.CODE_VALUE);
                WorkbookBuilder.SetBorder(cells[$"{GetEnColumn(ColumnIndex)}2"], CellBorderType.Thin, Color.Black);
                ColumnIndex++;

            }
            SetColumn(cells[$"{GetEnColumn(ColumnIndex)}2"], "小計");
            WorkbookBuilder.SetBorder(cells[$"{GetEnColumn(ColumnIndex)}2"], CellBorderType.Thin, Color.Black);

            //設編號、序號、機關別、提案機關欄位
            int strRow = 3;
            int row = strRow;
            int index = 1;
            string prevSponsorOrg = "";
            foreach (DeptStatisticsModel item in data)
            {
                //篩選組織
                if (prevSponsorOrg != item.SPONSOR_ORG)
                {
                    if (index > 1)
                    {
                        cells.InsertRow(row - 1);
                    }
                    SetColumn(cells[$"A{row}"], index);
                    SetColumn(cells[$"B{row}"], item.SQE, SetColor(cells[$"B{row}"], item));
                    SetColumn(cells[$"C{row}"], item.DEPT_TYPE, SetColor(cells[$"C{row}"], item));
                    SetColumn(cells[$"D{row}"], item.SPONSOR_ORG, SetColor(cells[$"D{row}"], item));
                    row++;
                    index++;
                }
                prevSponsorOrg = item.SPONSOR_ORG;

            }
            SetColumn(cells[$"D{row}"], "合計");
            SetData(ColumnIndex);

            // 總合計
            int endRow = index > 1 ? row - 1 : row;
            int subtotalRow = index > 1 ? row : row + 1;
            SetFormula(cells[$"{GetEnColumn(ColumnIndex)}{subtotalRow}"], $"=SUM({GetEnColumn(ColumnIndex)}{strRow}:{GetEnColumn(ColumnIndex)}{endRow})");

        }

        /// <summary>
        /// 設定資料
        /// </summary>
        private void SetData(int sumCoumnIndex)
        {
            Cells cells = Sheet.Cells;

            //設置各主題數量
            int ColumnIndex = 5;
            foreach (ProjectTitleModel item in projectTitle)
            {
                int strRow = 3;
                int row = strRow;
                string prevSponsorOrg = "";
                foreach (DeptStatisticsModel dept in data)
                {   
                    if (prevSponsorOrg != dept.SPONSOR_ORG)
                    {
                        //符合主題
                        if (dept.PROPOSALTYPE_MAIN == item.CODE_VALUE)
                        {
                            SetColumn(cells[$"{GetEnColumn(ColumnIndex)}{row}"], dept.PROPOSALTYPE_COUNT,SetColor(cells[$"{GetEnColumn(ColumnIndex)}{row}"], dept));
                        }
                        else
                        {
                            //數量為0
                            SetColumn(cells[$"{GetEnColumn(ColumnIndex)}{row}"], 0, SetColor(cells[$"{GetEnColumn(ColumnIndex)}{row}"], dept));
                        }
                        
                        WorkbookBuilder.SetBorder(cells[$"{GetEnColumn(ColumnIndex)}{row}"], CellBorderType.Thin, Color.Black);
                        row++;
                    }
                    else
                    {
                        if (dept.PROPOSALTYPE_MAIN == item.CODE_VALUE)
                        {
                            SetColumn(cells[$"{GetEnColumn(ColumnIndex)}{row - 1}"], dept.PROPOSALTYPE_COUNT, SetColor(cells[$"{GetEnColumn(ColumnIndex)}{row-1}"], dept));
                        }
                    }
                    prevSponsorOrg = dept.SPONSOR_ORG;
                }
                WorkbookBuilder.SetBorder(cells[$"{GetEnColumn(ColumnIndex)}{row}"], CellBorderType.Thin, Color.Black);
                SetFormula(cells[$"{GetEnColumn(ColumnIndex)}{row}"], $"=SUM({GetEnColumn(ColumnIndex)}{strRow}:{GetEnColumn(ColumnIndex)}{row - 1})");
                ColumnIndex++;
            }

            //計算各組織合計
            int org_row = 3;
            string prevOrg = "";
            foreach (DeptStatisticsModel item in data)
            {
                ColumnIndex = 5;
                SetFormula(cells[$"{GetEnColumn(sumCoumnIndex)}{org_row}"], 
                    $"=SUM({GetEnColumn(ColumnIndex)}{org_row}:{GetEnColumn(sumCoumnIndex - 1)}{org_row})",
                    SetColor(cells[$"{GetEnColumn(sumCoumnIndex)}{org_row}"], item));
                if (prevOrg != item.SPONSOR_ORG)
                {
                    WorkbookBuilder.SetBorder(cells[$"{GetEnColumn(sumCoumnIndex)}{org_row}"], CellBorderType.Thin, Color.Black);
                    org_row++;
                }
                prevOrg = item.SPONSOR_ORG;
            }
            WorkbookBuilder.SetBorder(cells[$"{GetEnColumn(sumCoumnIndex)}{org_row}"], CellBorderType.Thin, Color.Black);
        }

        /// <summary>
        /// 區公所加上底色
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static Style SetColor(Cell cell, DeptStatisticsModel item)
        {
            Style style = cell.GetStyle();
            style.ForegroundColor = item.DEPT_TYPE.Contains("區公所") ? Color.DarkSeaGreen : Color.White;
            style.Pattern = BackgroundType.Solid;
            return style;
        }
    }
}
