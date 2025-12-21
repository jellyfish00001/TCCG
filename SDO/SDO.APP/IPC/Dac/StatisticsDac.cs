using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using SDO.APP.IPC.Models.Statistics;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class StatisticsDac : Dac, IStatisticsDac
    {
        public StatisticsDac(IConnectionControlCenter connectionControlCenter,
           IHttpContextAccessor httpContextAccessor,
           ISqlTrace trace,
           IUserProfile profile,
           IParameterAdaptor ParameterAdaptor,
           IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        #region 綜合查詢
        /// <summary>
        /// 取得綜合查詢結果
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="selectedColumns"></param>
        /// <returns></returns>
        public async Task<(List<IDictionary<string, object>>, List<string>)> GetUnitingQuery(Dictionary<string, object> condition, List<OptionColumnModel> selectedColumns)
        {
            List<string> combineField = new List<string>();
            UnitingQuerySQLModel sqlModel = new UnitingQuerySQLModel();
            #region 順序不能變 join 內有需判斷 HaveVW2 欄位
            // 設定查詢欄位
            SetSelect(sqlModel, selectedColumns, combineField);
            // 設定查詢條件
            SetWhere(sqlModel, condition);
            // 設定JoinTable
            SetJoinTable(sqlModel, selectedColumns, condition);
            #endregion 順序不能變 join 內有需判斷 HaveVW2 欄位

            string sql = $@"
                SELECT DISTINCT 
                    M1.PROJECT_NO
                    ,M1.PROJECT_NAME
                    ,ISNULL(M1.CP_KIND, 0) AS CP_KIND
                    {sqlModel.SelectColumn}
                FROM PROJECT_BASIC (NOLOCK) M1
                LEFT JOIN PROJECT_MERGE_LOG (NOLOCK) M7 ON M1.PROJECT_NO = M7.PROJECT_NO
                LEFT JOIN PROJECT_ENGINEERING_PROGRESS (NOLOCK) M9 ON M1.PROJECT_NO = M9.PROJECT_NO AND YEAR = @STATISTICS_YEAR AND MONTH = @STATISTICS_MONTH
                LEFT JOIN PROJECT_CONTROL_EXECUTE (NOLOCK) M10 ON M1.PROJECT_NO = M10.PROJECT_NO
                LEFT JOIN (
                    SELECT  
	                    PROJECT_NO, TOTAL_ACTUAL_COMP, ACTUAL_PAY, UNPAY, BALANCE, 
	                    ROW_NUMBER() OVER ( PARTITION BY PROJECT_NO ORDER BY IDENTITY_FIELD DESC ) AS RN
                    FROM PROJECT_PAYMENT (NOLOCK)
                ) M11 ON M1.PROJECT_NO = M11.PROJECT_NO AND RN = 1
                LEFT JOIN PROJECT_BUDGET_EXECUTE (NOLOCK) M12 ON M1.PROJECT_NO = M12.PROJECT_NO AND M12.EXEC_YEAR = @STATISTICS_YEAR AND M12.EXEC_MONTH = @STATISTICS_MONTH
                LEFT JOIN PROJECT_CONTROL_EXECUTE (NOLOCK) M13 ON M1.PROJECT_NO = M13.PROJECT_NO
                LEFT JOIN PROJECT_DELAY_CAUSAL (NOLOCK) M16 ON M1.PROJECT_NO = M16.PROJECT_NO AND M16.DATA_YEAR = @STATISTICS_YEAR AND M16.DATA_MONTH = @STATISTICS_MONTH
                LEFT JOIN PROJECT_ENGINEERING_AUDIT_OPINION (NOLOCK) M19 ON M1.PROJECT_NO = M19.PROJECT_NO AND M19.YEAR = @STATISTICS_YEAR AND M19.MONTH = @STATISTICS_MONTH
                {sqlModel.JoinTable}               
                WHERE M1.IS_CANCELED = 0 --排除已刪除的計畫
                    {sqlModel.WhereSql}
                ";

            return ((await ExecuteQueryDictAsync(sql, condition)).ToList(), combineField);
        }

        /// <summary>
        /// 設定查詢欄位
        /// </summary>
        /// <param name="sqlModel"></param>
        /// <param name="selectedColumns"></param>
        /// <param name="combineField"></param>
        private void SetSelect(UnitingQuerySQLModel sqlModel, List<OptionColumnModel> selectedColumns, List<string> combineField)
        {
            foreach (OptionColumnModel field in selectedColumns)
            {
                switch (field.Key)
                {
                    #region 基本資料(A)
                    case "MASTER_ORGAN_C": //主管機關
                    case "EXEC_ORGAN_C": //執行機關
                    case "BUDGET_HOLD_ORGAN_C": //代辦機關
                        string keyColumn = field.Key.Replace("ORGAN_C", "ORGAN_NAME");
                        sqlModel.SelectColumn.AppendLine($",VW3.{keyColumn} AS {field.Key}");
                        break;
                    case "ASSISTANT_ORGAN_C": //協辦機關
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CONNECT_DATA(M1.PROJECT_NO, 'A', DEFAULT) AS {field.Key}");
                        break;
                    case "BUDGET_TOTAL": //計畫總經費
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_PROJECT_EXS(M1.PROJECT_NO, 'T') AS {field.Key}");
                        break;
                    case "BUDGET_CENTRAL": //中央補助款
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_PROJECT_EXS(M1.PROJECT_NO, 'C') AS {field.Key}");
                        break;
                    case "BUDGET_LOCAL": //地方自籌款
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_PROJECT_EXS(M1.PROJECT_NO, 'L') AS {field.Key}");
                        break;
                    case "TOWN_C": //地區別
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_IPC_PARAM(M1.PROJECT_NO, 'A') AS {field.Key}");
                        break;
                    case "BUILD_KIND": //建設類別
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CONNECT_DATA(M1.PROJECT_NO, 'B', '01') AS MAIN_BUILD");
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CONNECT_DATA(M1.PROJECT_NO, 'B', '02') AS SUB_BUILD");
                        combineField.Add(field.Key);
                        break;
                    case "MAIN_BUILD": //主要建設
                        if (!selectedColumns.Select(x => x.Key).Contains("BUILD_KIND"))
                            sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CONNECT_DATA(M1.PROJECT_NO, 'B', '01') AS MAIN_BUILD");
                        break;
                    case "SUB_BUILD": //附屬建設
                        if (!selectedColumns.Select(x => x.Key).Contains("BUILD_KIND"))
                            sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CONNECT_DATA(M1.PROJECT_NO, 'B', '02') AS SUB_BUILD");
                        break;
                    case "REVIEWITEM": //相關審查
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GetSetParam('COM_REVIEWITEM', M1.REVIEWITEM) AS {field.Key}");
                        break;
                    case "TUBE_STATUS": //列管狀態
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B1') AS {field.Key}");
                        break;
                    case "MERGE_STATUS": //分案或併案
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GetSetParam('PROMERGESTATUS', M7.MERGE_STATUS) AS {field.Key}");
                        break;
                    case "MANINFO": //廠商資訊
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CONNECT_DATA(M1.PROJECT_NO, 'C', DEFAULT) AS {field.Key}");
                        break;
                    #endregion

                    #region 預算執行情形(B)
                    case "GT_FINISH_BUDGET": //累計實際完成金額
                        sqlModel.SelectColumn.AppendLine($",(M12.GT_ACT_BUDGET + M12.GT_AP) AS {field.Key}");
                        break;
                    case "SPEC_NOTE": //特殊加註
                    case "IPCBGTEXECFAILED": //年度預算執行率未達80%原因
                    case "IPCBGTEXECFAILEDDUTY": //年度預算執行率未達80%責任歸屬
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_MAPPING_DATA(M1.PROJECT_NO, '{field.Key}', 1) AS {field.Key}");
                        break;
                    #endregion

                    #region 經費支用(C)
                    case "EXACUTIVE_RATE": //結案累計經費執行率
                        sqlModel.SelectColumn.AppendLine($",CASE WHEN dbo.FN_GET_PROJECT_EXS(M1.PROJECT_NO, 'T') = 0 THEN 0 ELSE ((ACTUAL_PAY + M11.UNPAY + M11.BALANCE) / (dbo.FN_GET_PROJECT_EXS(M1.PROJECT_NO, 'T')) * 100) END AS {field.Key}");
                        break;
                    #endregion

                    #region 執行進度(D)
                    case "RUNWAY_C": //執行方式
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_IPC_PARAM(M1.RUNWAY_C, 'E') AS {field.Key}");
                        break;
                    case "PROGRESS": //執行階段
                        sqlModel.HaveM8 = true;
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_EXEC_STAGE(M8.PROGRESS, 0) AS {field.Key}");
                        break;
                    case "RDEC_RES_PRG": //管考進度(預定)
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CHECKITEM_INFO(M1.PROJECT_NO, @STATISTICS_AD_YEAR_MONTH_LAST, 'A', 'A') AS {field.Key}");
                        break;
                    case "RDEC_ACT_PRG": //管考進度(實際)
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CHECKITEM_INFO(M1.PROJECT_NO, @STATISTICS_AD_YEAR_MONTH_LAST, 'B', 'A') AS {field.Key}");
                        break;
                    case "RES_CHECKITEM": //檢核點(預定)
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CHECKITEM_INFO(M1.PROJECT_NO, @STATISTICS_AD_YEAR_MONTH_LAST, 'A', 'B') AS {field.Key}");
                        break;
                    case "ACT_CHECKITEM": //檢核點(實際)
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CHECKITEM_INFO(M1.PROJECT_NO, @STATISTICS_AD_YEAR_MONTH_LAST, 'B', 'B') AS {field.Key}");
                        break;
                    case "ALL_CHECKITEM": //完整檢核點(預定/實際)
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CONNECT_DATA(M1.PROJECT_NO, 'J', DEFAULT) AS {field.Key}");
                        break;
                    case "BID_AWARD_ESTIMATED_ENDDATE": //預定工程標決標日期
                        sqlModel.HaveVW2 = true;
                        sqlModel.SelectColumn.AppendLine($",VW2.D_ESTIMATED_ENDDATE AS {field.Key}");
                        break;
                    case "BID_AWARD_ACTUAL_ENDDATE": //實際工程標決標日期
                        sqlModel.HaveVW2 = true;
                        sqlModel.SelectColumn.AppendLine($",VW2.D_ACTUAL_ENDDATE AS {field.Key}");
                        break;
                    case "ACCEPTANCE_ESTIMATED_ENDDATE": //預定驗收日期
                        sqlModel.HaveVW2 = true;
                        sqlModel.SelectColumn.AppendLine($",VW2.C_ESTIMATED_ENDDATE AS {field.Key}");
                        break;
                    case "ACCEPTANCE_ACTUAL_ENDDATE": //實際驗收日期
                        sqlModel.HaveVW2 = true;
                        sqlModel.SelectColumn.AppendLine($",VW2.C_ACTUAL_ENDDATE AS {field.Key}");
                        break;
                    case "FF_DATE_SCORE": //實地查證資料【含查證日期、次數、分數】
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CONNECT_DATA(M1.PROJECT_NO, 'H', DEFAULT) AS {field.Key}");
                        break;
                    case "FF_DATE_FFREPORT": //實地查證執行機關參採情形
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CONNECT_DATA(M1.PROJECT_NO, 'I', DEFAULT) AS {field.Key}");
                        break;
                    case "BID_01_FLOW": //設計標流標次數
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_BID_KIND_COUNT(M1.PROJECT_NO, '01', 0) AS {field.Key}");
                        break;
                    case "BID_01_SCRAP": //設計標廢標次數
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_BID_KIND_COUNT(M1.PROJECT_NO, '01', 1) AS {field.Key}");
                        break;
                    case "BID_02_FLOW": //設計暨監造標流標次數
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_BID_KIND_COUNT(M1.PROJECT_NO, '02', 0) AS {field.Key}");
                        break;
                    case "BID_02_SCRAP": //設計暨監造標廢標次數
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_BID_KIND_COUNT(M1.PROJECT_NO, '02', 1) AS {field.Key}");
                        break;
                    case "BID_03_FLOW": //工程標流標次數
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_BID_KIND_COUNT(M1.PROJECT_NO, '03', 0) AS {field.Key}");
                        break;
                    case "BID_03_SCRAP": //工程標廢標次數
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_BID_KIND_COUNT(M1.PROJECT_NO, '03', 1) AS {field.Key}");
                        break;
                    case "LAST_CHECKITEM_ACTUAL_ENDDATE": //計畫全案實際完成日期
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_IPC_PARAM(M1.PROJECT_NO, 'C') AS {field.Key}");
                        break;
                    case "CLOSED_OR_REVOKE": //結案/撤銷日期
                        sqlModel.SelectColumn.AppendLine($",(CASE WHEN ISNULL(M1.FINISH_DATE,'') = '' THEN  M1.CANCELED_DATE WHEN ISNULL(M1.CANCELED_DATE, '') = '' THEN M1.FINISH_DATE END) AS {field.Key}");
                        break;
                    case "RDEC_RAD_PRG": //管考進度(預定/實際/差異)
                    case "CHECKITEM": //檢核點(預定/實際)
                        combineField.Add(field.Key);
                        break;
                    #endregion

                    #region 開竣工日(E)
                    case "START_ESTIMATED_ENDDATE": //預定開工日期
                        sqlModel.HaveVW2 = true;
                        sqlModel.SelectColumn.AppendLine($",VW2.A_ESTIMATED_ENDDATE AS {field.Key}");
                        break;
                    case "COMPLETION_ESTIMATED_ENDDATE": //預定竣工日期
                        sqlModel.HaveVW2 = true;
                        sqlModel.SelectColumn.AppendLine($",VW2.B_ESTIMATED_ENDDATE AS {field.Key}");
                        break;
                    case "START_ACTUAL_ENDDATE": //實際開工日期
                        sqlModel.HaveVW2 = true;
                        sqlModel.SelectColumn.AppendLine($",VW2.A_ACTUAL_ENDDATE AS {field.Key}");
                        break;
                    case "COMPLETION_ACTUAL_ENDDATE": //實際竣工日期
                        sqlModel.HaveVW2 = true;
                        sqlModel.SelectColumn.AppendLine($",VW2.B_ACTUAL_ENDDATE AS {field.Key}");
                        break;
                    case "START_PCC_ESTIMATED_ENDDATE": //標案系統預定開工日期
                        sqlModel.HaveVW2 = true;
                        sqlModel.SelectColumn.AppendLine($",VW2.A_PCC_ESTIMATED_ENDDATE AS {field.Key}");
                        break;
                    case "COMPLETION_PCC_ESTIMATED_ENDDATE": //標案系統預定竣工日期
                        sqlModel.HaveVW2 = true;
                        sqlModel.SelectColumn.AppendLine($",VW2.B_PCC_ESTIMATED_ENDDATE AS {field.Key}");
                        break;
                    case "START_PCC_ACTUAL_ENDDATE": //標案系統實際開工日期
                        sqlModel.HaveVW2 = true;
                        sqlModel.SelectColumn.AppendLine($",VW2.A_PCC_ACTUAL_ENDDATE AS {field.Key}");
                        break;
                    case "COMPLETION_PCC_ACTUAL_ENDDATE": //標案系統實際竣工日期
                        sqlModel.HaveVW2 = true;
                        sqlModel.SelectColumn.AppendLine($",VW2.B_PCC_ACTUAL_ENDDATE AS {field.Key}");
                        break;
                    #endregion

                    #region 工程進度(F)
                    case "IPC_DIFF_PRG": //工程進度差異
                        sqlModel.SelectColumn.AppendLine($",(M9.IPC_ACT_PRG - M9.IPC_RES_PRG) AS {field.Key}");
                        break;
                    case "TEN_DIFF_PRG": //標案系統工程進度差異
                        sqlModel.SelectColumn.AppendLine($",(M9.TEN_ACT_PRG - M9.IPC_RES_PRG) AS {field.Key}");
                        break;
                    case "IPC_RAD_PRG": //工程進度(預定/實際/差異)
                    case "TEN_RAD_PRG": //標案系統工程進度(預定/實際/差異)
                        combineField.Add(field.Key);
                        break;
                    #endregion

                    #region 落後分析(G)
                    case "DELAY_KIND": //落後類型
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GetSetParam('DELAY_CLASS', M16.DELAY_KIND) AS {field.Key}");
                        break;
                    case "DELAY_CLASS_C": //落後類別
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GetSetParam('DELAY_TYPE', M16.DELAY_CLASS_C) AS {field.Key}");
                        break;
                    case "DELAY_SUBCLASS_C": //落後項目
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_IPC_PARAM(M16.DELAY_SUBCLASS_C, 'D') AS {field.Key}");
                        break;
                    case "DELAY_RESPON": //責任歸屬
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GetSetParam('DELAY_RESPON', M16.DELAY_RESPON) AS {field.Key}");
                        break;
                    case "DELAY_MONTH": //連續月份落後
                        sqlModel.SelectColumn.AppendLine($",VW1.MAX_CONSECUTIVE_MONTH AS {field.Key}");
                        break;
                    case "DELAY_DAY": //進度落後天數
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_DELAY_DAYS(M1.PROJECT_NO, @STATISTICS_AD_YEAR, @STATISTICS_MONTH, @STATISTICS_AD_YEAR_MONTH_LAST) AS {field.Key}");
                        break;
                    #endregion

                    #region 調整撤銷(H)
                    case "ADJ_AW01_INFO": //基本資料調整
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_ADJ_INFO(M1.PROJECT_NO, 'AW01', DEFAULT) AS {field.Key}");
                        break;
                    case "ADJ_AW02_Y_INFO": //總期程調整歷程
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_ADJ_INFO(M1.PROJECT_NO, 'AW02', 'Y') AS {field.Key}");
                        break;
                    case "ADJ_AW02_M_INFO": //分月期程調整歷程
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_ADJ_INFO(M1.PROJECT_NO, 'AW02', 'M') AS {field.Key}");
                        break;
                    case "ADJ_AW03_INFO": //計畫撤銷
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_ADJ_INFO(M1.PROJECT_NO, 'AW03', DEFAULT) AS {field.Key}");
                        break;
                    #endregion

                    #region 其他項目(I)
                    case "CONFERENCE_INFO": //會議列管
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CONNECT_DATA(M1.PROJECT_NO, 'D', DEFAULT) AS {field.Key}");
                        break;
                    case "FILL_REASON": //資料逾期繳交或填報
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CONNECT_DATA(M1.PROJECT_NO, 'E', DEFAULT) AS {field.Key}");
                        break;
                    case "PROJECT_ACTIVITY": //相關活動
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CONNECT_DATA(M1.PROJECT_NO, 'F', DEFAULT) AS {field.Key}");
                        break;
                    #endregion

                    #region 管考意見(J)
                    case "FF_DATE_COMMENT": //實地查證管考說明
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CONNECT_DATA(M1.PROJECT_NO, 'G', DEFAULT) AS {field.Key}");
                        break;
                    case "COM_IPCMEMO": //平時管考意見備註
                        sqlModel.SelectColumn.AppendLine($",dbo.FN_GET_CONNECT_DATA_PARM(M1.PROJECT_NO, @STATISTICS_YEAR, @STATISTICS_MONTH, 'A', '{field.Key}', '', '', '', '') AS {field.Key}");
                        break;
                    #endregion

                    default:
                        // 控制檢核點填報項目的 預定完成日期/實際完成日期
                        if (field.Key.EndsWith("ESTIMATED_ENDDATE") || field.Key.EndsWith("ACTUAL_ENDDATE"))
                        {
                            sqlModel.HaveVW2 = true;
                            string pointType = field.Key.Split("_").FirstOrDefault();
                            string column = field.Key.Remove(0, 2);
                            sqlModel.SelectColumn.AppendLine($",VW2.{pointType}_{column} AS {field.Key}");
                        }
                        else
                        {
                            sqlModel.SelectColumn.AppendLine($",{field.Table}.{field.Key}");
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 設定查詢條件
        /// </summary>
        /// <param name="sqlModel"></param>
        /// <param name="condition"></param>
        private void SetWhere(UnitingQuerySQLModel sqlModel, Dictionary<string, object> condition)
        {
            // 年度
            switch (condition["PROJECT_YEAR_STATUS"])
            {
                case "":
                    sqlModel.WhereSql.AppendLine("AND M1.PROJECT_YEAR = @PROJECT_YEAR_S");
                    break;
                case "A": // 含之前所有案件
                    sqlModel.WhereSql.AppendLine("AND M1.PROJECT_YEAR <= @PROJECT_YEAR_S");
                    break;
                case "B": // 含之前未結案件
                    sqlModel.WhereSql.AppendLine(@"AND (M1.PROJECT_YEAR = @PROJECT_YEAR_S 
	                    OR (M1.PROJECT_YEAR < @PROJECT_YEAR_S AND M1.PROJECT_STATUS IN ('1', '2', '3', '4', '5', '6')) 
	                    OR (year(M1.FINISH_DATE) - 1911 >= @PROJECT_YEAR_S AND M1.PROJECT_STATUS IN ('7', '8'))
                    )");
                    break;
            }
            // 釘選
            if (condition.CheckDictItemExistedAndNotEmpty("PIS_SELECT"))
            {
                sqlModel.WhereSql.AppendLine($"AND fav.PROJECT_NO is not null");
            }
            // 主管機關
            if (condition.CheckDictItemExistedAndNotEmpty("MASTER_DEPT"))
            {
                sqlModel.WhereSql.AppendLine($"AND M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }
            // 執行機關
            if (condition.CheckDictItemExistedAndNotEmpty("EXEC_DEPT"))
            {
                sqlModel.WhereSql.AppendLine($"AND M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }
            // 協辦機關
            if (condition.CheckDictItemExistedAndNotEmpty("ASS_DEPT"))
            {
                sqlModel.WhereSql.AppendLine($"AND M2.ASSISTANT_ORGAN_C = @ASS_DEPT");
            }
            // 代辦機關
            if (condition.CheckDictItemExistedAndNotEmpty("AGCY_DEPT"))
            {
                sqlModel.WhereSql.AppendLine($"AND M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }
            // 計畫總經費
            if (condition.CheckDictItemExistedAndNotEmpty("PROJECT_EXS_S"))
            {
                sqlModel.WhereSql.AppendLine($"AND @PROJECT_EXS_S <= dbo.FN_GET_PROJECT_EXS(M1.PROJECT_NO, 'T')");
            }
            if (condition.CheckDictItemExistedAndNotEmpty("PROJECT_EXS_E"))
            {
                sqlModel.WhereSql.AppendLine($"AND dbo.FN_GET_PROJECT_EXS(M1.PROJECT_NO, 'T') <= @PROJECT_EXS_E");
            }
            // 地區別
            if (condition.CheckDictItemExistedAndNotEmpty("AREA"))
            {
                sqlModel.WhereSql.AppendLine($"AND (M1.TOWN_C = @AREA OR M1.TOWN_M LIKE '%' + @AREA + '%')");
            }
            // 建設類別(主要建設or附屬設施相符即可)
            if (condition.CheckDictItemExistedAndNotEmpty("BUILD_KIND"))
            {
                sqlModel.WhereSql.AppendLine($"AND (M3_1.BUILD_KIND IN @BUILD_KIND OR M3_2.BUILD_KIND IN @BUILD_KIND_DUP)");
                condition["BUILD_KIND"] = JsonConvert.DeserializeObject<string[]>(condition["BUILD_KIND"].ToString());
                condition.Add("BUILD_KIND_DUP", condition["BUILD_KIND"]);
            }
            // 主要建設
            if (condition.CheckDictItemExistedAndNotEmpty("MAIN_BUILD"))
            {
                sqlModel.WhereSql.AppendLine($"AND M3_1.BUILD_KIND IN @MAIN_BUILD");
                condition["MAIN_BUILD"] = JsonConvert.DeserializeObject<string[]>(condition["MAIN_BUILD"].ToString());
            }
            // 附屬設施
            if (condition.CheckDictItemExistedAndNotEmpty("SUB_BUILD"))
            {
                sqlModel.WhereSql.AppendLine($"AND M3_2.BUILD_KIND IN @SUB_BUILD");
                condition["SUB_BUILD"] = JsonConvert.DeserializeObject<string[]>(condition["SUB_BUILD"].ToString());
            }
            // 相關審查
            if (condition.CheckDictItemExistedAndNotEmpty("REVIEWITEM"))
            {
                sqlModel.WhereSql.AppendLine($"AND M1.REVIEWITEM IN @REVIEWITEM");
                condition["REVIEWITEM"] = JsonConvert.DeserializeObject<string[]>(condition["REVIEWITEM"].ToString());
            }
            // 特殊加註
            if (condition.CheckDictItemExistedAndNotEmpty("SPEC_NOTE"))
            {
                sqlModel.WhereSql.AppendLine($"AND M5.SET_TYPE IN @SPEC_NOTE");
                condition["SPEC_NOTE"] = JsonConvert.DeserializeObject<string[]>(condition["SPEC_NOTE"].ToString());
            }
            // 會議種類
            if (condition.CheckDictItemExistedAndNotEmpty("CONFERENCE_GENRE"))
            {
                sqlModel.WhereSql.AppendLine($"AND M6.CONFERENCE_GENRE IN @CONFERENCE_GENRE");
                condition["CONFERENCE_GENRE"] = JsonConvert.DeserializeObject<string[]>(condition["CONFERENCE_GENRE"].ToString());
            }
            // 立案時間
            if (condition.CheckDictItemExistedAndNotEmpty("CREATEDTIME_S"))
            {
                sqlModel.WhereSql.AppendLine($"AND @CREATEDTIME_S <= M1.CREATEDTIME");
            }
            if (condition.CheckDictItemExistedAndNotEmpty("CREATEDTIME_E"))
            {
                sqlModel.WhereSql.AppendLine($"AND M1.CREATEDTIME <= @CREATEDTIME_E");
            }
            // 列管狀態
            if (condition.CheckDictItemExistedAndNotEmpty("TUBE_STATUS"))
            {
                sqlModel.WhereSql.AppendLine($"AND dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }
            // 分案或併案
            if (condition.CheckDictItemExistedAndNotEmpty("MERGE_STATUS"))
            {
                sqlModel.WhereSql.AppendLine($"AND M7.MERGE_STATUS = @MERGE_STATUS");
            }
            // 執行類別
            if (condition.CheckDictItemExistedAndNotEmpty("CP_KIND"))
            {
                sqlModel.WhereSql.AppendLine($"AND M1.CP_KIND = @CP_KIND");
            }
            // 執行方式
            if (condition.CheckDictItemExistedAndNotEmpty("RUNWAY_C"))
            {
                sqlModel.WhereSql.AppendLine($"AND M1.RUNWAY_C = @RUNWAY_C");
            }
            // 平時管考意見備註
            if (condition.CheckDictItemExistedAndNotEmpty("COM_IPCMEMO"))
            {
                sqlModel.WhereSql.AppendLine($"AND M20.COM_IPCMEMO IN @COM_IPCMEMO");
                condition["COM_IPCMEMO"] = JsonConvert.DeserializeObject<string[]>(condition["COM_IPCMEMO"].ToString());
            }
            // 期程調整日期區間
            if (condition.CheckDictItemExistedAndNotEmpty("APPRV_DATE_S"))
            {
                sqlModel.WhereSql.AppendLine($"AND @APPRV_DATE_S <= M21.APPRV_DATE");
            }
            if (condition.CheckDictItemExistedAndNotEmpty("APPRV_DATE_E"))
            {
                sqlModel.WhereSql.AppendLine($"AND M21.APPRV_DATE <= @APPRV_DATE_E");
            }

            #region 以下限定工程類使用條件
            // 執行階段
            if (condition.CheckDictItemExistedAndNotEmpty("EXEC_STAGE"))
            {
                sqlModel.HaveM8 = true;
                sqlModel.WhereSql.Append(" AND dbo.FN_GET_EXEC_STAGE(M8.PROGRESS, 1) = @EXEC_STAGE");
            }

            // 開竣工區間
            if (condition.CheckDictItemExistedAndNotEmpty("CTRL_POINT")
               && condition.CheckDictItemExistedAndNotEmpty("COMPLETED_START")
               && condition.CheckDictItemExistedAndNotEmpty("COMPLETED_END"))
            {
                sqlModel.HaveVW2 = true;
                // 開工:A，竣工:B
                sqlModel.WhereSql.AppendLine($"AND (VW2.{condition["CTRL_POINT"]}_ESTIMATED_ENDDATE BETWEEN @COMPLETED_START AND @COMPLETED_END)");
            }
            // 實際工程進度
            if (condition.CheckDictItemExistedAndNotEmpty("IPC_ACT_PRG_STAR")
                && condition.CheckDictItemExistedAndNotEmpty("IPC_ACT_PRG_END"))
            {
                sqlModel.WhereSql.AppendLine($"AND (M9.IPC_ACT_PRG BETWEEN @IPC_ACT_PRG_STAR AND @IPC_ACT_PRG_END)");
            }
            // 落後類型
            if (condition.CheckDictItemExistedAndNotEmpty("DELAY_TYPE"))
            {
                // D0：無落後
                sqlModel.WhereSql.AppendLine($"AND ISNULL(M16.DELAY_KIND,'D0') IN @DELAY_TYPE");
                condition["DELAY_TYPE"] = JsonConvert.DeserializeObject<string[]>(condition["DELAY_TYPE"].ToString());
            }
            // 落後進度值 - 檢核點進度落後天數
            if (condition.CheckDictItemExistedAndNotEmpty("DELAY_DAY_START")
                && condition.CheckDictItemExistedAndNotEmpty("DELAY_DAY_END"))
            {
                sqlModel.WhereSql.AppendLine($"AND ((dbo.FN_GET_DELAY_DAYS(M1.PROJECT_NO, @STATISTICS_AD_YEAR, @STATISTICS_MONTH, @STATISTICS_AD_YEAR_MONTH_LAST) BETWEEN @DELAY_DAY_START AND @DELAY_DAY_END))");
            }
            // 落後進度值 - 施工進度落後
            if (condition.CheckDictItemExistedAndNotEmpty("DELAY_PRG"))
            {
                sqlModel.WhereSql.AppendLine($"AND (M9.IPC_ACT_PRG - M9.IPC_RES_PRG) <= @DELAY_PRG");
            }
            // 比對標案系統
            if (condition.CheckDictItemExistedAndNotEmpty("COMPARE_PCC_INFO")
                && condition.CheckDictItemExistedAndNotEmpty("COMPARE_PCC_CONDITION"))
            {
                sqlModel.HaveVW2 = true;
                sqlModel.WhereSql.AppendLine(ComparePcc(condition["COMPARE_PCC_INFO"].ToString(), condition["COMPARE_PCC_CONDITION"].ToString()).ToString());
            }
            // 預警類型1(以月區間進行查詢)
            if (condition.CheckDictItemExistedAndNotEmpty("ALERT_TYPE_1"))
            {
                sqlModel.HaveM8 = true;
                switch (condition["ALERT_TYPE_1"].ToString())
                {
                    case "C1": //當月屆期已完成
                        sqlModel.WhereSql.AppendLine($"AND (FORMAT(M8.ESTIMATED_ENDDATE,'yyyy/MM') = @STATISTICS_AD_YEAR_MONTH AND M8.ACTUAL_ENDDATE IS NOT NULL)");
                        break;
                    case "C2": //當月屆期未完成
                        sqlModel.WhereSql.AppendLine($"AND (FORMAT(M8.ESTIMATED_ENDDATE,'yyyy/MM') = @STATISTICS_AD_YEAR_MONTH AND M8.ACTUAL_ENDDATE IS NULL)");
                        break;
                    case "C3": //次月屆期已完成
                        sqlModel.WhereSql.AppendLine($"AND (FORMAT(M8.ESTIMATED_ENDDATE,'yyyy/MM') = @STATISTICS_AD_YEAR_NEXT_MONTH AND M8.ACTUAL_ENDDATE IS NOT NULL)");
                        break;
                    case "C4": //次月屆期未完成
                        sqlModel.WhereSql.AppendLine($"AND (FORMAT(M8.ESTIMATED_ENDDATE,'yyyy/MM') = @STATISTICS_AD_YEAR_NEXT_MONTH AND M8.ACTUAL_ENDDATE IS NULL)");
                        break;
                }
            }
            // 預警類型2
            if (condition.CheckDictItemExistedAndNotEmpty("ALERT_TYPE_2_INFO")
                && condition.CheckDictItemExistedAndNotEmpty("ALERT_TYPE_2_CONDITION"))
            {
                sqlModel.HaveVW2 = true;
                string alterTypeInfo = condition["ALERT_TYPE_2_INFO"].ToString();
                string alterTypeondition = condition["ALERT_TYPE_2_CONDITION"].ToString();
                switch (alterTypeondition)
                {
                    case "C1": //逾期未完成:預計完成日期小於統計年月且實際完成日期未輸入的資料
                        sqlModel.WhereSql.AppendLine($"AND (VW2.{alterTypeInfo}_ESTIMATED_ENDDATE < @STATISTICS_AD_YEAR_MONTH_START AND VW2.{alterTypeInfo}_ACTUAL_ENDDATE IS NULL)");
                        break;
                    case "C2": //當月屆期未完成:預計完成日期為統計年月當月且實際完成日期未輸入的資料
                        sqlModel.WhereSql.AppendLine($"AND (FORMAT(VW2.{alterTypeInfo}_ESTIMATED_ENDDATE,'yyyy/MM') = @STATISTICS_AD_YEAR_MONTH AND VW2.{alterTypeInfo}_ACTUAL_ENDDATE IS NULL)");
                        break;
                    case "C3": //次月屆期未完成:預計完成日期為統計年月加1的當月且實際完成日期未輸入的資料
                        sqlModel.WhereSql.AppendLine($"AND (FORMAT(VW2.{alterTypeInfo}_ESTIMATED_ENDDATE,'yyyy/MM') = @STATISTICS_AD_YEAR_NEXT_MONTH AND VW2.{alterTypeInfo}_ACTUAL_ENDDATE IS NULL)");
                        break;
                }
            }

            // 是否使用界接資料
            if (condition.CheckDictItemExistedAndNotEmpty("IS_USER_FTY_DATA"))
            {
                // 條件僅在"使用界接資料"才加入，無不使用界接資料選項
                if (Boolean.TryParse(condition["IS_USER_FTY_DATA"].ToString(), out bool isUserFtyData) && isUserFtyData)
                {
                    sqlModel.WhereSql.AppendLine("AND M1.IS_USER_FTY_DATA = @IS_USER_FTY_DATA");
                }
            }
            #endregion 以下限定工程類使用條件

            // 管考意見
            if (condition.CheckDictItemExistedAndNotEmpty("AUDIT_OPINION"))
            {
                List<string> fuzzyMatchSqls = condition["AUDIT_OPINION"].ToString()
                    .Split(new char[] { ',', ' ', '　' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => $"M19.AUDIT_OPINION LIKE '%{x}%'")
                    .ToList();

                if (fuzzyMatchSqls.Any())
                {
                    sqlModel.WhereSql.AppendLine($"AND ({string.Join(" OR ", fuzzyMatchSqls)})");
                }
            }

            // 計畫編號
            if (condition.CheckDictItemExistedAndNotEmpty("PROJECT_NO"))
            {
                List<string> fuzzyMatchSqls = condition["PROJECT_NO"].ToString()
                    .Split(new char[] { ',', ' ', '　' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => $"M1.PROJECT_NO LIKE '%{x}%'")
                    .ToList();

                if (fuzzyMatchSqls.Any())
                {
                    sqlModel.WhereSql.AppendLine($"AND ({string.Join(" OR ", fuzzyMatchSqls)})");
                }
            }
            // 計畫名稱
            if (condition.CheckDictItemExistedAndNotEmpty("PROJECT_NAME"))
            {
                List<string> fuzzyMatchSqls = condition["PROJECT_NAME"].ToString()
                    .Split(new char[] { ',', ' ', '　' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => $"M1.PROJECT_NAME LIKE '%{x}%'")
                    .ToList();

                if (fuzzyMatchSqls.Any())
                {
                    sqlModel.WhereSql.AppendLine($"AND ({string.Join(" OR ", fuzzyMatchSqls)})");
                }
            }
        }

        /// <summary>
        /// 設定JoinTable
        /// </summary>
        /// <param name="sqlModel"></param>
        /// <param name="selectedColumns"></param>
        /// <param name="condition"></param>
        private void SetJoinTable(UnitingQuerySQLModel sqlModel, List<OptionColumnModel> selectedColumns, Dictionary<string, object> condition)
        {
            // 如果有連續月份落後
            if (selectedColumns.Any(x => x.Key == "DELAY_MONTH"))
            {
                sqlModel.JoinTable.AppendLine($"LEFT JOIN VW_DELAY_MAX_CONSECUTIVE_MONTH (NOLOCK) VW1 ON M1.PROJECT_NO = VW1.PROJECT_NO");
            }

            if (sqlModel.HaveVW2)
            {
                sqlModel.JoinTable.AppendLine("LEFT JOIN VW_PROJECT_CHECKITEM_DATE (NOLOCK) VW2 ON M1.PROJECT_NO = VW2.PROJECT_NO");
            }

            // 如果有主管機關、執行機關、代辦機關 
            List<string> ouIds = new List<string> { "MASTER_ORGAN_C", "EXEC_ORGAN_C", "BUDGET_HOLD_ORGAN_C" };
            if (selectedColumns.Any(x => ouIds.Contains(x.Key)))
            {
                sqlModel.JoinTable.AppendLine("LEFT JOIN VW_PROJ_OUNAME (NOLOCK) VW3 ON M1.PROJECT_NO = VW3.PROJECT_NO");
            }

            // 釘選
            if (condition.CheckDictItemExistedAndNotEmpty("PIS_SELECT"))
            {
                sqlModel.JoinTable.AppendLine($"LEFT JOIN SUPERIOR_FAVORITEPROJECT (NOLOCK) fav ON M1.PROJECT_NO = fav.PROJECT_NO AND fav.CRT_USER = '{UserId}'");
            }

            // 協辦機關
            if (condition.CheckDictItemExistedAndNotEmpty("ASS_DEPT"))
            {
                sqlModel.JoinTable.AppendLine("LEFT JOIN PROJECT_ASST_ORG (NOLOCK) M2 ON M1.PROJECT_NO = M2.PROJECT_NO");
            }

            // 主要建設
            if (condition.CheckDictItemExistedAndNotEmpty("MAIN_BUILD") || condition.CheckDictItemExistedAndNotEmpty("BUILD_KIND"))
            {
                sqlModel.JoinTable.AppendLine("LEFT JOIN PROJECT_BUILD_KIND (NOLOCK) M3_1 ON M1.PROJECT_NO = M3_1.PROJECT_NO AND M3_1.BUILD_KIND_TYPE = '01'");
            }

            // 附屬設施
            if (condition.CheckDictItemExistedAndNotEmpty("SUB_BUILD") || condition.CheckDictItemExistedAndNotEmpty("BUILD_KIND"))
            {
                sqlModel.JoinTable.AppendLine("LEFT JOIN PROJECT_BUILD_KIND (NOLOCK) M3_2 ON M1.PROJECT_NO = M3_2.PROJECT_NO AND M3_2.BUILD_KIND_TYPE = '02'");
            }

            // 特殊加註
            if (condition.CheckDictItemExistedAndNotEmpty("SPEC_NOTE"))
            {
                sqlModel.JoinTable.AppendLine("LEFT JOIN PROJECT_MAPPING_DATA (NOLOCK) M5 ON M1.PROJECT_NO = M5.PROJECT_NO AND M5.SET_ITEM = 'SPEC_NOTE'");
            }

            // 會議種類
            if (condition.CheckDictItemExistedAndNotEmpty("CONFERENCE_GENRE"))
            {
                sqlModel.JoinTable.AppendLine("LEFT JOIN PROJECT_CONFERENCE (NOLOCK) M6 ON M1.PROJECT_NO = M6.PROJECT_NO");
            }

            // 平時管考意見備註
            if (condition.CheckDictItemExistedAndNotEmpty("COM_IPCMEMO"))
            {
                sqlModel.JoinTable.AppendLine(@"
                    LEFT JOIN (
	                    SELECT PEAO.PROJECT_NO,
                               PMD.SET_TYPE as COM_IPCMEMO
	                    FROM PROJECT_ENGINEERING_AUDIT_OPINION PEAO (NOLOCK)
	                    INNER JOIN PROJECT_MAPPING_DATA PMD 
		                    ON PEAO.SEQ = PMD.SOURCE_ID 
		                    AND PMD.SET_ITEM = 'COM_IPCMEMO' 
                            AND PEAO.YEAR = @STATISTICS_YEAR AND PEAO.MONTH = @STATISTICS_MONTH
                    ) M20 ON M1.PROJECT_NO = M20.PROJECT_NO");
            }

            // 查詢:執行階段 條件:執行階段、預警類型1
            if (sqlModel.HaveM8)
            {
                sqlModel.JoinTable.AppendLine(@$"
                    LEFT JOIN (
                        SELECT 
		                    MB.PROJECT_NO,
		                    MPC.PROGRESS,
		                    MPC.ACTUAL_ENDDATE,
                            MPC.ESTIMATED_ENDDATE,
		                    ROW_NUMBER () OVER (PARTITION BY MB.PROJECT_NO ORDER BY MPC.PROGRESS DESC, MPC.ACTUAL_ENDDATE DESC ) AS RN
	                    FROM PROJECT_BASIC (NOLOCK) MB
	                    LEFT JOIN PROJECT_CHECKITEM (NOLOCK) MPC 
                            ON MB.PROJECT_NO = MPC.PROJECT_NO AND MPC.ACTUAL_ENDDATE IS NOT NULL
                    ) M8 ON M1.PROJECT_NO = M8.PROJECT_NO AND M8.RN = 1");
            }

            // 期程調整核准日期
            if (condition.CheckDictItemExistedAndNotEmpty("APPRV_DATE_S") || condition.CheckDictItemExistedAndNotEmpty("APPRV_DATE_E"))
            {
                sqlModel.JoinTable.AppendLine(@"LEFT JOIN PROJECT_BASIC_ADJ M21 (NOLOCK) 
                                                    ON M1.PROJECT_NO = M21.PROJECT_NO 
                                                    and M21.SCHE_TYPE is not null
                                                    and M21.APPRV_DATE is not null");
            }

        }

        /// <summary>
        /// 比對標案系統 查詢條件
        /// </summary>
        /// <param name="info"></param>
        /// <param name="pccCondition"></param>
        /// <returns></returns>
        private StringBuilder ComparePcc(string info, string pccCondition)
        {
            StringBuilder sql = new StringBuilder();
            string[] pccInfo = JsonConvert.DeserializeObject<string[]>(info);
            string judgment = pccCondition == "C1" ? "<" : pccCondition == "C2" ? ">" : "=";
            if (pccInfo.Contains("C1"))
            {
                sql.AppendLine($"AND VW2.A_ESTIMATED_ENDDATE {judgment} VW2.A_PCC_ESTIMATED_ENDDATE");
            }
            else if (pccInfo.Contains("C2"))
            {
                sql.AppendLine($"AND VW2.A_ACTUAL_ENDDATE {judgment} VW2.A_PCC_ACTUAL_ENDDATE");
            }
            else if (pccInfo.Contains("C3"))
            {
                sql.AppendLine($"AND M9.IPC_RES_PRG {judgment} M9.TEN_RES_PRG");
            }
            else if (pccInfo.Contains("C4"))
            {
                sql.AppendLine($"AND M9.IPC_ACT_PRG {judgment} M9.TEN_ACT_PRG");
            }
            else if (pccInfo.Contains("C5"))
            {
                sql.AppendLine($"AND VW2.B_ESTIMATED_ENDDATE {judgment} VW2.B_PCC_ESTIMATED_ENDDATE");
            }
            else if (pccInfo.Contains("C6"))
            {
                sql.AppendLine($"AND VW2.B_ACTUAL_ENDDATE {judgment} VW2.B_PCC_ACTUAL_ENDDATE");
            }
            else if (pccInfo.Contains("C7"))
            {
                sql.AppendLine($"AND VW2.C_ESTIMATED_ENDDATE {judgment} VW2.C_PCC_ESTIMATED_ENDDATE");
            }
            else if (pccInfo.Contains("C8"))
            {
                sql.AppendLine($"AND VW2.C_ACTUAL_ENDDATE {judgment} VW2.C_PCC_ACTUAL_ENDDATE");
            }
            return sql;
        }
        #endregion 綜合查詢

        #region 表1: 每月案件統計表
        /// <summary>
        /// 取得每月案件統計表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectStatisticsModel>> GetIPCProjectStatistics(StatisticsModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine($@"
                SELECT M1.PROJECT_NO
                    ,M1.PROJECT_NAME
                    ,M1.PROJECT_YEAR
	                ,(CASE WHEN M1.FINISH_DATE > DATEADD(day,1,@YEAR_MONTH_END) THEN '4' ELSE M1.PROJECT_STATUS END) PROJECT_STATUS
	                ,M1.EXEC_ORGAN_C
	                ,dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) AS EXEC_DEPT
	                ,dbo.FN_GET_DELAY_TYPE(M1.PROJECT_NO, '', @YEAR_MONTH_END) AS DELAY_TYPE
                    ,M2.BUDGET_TOTAL
                    ,M3.OU_SORT_ORDER
                FROM PROJECT_BASIC (NOLOCK) M1
                LEFT JOIN (
	                SELECT PROJECT_NO ,SUM(BUDGET_CENTRAL + BUDGET_LOCAL) AS BUDGET_TOTAL
	                FROM PROJECT_BUDGET_SOURCE_G (NOLOCK)
	                GROUP BY PROJECT_NO
	            ) M2 ON M1.PROJECT_NO = M2.PROJECT_NO
                INNER JOIN {SC30_M}.SCORG_UNITM M3 ON M1.EXEC_ORGAN_C = M3.OU_ID
                WHERE M1.EXEC_ORGAN_C IS NOT NULL 
                    AND M1.IS_CANCELED = 0
					AND M1.CREATEDTIME < DATEADD(day,1,@YEAR_MONTH_END)
					");

            // 年度
            switch (model.PROJECT_YEAR_STATUS)
            {
                case "":
                    sql.AppendLine("AND M1.PROJECT_YEAR = @PROJECT_YEAR");
                    break;
                case "A":// 含之前所有案件
                    sql.AppendLine("AND M1.PROJECT_YEAR <= @PROJECT_YEAR");
                    break;
                case "B":// 含之前未結案件
                    sql.AppendLine(@"AND (M1.PROJECT_YEAR = @PROJECT_YEAR 
	                    OR (M1.PROJECT_YEAR < @PROJECT_YEAR AND M1.PROJECT_STATUS IN ('1', '2', '3', '4', '5', '6'))
	                    OR (year(M1.FINISH_DATE) - 1911 >= @PROJECT_YEAR AND M1.PROJECT_STATUS IN ('7', '8'))
                    )");
                    break;
            }
            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine("AND M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }
            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine("AND M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }
            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sql.AppendLine("AND M1.PROJECT_NO IN (SELECT PROJECT_NO FROM PROJECT_ASST_ORG (NOLOCK) WHERE ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }
            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sql.AppendLine("AND M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }
            // 列管狀態
            if (!string.IsNullOrEmpty(model.TUBE_STATUS))
            {
                sql.AppendLine("AND dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }
            // 特殊加註
            if (model.SPEC_NOTE.Any())
            {
                sql.AppendLine("AND M1.PROJECT_NO IN (SELECT PROJECT_NO FROM PROJECT_MAPPING_DATA (NOLOCK) WHERE SET_ITEM = 'SPEC_NOTE' AND SET_TYPE IN @SPEC_NOTE)");
            }
            return (await ExecuteQueryAsync<ProjectStatisticsModel>(sql.ToString(), model)).ToList();
        }

        #endregion 表1: 每月案件統計表

        #region 表2、3: 每月案件地區/機關統計表
        /// <summary>
        /// 取得每月案件地區/機關統計表(簡版)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectAreaDeptShortModel>> GetIPCProjectAreaDeptShort(StatisticsModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine($@"
                SELECT M1.PROJECT_NO
                    ,M1.PROJECT_NAME
                    ,M1.PROJECT_YEAR
	                ,(CASE WHEN M1.FINISH_DATE > DATEADD(day,1,@YEAR_MONTH_END) THEN '4' ELSE M1.PROJECT_STATUS END) PROJECT_STATUS
                    ,M1.MASTER_ORGAN_C
                    ,dbo.FN_GetOuName(M1.MASTER_ORGAN_C, 3) AS MASTER_DEPT
	                ,M1.EXEC_ORGAN_C
	                ,dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) AS EXEC_DEPT
                    ,M1.TOWN_C
                    ,M3.TOWNNAME
                    ,M1.CP_KIND
	                ,M2.PROJECT_EXS
	                ,dbo.FN_GET_DELAY_TYPE(M1.PROJECT_NO, '', @YEAR_MONTH_END) AS DELAY_TYPE
                    ,dbo.FN_GET_PROJECT_CHECKITEM(M1.PROJECT_NO, 'A', 'A', 0) AS EST_START
                    ,dbo.FN_GET_PROJECT_CHECKITEM(M1.PROJECT_NO, 'A', 'B', 0) AS ACT_START
                    ,dbo.FN_GET_PROJECT_CHECKITEM(M1.PROJECT_NO, 'B', 'A', 0) AS EST_COM
                    ,dbo.FN_GET_PROJECT_CHECKITEM(M1.PROJECT_NO, 'B', 'B', 0) AS ACT_COM
                    ,dbo.FN_GET_PROJECT_CHECKITEM(M1.PROJECT_NO, 'C', 'A', 1) AS EST_ACPT
                    ,dbo.FN_GET_PROJECT_CHECKITEM(M1.PROJECT_NO, 'C', 'B', 0) AS ACT_ACPT
                    ,M4.OU_SORT_ORDER
                    ,M3.SORT_ORDER
                    ,M1.FINISH_DATE
                FROM PROJECT_BASIC (NOLOCK) M1
                LEFT JOIN (
	                SELECT PROJECT_NO ,SUM(BUDGET_CENTRAL + BUDGET_LOCAL) AS PROJECT_EXS
	                FROM PROJECT_BUDGET_SOURCE_G (NOLOCK)
	                GROUP BY PROJECT_NO
	            ) M2 ON M1.PROJECT_NO = M2.PROJECT_NO
                INNER JOIN CODE_TOWN (NOLOCK) M3 ON M1.TOWN_C = M3.TOWN_ID
                INNER JOIN {SC30_M}.SCORG_UNITM M4 ON M1.EXEC_ORGAN_C = M4.OU_ID
                WHERE M1.EXEC_ORGAN_C IS NOT NULL 
                    AND M1.IS_CANCELED = 0
					AND M1.CREATEDTIME < DATEADD(day,1,@YEAR_MONTH_END)
					");
            // 計畫年度
            switch (model.PROJECT_YEAR_STATUS)
            {
                case "":
                    sql.AppendLine("AND M1.PROJECT_YEAR = @PROJECT_YEAR");
                    break;
                case "A":// 含之前所有案件
                    sql.AppendLine("AND M1.PROJECT_YEAR <= @PROJECT_YEAR");
                    break;
                case "B":// 含之前未結案件
                    sql.AppendLine(@"AND (M1.PROJECT_YEAR = @PROJECT_YEAR 
	                    OR (M1.PROJECT_YEAR < @PROJECT_YEAR AND M1.PROJECT_STATUS IN ('1', '2', '3', '4', '5', '6'))
	                    OR (year(M1.FINISH_DATE)-1911 >= @PROJECT_YEAR AND  M1.PROJECT_STATUS IN ('7', '8'))
                    )");
                    break;
            }
            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine("AND M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }
            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine("AND M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }
            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sql.AppendLine("AND M1.PROJECT_NO IN (SELECT PROJECT_NO FROM PROJECT_ASST_ORG (NOLOCK) WHERE ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }
            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sql.AppendLine("AND M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }
            // 列管狀態
            if (!string.IsNullOrEmpty(model.TUBE_STATUS))
            {
                sql.AppendLine("AND dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }
            // 特殊加註
            if (model.SPEC_NOTE.Any())
            {
                sql.AppendLine("AND M1.PROJECT_NO IN (SELECT PROJECT_NO FROM PROJECT_MAPPING_DATA (NOLOCK) WHERE SET_ITEM = 'SPEC_NOTE' AND SET_TYPE IN @SPEC_NOTE)");
            }
            return (await ExecuteQueryAsync<ProjectAreaDeptShortModel>(sql.ToString(), model)).ToList();
        }

        /// <summary>
        /// 取得每月案件地區/機關統計表(詳版)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectAreaDeptDetailedModel>> GetIPCProjectAreaDeptDetailed(StatisticsModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine($@"
                SELECT M1.PROJECT_NO
                    ,M1.PROJECT_NAME
                    ,M1.PROJECT_YEAR
	                ,(CASE WHEN M1.FINISH_DATE > DATEADD(day,1,@YEAR_MONTH_END) THEN '4' ELSE M1.PROJECT_STATUS END) PROJECT_STATUS
                    ,M1.MASTER_ORGAN_C
                    ,dbo.FN_GetOuName(M1.MASTER_ORGAN_C, 3) AS MASTER_DEPT
	                ,M1.EXEC_ORGAN_C
	                ,dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) AS EXEC_DEPT
                    ,M1.TOWN_C
                    ,M2.TOWNNAME
                    ,M1.CP_KIND
                    ,dbo.FN_GET_PROJECT_EXS(M1.PROJECT_NO, 'T') AS PROJECT_EXS
	                ,dbo.FN_GET_DELAY_TYPE(M1.PROJECT_NO, '', @YEAR_MONTH_END) AS DELAY_TYPE
                    ,dbo.FN_GET_PROJECT_CHECKITEM(M1.PROJECT_NO, 'A', 'A', 0) AS EST_START
                    ,dbo.FN_GET_PROJECT_CHECKITEM(M1.PROJECT_NO, 'A', 'B', 0) AS ACT_START
                    ,dbo.FN_GET_PROJECT_CHECKITEM(M1.PROJECT_NO, 'B', 'A', 0) AS EST_COM
                    ,dbo.FN_GET_PROJECT_CHECKITEM(M1.PROJECT_NO, 'B', 'B', 0) AS ACT_COM
                    ,dbo.FN_GET_PROJECT_CHECKITEM(M1.PROJECT_NO, 'C', 'A', 1) AS EST_ACPT
                    ,dbo.FN_GET_PROJECT_CHECKITEM(M1.PROJECT_NO, 'C', 'B', 0) AS ACT_ACPT
                    ,M3.IPC_RES_PRG
                    ,M3.IPC_ACT_PRG
                    ,M3.EXECUTE_CONDITION
                    ,M4.CHECKITEM_NAME
                    ,M4.ESTIMATED_ENDDATE
                    ,M4.ACTUAL_ENDDATE
                    ,M5.DELAY_CAUSAL
                    ,dbo.FN_GET_MAPPING_DATA(M1.PROJECT_NO,'SPEC_NOTE', 1) AS SPEC_NOTE
                    ,STUFF((
                        SELECT ',' + S.SET_VALUE 
                        FROM PROJECT_BUILD_KIND (NOLOCK) B
                        INNER JOIN SET_PARAM (NOLOCK) S 
                        ON B.BUILD_KIND = S.SET_TYPE 
                        AND S.SET_ITEM = 'COM_PLANKIND'
                        WHERE PROJECT_NO = M1.PROJECT_NO
                        FOR XML PATH('')), 1, 1, '') AS CHECKPOINT_CLASS
                    ,M6.OU_SORT_ORDER
                    ,M2.SORT_ORDER
                    ,M1.FINISH_DATE
                    ,dbo.FN_GET_CHECKITEM_INFO(M1.PROJECT_NO, @YEAR_MONTH_END, 'A', 'B') AS RES_CHECKITEM
	                ,dbo.FN_GET_CHECKITEM_INFO(M1.PROJECT_NO, @YEAR_MONTH_END, 'B', 'B') AS ACT_CHECKITEM
                FROM PROJECT_BASIC (NOLOCK) M1
                INNER JOIN CODE_TOWN (NOLOCK) M2 ON M1.TOWN_C = M2.TOWN_ID
                LEFT JOIN PROJECT_ENGINEERING_PROGRESS (NOLOCK) M3 ON M3.PROJECT_NO = M1.PROJECT_NO 
                    AND M3.YEAR = @STATISTICS_YEAR 
                    AND M3.MONTH = IIF(@STATISTICS_MONTH < 10, '0' + @STATISTICS_MONTH, @STATISTICS_MONTH)
                LEFT JOIN (
                    SELECT A.PROJECT_NO
                        ,A.CHECKITEM_SEQ
                        ,A.CHECKITEM_NAME
                        ,A.PROGRESS
                        ,A.ESTIMATED_STARTDATE
                        ,A.ESTIMATED_ENDDATE
                        ,A.ACTUAL_ENDDATE
                        ,A.IS_DELAY
                        ,A.PCC_ESTIMATED_ENDDATE
                        ,A.PCC_ACTUAL_ENDDATE
	                    ,ROW_NUMBER() OVER(PARTITION BY PROJECT_NO ORDER BY A.ACTUAL_ENDDATE DESC) AS RN
                    FROM PROJECT_CHECKITEM (NOLOCK) A
                    INNER JOIN CODE_CHECKPOINT_ITEM (NOLOCK) B ON A.CHECKITEM_SEQ = B.SEQ 
                    WHERE CONVERT(VARCHAR, A.ESTIMATED_STARTDATE, 111) <= CONVERT(VARCHAR, @YEAR_MONTH_END, 111)
                        AND ISNULL(A.ACTUAL_ENDDATE,'') != ''
                ) M4 ON M1.PROJECT_NO = M4.PROJECT_NO AND M4.RN = '1'
                LEFT JOIN PROJECT_DELAY_CAUSAL (NOLOCK) M5 ON M1.PROJECT_NO = M5.PROJECT_NO 
                    AND M5.DATA_YEAR = @STATISTICS_YEAR 
                    AND M5.DATA_MONTH = IIF(@STATISTICS_MONTH < 10, '0' + @STATISTICS_MONTH, @STATISTICS_MONTH)
                INNER JOIN {SC30_M}.SCORG_UNITM M6 ON M1.EXEC_ORGAN_C = M6.OU_ID
                WHERE M1.EXEC_ORGAN_C IS NOT NULL AND M1.IS_CANCELED = 0
				AND M1.CREATEDTIME < DATEADD(day,1,@YEAR_MONTH_END)
				");

            // 年度
            switch (model.PROJECT_YEAR_STATUS)
            {
                case "":
                    sql.AppendLine("AND M1.PROJECT_YEAR = @PROJECT_YEAR");
                    break;
                case "A":// 含之前所有案件
                    sql.AppendLine("AND M1.PROJECT_YEAR <= @PROJECT_YEAR");
                    break;
                case "B":// 含之前未結案件
                    sql.AppendLine(@"AND (M1.PROJECT_YEAR = @PROJECT_YEAR 
	                    OR (M1.PROJECT_YEAR < @PROJECT_YEAR AND M1.PROJECT_STATUS IN ('1', '2', '3', '4', '5', '6'))
	                    OR (year(M1.FINISH_DATE) - 1911 >= @PROJECT_YEAR AND M1.PROJECT_STATUS IN ('7', '8'))
                    )");
                    break;
            }
            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine("AND M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }
            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine("AND M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }
            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sql.AppendLine("AND M1.PROJECT_NO IN (SELECT PROJECT_NO FROM PROJECT_ASST_ORG (NOLOCK) WHERE ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }
            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sql.AppendLine("AND M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }
            // 列管狀態
            if (!string.IsNullOrEmpty(model.TUBE_STATUS))
            {
                sql.AppendLine("AND dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }
            // 特殊加註
            if (model.SPEC_NOTE.Any())
            {
                sql.AppendLine("AND M1.PROJECT_NO IN (SELECT PROJECT_NO FROM PROJECT_MAPPING_DATA (NOLOCK) WHERE SET_ITEM = 'SPEC_NOTE' AND SET_TYPE IN @SPEC_NOTE)");
            }
            return (await ExecuteQueryAsync<ProjectAreaDeptDetailedModel>(sql.ToString(), model)).ToList();
        }
        #endregion 表2、3: 每月案件地區/機關統計表

        #region 表4: 每月案件連續落後 統計表/挑案列表
        /// <summary>
        /// 取得連續落後統計表資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<DelayStatisticsModel>> GetDelayStatistics(StatisticsModel model)
        {
            StringBuilder sql = new();
            sql.Append($@"select 
                            M3.PROJECT_NO,
                            M3.DATA_YEAR,
                            M3.DATA_MONTH,
                            M1.PROJECT_NAME,
                            M1.EXEC_ORGAN_C,
                            dbo.FN_GetOuName(M1.EXEC_ORGAN_C,3) as EXEC_ORGAN_NAME , 
                            M3.DELAY_KIND,
                            M1.PROJECT_STATUS
                        from PROJECT_DELAY_CAUSAL M3 (nolock)
                        left join PROJECT_BASIC M1 (nolock)
                            on M1.PROJECT_NO = M3.PROJECT_NO
                        where M1.EXEC_ORGAN_C is not null 
                            and M1.IS_CANCELED = 0 ");

            // 年度
            switch (model.PROJECT_YEAR_STATUS)
            {
                case "":
                    sql.AppendLine(" and M1.PROJECT_YEAR = @PROJECT_YEAR");
                    break;
                case "A":
                    sql.AppendLine(" and M1.PROJECT_YEAR <= @PROJECT_YEAR");
                    break;
                case "B":
                    sql.AppendLine(@" and (M1.PROJECT_YEAR = @PROJECT_YEAR 
	                    or (M1.PROJECT_YEAR < @PROJECT_YEAR and M1.PROJECT_STATUS in ('1', '2', '3', '4', '5', '6'))
	                    or (year(M1.FINISH_DATE) - 1911 >= @PROJECT_YEAR and M1.PROJECT_STATUS in ('7', '8'))
                    )");
                    break;
            }

            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine(" and M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }

            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine(" and M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }

            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sql.AppendLine(" and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_ASST_ORG (nolock) where ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }

            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sql.AppendLine(" and M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }

            // 列管狀態
            if (!string.IsNullOrEmpty(model.TUBE_STATUS))
            {
                sql.AppendLine("and dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }

            // 特殊加註
            if (model.SPEC_NOTE != null && model.SPEC_NOTE.Any())
            {
                sql.AppendLine(" and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_MAPPING_DATA (nolock) where SET_ITEM = 'SPEC_NOTE' and SET_TYPE in @SPEC_NOTE)");
            }

            // 執行落後類型
            if (model.DELAY_TYPE != null && model.DELAY_TYPE.Any())
            {
                sql.AppendLine(" and M3.DELAY_KIND in @DELAY_TYPE");
            }

            // 計畫狀態
            if(model.ProjectStatuses !=null && model.ProjectStatuses.Any())
            {
                sql.AppendLine(" and M1.PROJECT_STATUS in @ProjectStatuses");
            }

            return (await ExecuteQueryAsync<DelayStatisticsModel>(sql.ToString(), model)).ToList();

        }

        #region 挑案列表
        /// <summary>
        /// 取得每月案件落後挑案列表資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectDelayListModel>> GetProjectDelayList(StatisticsModel model)
        {
            StringBuilder sql = new();
            sql.Append($@"select 
                            M3.PROJECT_NO,
                            M3.DATA_YEAR,
                            M3.DATA_MONTH,
                            M1.PROJECT_NAME,
                            [dbo].[FN_GetOuName](M1.EXEC_ORGAN_C,3) as EXEC_ORGAN_NAME , 
                            [dbo].[FN_GET_PROJECT_EXS](M3.PROJECT_NO, 'T') as BUDGET,
                            M4.IPC_RES_PRG,
                            M4.IPC_ACT_PRG,
                            M4.EXECUTE_CONDITION,
                            M3.DELAY_KIND,
                            M3.DELAY_CAUSAL,
                            M5.OU_SORT_ORDER,
                            M1.CP_KIND,
	                        M6.ACTUAL_ENDDATE
                        from PROJECT_DELAY_CAUSAL M3
                        left join PROJECT_BASIC M1
                            on M1.PROJECT_NO = M3.PROJECT_NO
                        left join PROJECT_ENGINEERING_PROGRESS M4
                            on M3.PROJECT_NO = M4.PROJECT_NO 
                            and M4.YEAR = @STATISTICS_YEAR and M4.MONTH = @STATISTICS_MONTH
                        inner join {SC30_M}.SCORG_UNITM M5 on M1.EXEC_ORGAN_C = M5.OU_ID
                        left join (
	                        select PROJECT_NO, ACTUAL_ENDDATE from PROJECT_CHECKITEM m1
	                        inner join CODE_CHECKPOINT_ITEM m2 on m1.CHECKITEM_SEQ = m2.SEQ
	                        where m2.CTRL_POINT = 'A'
                        ) as M6
	                        on M6.PROJECT_NO = M1.PROJECT_NO
                        where 1 = 1 and M1.IS_CANCELED = 0 ");

            // 年度
            switch (model.PROJECT_YEAR_STATUS)
            {
                case "":
                    sql.AppendLine(" and M1.PROJECT_YEAR = @PROJECT_YEAR");
                    break;
                case "A":
                    sql.AppendLine(" and M1.PROJECT_YEAR <= @PROJECT_YEAR");
                    break;
                case "B":
                    sql.AppendLine(@" and (M1.PROJECT_YEAR = @PROJECT_YEAR 
	                    or (M1.PROJECT_YEAR < @PROJECT_YEAR and M1.PROJECT_STATUS in ('1', '2', '3', '4', '5', '6'))
	                    or (year(M1.FINISH_DATE) - 1911 >= @PROJECT_YEAR and M1.PROJECT_STATUS in ('7', '8'))
                    )");
                    break;
            }

            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine(" and M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }

            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine(" and M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }

            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sql.AppendLine(" and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_ASST_ORG (nolock) where ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }

            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sql.AppendLine(" and M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }

            // 列管狀態
            if (!string.IsNullOrEmpty(model.TUBE_STATUS))
            {
                sql.AppendLine("and dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }

            // 特殊加註
            if (model.SPEC_NOTE != null && model.SPEC_NOTE.Any())
            {
                sql.AppendLine(" and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_MAPPING_DATA (nolock) where SET_ITEM = 'SPEC_NOTE' and SET_TYPE in @SPEC_NOTE)");
            }

            // 執行落後類型
            if (model.DELAY_TYPE != null && model.DELAY_TYPE.Any())
            {
                sql.AppendLine(" and M3.DELAY_KIND in @DELAY_TYPE");
            }

            return (await ExecuteQueryAsync<ProjectDelayListModel>(sql.ToString(), model)).ToList();
        }

        /// <summary>
        /// 取得表4會議列管資料
        /// </summary>
        /// <param name="projectNos"></param>
        /// <returns></returns>
        public async Task<List<ProjectConferenceRPTModel>> GetProjectConferenceForRPT(List<string> projectNos)
        {
            string sql = @"select M1.PROJECT_NO,
                                M1.CONFERENCE_GENRE,
                                M2.SET_VALUE as CONFERENCE_NAME,
                                M1.CONFERENCE_NUM,
                                M1.CONFERENCE_TIME
                            from PROJECT_CONFERENCE M1 (nolock)
                            left join SET_PARAM M2 (nolock)
                                on M1.CONFERENCE_GENRE = M2.SET_TYPE and M2.SET_ITEM = 'COM_CONFERENCEGENRE'
                            where M1.PROJECT_NO  in @projectNos";
            return (await ExecuteQueryAsync<ProjectConferenceRPTModel>(sql, new { projectNos })).ToList();
        }

        /// <summary>
        /// 取得表4 完整檢核點資料
        /// </summary>
        /// <param name="projectNos"></param>
        /// <returns></returns>
        public async Task<List<ProjectCusCheckpointModel>> GetProjectCheckItemForRPT(List<string> projectNos)
        {
            string sql = @"select m.SEQ,
                                    m.PROJECT_NO,
                                    m.CHECKITEM_NAME,
                                    m.ESTIMATED_ENDDATE,
                                    m.ACTUAL_ENDDATE
                            from PROJECT_CHECKITEM (nolock) m
                            where PROJECT_NO in @projectNos
                            order by m.ESTIMATED_ENDDATE";
            return (await ExecuteQueryAsync<ProjectCusCheckpointModel>(sql, new { projectNos })).ToList();
        }

        #endregion
        #endregion 表4: 每月案件連續落後 統計表/挑案列表

        #region 表5: 每月未完成進度填報清單
        /// <summary>
        /// 取得計畫未完成進度填報清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectUnFilledModel>> GetProjectUnFilledList(StatisticsModel model)
        {
            StringBuilder sql = new();
            sql.Append($@"select 
                            progress.PROJECT_NO,
                            M1.PROJECT_NAME,
                            [dbo].[FN_GetOuName](M1.EXEC_ORGAN_C,3) as EXEC_ORGAN_NAME,
                            -- 若無計畫聯絡人，以執行機關人員聯絡資訊替代
                            IIF(M2.REAL_CONTACT is not null, M2.REAL_CONTACT, 
	                            (select USR_NAME from {SC30_M}.SCUSERM where USR_ID = M1.EXEC_UNDERTAKER_C)
                            ) as REAL_CONTACT,
                            IIF(M2.REAL_CONTACT is not null , M2.REAL_TEL, 
	                            (select USR_CUSTOM1 from {SC30_M}.SCUSERM where USR_ID = M1.EXEC_UNDERTAKER_C)
                            ) as REAL_TEL,
                            isnull(M1.IS_USER_FTY_DATA, 0) as IS_USER_FTY_DATA
                        from PROJECT_ENGINEERING_PROGRESS progress	(nolock)
                        left join PROJECT_BASIC M1 (nolock)
	                        on progress.PROJECT_NO = M1.PROJECT_NO
                        left join PROJECT_CONTROL_EXECUTE M2 (nolock)
	                        on progress.PROJECT_NO = M2.PROJECT_NO
                        where progress.MONTH = @STATISTICS_MONTH
                            and progress.YEAR = @STATISTICS_YEAR
                            and progress.IS_SEND = 0");

            // 年度
            switch (model.PROJECT_YEAR_STATUS)
            {
                case "":
                    sql.AppendLine("and M1.PROJECT_YEAR = @PROJECT_YEAR");
                    break;
                case "A":
                    sql.AppendLine("and M1.PROJECT_YEAR <= @PROJECT_YEAR");
                    break;
                case "B":
                    sql.AppendLine(@"and (M1.PROJECT_YEAR = @PROJECT_YEAR 
	                    or (M1.PROJECT_YEAR < @PROJECT_YEAR and M1.PROJECT_STATUS in ('1', '2', '3', '4', '5', '6'))
	                    or (year(M1.FINISH_DATE) - 1911 >= @PROJECT_YEAR and M1.PROJECT_STATUS in ('7', '8'))
                    )");
                    break;
            }

            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine("and M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }

            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine("and M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }

            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sql.AppendLine("and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_ASST_ORG (nolock) where ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }

            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sql.AppendLine("and M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }

            // 列管狀態
            if (!string.IsNullOrEmpty(model.TUBE_STATUS))
            {
                sql.AppendLine("and dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }

            // 特殊加註
            if (model.SPEC_NOTE != null && model.SPEC_NOTE.Any())
            {
                sql.AppendLine("and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_MAPPING_DATA (nolock) where SET_ITEM = 'SPEC_NOTE' and SET_TYPE in @SPEC_NOTE)");
            }

            return (await ExecuteQueryAsync<ProjectUnFilledModel>(sql.ToString(), model)).ToList();

        }
        #endregion 表5: 每月未完成進度填報清單

        #region 表6: 檢核點屆期預告
        /// <summary>
        /// 取得檢核點屆期預告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<CheckpointExpiryModel>> GetCheckItemExpiry(StatisticsModel model)
        {
            StringBuilder sql = new();
            sql.Append($@" select 
                                M1.PROJECT_NO,
                                M1.EXEC_ORGAN_C,
                                dbo.FN_GetOuName(M1.EXEC_ORGAN_C,3) as EXEC_ORGAN_NAME , 
                                M1.PROJECT_NAME,
                                M2.CHECKITEM_NAME,
                                M2.ESTIMATED_ENDDATE,
                                M2.ACTUAL_ENDDATE,
                                M3.OU_SORT_ORDER,
                                M1.PROJECT_STATUS
                            from PROJECT_BASIC M1 (nolock)
                            left join PROJECT_CHECKITEM M2 (nolock)
                                on M1.PROJECT_NO = M2.PROJECT_NO
                            inner join {SC30_M}.SCORG_UNITM M3 
                                on M1.EXEC_ORGAN_C = M3.OU_ID
                            where M1.EXEC_ORGAN_C is not null and M1.IS_CANCELED = 0 ");

            // 年度
            switch (model.PROJECT_YEAR_STATUS)
            {
                case "":
                    sql.AppendLine("and M1.PROJECT_YEAR = @PROJECT_YEAR");
                    break;
                case "A":
                    sql.AppendLine("and M1.PROJECT_YEAR <= @PROJECT_YEAR");
                    break;
                case "B":
                    sql.AppendLine(@"and (M1.PROJECT_YEAR = @PROJECT_YEAR 
	                    or (M1.PROJECT_YEAR < @PROJECT_YEAR and M1.PROJECT_STATUS in ('1', '2', '3', '4', '5', '6'))
	                    or (year(M1.FINISH_DATE) - 1911 >= @PROJECT_YEAR and M1.PROJECT_STATUS in ('7', '8'))
                    )");
                    break;
            }

            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine("and M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }

            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine("and M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }

            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sql.AppendLine("and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_ASST_ORG (nolock) where ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }

            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sql.AppendLine("and M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }

            // 列管狀態
            if (!string.IsNullOrEmpty(model.TUBE_STATUS))
            {
                sql.AppendLine("and dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }

            // 特殊加註
            if (model.SPEC_NOTE != null && model.SPEC_NOTE.Any())
            {
                sql.AppendLine("and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_MAPPING_DATA (nolock) where SET_ITEM = 'SPEC_NOTE' and SET_TYPE in @SPEC_NOTE)");
            }

            return (await ExecuteQueryAsync<CheckpointExpiryModel>(sql.ToString(), model)).ToList();

        }

        #endregion 表6: 檢核點屆期預告

        #region 表7: 特定檢核點屆期情形
        /// <summary>
        /// 取得特定檢核點屆期情形
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<SpecCheckpointExpiryModel>> GetSpecCheckpointExpiry(StatisticsModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine($@"
                select
	                M1.PROJECT_NO,
	                M1.PROJECT_NAME,
	                M1.EXEC_ORGAN_C,
	                dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) as EXEC_ORGAN_NAME,
	                M2.ESTIMATED_ENDDATE,
                    M2.ACTUAL_ENDDATE,
                    M2.CHECKITEM_NAME,
	                D1.CTRL_POINT,
                    M3.OU_SORT_ORDER,
                    M1.PROJECT_STATUS
                from PROJECT_BASIC M1 (nolock)
                inner join PROJECT_CHECKITEM M2 (nolock) on M2.PROJECT_NO = M1.PROJECT_NO
                left join CODE_CHECKPOINT_ITEM D1 (nolock) on D1.SEQ = M2.CHECKITEM_SEQ
                inner join {SC30_M}.SCORG_UNITM M3 on M1.EXEC_ORGAN_C = M3.OU_ID
                where M1.IS_CANCELED = 0  ");

            // 年度
            switch (model.PROJECT_YEAR_STATUS)
            {
                case "":
                    sql.AppendLine("and M1.PROJECT_YEAR = @PROJECT_YEAR");
                    break;
                case "A":
                    sql.AppendLine("and M1.PROJECT_YEAR <= @PROJECT_YEAR");
                    break;
                case "B":
                    sql.AppendLine(@"and (M1.PROJECT_YEAR = @PROJECT_YEAR 
	                    or (M1.PROJECT_YEAR < @PROJECT_YEAR and M1.PROJECT_STATUS in ('1', '2', '3', '4', '5', '6'))
	                    or (year(M1.FINISH_DATE) - 1911 >= @PROJECT_YEAR and M1.PROJECT_STATUS in ('7', '8'))
                    )");
                    break;
            }

            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine("and M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }

            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine("and M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }

            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sql.AppendLine("and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_ASST_ORG (nolock) where ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }

            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sql.AppendLine("and M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }

            // 列管狀態
            if (!string.IsNullOrEmpty(model.TUBE_STATUS))
            {
                sql.AppendLine("and dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }

            // 特殊加註
            if (model.SPEC_NOTE != null && model.SPEC_NOTE.Any())
            {
                sql.AppendLine("and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_MAPPING_DATA (nolock) where SET_ITEM = 'SPEC_NOTE' and SET_TYPE in @SPEC_NOTE)");
            }

            // 工作項目
            if (model.CTRL_CHK_POINT_TYPE != null && model.CTRL_CHK_POINT_TYPE.Any())
            {
                sql.AppendLine("and D1.CTRL_POINT in @CTRL_CHK_POINT_TYPE");
            }

            return (await ExecuteQueryAsync<SpecCheckpointExpiryModel>(sql.ToString(), model)).ToList();
        }
        #endregion 表7: 特定檢核點屆期情形

        #region 表8: 落後案件特定檢核點逾期情形
        /// <summary>
        /// 取得落後案件特定檢核點逾期情形
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<SpecChkPointOverdueSituationModel>> GetSpecCheckpointOverdueSituation(StatisticsModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine($@"
                select
	                M1.PROJECT_NO,
	                M1.PROJECT_NAME,
	                M1.EXEC_ORGAN_C,
	                dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) as EXEC_ORGAN_NAME,
	                M2.ESTIMATED_ENDDATE,
                    M2.ACTUAL_ENDDATE,
                    M2.CHECKITEM_NAME,
	                D1.CTRL_POINT,
                    M3.OU_SORT_ORDER,
                    M1.PROJECT_STATUS
                from PROJECT_BASIC M1 (nolock)
                inner join PROJECT_CHECKITEM M2 (nolock) on M2.PROJECT_NO = M1.PROJECT_NO
                left join CODE_CHECKPOINT_ITEM D1 (nolock) on D1.SEQ = M2.CHECKITEM_SEQ
                inner join {SC30_M}.SCORG_UNITM M3 on M1.EXEC_ORGAN_C = M3.OU_ID
                where M1.IS_CANCELED = 0 ");

            // 年度
            switch (model.PROJECT_YEAR_STATUS)
            {
                case "":
                    sql.AppendLine("and M1.PROJECT_YEAR = @PROJECT_YEAR");
                    break;
                case "A":
                    sql.AppendLine("and M1.PROJECT_YEAR <= @PROJECT_YEAR");
                    break;
                case "B":
                    sql.AppendLine(@"and (M1.PROJECT_YEAR = @PROJECT_YEAR 
	                    or (M1.PROJECT_YEAR < @PROJECT_YEAR and M1.PROJECT_STATUS in ('1', '2', '3', '4', '5', '6'))
	                    or (year(M1.FINISH_DATE) - 1911 >= @PROJECT_YEAR and M1.PROJECT_STATUS in ('7', '8'))
                    )");
                    break;
            }

            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine("and M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }

            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine("and M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }

            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sql.AppendLine("and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_ASST_ORG (nolock) where ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }

            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sql.AppendLine("and M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }

            // 列管狀態
            if (!string.IsNullOrEmpty(model.TUBE_STATUS))
            {
                sql.AppendLine("and dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }

            // 特殊加註
            if (model.SPEC_NOTE != null && model.SPEC_NOTE.Any())
            {
                sql.AppendLine("and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_MAPPING_DATA (nolock) where SET_ITEM = 'SPEC_NOTE' and SET_TYPE in @SPEC_NOTE)");
            }

            // 工作項目
            if (model.CTRL_CHK_POINT_TYPE != null && model.CTRL_CHK_POINT_TYPE.Any())
            {
                sql.AppendLine("and D1.CTRL_POINT in @CTRL_CHK_POINT_TYPE");
            }

            return (await ExecuteQueryAsync<SpecChkPointOverdueSituationModel>(sql.ToString(), model)).ToList();
        }
        #endregion 表8: 落後案件特定檢核點逾期情形

        #region 表9: 預算執行情形明細表
        /// <summary>
        /// 取得預算執行情形明細表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<IPCProjectBudgetExecModel>> GetIPCProjectBudgetExec(StatisticsModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"
                select 
	                M1.PROJECT_NO,
                    dbo.FN_GetOuName(M1.MASTER_ORGAN_C, 3) as MASTER_ORGAN_NAME,
                    dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) as EXEC_ORGAN_NAME,
	                M1.PROJECT_NAME,
	                M2.CONTROL_DATE1,
	                M2.CONTROL_DATE6,
	                dbo.FN_GET_PROJECT_EXS(M1.PROJECT_NO, 'T') as 'PROJECT_EXS',
	                D1.GT_EXPANDED_BUDGET,
	                D1.GT_TOTAL,
	                D1.GT_EXEC_RATE,
	                D1.YEAR_BUDGET_EXPANDED,
	                D1.YEAR_BUDGET_ALLOCATED,
	                D1.YEAR_EXEC_BUDGET,
	                D1.YEAR_EXEC_RATE,
	                dbo.FN_GET_MAPPING_DATA(M1.PROJECT_NO, 'IPCBGTEXECFAILED', 0) as 'IPCBGTEXECFAILED',
	                dbo.FN_GET_MAPPING_DATA(M1.PROJECT_NO, 'IPCBGTEXECFAILEDDUTY', 0) as 'IPCBGTEXECFAILEDDUTY',
	                D1.EXEC_RATE_FAILED_NOTE,
	                M2.PCC_PROJECT_NO + ' ' + M2.PCC_PROJECT_NAME as 'PCC_PROJECT_NO',
	                M2.FACTORY_CONTACT + ' ' + M2.FACTORY_TEL as 'FACTORY_CONTACT'
                from PROJECT_BASIC M1 (nolock)
                inner join PROJECT_CONTROL_EXECUTE M2 (nolock) on M2.PROJECT_NO = M1.PROJECT_NO
                left join PROJECT_BUDGET_EXECUTE D1 on D1.PROJECT_NO = M1.PROJECT_NO and D1.EXEC_YEAR = @STATISTICS_YEAR AND D1.EXEC_MONTH = @STATISTICS_MONTH
                where M1.PROJECT_STATUS > 3");

            // 年度
            switch (model.PROJECT_YEAR_STATUS)
            {
                case "":
                    sql.AppendLine("and M1.PROJECT_YEAR = @PROJECT_YEAR");
                    break;
                case "A":
                    sql.AppendLine("and M1.PROJECT_YEAR <= @PROJECT_YEAR");
                    break;
                case "B":
                    sql.AppendLine(@"and (M1.PROJECT_YEAR = @PROJECT_YEAR 
	                    or (M1.PROJECT_YEAR < @PROJECT_YEAR and M1.PROJECT_STATUS in ('1', '2', '3', '4', '5', '6'))
	                    or (year(M1.FINISH_DATE) - 1911 >= @PROJECT_YEAR and M1.PROJECT_STATUS in ('7', '8'))
                    )");
                    break;
            }

            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine("and M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }

            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine("and M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }

            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sql.AppendLine("and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_ASST_ORG (nolock) where ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }

            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sql.AppendLine("and M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }

            // 列管狀態
            if (!string.IsNullOrEmpty(model.TUBE_STATUS))
            {
                sql.AppendLine("and dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }

            // 特殊加註
            if (model.SPEC_NOTE != null && model.SPEC_NOTE.Any())
            {
                sql.AppendLine("and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_MAPPING_DATA (nolock) where SET_ITEM = 'SPEC_NOTE' and SET_TYPE in @SPEC_NOTE)");
            }

            // 計畫總經費(元) 開始
            if (model.PROJECT_EXS_S.HasValue)
            {
                sql.AppendLine("and @PROJECT_EXS_S <= dbo.FN_GET_PROJECT_EXS(M1.PROJECT_NO, 'T')");
            }

            // 計畫總經費(元) 結束
            if (model.PROJECT_EXS_E.HasValue)
            {
                sql.AppendLine("and dbo.FN_GET_PROJECT_EXS(M1.PROJECT_NO, 'T') <= @PROJECT_EXS_E");
            }

            // 執行率
            switch (model.EXEC_RATE)
            {
                case "S2": // 當年度執行率未達80%
                    sql.AppendLine(@"and 
                        (case
	                        when isnull(D1.YEAR_BUDGET_ALLOCATED, 0) = 0 
                            then 0
	                        else isnull(D1.YEAR_EXEC_BUDGET, 0) / isnull(D1.YEAR_BUDGET_ALLOCATED, 0)
	                     end) < 0.8");
                    break;
                case "S3": // 累計執行率未達80%
                    sql.AppendLine(@"and 
                         (case 
	                        when isnull(D1.GT_EXPANDED_BUDGET, 0) = 0 
                            then 0
	                        else (isnull(D1.GT_ACT_BUDGET, 0) + isnull(D1.GT_AP, 0) + isnull(D1.GT_BALANCE, 0)) / isnull(D1.GT_EXPANDED_BUDGET, 0)
	                      end) < 0.8");
                    break;
            }

            return (await ExecuteQueryAsync<IPCProjectBudgetExecModel>(sql.ToString(), model)).ToList();
        }
        #endregion 表9: 預算執行情形明細表

        #region 表10: 選項列管案件計畫歷次調整審查表

        /// <summary>
        /// 取得 表10選項列管案件計畫歷次調整審查表 資料
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <returns></returns>
        public async Task<List<ProjectAdjustDetailedModel>> GetProjAdjDetailed(string PROJECT_NO)
        {
            string sql = @" SELECT a.PROJECT_NO
	                            ,a.PROJECT_NAME
	                            ,dbo.FN_GET_PROJECT_EXS(a.PROJECT_NO, 'T') AS TOTAL_BUDGET
	                            ,dbo.FN_GetOuName(a.EXEC_ORGAN_C, 3) AS EXEC_ORGAN_NAME
	                            ,b.PROJ_ADJ_ID
                                ,b.LOG_ID
	                            ,b.RUNWAY_C_ORI
	                            ,b.RUNWAY_C
                            FROM PROJECT_BASIC(NOLOCK) a
                            LEFT JOIN PROJECT_BASIC_ADJ(NOLOCK) b ON a.PROJECT_NO = b.PROJECT_NO
	                            AND b.PROJECT_AW_STATUS = 'B05' --期程調整審核通過
                            WHERE a.PROJECT_NO = @PROJECT_NO
                            ORDER BY b.PROJ_ADJ_ID ";
            return (await ExecuteQueryAsync<ProjectAdjustDetailedModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 取得屬於工程類的計畫
        /// </summary>
        /// <returns>屬於工程類的計畫清單(PROJECT_NO: 計畫編號、PROJ_ADJ_ID: 最近一次的調整流水號)</returns>
        public async Task<List<object>> GetEngineeringProjects()
        {
            string sql = @" SELECT 
	                            a.PROJECT_NO,
	                            a.CP_KIND,
	                            b.PROJ_ADJ_ID
                            FROM PROJECT_BASIC(NOLOCK) a
                            LEFT JOIN (
	                            SELECT MAX(PROJ_ADJ_ID) AS PROJ_ADJ_ID
		                            ,PROJECT_NO
	                            FROM PROJECT_BASIC_ADJ(NOLOCK)
	                            WHERE DEL_FLG != 1 AND PROJECT_AW_STATUS = 'B05'
	                            GROUP BY PROJECT_NO
	                            ) b ON a.PROJECT_NO = b.PROJECT_NO
                            WHERE a.IS_CANCELED = 0";
            return (await ExecuteQueryAsync<object>(sql)).ToList();
        }

        #endregion 表10: 選項列管案件計畫歷次調整審查表

        #region 表11: 年終考核案件成績表
        /// <summary>
        /// 取得年終考核案件成績表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<IPCProjectFillYearAssModel>> GetIPCProjectFillYearAss(StatisticsModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"
                SELECT M1.PROJECT_NO
                    ,M1.PROJECT_NAME
	                ,M1.PROJECT_STATUS
                    ,dbo.FN_GET_PROJECT_EXS(M1.PROJECT_NO, 'T') AS PROJECT_EXS
	                ,M1.EXEC_ORGAN_C
	                ,dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) AS EXEC_DEPT
                    ,M1.CP_KIND
                    ,dbo.FN_GetSetParam('CP_KIND', M1.CP_KIND) AS CP_KIND_NAME
                    ,checkItem.ACTUAL_ENDDATE
                    ,M1.FINISH_DATE
                    ,(CAST(DATEDIFF(DAY , checkItem.ACTUAL_ENDDATE, M1.FINISH_DATE) / 30.0 as decimal(38, 1))) AS DIFF_MONTH
	                ,M1.SCORE_A
                    ,ISNULL(progress.OVERDUE_DAY, 0) AS OVERDUE_DAY
                    ,ISNULL(close1.TYPE_1_CNT, 0) AS TYPE_1_CNT
                    ,ISNULL(scheY.SCHE_Y_CNT, 0) AS SCHE_Y_TOTCNT
                    ,ISNULL(scheM.SCHE_M_CNT, 0) AS SCHE_M_TOTCNT
                    ,fact.AVG_FFSCORE
                    ,DATEDIFF(MONTH, M1.CREATEDTIME, M1.PROJECT_LAST_DATE) AS EX_MONTH
                    ,CASE WHEN delayCausal.DELAY_CNT IS NULL THEN 0 ELSE delayCausal.DELAY_CNT END AS DELAY_CNT
                    ,payment.ACTUAL_PAY
                    ,payment.UNPAY
                    ,payment.BALANCE
                    ,ISNULL(close2.TYPE_2_CNT, 0) AS TYPE_2_CNT
                    ,ISNULL(delayApply.DELAY_APPLY_CNT, 0) AS DELAY_APPLY_CNT
                    ,ISNULL(close4.TYPE_4_CNT, 0) AS TYPE_4_CNT
                    ,ISNULL(mergeLog.MERGE_1_CNT, 0) AS MERGE_1_CNT
                    ,exe.CONTROL_DATE1
                    ,M1.PROJECT_LAST_DATE
                FROM PROJECT_BASIC (NOLOCK) M1
                LEFT JOIN PROJECT_CONTROL_EXECUTE (NOLOCK) exe on M1.PROJECT_NO = exe.PROJECT_NO
                LEFT JOIN (
	                SELECT PROJECT_NO, ACTUAL_ENDDATE 
	                FROM PROJECT_CHECKITEM (NOLOCK)
	                WHERE PROGRESS = 100
                ) checkItem ON M1.PROJECT_NO = checkItem.PROJECT_NO
                LEFT JOIN (
	                SELECT PROJECT_NO, SUM(OVERDUE_DAY) AS OVERDUE_DAY
	                FROM PROJECT_ENGINEERING_PROGRESS (NOLOCK)
	                GROUP BY PROJECT_NO
                ) progress ON M1.PROJECT_NO = progress.PROJECT_NO
                LEFT JOIN (
	                SELECT PROJECT_NO, CLOSE_DETAILS_TYPE, COUNT(PROJECT_NO) AS TYPE_1_CNT
	                FROM PROJECT_CLOSE_DETAILS (NOLOCK) 
	                WHERE CLOSE_DETAILS_TYPE = '1'
	                GROUP BY PROJECT_NO, CLOSE_DETAILS_TYPE
                ) close1 ON M1.PROJECT_NO = close1.PROJECT_NO
                LEFT JOIN (
	                SELECT PROJECT_NO, SCHE_TYPE, COUNT(SCHE_TYPE) AS SCHE_M_CNT
	                FROM PROJECT_BASIC_ADJ (NOLOCK)
	                WHERE SCHE_TYPE = 'M' AND DEL_FLG = 0 AND PROJECT_AW_STATUS = 'B05'
	                GROUP BY PROJECT_NO, SCHE_TYPE
                ) scheM ON M1.PROJECT_NO = scheM.PROJECT_NO
                LEFT JOIN (
	                SELECT PROJECT_NO, SCHE_TYPE, COUNT(SCHE_TYPE) AS SCHE_Y_CNT
	                FROM PROJECT_BASIC_ADJ (NOLOCK)
	                WHERE SCHE_TYPE = 'Y' AND DEL_FLG = 0 AND PROJECT_AW_STATUS = 'B05'
	                GROUP BY PROJECT_NO, SCHE_TYPE
                ) scheY ON M1.PROJECT_NO = scheY.PROJECT_NO
                LEFT JOIN(
	                SELECT PROJECT_NO, AVG(FFSCORE) AS AVG_FFSCORE
	                FROM PROJECT_FACT_FINDING (NOLOCK)
	                GROUP BY PROJECT_NO
                ) fact ON M1.PROJECT_NO = fact.PROJECT_NO
                LEFT JOIN(
	                SELECT PROJECT_NO, COUNT(PROJECT_NO) AS DELAY_CNT
	                FROM PROJECT_DELAY_CAUSAL (NOLOCK)
	                GROUP BY PROJECT_NO
                ) delayCausal ON M1.PROJECT_NO = delayCausal.PROJECT_NO
                LEFT JOIN (
                    SELECT  
	                    PROJECT_NO, ACTUAL_PAY, UNPAY, BALANCE ,
	                    ROW_NUMBER() OVER ( PARTITION BY PROJECT_NO ORDER BY IDENTITY_FIELD DESC ) AS RN
                    FROM PROJECT_PAYMENT (NOLOCK)
                ) payment ON M1.PROJECT_NO = payment.PROJECT_NO AND RN = 1
                LEFT JOIN (
	                SELECT PROJECT_NO, CLOSE_DETAILS_TYPE, COUNT(PROJECT_NO) AS TYPE_2_CNT
	                FROM PROJECT_CLOSE_DETAILS (NOLOCK) 
	                WHERE CLOSE_DETAILS_TYPE = '2'
	                GROUP BY PROJECT_NO, CLOSE_DETAILS_TYPE
                ) close2 ON M1.PROJECT_NO = close2.PROJECT_NO
                LEFT JOIN (
	                SELECT PROJECT_NO, COUNT(PROJECT_NO) AS DELAY_APPLY_CNT
	                FROM PROJECT_BASIC_ADJ (NOLOCK)
	                WHERE IS_DELAY_APPLY = 1 AND DEL_FLG = 0
	                GROUP BY PROJECT_NO
                ) delayApply ON M1.PROJECT_NO = delayApply.PROJECT_NO
                LEFT JOIN (
	                SELECT PROJECT_NO, CLOSE_DETAILS_TYPE, COUNT(PROJECT_NO) AS TYPE_4_CNT
	                FROM PROJECT_CLOSE_DETAILS (NOLOCK) 
	                WHERE CLOSE_DETAILS_TYPE = '4'
	                GROUP BY PROJECT_NO, CLOSE_DETAILS_TYPE
                ) close4 ON M1.PROJECT_NO = close4.PROJECT_NO
                LEFT JOIN (
	                SELECT PROJECT_NO, MERGE_STATUS, COUNT(PROJECT_NO) AS MERGE_1_CNT
	                FROM PROJECT_MERGE_LOG (NOLOCK) 
	                WHERE MERGE_STATUS = '01'
	                GROUP BY PROJECT_NO, MERGE_STATUS
                ) mergeLog ON M1.PROJECT_NO = mergeLog.PROJECT_NO
                WHERE M1.EXEC_ORGAN_C IS NOT NULL AND M1.IS_CANCELED = 0");

            // 計畫年度
            sql.AppendLine("AND M1.PROJECT_YEAR BETWEEN @PROJECT_YEAR AND @PROJECT_YEAR_E");
            // 結案年月 列管狀態選擇已結案才須加上條件
            if (model.TUBE_STATUS.Equals("S2"))
            {
                sql.AppendLine("AND CONVERT(VARCHAR, M1.FINISH_DATE, 111) BETWEEN CONVERT(VARCHAR, @CLOSE_START, 111) AND CONVERT(VARCHAR, @CLOSE_END, 111)");
            }
            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine("AND M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }
            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine("AND M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }
            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sql.AppendLine("AND M1.PROJECT_NO IN (SELECT PROJECT_NO FROM PROJECT_ASST_ORG (NOLOCK) WHERE ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }
            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sql.AppendLine("AND M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }
            // 列管狀態
            if (!string.IsNullOrEmpty(model.TUBE_STATUS))
            {
                sql.AppendLine("AND dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }
            // 特殊加註
            if (model.SPEC_NOTE.Any())
            {
                sql.AppendLine("AND M1.PROJECT_NO IN (SELECT PROJECT_NO FROM PROJECT_MAPPING_DATA (NOLOCK) WHERE SET_ITEM = 'SPEC_NOTE' AND SET_TYPE IN @SPEC_NOTE)");
            }
            return (await ExecuteQueryAsync<IPCProjectFillYearAssModel>(sql.ToString(), model)).ToList();
        }

        /// <summary>
        /// 取得所有計畫檢核點
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProjectCusCheckpointModel>> GetProjectCusCheckpointList()
        {
            string sql = $@"SELECT SEQ,
                                  PROJECT_NO,
                                  PROGRESS,
                                  ESTIMATED_STARTDATE,
                                  ESTIMATED_ENDDATE,
                                  ACTUAL_ENDDATE,
                                  MDF_DATE
                           FROM PROJECT_CHECKITEM (NOLOCK)";
            return (await ExecuteQueryAsync<ProjectCusCheckpointModel>(sql)).ToList();
        }

        /// <summary>
        /// 取得所有計畫每月辦理情形進度
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectEngineeringProgressGridModel>> GetProjecFillExecuteList()
        {
            string sql = @"
                SELECT 
                    M2.PROJECT_NO,
                    CAST(M2.YEAR AS INT) + 1911 AS YEAR,
                    M2.MONTH,
                    M2.IPC_ACT_PRG
                FROM PROJECT_BASIC (NOLOCK) M1 
                INNER JOIN PROJECT_ENGINEERING_PROGRESS (NOLOCK) M2 
                    ON M1.PROJECT_NO = M2.PROJECT_NO
                INNER JOIN PROJECT_FILL_CYCLE (NOLOCK) M3 
				    ON M3.PROJECT_YEAR = M2.YEAR AND M3.PROJECT_MONTH = M2.MONTH
                ORDER BY M1.PROJECT_NO, M3.SEQ DESC";
            return (await ExecuteQueryAsync<ProjectEngineeringProgressGridModel>(sql)).ToList();
        }

        /// <summary>
        /// 取得所有落後計畫
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectDelayCausalModel>> GetProjectDelayCausalList()
        {
            string sql = @"
                SELECT 
                    PROJECT_NO,
                    CAST(DATA_YEAR AS INT) + 1911 AS DATA_YEAR,
                    DATA_MONTH
                FROM PROJECT_DELAY_CAUSAL (NOLOCK)
                GROUP BY PROJECT_NO, DATA_YEAR, DATA_MONTH --防止計畫出現同筆年月資料
				ORDER BY PROJECT_NO, DATA_YEAR, DATA_MONTH";
            return (await ExecuteQueryAsync<ProjectDelayCausalModel>(sql)).ToList();
        }
        #endregion 表11: 年終考核案件成績表

        #region 表12 平時管考意見備註統計表
        /// <summary>
        /// 取得平時管考意見備註統計表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectComIpcMemoModel>> GetProjectComIpcMemoList(StatisticsModel model)
        {
            StringBuilder sql = new();
            sql.Append($@"SELECT M1.PROJECT_NO,
	                              M1.PROJECT_NAME,
	                              M2.YEAR,
	                              M2.MONTH,
	                              M3.SET_TYPE as COM_IPCMEMO,
	                              M1.EXEC_ORGAN_C ,
	                              dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) AS EXEC_ORGAN_NAME
                            FROM PROJECT_ENGINEERING_AUDIT_OPINION M2
                            INNER JOIN PROJECT_BASIC M1 
                                ON M1.PROJECT_NO =M2.PROJECT_NO
                            INNER JOIN PROJECT_MAPPING_DATA M3  
  	                            ON  M3.PROJECT_NO =M2.PROJECT_NO 
  	                            AND M3.SET_ITEM = 'COM_IPCMEMO' --管考意見備註
  	                            AND M3.SOURCE_ID = M2.SEQ 
                            INNER JOIN {SC30_M}.SCORG_UNITM M4
                                ON M1.EXEC_ORGAN_C = M4.OU_ID
                            WHERE M1.EXEC_ORGAN_C IS NOT NULL
                                AND M2.YEAR = @STATISTICS_YEAR 
                                AND M2.MONTH = IIF(@STATISTICS_MONTH < 10, '0' + @STATISTICS_MONTH, @STATISTICS_MONTH)
                            ");
            // 年度
            switch (model.PROJECT_YEAR_STATUS)
            {
                case "":
                    sql.AppendLine(" and M1.PROJECT_YEAR = @PROJECT_YEAR");
                    break;
                case "A":
                    sql.AppendLine(" and M1.PROJECT_YEAR <= @PROJECT_YEAR");
                    break;
                case "B":
                    sql.AppendLine(@" and (M1.PROJECT_YEAR = @PROJECT_YEAR 
	                    or (M1.PROJECT_YEAR < @PROJECT_YEAR and M1.PROJECT_STATUS in ('1', '2', '3', '4', '5', '6'))
	                    or (year(M1.FINISH_DATE) - 1911 >= @PROJECT_YEAR and M1.PROJECT_STATUS in ('7', '8'))
                    )");
                    break;
            }

            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine(" AND M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }
            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine(" AND M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }
            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sql.AppendLine("AND M1.PROJECT_NO IN (SELECT PROJECT_NO FROM PROJECT_ASST_ORG (NOLOCK) WHERE ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }
            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sql.AppendLine("AND M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }
            // 列管狀態
            if (!string.IsNullOrEmpty(model.TUBE_STATUS))
            {
                sql.AppendLine("AND dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }
            // 特殊加註
            if (model.SPEC_NOTE.Any())
            {
                sql.AppendLine("AND M1.PROJECT_NO IN (SELECT PROJECT_NO FROM PROJECT_MAPPING_DATA (NOLOCK) WHERE SET_ITEM = 'SPEC_NOTE' AND SET_TYPE IN @SPEC_NOTE)");
            }

            sql.AppendLine("ORDER BY M4.OU_SORT_ORDER, M1.PROJECT_NO");

            return (await ExecuteQueryAsync<ProjectComIpcMemoModel>(sql.ToString(), model)).ToList();
        }

        /// <summary>
        /// 取得各計畫未完成的第一項檢核點
        /// </summary>
        /// <returns></returns>
        public async Task<List<CheckpointExpiryModel>> GetProjectFirstUnFilledCheckPoint(List<string> projectNos)
        {
            string sql = $@";WITH cte AS
                            (
                                SELECT M1.PROJECT_NO,
   	                                    M2.CTRL_POINT,
   	                                    M1.CHECKITEM_NAME,
   	                                    M1.ESTIMATED_ENDDATE,
                                        ROW_NUMBER() OVER (PARTITION BY PROJECT_NO ORDER BY M1.PROGRESS ) AS rn
                                FROM PROJECT_CHECKITEM M1
                                INNER JOIN CODE_CHECKPOINT_ITEM M2 
                                ON M2.SEQ = M1.CHECKITEM_SEQ
                                WHERE M1.PROGRESS IS NOT NULL 
                                    AND ACTUAL_ENDDATE IS NULL 
                                    AND ESTIMATED_ENDDATE IS NOT NULL
                            )
                            SELECT M1.PROJECT_NO,
	                                M1.CTRL_POINT,
	                                M1.CHECKITEM_NAME, 
	                                M1.ESTIMATED_ENDDATE,
	                                M2.EXEC_ORGAN_C,
	                                dbo.FN_GetOuName(M2.EXEC_ORGAN_C, 3) AS EXEC_ORGAN_NAME
	   
                            FROM cte M1
                            INNER JOIN PROJECT_BASIC M2
	                            ON M1.PROJECT_NO = M2.PROJECT_NO
                            INNER JOIN {SC30_M}.SCORG_UNITM M3
	                            ON M2.EXEC_ORGAN_C = M3.OU_ID
                            WHERE rn = 1 
                                and M1.PROJECT_NO in @projectNos";
            return (await ExecuteQueryAsync<CheckpointExpiryModel>(sql, new { projectNos })).ToList();
        }

        /// <summary>
        /// 取得近三個月計畫工程進度
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectEngPrgRPTModel>> GetProjectProgressWithinThreeMonths(int year, int month, List<string> projectNos)
        {
            // 3個月統計條件
            string conditionWithinThreeMonths = GetConditionWithinNMonths(year, month, 3);
            string sql = $@"select SEQ,
                                   PEP.PROJECT_NO, 
	                               YEAR, 
	                               MONTH,
	                               IPC_RES_PRG,
	                               IPC_ACT_PRG,
                                   M1.EXEC_ORGAN_C
                            from PROJECT_ENGINEERING_PROGRESS PEP  (nolock)
                            inner join PROJECT_BASIC M1 (nolock)
                                on PEP.PROJECT_NO = M1.PROJECT_NO
                            where IPC_RES_PRG is not null
                                and PEP.PROJECT_NO in @projectNos
                            {conditionWithinThreeMonths}";
            return (await ExecuteQueryAsync<ProjectEngPrgRPTModel>(sql, new { projectNos })).ToList();
        }

        /// <summary>
        /// 取得N個月內統計條件
        /// </summary>
        /// <param name="startYear"></param>
        /// <param name="startMonth"></param>
        /// <param name="statPeriod">統計N個月</param>
        /// <returns></returns>
        private string GetConditionWithinNMonths(int startYear, int startMonth, int statPeriod)
        {
            // 起始統計年月
            DateTime startYM = new DateTime(startYear + 1911, startMonth, 1);
            string[] conditions = new string[statPeriod];
            // 組織前N個月查詢條件
            for (int i = 0; i < statPeriod; i++)
            {
                DateTime statYM = startYM.AddMonths(-i);
                conditions[i] = $"(YEAR = {statYM.Year - 1911} AND MONTH = {statYM.Month:D2})";
            }
            return $" AND ({string.Join(" OR ", conditions)})";
        }
        #endregion 平時管考意見備註統計表

        #region 表13 重大建設系統介接公共工程雲端服務網資料統計表
        public async Task<List<ProjectSyncLogModel>> GetProjectSyncLogList(StatisticsModel model)
        {
            StringBuilder sql = new();
            sql.Append($@"SELECT
                            M1.IS_USER_FTY_DATA,
                            M1.PROJECT_YEAR AS PROJECT_YEAR,
                            M1.PROJECT_NO,
                            M1.PROJECT_NAME,
                            M1.EXEC_ORGAN_C,
                            M2.GDB_ERROR_ITEM,
                            dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) AS EXEC_ORGAN_NAME,
                            dbo.FN_GET_PROJ_STAGE(M1.PROJECT_NO) AS PROJECT_STAGE,
                            M3.IPC_RES_PRG, 
                            M3.IPC_ACT_PRG,
                            VW1.B_ACTUAL_ENDDATE AS COMPLETION_ACTUAL_ENDDATE
                          FROM
                            PROJECT_BASIC M1 (NOLOCK)
                          LEFT JOIN
                            PROJECT_SYNC_LOG M2 (NOLOCK)
                          ON
                            M1.PROJECT_NO = M2.PROJECT_NO
                          INNER JOIN
                            PROJECT_ENGINEERING_PROGRESS (nolock) M3
                          ON
                              M1.PROJECT_NO = M3.PROJECT_NO
                          AND
	                          M3.[YEAR] = @STATISTICS_YEAR
                          AND
	                          M3.[MONTH] = @STATISTICS_MONTH            
                          LEFT JOIN
                              VW_PROJECT_CHECKITEM_DATE (nolock) VW1
                          ON
                              M1.PROJECT_NO = VW1.PROJECT_NO
                          WHERE
                              M1.EXEC_ORGAN_C IS NOT NULL
                          AND
                              M2.MONTH = @STATISTICS_MONTH
                          AND
                              dbo.FN_GET_PROJ_STAGE(M1.PROJECT_NO) IN ('E1', 'E2', 'E3', 'S1')
                          ");
            // 年度代號，A:含之前所有案件，B:含之前未結案件
            switch (model.PROJECT_YEAR_STATUS)
            {
                case "":
                    sql.AppendLine(" AND M1.PROJECT_YEAR = @PROJECT_YEAR");
                    break;
                case "A":
                    sql.AppendLine(" AND M1.PROJECT_YEAR <= @PROJECT_YEAR");
                    break;
                case "B":
                    sql.AppendLine(@"AND (
                                     M1.PROJECT_YEAR = @PROJECT_YEAR 
                                  OR (M1.PROJECT_YEAR < @PROJECT_YEAR
                                     AND M1.PROJECT_STATUS IN ('1', '2', '3', '4', '5', '6'))
                                  OR (DATEDIFF(YEAR, year(M1.FINISH_DATE) , 1911) >= @PROJECT_YEAR
                                     AND M1.PROJECT_STATUS IN ('7', '8'))
                                    )");
                    break;
            }

            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine(" AND M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }
            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine(" AND M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }
            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sql.AppendLine(@" AND M1.PROJECT_NO
                                 IN (SELECT PROJECT_NO
                                 FROM PROJECT_ASST_ORG (NOLOCK)
                                 WHERE ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }
            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sql.AppendLine(" AND M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }
            // 列管狀態
            if (!string.IsNullOrEmpty(model.TUBE_STATUS))
            {
                sql.AppendLine(" AND dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }
            // 特殊加註
            if (model.SPEC_NOTE.Any())
            {
                sql.AppendLine(@" AND M1.PROJECT_NO
                                  IN (SELECT PROJECT_NO
                                      FROM PROJECT_MAPPING_DATA (NOLOCK)
                                      WHERE SET_ITEM = 'SPEC_NOTE'
                                      AND SET_TYPE IN @SPEC_NOTE)");
            }

            return (await ExecuteQueryAsync<ProjectSyncLogModel>(sql.ToString(), model)).ToList();
        }

        #endregion 重大建設系統介接公共工程雲端服務網資料統計表

        // 表13 重大建設系統介接公共工程雲雲端服務網資料一覽表
        public async Task<List<ProjectSyncLogOverviewModel>> GetProjectSyncLogOverviewList(StatisticsModel model)
        {
            StringBuilder sql = new();
            sql.Append($@"select
                            M3.GDB_ERROR_ITEM as GDB_ERROR_ITEM,
                            M1.PROJECT_NAME as PROJECT_NAME,
                            M1.PROJECT_NO as PROJECT_NO,
                            M2.PCC_PROJECT_NO as PCC_PROJECT_NO, 
                            M2.PCC_PROJECT_UID as PCC_PROJECT_UID,
                            dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) AS EXEC_ORGAN_NAME,
                            M5.EXECUTE_CONDITION as EXECUTE_CONDITION,
                            M5.ASSISTANT_ITEM as ASSISTANT_ITEM,
                            VW1.A_ESTIMATED_ENDDATE AS START_ESTIMATED_ENDDATE,
                            VW1.A_ACTUAL_ENDDATE AS START_ACTUAL_ENDDATE,
                            VW1.B_ESTIMATED_ENDDATE AS COMPLETION_ESTIMATED_ENDDATE,
                            VW1.B_ACTUAL_ENDDATE AS COMPLETION_ACTUAL_ENDDATE,
                            VW1.C_ESTIMATED_ENDDATE AS ACCEPT_ESTIMATED_ENDDATE, 
                            VW1.C_ACTUAL_ENDDATE AS ACCEPT_ACTUAL_ENDDATE, 
                            VW1.A_PCC_ESTIMATED_ENDDATE AS START_PCC_ESTIMATED_ENDDATE,
                            VW1.A_PCC_ACTUAL_ENDDATE AS START_PCC_ACTUAL_ENDDATE,
                            VW1.B_PCC_ESTIMATED_ENDDATE AS COMPLETION_PCC_ESTIMATED_ENDDATE,
                            VW1.B_PCC_ACTUAL_ENDDATE AS COMPLETION_PCC_ACTUAL_ENDDATE,
                            M5.IPC_RES_PRG as IPC_RES_PRG, 
                            M5.IPC_ACT_PRG as IPC_ACT_PRG,
                            M5.TEN_RES_PRG as TEN_RES_PRG,
                            M5.TEN_ACT_PRG as TEN_ACT_PRG,
                            dbo.FN_GetSetParam('DELAY_CLASS', M6.DELAY_KIND) AS DELAY_KIND,
                            dbo.FN_GetSetParam('DELAY_TYPE', M6.DELAY_CLASS_C) AS DELAY_CLASS_C,
                            dbo.FN_GET_IPC_PARAM(M6.DELAY_SUBCLASS_C, 'D') AS DELAY_SUBCLASS_C,
                            M6.DELAY_RESPON as DELAY_RESPON,
                            M6.DELAY_CAUSAL as DELAY_CAUSAL,
                            M6.SOLUTION as SOLUTION,
                            M6.COORDINATION as COORDINATION,
                            M6.DEADLINES as DEADLINES
                        from
                            PROJECT_BASIC (nolock) M1
                        left join
                            PROJECT_CONTROL_EXECUTE (nolock) M2
                        on
                            M1.PROJECT_NO = M2.PROJECT_NO
                        inner join 
                            PROJECT_SYNC_LOG (nolock) M3
                        on
                            M1.PROJECT_NO = M3.PROJECT_NO
                        inner join
                            PROJECT_ENGINEERING_PROGRESS (nolock) M5
                        on
                            M1.PROJECT_NO = M5.PROJECT_NO
                        and
	                        M5.[YEAR] = @STATISTICS_YEAR
                        and
	                        M5.[MONTH] = @STATISTICS_MONTH
                        left join
                            PROJECT_DELAY_CAUSAL (nolock) M6
                        on
                            M1.PROJECT_NO = M6.PROJECT_NO
                        left join
                            VW_PROJECT_CHECKITEM_DATE (nolock) VW1
                        on
                            M1.PROJECT_NO = VW1.PROJECT_NO
                        where
                            M1.IS_USER_FTY_DATA = '1'
                        and
                            M3.MONTH = @STATISTICS_MONTH
                        and
                            M6.DATA_YEAR = @STATISTICS_YEAR
                        and
                            M6.DATA_MONTH = @STATISTICS_MONTH
                        and
                            M1.IS_CANCELED = 0
                          ");
            // 年度代號，A:含之前所有案件，B:含之前未結案件
            switch (model.PROJECT_YEAR_STATUS)
            {
                case "":
                    sql.AppendLine(" AND M1.PROJECT_YEAR = @PROJECT_YEAR");
                    break;
                case "A":
                    sql.AppendLine(" AND M1.PROJECT_YEAR <= @PROJECT_YEAR");
                    break;
                case "B":
                    sql.AppendLine(@"AND (
                                     M1.PROJECT_YEAR = @PROJECT_YEAR 
                                  OR (M1.PROJECT_YEAR < @PROJECT_YEAR
                                     AND M1.PROJECT_STATUS IN ('1', '2', '3', '4', '5', '6'))
                                  OR (DATEDIFF(YEAR, year(M1.FINISH_DATE) , 1911) >= @PROJECT_YEAR
                                     AND M1.PROJECT_STATUS IN ('7', '8'))
                                    )");
                    break;
            }
            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine(" AND M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }
            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine(" AND M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }
            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sql.AppendLine(@" AND M1.PROJECT_NO
                                 IN (SELECT PROJECT_NO
                                 FROM PROJECT_ASST_ORG (NOLOCK)
                                 WHERE ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }
            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sql.AppendLine(" AND M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }
            // 列管狀態
            if (!string.IsNullOrEmpty(model.TUBE_STATUS))
            {
                sql.AppendLine(" AND dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }
            // 特殊加註
            if (model.SPEC_NOTE.Any())
            {
                sql.AppendLine(@" AND M1.PROJECT_NO
                                  IN (SELECT PROJECT_NO
                                      FROM PROJECT_MAPPING_DATA (NOLOCK)
                                      WHERE SET_ITEM = 'SPEC_NOTE'
                                      AND SET_TYPE IN @SPEC_NOTE)");
            }
            
            return (await ExecuteQueryAsync<ProjectSyncLogOverviewModel>(sql.ToString(), model)).ToList();
        }
            #region 共用
            /// <summary>
            /// 取得機關列管件數
            /// </summary>
            /// <param name="model"></param>
            /// <returns></returns>
            public async Task<List<OrgProjectCntModel>> GetExecOrgProjectCnt(StatisticsModel model)
        {
            StringBuilder sql = new();
            sql.Append($@"select COUNT(PROJECT_NO) as ProjectCnt,
                                EXEC_ORGAN_C as OrgId,
                                dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) as OrgName,
                                M3.OU_SORT_ORDER
                         from PROJECT_BASIC M1 
                         inner join {SC30_M}.SCORG_UNITM M3 
                            on M1.EXEC_ORGAN_C = M3.OU_ID
                         where EXEC_ORGAN_C is not null
                            and M1.IS_CANCELED = 0 
                            and M1.CREATEDTIME < DATEADD(day,1,@YEAR_MONTH_END)");

            // 年度
            switch (model.PROJECT_YEAR_STATUS)
            {
                case "":
                    sql.AppendLine(" and M1.PROJECT_YEAR = @PROJECT_YEAR");
                    break;
                case "A":
                    sql.AppendLine(" and M1.PROJECT_YEAR <= @PROJECT_YEAR");
                    break;
                case "B":
                    sql.AppendLine(@"and (M1.PROJECT_YEAR = @PROJECT_YEAR 
	                    or (M1.PROJECT_YEAR < @PROJECT_YEAR and M1.PROJECT_STATUS in ('1', '2', '3', '4', '5', '6'))
	                    or (year(M1.FINISH_DATE) - 1911 >= @PROJECT_YEAR and M1.PROJECT_STATUS in ('7', '8'))
                    )");
                    break;
            }

            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine(" and M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }

            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine(" and M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }

            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sql.AppendLine(" and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_ASST_ORG (nolock) where ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }

            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sql.AppendLine(" and M1.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }

            // 列管狀態
            if (!string.IsNullOrEmpty(model.TUBE_STATUS))
            {
                sql.AppendLine(" and dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS, 'B2') = @TUBE_STATUS");
            }

            // 特殊加註
            if (model.SPEC_NOTE != null && model.SPEC_NOTE.Any())
            {
                sql.AppendLine(" and M1.PROJECT_NO in (select PROJECT_NO from PROJECT_MAPPING_DATA (nolock) where SET_ITEM = 'SPEC_NOTE' and SET_TYPE in @SPEC_NOTE)");
            }

            // 計畫狀態
            if(model.ProjectStatuses !=null && model.ProjectStatuses.Any())
            {
                sql.AppendLine(" and M1.PROJECT_STATUS in @ProjectStatuses");
            }

            sql.AppendLine(@"group by EXEC_ORGAN_C,M3.OU_SORT_ORDER");
            sql.AppendLine(@"order by M3.OU_SORT_ORDER");

            return (await ExecuteQueryAsync<OrgProjectCntModel>(sql.ToString(), model)).ToList();
        }
        #endregion
    }
}
