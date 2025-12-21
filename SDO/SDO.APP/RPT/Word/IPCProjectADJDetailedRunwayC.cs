using Aspose.Words;
using Aspose.Words.Tables;
using Autofac;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.APP.IPC.Models.Statistics;
using SDO.Base.Utils.Models;
using SDO.Models;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    public class IPCProjectADJDetailedRunwayC : WContentBuilder
    {
        public IPCProjectADJDetailedRunwayC(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "IPCProjectADJDetailedRPTRunwayC.doc";
        }

        protected override Task GetData()
        {
            ProjectAdjustDetailedModel data = (ProjectAdjustDetailedModel)Parameter.ObjectModel;
            AdjustScheHistoryModel adjustItem = data.HistoryModels?.Find(x => x.PROJ_ADJ_ID == Parameter.PROJ_ADJ_ID);
            BasicData = new
            {
                data.PROJECT_NO,
                data.PROJECT_NAME,
                TOTAL_BUDGET = $"{data.TOTAL_BUDGET:N0}",
                data.EXEC_ORGAN_NAME,
                ADJ_NUM = adjustItem != null ? adjustItem.SEQ.ToString() : string.Empty,
                SCHE_TYPE = adjustItem != null ? (adjustItem.SCHE_TYPE == "Y" ? "總期程" : "分月") : string.Empty
            };
            
            if (Parameter.PROJ_ADJ_ID == 0)
            {
                return Task.CompletedTask;
            }

            ListData = new List<ITableData>();
            SetHistory(data.HistoryModels);
            SetCheckPoint(data);
            return Task.CompletedTask;
        }

        protected override void Other()
        {
            Table targetTable = (Table)Doc.GetChildNodes(NodeType.Table, true)[0];
            List<string> mergeColumns = new() { "調整歷程", "總期程", "分月\v期程" };
            foreach (string mergeColumn in mergeColumns)
            {
                VerticalMergeCells(FindCell(targetTable, mergeColumn, false));
            }
        }

        /// <summary>
        /// 設定期程調整歷程
        /// </summary>
        /// <param name="data">期程調整歷程資料</param>
        private void SetHistory(List<AdjustScheHistoryModel> data)
        {
            List<object> yearList = new();
            List<object> monthList = new();
            foreach (AdjustScheHistoryModel item in data)
            {
                if (item.SCHE_TYPE == "Y")
                {
                    string historyStr = $"第{item.SEQ}次：" +
                        $"原定{(item.ORI_ESTIMATED_ENDDATE != null ? item.ORI_ESTIMATED_ENDDATE.ToTwDateString() : string.Empty)}完成，" +
                        $"調整至{(item.ADJ_LAST_DATE != null ? item.ADJ_LAST_DATE.ToTwDateString() : string.Empty)}" +
                        $"（核准日期\r\n　　　　{item.APPRV_DATE.ToTwDateString()}）。\r\n" +
                        $"調整原因：{item.REASON}";
                    yearList.Add(new
                    {
                        ADJ_HIS_Y = historyStr
                    });
                }
                else if (item.SCHE_TYPE == "M")
                {
                    string historyStr = $"第{item.SEQ}次：" +
                        (item.ADJ_LAST_DATE != null ? item.ADJ_LAST_DATE.ToTwDateString() : string.Empty) +
                        $"（核准日期：{item.APPRV_DATE.ToTwDateString()}）。\v" +
                        $"調整原因：{item.REASON}";
                    monthList.Add(new
                    {
                        ADJ_HIS_M = historyStr
                    });
                }
            }

            if (yearList.Any())
            {
                ListData.Add(new WordTableData { LIST_DATA = yearList });
            }
            if (monthList.Any())
            {
                ListData.Add(new WordTableData { LIST_DATA = monthList });
            }
        }

        /// <summary>
        /// 設定檢核點
        /// </summary>
        /// <param name="data">檢核點資料</param>
        private void SetCheckPoint(ProjectAdjustDetailedModel data)
        {
            if (data.CusChkpt != null && data.CusChkpt.Any())
            {
                List<ProjectCusCheckpointModel> checkPoint = data.CusChkpt;
                List<AdjustCusCheckPointModel> adjustCheckPoint = data.AdjustCusChkpt;
                // checkPoint vs adjustCheckPoint 選擇較大長度的來跑迴圈
                int checkPointCntMax = checkPoint.Count > adjustCheckPoint.Count ? checkPoint.Count : adjustCheckPoint.Count;
                List<object> checkpointList = new();
                for (int i = 0; i < checkPointCntMax; i++)
                {
                    checkpointList.Add(new
                    {
                        CHECKITEM_NAME = i < checkPoint.Count ? checkPoint[i].CHECKITEM_NAME : string.Empty,
                        ESTIMATED_ENDDATE = i < checkPoint.Count ? checkPoint[i].ESTIMATED_ENDDATE.ToTwDateString() : string.Empty,
                        CHECKITEM_NAME_ADJ = i < adjustCheckPoint.Count ? adjustCheckPoint[i].CHECKITEM_NAME : string.Empty,
                        ESTIMATED_ENDDATE_ADJ = i < adjustCheckPoint.Count ? adjustCheckPoint[i].ESTIMATED_ENDDATE.ToTwDateString() : string.Empty,
                    });
                }
                ListData.Add(new WordTableData { LIST_DATA = checkpointList });
            }
        }

    }
}
