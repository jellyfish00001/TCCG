using Aspose.Cells;
using Autofac;
using Renci.SshNet.Common;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Excel
{
    /// <summary>
    /// 儀表板表3：各區落後報表
    /// </summary>
    public class RPTIPCDProjectAreaDelay : XlsBuilder
    {
        private readonly IStatisticsDac statisticsDac;
        private readonly IIPCSetParamDac paramDac;
        // 特殊加註資料
        List<IPCSetParamModel> specNoteDatas;
        // 原始SQL資料
        List<ProjectAreaDeptDetailedModel> statisticsData;
        // 地區資料
        List<ProjectAreaDeptDetailedModel> areaData = new();
        // 統計資料
        StatisticsModel model;
        // 上一筆是否有資料
        bool isAnyValue = false;

        public RPTIPCDProjectAreaDelay(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "IPCDABAreaDetailRPT.xls";
            statisticsDac = coms.Resolve<IStatisticsDac>();
            paramDac = coms.Resolve<IIPCSetParamDac>();
        }

        protected override async Task GetData()
        {
            model = (StatisticsModel)Parameter.ObjectModel;
            statisticsData = await statisticsDac.GetIPCDashBoardProjectAreaDept(model);
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
                .Select(x => new { x.TOWN_C, x.TOWNNAME })
                .Distinct()
                .ToDictionary(x => x.TOWN_C, y => y.TOWNNAME);
        }

        /// <summary>
        /// 組地區資料並排序
        /// </summary>
        private void SetOrderData()
        {
            Dictionary<string, string> areaDict = GetAreaDic();
            foreach (KeyValuePair<string, string> item in areaDict)
            {
                List<ProjectAreaDeptDetailedModel> data = statisticsData.Where(x => x.TOWN_C == item.Key).ToList();
                int areaNum = data.Count;
                int areaDelayNum = data.Where(x => x.STATUS.StartsWith("D")).Count();
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

            // 所有資料依地區排序
            areaData = areaData.OrderBy(x => x.SORT_ORDER).ToList();
        }

        protected override void MakeContent()
        {
            CellReplaceByExcel(new
            {
                YEAR_MONTH = model.YEAR_MONTH_END.ToTwDateString("yyy年M月"),
                DATE = DateTime.Now.ToTwDateString(),
                SPEC_NOTE = "各區重大重進度(詳表)"
            });

            // 組地區資料並排序
            SetOrderData();
            // 統計總表
            SetStatGeneralTable();
            // 資料
            SetData();
            // 刪除 #AREA#(#PROJECT_NUM#) 樣板
            Xls.Worksheets.RemoveAt("#AREA#(#PROJECT_NUM#)");
        }

        /// <summary>
        /// 設定統計總表
        /// </summary>
        private void SetStatGeneralTable()
        {
            Sheet = Xls.Worksheets[0];
            Cells cells = Sheet.Cells;

            // 施工中
            List<ProjectAreaDeptDetailedModel> underConstructionData = statisticsData.Where(x => x.ACT_START.HasValue && !x.EST_COM.HasValue).ToList();
            // 尚未開工
            List<ProjectAreaDeptDetailedModel> notStartedData = statisticsData.Where(x => !x.ACT_START.HasValue).ToList();
            // 近一年已完工
            List<ProjectAreaDeptDetailedModel> completedWithinYearData = statisticsData.Where(x => x.ACT_COM.HasValue && x.ACT_COM.Value >= DateTime.Now.AddYears(-1)).ToList();
            // 前一年度至今已完工案件
            List<ProjectAreaDeptDetailedModel> completedSinceLastYearData = statisticsData.Where(x => x.ACT_COM.HasValue && x.ACT_COM.Value >= new DateTime(DateTime.Now.Year - 1, 1, 1)).ToList();
            // 當年度應完工案件數
            List<ProjectAreaDeptDetailedModel> dueThisYearData = statisticsData.Where(x => x.EST_COM.HasValue && x.EST_COM.Value.Year == DateTime.Now.Year).ToList();

            CellReplaceBySheet(new
            {
                // 施工件數
                UNDER_CONSTRUCTION_NUM = underConstructionData.Count,
                // 施工落後件數
                UNDER_CONSTRUCTION_NUM_D = underConstructionData.Where(x => x.STATUS.StartsWith("D")).Count(),
                // 尚未開工件數
                NOT_STARTED_NUM = notStartedData.Count,
                // 尚未開工落後件數
                NOT_STARTED_NUM_D = notStartedData.Where(x => x.STATUS.StartsWith("D")).Count(),
                // 近一年已完工件數
                COMPLETED_WITHIN_YEAR_NUM = completedWithinYearData.Count,
                // 近一年已完工落後件數
                COMPLETED_WITHIN_YEAR_NUM_D = completedWithinYearData.Where(x => x.STATUS.StartsWith("D")).Count(),
                // 前一年度至今已完工件數
                COMPLETED_SINCE_LAST_YEAR_NUM = completedSinceLastYearData.Count,
                // 當年度應完工案件數
                DUE_THIS_YEAR_NUM = dueThisYearData.Count,
                // 當年度應完工落後案件數
                DUE_THIS_YEAR_NUM_DOWN = dueThisYearData.Where(x => x.ACT_COM.HasValue).Count()
            });

            int currentRow = 3;
            isAnyValue = false;

            // 施工中
            currentRow = AddDataToSheet(cells, statisticsData.Where(x => x.ACT_START.HasValue && !x.EST_COM.HasValue).ToList(), currentRow);

            // 尚未開工
            currentRow = AddDataToSheet(cells, statisticsData.Where(x => !x.ACT_START.HasValue).ToList(), currentRow);

            // 近一年已完工
            currentRow = AddDataToSheet(cells, statisticsData.Where(x => x.ACT_COM.HasValue && x.ACT_COM.Value >= DateTime.Now.AddYears(-1)).ToList(), currentRow);

            // 前一年度至今已完工案件
            currentRow = AddDataToSheet(cells, statisticsData.Where(x => x.ACT_COM.HasValue && x.ACT_COM.Value >= new DateTime(DateTime.Now.Year - 1, 1, 1)).ToList(), currentRow);

            // 當年度應完工案件數
            currentRow = AddDataToSheet(cells, statisticsData.Where(x => x.EST_COM.HasValue && x.EST_COM.Value.Year == DateTime.Now.Year).ToList(), currentRow);
        }

        /// <summary>
        /// 設定資料
        /// </summary>
        private void SetData()
        {
            foreach (ProjectAreaDeptDetailedModel item in areaData)
            {
                IEnumerable<ProjectAreaDeptDetailedModel> data = statisticsData.Where(x => x.TOWN_C == item.TOWN_C);
                // 上一筆是否有資料
                isAnyValue = false;
                // 施工中
                List<ProjectAreaDeptDetailedModel> underConstructionData = data.Where(x => x.ACT_START.HasValue && !x.EST_COM.HasValue).ToList();
                // 尚未開工
                List<ProjectAreaDeptDetailedModel> notStartedData = data.Where(x => !x.ACT_START.HasValue).ToList();
                // 近一年已完工
                List<ProjectAreaDeptDetailedModel> completedWithinYearData = data.Where(x => x.ACT_COM.HasValue && x.ACT_COM.Value >= DateTime.Now.AddYears(-1)).ToList();
                // 前一年度至今已完工案件
                List<ProjectAreaDeptDetailedModel> completedSinceLastYearData = data.Where(x => x.ACT_COM.HasValue && x.ACT_COM.Value >= new DateTime(DateTime.Now.Year - 1, 1, 1)).ToList();
                // 當年度應完工案件數
                List<ProjectAreaDeptDetailedModel> dueThisYearData = data.Where(x => x.EST_COM.HasValue && x.EST_COM.Value.Year == DateTime.Now.Year).ToList();

                Sheet = Xls.Worksheets[Xls.Worksheets.AddCopy("#AREA#(#PROJECT_NUM#)")];
                Sheet.Name = $"{item.TOWNNAME}({underConstructionData.Count + notStartedData.Count + completedWithinYearData.Count + completedSinceLastYearData.Count + dueThisYearData.Count})";

                CellReplaceBySheet(new
                {
                    AREA = item.TOWNNAME,
                    // 施工件數
                    UNDER_CONSTRUCTION_NUM = underConstructionData.Count,
                    // 施工落後件數
                    UNDER_CONSTRUCTION_NUM_D = underConstructionData.Where(x => x.STATUS.StartsWith("D")).Count(),
                    // 尚未開工件數
                    NOT_STARTED_NUM = notStartedData.Count,
                    // 尚未開工落後件數
                    NOT_STARTED_NUM_D = notStartedData.Where(x => x.STATUS.StartsWith("D")).Count(),
                    // 近一年已完工件數
                    COMPLETED_WITHIN_YEAR_NUM = completedWithinYearData.Count,
                    // 近一年已完工落後件數
                    COMPLETED_WITHIN_YEAR_NUM_D = completedWithinYearData.Where(x => x.STATUS.StartsWith("D")).Count(),
                    // 前一年度至今已完工件數
                    COMPLETED_SINCE_LAST_YEAR_NUM = completedSinceLastYearData.Count,
                    // 當年度應完工案件數
                    DUE_THIS_YEAR_NUM = dueThisYearData.Count,
                    // 當年度應完工落後案件數
                    DUE_THIS_YEAR_NUM_DOWN = dueThisYearData.Where(x => x.ACT_COM.HasValue).Count()
                });

                Cells cells = Sheet.Cells;

                int currentRow = 3;

                // 施工中
                currentRow = AddDataToSheet(cells, underConstructionData, currentRow);

                // 尚未開工
                currentRow = AddDataToSheet(cells, notStartedData, currentRow);

                // 近一年已完工
                currentRow = AddDataToSheet(cells, completedWithinYearData, currentRow);

                // 前一年度至今已完工案件
                currentRow = AddDataToSheet(cells, completedSinceLastYearData, currentRow);

                // 當年度應完工案件數
                currentRow = AddDataToSheet(cells, dueThisYearData, currentRow);
            }
        }

        private int AddDataToSheet(Cells cells, List<ProjectAreaDeptDetailedModel> data, int startRow)
        {
            // 無資料則不顯示並跳行
            if (!data.Any())
            {
                if (isAnyValue)
                {
                    cells.DeleteRow(startRow - 1);
                    startRow--;
                }
                else
                {
                    cells.DeleteRow(startRow);
                }
                isAnyValue = false;
                return startRow + 1;
            }
            else
            {
                if (!isAnyValue) { startRow++; }
                int row = startRow;
                int index = 1;
                foreach (ProjectAreaDeptDetailedModel item in data)
                {
                    if (index > 1)
                    {
                        cells.InsertRow(row - 1);
                    }

                    SetColumn(cells[$"A{row}"], index);
                    SetColumn(cells[$"B{row}"], item.PROJECT_NO);
                    SetColumn(cells[$"C{row}"], item.PROJECT_NAME);
                    SetColumn(cells[$"D{row}"], item.EXEC_DEPT);
                    SetColumn(cells[$"E{row}"], item.PROJECT_EXS);
                    SetColumn(cells[$"F{row}"], item.PROJECT_STATUS_NOTE);
                    SetColumn(cells[$"G{row}"], item.TOWNNAME);
                    SetColumn(cells[$"H{row}"], item.EST_START.ToTwDateString());
                    SetColumn(cells[$"I{row}"], item.ACT_START.ToTwDateString());
                    SetColumn(cells[$"J{row}"], item.EST_COM.ToTwDateString());
                    SetColumn(cells[$"K{row}"], item.ACT_COM.ToTwDateString());
                    SetColumn(cells[$"L{row}"], $"預定：{item.IPC_RES_PRG:N1}\n實際：{item.IPC_ACT_PRG:N1}\n差異：{(item.IPC_ACT_PRG - item.IPC_RES_PRG):N1}%");
                    SetColumn(cells[$"M{row}"], item.DELAY_TYPE_NOTE);
                    SetColumn(cells[$"N{row}"], item.FINISH_DATE.ToTwDateString());

                    row++;
                    index++;
                    isAnyValue = true;
                }

                return row + 1;
            }
        }
    }
}

