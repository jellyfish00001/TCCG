using Aspose.Words;
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
    public class IPCProjectADJShort : WContentBuilder
    {
        private readonly IStatisticsService service;
        public IPCProjectADJShort(IComponentContext coms) : base(coms)
        {
            this.service = coms.Resolve<IStatisticsService>();
            TemplateFileName = "IPCProjectADJShortRPT.doc";
        }

        protected override async Task<Task> GetData()
        {
            List<ProjectAdjustDetailedModel> data = await service.GetProjAdjShort(Parameter.PROJECT_NO);

            if (data[0].PROJ_ADJ_ID == 0)
            {
                return Task.CompletedTask;
            }
            
            List<ProjectCusCheckpointModel> oriCtrlPoint = data[0].CusChkpt;
            BasicData = new
            {
                data[0].PROJECT_NAME,
                TOTAL_BUDGET = $"{data[0].TOTAL_BUDGET:N0}",
                data[0].EXEC_ORGAN_NAME,
                ORI_DATE_E = oriCtrlPoint.Find(x => x.CTRL_POINT == "E")?.ESTIMATED_ENDDATE.ToTwDateString(),
                ORI_DATE_F = oriCtrlPoint.Find(x => x.CTRL_POINT == "F")?.ESTIMATED_ENDDATE.ToTwDateString(),
                ORI_DATE_D = oriCtrlPoint.Find(x => x.CTRL_POINT == "D")?.ESTIMATED_ENDDATE.ToTwDateString(),
                ORI_DATE_A = oriCtrlPoint.Find(x => x.CTRL_POINT == "A")?.ESTIMATED_ENDDATE.ToTwDateString(),
                ORI_DATE_B = oriCtrlPoint.Find(x => x.CTRL_POINT == "B")?.ESTIMATED_ENDDATE.ToTwDateString(),
                ORI_DATE_C = oriCtrlPoint.Find(x => x.CTRL_POINT == "C")?.ESTIMATED_ENDDATE.ToTwDateString(),
            };

            ListData = new List<ITableData>();
            List<object> listData = new();
            foreach (ProjectAdjustDetailedModel item in data)
            {
                List<AdjustCusCheckPointModel> adjCusChkPts = item.AdjustCusChkpt;
                AdjustScheHistoryModel adjustItem = item.HistoryModels?.Find(x => x.PROJ_ADJ_ID == item.PROJ_ADJ_ID);
                listData.Add(new
                {
                    ADJ_NUM = adjustItem != null ? adjustItem.SEQ.ToString() : string.Empty,
                    SCHE_TYPE = adjustItem != null ? (adjustItem.SCHE_TYPE == "Y" ? "總期程" : "分月") : string.Empty,
                    DATE_E = adjCusChkPts.Find(x => x.CTRL_POINT == "E")?.ESTIMATED_ENDDATE.ToTwDateString(),
                    DATE_F = adjCusChkPts.Find(x => x.CTRL_POINT == "F")?.ESTIMATED_ENDDATE.ToTwDateString(),
                    DATE_D = adjCusChkPts.Find(x => x.CTRL_POINT == "D")?.ESTIMATED_ENDDATE.ToTwDateString(),
                    DATE_A = adjCusChkPts.Find(x => x.CTRL_POINT == "A")?.ESTIMATED_ENDDATE.ToTwDateString(),
                    DATE_B = adjCusChkPts.Find(x => x.CTRL_POINT == "B")?.ESTIMATED_ENDDATE.ToTwDateString(),
                    DATE_C = adjCusChkPts.Find(x => x.CTRL_POINT == "C")?.ESTIMATED_ENDDATE.ToTwDateString(),
                });
            }
            ListData.Add(new WordTableData { LIST_DATA = listData });
            return Task.CompletedTask;
        }
        
    }
}
