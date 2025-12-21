using Microsoft.Extensions.Logging;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Base.RPT.Enums;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Interface;
using SDO.ReportBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class RPTService : Service, IRPTService
    {
        private readonly IReportFactory rpt;
        private readonly IProjectDac projectDac;
        private readonly IStatisticsService statisticsService;

        public RPTService(IReportFactory rpt, IProjectDac projectDac, IStatisticsService statisticsService)
        {
            this.rpt = rpt;
            this.projectDac = projectDac;
            this.statisticsService = statisticsService;
        }

        private async Task<RtnRptModel> CreateRPT(RptParameter param)
        {
            return await rpt.CreateRPT<RPTService>(param);
        }

        /// <summary>
        /// 匯出Grid共用
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        public async Task<RtnRptModel> ExportGrid(ExportGridModel gridData)
        {
            ExportGridParameter parameter = new()
            {
                FileName = gridData.OutputName,
                ReportId = "RPTExportGrid",
                Extension = gridData.Format,
                GridData = gridData,
                ReportType = ReportTypeEnum.Excel
            };
           return await CreateRPT(parameter);
        }

        /// <summary>
        /// Demo 匯出 word
        /// </summary>
        /// <returns></returns>
        public async Task<RtnRptModel> ExportWord()
        {
            RptParameter param = new RptParameter
            {
                ReportId = "RPTExportWord",
                FileName = "DemoExportWord",
                ReportType = ReportTypeEnum.Word
            };
            return await CreateRPT(param);
        }

        /// <summary>
        /// 重大建設計畫B級管制案件機關統計表 TODO
        /// </summary>
        /// <returns></returns>
        public async Task<RtnRptModel> RPTProjectDeptDetailed()
        {
            RptParameter param = new RptParameter
            {
                ReportId = "RPTProjectDeptDetailed",
                FileName = "重大建設計畫B級管制案件機關統計表",
                ReportType = ReportTypeEnum.Excel
            };
            return await CreateRPT(param);
        }

        /// <summary>
        /// 取得期程調整申請表
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        /// <returns></returns>
        public async Task<RtnRptModel> RPTAdjustSchedule(string PROJECT_NO, int PROJ_ADJ_ID)
        {
            RptParameter param = new RptParameter
            {
                ReportId = "RPTAdjustSchedule",
                FileName = "期程調整申請表",
                ReportType = ReportTypeEnum.Word,
                PROJECT_NO = PROJECT_NO,
                PROJ_ADJ_ID = PROJ_ADJ_ID
            };
            return await CreateRPT(param);
        }

        /// <summary>
        /// 產出計畫年終考核評分表
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<RtnRptModel> RPTProjectFillYearAss(string PROJECT_NO)
        {
            string status = await projectDac.GetProjectStatus(PROJECT_NO);
            string title = status == "7" ? "表" : "試算表";
            RptParameter param = new RptParameter
            {
                ReportId = "RPTProjectFillYearAss",
                FileName = $"桃園市政府重大建設計畫選項列管案件執行進度年终考核評分{title}",
                ReportType = ReportTypeEnum.Word,
                PROJECT_NO = PROJECT_NO,
            };
            return await CreateRPT(param);
        }

        /// <summary>
        /// 計畫預覽列印-下載報表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnRptModel> ProjectPrint(ProjectPrintQueryModel model)
        {
            ProjectBasicModel data = await projectDac.GetProjectBasic(model.PROJECT_NO);
            string projectName = data.PROJECT_NAME.Replace("*", "");
            RptParameter param = new RptParameter
            {
                ReportId = "RPTProjectPrint",
                FileName = projectName,
                ReportType = ReportTypeEnum.Word,
                PROJECT_NO = model.PROJECT_NO,
                Extension = model.Extension,
                Type = model.Type,
                IsRdecFun = model.IsRdecFun,
                IsDiffCompare = model.IsDiffCompare, //目前只有Type = A，基本資料有差異比對
                ObjectModel = model.LOG_ID
            };
            return await CreateRPT(param);
        }

        /// <summary>
        /// 統計報表
        /// </summary>
        /// <param name="model">統計報表model</param>
        /// <returns></returns>
        public async Task<RtnRptModel> RPTStatistics(StatisticsModel model)
        {
            List<(int, string, ReportTypeEnum)> reports = new List<(int, string, ReportTypeEnum)>
            {
                new (1, "RPTProjectStatistics", ReportTypeEnum.Word),
                new (2, model.Type == "Short" ? "RPTIPCProjectAreaShort" : "RPTIPCProjectAreaDetailed", ReportTypeEnum.Excel),
                new (3, model.Type == "Short" ? "RPTIPCProjectDeptShort" : "RPTProjectDeptDetailed", ReportTypeEnum.Excel),
                new (4, model.Type == "1" ? "RPTDelayStatistics" : "RPTProjectDelayList",
                    model.Type == "1" ? ReportTypeEnum.Word : ReportTypeEnum.Excel),
                new (5, "RPTProjectUnFilledList", ReportTypeEnum.Excel),
                new (6, "RPTCheckpointExpirationNotice", ReportTypeEnum.Word),
                new (7, "RPTSpecCheckpointExpiry", ReportTypeEnum.Word),
                new (8, "RPTSpecCheckpointOverdueSituation", ReportTypeEnum.Word),
                new (9, "RPTIPCProjectBudgetExec", ReportTypeEnum.Excel),
                new (10, "RPTIPCProjectADJ", ReportTypeEnum.Word),
                new (11, "RPTIPCProjectFillYearAss", ReportTypeEnum.Excel),
                new (12, "RPTProjectComIpcMemo", ReportTypeEnum.Word),
                new (13, "RPTProjectSyncLog", model.Type == "Word" ? ReportTypeEnum.Word : ReportTypeEnum.Excel),
            };

            var report = reports.FirstOrDefault(x => x.Item1 == model.STATISTICS_ID);
            RptParameter param = new RptParameter
            {
                ReportId = report.Item2,
                ReportType = report.Item3,
                PROJECT_NO = model.PROJECT_NO,
                ObjectModel = model,
                Type = model.Type,
                FileName = model.STATISTICS_NAME
            };
            return await CreateRPT(param);
        }

        /// <summary>
        /// 取得屬於工程類的計畫 (用於統計報表 表10:選項列管案件計畫歷次調整審查表(簡表))
        /// </summary>
        /// <returns>屬於工程類的計畫清單(PROJECT_NO: 計畫編號、PROJ_ADJ_ID: 最近一次的調整流水號)</returns>
        public async Task<List<object>> GetEngineeringProjects()
        {
            return await statisticsService.GetEngineeringProjects();
        }

        /// <summary>
        /// 計畫調整內容比對結果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnRptModel> ProjectAdjustDiff(ProjectPrintQueryModel model)
        {
            ProjectBasicModel data = await projectDac.GetProjectBasic(model.PROJECT_NO);
            string projectName = data.PROJECT_NAME.Replace("*", "");
            
            RptParameter param = new()
            {
                ReportId = "RPTProjectAdjustDiff",
                FileName = $"{projectName}─調整內容",
                ReportType = ReportTypeEnum.Word,
                PROJECT_NO = model.PROJECT_NO,
                PROJ_ADJ_ID = model.LOG_ID[1],
                Extension = model.Extension,
                Type = model.Type,
                IsRdecFun = model.IsRdecFun,
                IsDiffCompare = model.IsDiffCompare, //目前只有Type = A，基本資料有差異比對
                ObjectModel = model.LOG_ID
            };
            return await CreateRPT(param);
        }

        /// <summary>
        /// 綜合查詢
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        public async Task<RtnRptModel> RPTUnitingQuery(ExportGridModel gridData)
        {
            ExportGridParameter parameter = new()
            {
                FileName = gridData.OutputName,
                ReportId = "RPTUnitingQuery",
                Extension = gridData.Format,
                GridData = gridData,
                ReportType = ReportTypeEnum.Excel
            };
            return await CreateRPT(parameter);
        }

        /// <summary>
        /// 計畫期程一覽表
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<RtnRptModel> ProjectScheduleOverview(ProjectScheOverviewQueryModel model)
        {
            RptParameter param = new()
            {
                ReportId = "RPTProjectScheOverview",
                FileName = "桃園市政府重大建設計畫期程一覽表",
                ReportType = ReportTypeEnum.Excel,
                PROJECT_NO = model.PROJECT_NO,
                ObjectModel = model,
                Extension = "xlsx"
            };
            return await CreateRPT(param);
        }
    }
}
