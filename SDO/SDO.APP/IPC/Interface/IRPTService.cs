using Microsoft.AspNetCore.Mvc;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using SDO.ReportBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IRPTService
    {
        /// <summary>
        /// 匯出Grid共用
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        Task<RtnRptModel> ExportGrid(ExportGridModel gridData);

        /// <summary>
        /// Demo 匯出 word
        /// </summary>
        /// <returns></returns>
        Task<RtnRptModel> ExportWord();

        /// <summary>
        /// 重大建設計畫B級管制案件機關統計表 TODO
        /// </summary>
        /// <returns></returns>
        Task<RtnRptModel> RPTProjectDeptDetailed();

        /// <summary>
        /// 取得期程調整申請表
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        /// <returns></returns>
        Task<RtnRptModel> RPTAdjustSchedule(string PROJECT_NO, int PROJ_ADJ_ID);

        /// <summary>
        /// 產出計畫年終考核評分表
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<RtnRptModel> RPTProjectFillYearAss(string PROJECT_NO);

        /// <summary>
        /// 計畫預覽列印-下載報表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<RtnRptModel> ProjectPrint(ProjectPrintQueryModel model);

        /// <summary>
        /// 統計報表
        /// </summary>
        /// <param name="model">統計報表model</param>
        /// <returns></returns>
        Task<RtnRptModel> RPTStatistics(StatisticsModel model);

        /// <summary>
        /// 取得屬於工程類的計畫 (用於統計報表 表10:選項列管案件計畫歷次調整審查表(簡表))
        /// </summary>
        /// <returns>屬於工程類的計畫清單(PROJECT_NO: 計畫編號、PROJ_ADJ_ID: 最近一次的調整流水號)</returns>
        Task<List<object>> GetEngineeringProjects();

        /// <summary>
        /// 計畫調整內容比對結果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<RtnRptModel> ProjectAdjustDiff(ProjectPrintQueryModel model);

        /// <summary>
        /// 綜合查詢
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        Task<RtnRptModel> RPTUnitingQuery(ExportGridModel gridData);

        Task<RtnRptModel> ProjectScheduleOverview(ProjectScheOverviewQueryModel PROJECT_NO);
    }
}
