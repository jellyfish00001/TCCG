using SDO.APP.IPC.Models.Statistics;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IStatisticsDac : IDac
    {
        #region 綜合查詢
        /// <summary>
        /// 取得綜合查詢結果
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="selectedColumns"></param>
        /// <returns></returns>
        Task<(List<IDictionary<string, object>>, List<string>)> GetUnitingQuery(Dictionary<string, object> condition, List<OptionColumnModel> selectedColumns);
        #endregion

        #region 表1: 每月案件統計表
        /// <summary>
        /// 取得每月案件統計表 簡版(重大建設計畫B級管制案件統計表)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProjectStatisticsModel>> GetIPCProjectStatistics(StatisticsModel model);
        #endregion 表1: 每月案件統計表

        #region 表2、3: 每月案件地區/機關統計表
        /// <summary>
        /// 取得每月案件地區/機關統計表(簡版)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProjectAreaDeptShortModel>> GetIPCProjectAreaDeptShort(StatisticsModel model);

        /// <summary>
        /// 取得每月案件地區/機關統計表(詳版)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProjectAreaDeptDetailedModel>> GetIPCProjectAreaDeptDetailed(StatisticsModel model);
        #endregion 表2、3: 每月案件地區/機關統計表

        #region 表4: 每月案件連續落後統計表/挑案列表
        /// <summary>
        /// 取得每月案件連續落後統計表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<DelayStatisticsModel>> GetDelayStatistics(StatisticsModel model);
        /// <summary>
        /// 取得每月案件落後挑案列表資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProjectDelayListModel>> GetProjectDelayList(StatisticsModel model);
        /// <summary>
        /// 取得表4會議列管資料
        /// </summary>
        /// <param name="projectNos"></param>
        /// <returns></returns>
        Task<List<ProjectConferenceRPTModel>> GetProjectConferenceForRPT(List<string> projectNos);
        /// <summary>
        /// 取得表4 完整檢核點資料
        /// </summary>
        /// <param name="projectNos"></param>
        /// <returns></returns>
        Task<List<ProjectCusCheckpointModel>> GetProjectCheckItemForRPT(List<string> projectNos);
        #endregion 表4: 每月案件連續落後統計表/挑案列表
        
        #region 表5: 每月未完成進度填報清單
        /// <summary>
        /// 取得計畫未完成進度填報清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProjectUnFilledModel>> GetProjectUnFilledList(StatisticsModel model);
        #endregion 表5: 每月未完成進度填報清單
        
        #region 表6: 檢核點屆期預告
        /// <summary>
        /// 取得檢核點屆期預告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<CheckpointExpiryModel>> GetCheckItemExpiry(StatisticsModel model);
        #endregion

        #region 表7: 特定檢核點屆期情形
        /// <summary>
        /// 取得特定檢核點屆期情形
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<SpecCheckpointExpiryModel>> GetSpecCheckpointExpiry(StatisticsModel model);
        #endregion 表7: 特定檢核點屆期情形

        #region 表8: 落後案件特定檢核點逾期情形
        /// <summary>
        /// 取得落後案件特定檢核點逾期情形
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<SpecChkPointOverdueSituationModel>> GetSpecCheckpointOverdueSituation(StatisticsModel model);
        #endregion 表8: 落後案件特定檢核點逾期情形

        #region 表9: 預算執行情形明細表
        /// <summary>
        /// 取得預算執行情形明細表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<IPCProjectBudgetExecModel>> GetIPCProjectBudgetExec(StatisticsModel model);
        #endregion 表9: 預算執行情形明細表

        #region 表10: 選項列管案件計畫歷次調整審查表

        /// <summary>
        /// 取得 表10選項列管案件計畫歷次調整審查表 資料
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <returns></returns>
        Task<List<ProjectAdjustDetailedModel>> GetProjAdjDetailed(string PROJECT_NO);

        /// <summary>
        /// 取得屬於工程類的計畫
        /// </summary>
        /// <returns>屬於工程類的計畫清單(PROJECT_NO: 計畫編號、PROJ_ADJ_ID: 最近一次的調整流水號)</returns>
        Task<List<object>> GetEngineeringProjects();

        #endregion 表10: 選項列管案件計畫歷次調整審查表

        #region 表11: 年終考核案件成績表
        /// <summary>
        /// 取得年終考核案件成績表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<IPCProjectFillYearAssModel>> GetIPCProjectFillYearAss(StatisticsModel model);

        /// <summary>
        /// 取得所有計畫檢核點
        /// </summary>
        /// <returns></returns>
        Task<List<ProjectCusCheckpointModel>> GetProjectCusCheckpointList();

        /// <summary>
        /// 取得所有計畫每月辦理情形進度
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectEngineeringProgressGridModel>> GetProjecFillExecuteList();

        /// <summary>
        /// 取得所有落後計畫
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectDelayCausalModel>> GetProjectDelayCausalList();
        #endregion 表11: 年終考核案件成績表
        #region 表12 平時管考意見備註統計表
        /// <summary>
        /// 取得平時管考意見備註統計表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProjectComIpcMemoModel>> GetProjectComIpcMemoList(StatisticsModel model);
        /// <summary>
        /// 取得各計畫未完成的第一項檢核點
        /// </summary>
        /// <returns></returns>
        Task<List<CheckpointExpiryModel>> GetProjectFirstUnFilledCheckPoint(List<string> projectNos);

        /// <summary>
        /// 取得近三個月計畫工程進度
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="projectNos"></param>
        /// <returns></returns>
        Task<List<ProjectEngPrgRPTModel>> GetProjectProgressWithinThreeMonths(int year, int month, List<string> projectNos);
        #endregion 表12 平時管考意見備註統計表

        /// <summary>
        /// 表13: 取得重大建設介接公共工程資料統計表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProjectSyncLogModel>> GetProjectSyncLogList(StatisticsModel model);

        /// <summary>
        /// 表13: 重大建設系統介接公共工程雲雲端服務網資料一覽表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProjectSyncLogOverviewModel>> GetProjectSyncLogOverviewList(StatisticsModel model);

        #region 共用
        /// <summary>
        /// 取得機關列管件數
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<OrgProjectCntModel>> GetExecOrgProjectCnt(StatisticsModel model);
        #endregion
    }
}
