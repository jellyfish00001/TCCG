using Aspose.Cells;
using Autofac;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Services;
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
    /// 表2：每月案件地區統計表 詳版
    /// </summary>
    public class RPTIPCProjectAreaDetailed : XlsBuilder
    {
        private readonly IStatisticsDac statisticsDac;
        private readonly IIPCSetParamDac paramDac;
        // 特殊加註資料
        List<IPCSetParamModel> specNoteDatas;
        // 原始SQL資料
        List<ProjectAreaDeptDetailedModel> statisticsData;
        // 地區資料
        List<ProjectAreaDeptDetailedModel> areaData = new();
        StatisticsModel model;
        public RPTIPCProjectAreaDetailed(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "IPCProjectAreaDetailedRPT.xls";
            statisticsDac = coms.Resolve<IStatisticsDac>();
            paramDac = coms.Resolve<IIPCSetParamDac>();
        }

        protected override async Task GetData()
        {
            model = (StatisticsModel)Parameter.ObjectModel;
            statisticsData = await statisticsDac.GetIPCProjectAreaDeptDetailed(model);
            specNoteDatas = (await paramDac.GetSysParams("SPEC_NOTE", false)).ToList();
        }

        /// <summary>
        /// 取得地區Dic
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetAreaDic()
        {
            return statisticsData
                .Where(x => !string.IsNullOrEmpty(x.TOWN_C))
                .Select(x => new { x.TOWN_C, x.TOWNNAME, x.SORT_ORDER })
                .Distinct()
                .ToDictionary(x => x.TOWN_C, y => y.TOWNNAME);
        }

        /// <summary>
        /// 組地區資料並排序
        /// </summary>
        private void SetOrderData()
        {
            Dictionary<string, string> areaDict = GetAreaDic();
            foreach (KeyValuePair<string, string> item in GetAreaDic())
            {
                List<ProjectAreaDeptDetailedModel> data = statisticsData.Where(x => x.TOWN_C == item.Key).ToList();
                int sortOrder = data.FirstOrDefault() == null ? areaDict.Count + 1 : data.First().SORT_ORDER;
                int areaNum = data.Count;
                int areaDelayNum = data.Where(x => x.STATUS.StartsWith("D")).Count();
                areaData.Add(new ProjectAreaDeptDetailedModel
                {
                    TOWN_C = item.Key,
                    TOWNNAME = item.Value,
                    NUM = areaNum,
                    EXS = Math.Round((double)data.Sum(x => x.PROJECT_EXS) / 100000000, 1),
                    DELAY_NUM = areaDelayNum,
                    SORT_ORDER = sortOrder
                });
            }
            switch (model.SORT_TYPE_2)
            {
                case "A": //依地區
                    areaData = areaData.OrderBy(x => x.SORT_ORDER).ToList();
                    break;
                case "C": //列管件數
                    areaData = areaData.OrderByDescending(x => x.NUM).ToList();
                    break;
                case "D": //總金額
                    areaData = areaData.OrderByDescending(x => x.EXS).ToList();
                    break;
            }
        }

        protected override void MakeContent()
        {
            Sheet.Name = "統計簡表";

            CellReplaceByExcel(new
            {
                YEAR_MONTH = model.YEAR_MONTH_END.ToTwDateString("yyy年M月"),
                DATE = DateTime.Now.ToTwDateString(),
                SPEC_NOTE = model.SPEC_NOTE.Count == 0 ? "重大建設計畫B級管制" :
                    string.Join("、", specNoteDatas.Where(x => model.SPEC_NOTE.Contains(x.SET_TYPE))
                                                   .Select(x => x.SET_VALUE).ToArray())
            });

            // 組地區資料並排序
            SetOrderData();
            // 統計簡表
            SetStatBriefTable();
            // 統計總表
            SetStatGeneralTable();
            // 資料
            SetData();
            // 刪除 #AREA#(#PROJECT_NUM#) 樣板
            Xls.Worksheets.RemoveAt("#AREA#(#PROJECT_NUM#)");
        }

        /// <summary>
        /// 設定統計簡表
        /// </summary>
        private void SetStatBriefTable()
        {
            Sheet = Xls.Worksheets[0];

            int sumA = statisticsData.Count;
            int sumB = statisticsData.Where(x => x.STATUS == "B").Count();
            int sumC = statisticsData.Where(x => x.STATUS == "C").Count();
            int sumD = statisticsData.Where(x => x.STATUS.StartsWith("D")).Count();
            int sumD1 = statisticsData.Where(x => x.STATUS == "D1").Count();
            int sumD2 = statisticsData.Where(x => x.STATUS == "D2").Count();
            int sumD3 = statisticsData.Where(x => x.STATUS == "D3").Count();
            int sumE = statisticsData.Where(x => x.STATUS == "E").Count();
            double rateB = Math.Round((double)sumB / (double)sumA * 100, 1);
            double rateC = Math.Round((double)sumC / (double)sumA * 100, 1);
            double rateD = Math.Round((double)sumD / (double)sumA * 100, 1);
            double rateE = Math.Round((double)sumE / (double)sumA * 100, 1);

            CellReplaceBySheet(new
            {
                YEAR = model.STATISTICS_YEAR,
                TOTAL_NUM = sumA,
                NEW_NUM = statisticsData.Where(x => x.PROJECT_YEAR == Int16.Parse(model.PROJECT_YEAR)).Count(),
                BEFORE_YEAE = Int16.Parse(model.STATISTICS_YEAR) - 1,
                BEFORE_NUM = $"{(model.PROJECT_YEAR_STATUS == "B" ? "持續列管" : "")}{statisticsData.Where(x => x.PROJECT_YEAR <= Int16.Parse(model.PROJECT_YEAR) - 1).Count()}",
                CLOSE_NUM = sumB,
                CLOSE_RATE = rateB,
                CONFORM_NUM = sumC,
                CONFORM_RATE = rateC,
                DELAY_NUM = sumD,
                DELAY_RATE = rateD,
                DELAY_D1_NUM = sumD1,
                DELAY_D2_NUM = sumD2,
                DELAY_D3_NUM = sumD3,
                REVOKE_NUM = sumE,
                REVOKE_RATE = rateE
            });

            Cells cells = Sheet.Cells;

            int strRow = 6;
            int row = strRow;
            int index = 1;
            foreach (ProjectAreaDeptDetailedModel item in areaData)
            {
                if (index > 1)
                {
                    cells.InsertRow(row - 1);
                }

                SetColumn(cells[$"A{row}"], item.TOWNNAME);
                SetColumn(cells[$"B{row}"], item.NUM);
                SetColumn(cells[$"C{row}"], item.EXS);
                SetColumn(cells[$"D{row}"], item.DELAY_NUM);
                SetFormula(cells[$"E{row}"], $"=IF(B{row}=0,0,D{row}/B{row})");
                row++;
                index++;
            }

            // 合計
            int endRow = index > 1 ? row - 1 : row;
            int subtotalRow = index > 1 ? row : row + 1;
            List<string> enColumns = new List<string> { "B", "C", "D" };
            foreach (string enColumn in enColumns)
            {
                SetFormula(cells[$"{enColumn}{subtotalRow}"], $"=SUM({enColumn}{strRow}:{enColumn}{endRow})");
            }

            SetFormula(cells[$"E{subtotalRow}"], $"=IF(B{subtotalRow}=0,0,D{subtotalRow}/B{subtotalRow})");
        }

        /// <summary>
        /// 設定統計總表
        /// </summary>
        private void SetStatGeneralTable()
        {
            Sheet = Xls.Worksheets[1];
            Cells cells = Sheet.Cells;

            int strRow = 5;
            int row = strRow;
            int index = 1;
            foreach (ProjectAreaDeptDetailedModel item in statisticsData)
            {
                if (index > 1)
                {
                    cells.InsertRow(row - 1);
                }

                SetColumn(cells[$"A{row}"], index);
                SetColumn(cells[$"B{row}"], item.PROJECT_NAME, SetColor(cells[$"B{row}"], item));
                SetColumn(cells[$"C{row}"], item.CHECKPOINT_CLASS);
                SetColumn(cells[$"D{row}"], item.SPEC_NOTE);
                SetColumn(cells[$"E{row}"], item.MASTER_DEPT);
                SetColumn(cells[$"F{row}"], item.EXEC_DEPT);
                SetColumn(cells[$"G{row}"], item.TOWNNAME);
                SetColumn(cells[$"H{row}"], item.PROJECT_EXS);
                SetColumn(cells[$"I{row}"], item.ACT_START.HasValue
                    ? $"{item.ACT_START.ToTwDateString()}\n已開工"
                    : item.EST_START.ToTwDateString());
                SetColumn(cells[$"J{row}"], item.ACT_COM.HasValue
                    ? $"{item.ACT_COM.ToTwDateString()}\n已竣工"
                    : item.EST_COM.ToTwDateString());
                SetColumn(cells[$"K{row}"], $"預定：\n{item.RES_CHECKITEM}\n實際：\n{item.ACT_CHECKITEM}");
                SetColumn(cells[$"L{row}"], $"預定：{item.IPC_RES_PRG:N1}\n實際：{item.IPC_ACT_PRG:N1}\n差異：{(item.IPC_ACT_PRG - item.IPC_RES_PRG):N1}%");
                SetColumn(cells[$"M{row}"], item.EXECUTE_CONDITION);
                SetColumn(cells[$"N{row}"], item.DELAY_CAUSAL);
                SetColumn(cells[$"O{row}"], item.DELAY_TYPE);
                SetColumn(cells[$"P{row}"], item.PROJECT_STATUS == "8" || item.PROJECT_STATUS == "7"
                    ? $"{item.FINISH_DATE.ToTwDateString()}\n{(item.PROJECT_STATUS == "8" ? "已撤銷" : "已結案")}"
                    : item.EST_ACPT.ToTwDateString());
                row++;
                index++;
            }

            // 合計
            int endRow = index > 1 ? row - 1 : row;
            int subtotalRow = index > 1 ? row : row + 1;
            SetFormula(cells[$"H{subtotalRow}"], $"=SUM(H{strRow}:H{endRow})");
        }

        /// <summary>
        /// 設定資料
        /// </summary>
        private void SetData()
        {
            foreach (ProjectAreaDeptDetailedModel item in areaData)
            {
                IEnumerable<ProjectAreaDeptDetailedModel> data = statisticsData.Where(x => x.TOWN_C == item.TOWN_C);
                List<ProjectAreaDeptDetailedModel> cp0Data = data.Where(x => x.CP_KIND == "0").ToList();
                List<ProjectAreaDeptDetailedModel> cp1Data = data.Where(x => x.CP_KIND == "1").ToList();

                Sheet = Xls.Worksheets[Xls.Worksheets.AddCopy("#AREA#(#PROJECT_NUM#)")];
                Sheet.Name = $"{item.TOWNNAME}({cp0Data.Count + cp1Data.Count})";

                CellReplaceBySheet(new
                {
                    AREA = item.TOWNNAME,
                    CP_KIND_0_NUM = cp0Data.Count,
                    CP_KIND_1_NUM = cp1Data.Count
                });

                Cells cells = Sheet.Cells;

                int cp0StrRow = 6;
                int row = cp0StrRow;
                int cp0Row = row;
                int cp0Index = 1;
                if (!cp0Data.Any())
                {
                    cells.DeleteRow(cp0Row - 1);
                }
                foreach (ProjectAreaDeptDetailedModel cp0 in cp0Data)
                {
                    if (cp0Index > 1)
                    {
                        cells.InsertRow(cp0Row - 1);
                    }

                    SetColumn(cells[$"A{cp0Row}"], cp0Index);
                    SetColumn(cells[$"B{cp0Row}"], cp0.PROJECT_NAME, SetColor(cells[$"B{cp0Row}"], cp0));
                    SetColumn(cells[$"C{cp0Row}"], cp0.CHECKPOINT_CLASS);
                    SetColumn(cells[$"D{cp0Row}"], cp0.SPEC_NOTE);
                    SetColumn(cells[$"E{cp0Row}"], cp0.MASTER_DEPT);
                    SetColumn(cells[$"F{cp0Row}"], cp0.EXEC_DEPT);
                    SetColumn(cells[$"G{cp0Row}"], cp0.PROJECT_EXS);
                    SetColumn(cells[$"H{cp0Row}"], cp0.ACT_START.HasValue
                        ? $"{cp0.ACT_START.ToTwDateString()}\n已開工"
                        : cp0.EST_START.ToTwDateString());
                    SetColumn(cells[$"I{cp0Row}"], cp0.ACT_COM.HasValue
                        ? $"{cp0.ACT_COM.ToTwDateString()}\n已竣工"
                        : cp0.EST_COM.ToTwDateString());
                    SetColumn(cells[$"J{cp0Row}"], $"預定：\n{cp0.RES_CHECKITEM}\n實際：\n{cp0.ACT_CHECKITEM}");
                    SetColumn(cells[$"K{cp0Row}"], $"預定：{cp0.IPC_RES_PRG:N1}\n實際：{cp0.IPC_ACT_PRG:N1}\n差異：{(cp0.IPC_ACT_PRG - cp0.IPC_RES_PRG):N1}%");
                    SetColumn(cells[$"L{cp0Row}"], cp0.EXECUTE_CONDITION);
                    SetColumn(cells[$"M{cp0Row}"], cp0.DELAY_CAUSAL);
                    SetColumn(cells[$"N{cp0Row}"], cp0.DELAY_TYPE);
                    SetColumn(cells[$"O{cp0Row}"], cp0.PROJECT_STATUS == "8" || cp0.PROJECT_STATUS == "7"
                        ? $"{cp0.FINISH_DATE.ToTwDateString()}\n{(cp0.PROJECT_STATUS == "8" ? "已撤銷" : "已結案")}"
                        : cp0.EST_ACPT.ToTwDateString());

                    cp0Row++;
                    cp0Index++;
                }

                int cp1StrRow = row + (cp0Data.Any() ? cp0Data.Count + 1 : 1);
                int cp1Row = cp1StrRow;
                int cp1Index = 1;
                if (!cp1Data.Any())
                {
                    cells.DeleteRow(cp1Row - 1);
                }
                foreach (ProjectAreaDeptDetailedModel cp1 in cp1Data)
                {
                    if (cp1Index > 1)
                    {
                        cells.InsertRow(cp1Row - 1);
                    }

                    SetColumn(cells[$"A{cp1Row}"], cp0Index + cp1Index - 1);
                    SetColumn(cells[$"B{cp1Row}"], cp1.PROJECT_NAME, SetColor(cells[$"B{cp1Row}"], cp1));
                    SetColumn(cells[$"C{cp1Row}"], cp1.CHECKPOINT_CLASS);
                    SetColumn(cells[$"D{cp1Row}"], cp1.SPEC_NOTE);
                    SetColumn(cells[$"E{cp1Row}"], cp1.MASTER_DEPT);
                    SetColumn(cells[$"F{cp1Row}"], cp1.EXEC_DEPT);
                    SetColumn(cells[$"G{cp1Row}"], cp1.PROJECT_EXS);
                    SetColumn(cells[$"H{cp1Row}"], $"{cp1.EST_START.ToTwDateString()}{(cp1.ACT_START.HasValue ? "\n已開工" : "")}");
                    SetColumn(cells[$"I{cp1Row}"], $"{cp1.EST_COM.ToTwDateString()}{(cp1.ACT_COM.HasValue ? "\n已竣工" : "")}");
                    SetColumn(cells[$"J{cp1Row}"], $"預定:{cp1.CHECKITEM_NAME}{cp1.ESTIMATED_ENDDATE.ToTwDateString()}\n實際:{cp1.CHECKITEM_NAME}{cp1.ACTUAL_ENDDATE.ToTwDateString()}");
                    SetColumn(cells[$"K{cp1Row}"], $"預定:{cp1.IPC_RES_PRG:N1}\n實際:{cp1.IPC_ACT_PRG:N1}\n差異:{(cp1.IPC_ACT_PRG - cp1.IPC_RES_PRG):N1}%");
                    SetColumn(cells[$"L{cp1Row}"], cp1.EXECUTE_CONDITION);
                    SetColumn(cells[$"M{cp1Row}"], cp1.DELAY_CAUSAL);
                    SetColumn(cells[$"N{cp1Row}"], cp1.DELAY_TYPE);
                    SetColumn(cells[$"O{cp1Row}"], cp1.PROJECT_STATUS == "8" || cp1.PROJECT_STATUS == "7"
                        ? $"{cp1.FINISH_DATE.ToTwDateString()}\n{(cp1.PROJECT_STATUS == "8" ? "已撤銷" : "已結案")}"
                        : cp1.EST_ACPT.ToTwDateString());
                    cp1Row++;
                    cp1Index++;
                }

                // 合計
                SetColumn(cells[$"G{ cp1Row }"], (cp0Data.Sum(x => x.PROJECT_EXS) + cp1Data.Sum(x => x.PROJECT_EXS)));
            }
        }

        /// <summary>
        /// 依據落後案件設定文字顏色
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static Style SetColor(Cell cell, ProjectAreaDeptShortModel item)
        {
            Style style = cell.GetStyle();
            style.Font.Color = item.STATUS.StartsWith("D") ? Color.Red : Color.Black;
            return style;
        }
    }
}
