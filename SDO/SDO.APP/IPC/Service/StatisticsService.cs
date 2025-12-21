using Aspose.Cells;
using Newtonsoft.Json;
using SDO.APP.IPC.Enum;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.APP.IPC.Models.Statistics;
using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class StatisticsService : Service, IStatisticsService
    {

        private readonly IStatisticsDac dac;
        private readonly IProjectDac projectDac;
        private readonly IProjectAdjustDac adjustDac;
        private readonly IIPCSetParamService iIPCSetParamService;
        public StatisticsService(IStatisticsDac dac, IProjectDac projectDac, IProjectAdjustDac adjustDac, IIPCSetParamService iIPCSetParamService)
        {
            this.dac = dac;
            this.projectDac = projectDac;
            this.adjustDac = adjustDac;
            this.iIPCSetParamService = iIPCSetParamService;
        }

        #region 綜合查詢
        /// <summary>
        /// 取得自選欄位
        /// </summary>
        /// <param name="isRdec">檢查登入者是否有管考權限(管考角色)</param>
        /// <returns></returns>
        public async Task<List<OptionColumnModel>> GetOptionColumns(bool isRdec)
        {
            List<OptionColumnModel> result = new List<OptionColumnModel>();

            #region 自選欄位
            #region 基本資料(A)
            result.Add(new OptionColumnModel() { Title = "基本資料", Key = "A", Level = LevelType.Group });
            result.Add(new OptionColumnModel() { Title = "計畫名稱", Key = "PROJECT_NAME", Parent = "A", Table = "M1", Width = 250, IsCheck = true });
            result.Add(new OptionColumnModel() { Title = "計畫編號", Key = "PROJECT_NO", Parent = "A", Table = "M1", Width = 100, IsCheck = true });
            result.Add(new OptionColumnModel() { Title = "主管機關", Key = "MASTER_ORGAN_C", Parent = "A", Table = "M1", IsCheck = true });
            result.Add(new OptionColumnModel() { Title = "執行機關", Key = "EXEC_ORGAN_C", Parent = "A", Table = "M1", IsCheck = true });
            result.Add(new OptionColumnModel() { Title = "協辦機關", Key = "ASSISTANT_ORGAN_C", Parent = "A", Width = 150 });
            result.Add(new OptionColumnModel() { Title = "代辦機關", Key = "BUDGET_HOLD_ORGAN_C", Parent = "A", Table = "M1" });
            result.Add(new OptionColumnModel() { Title = "計畫總經費", Key = "BUDGET_TOTAL", Parent = "A", Type = CellValueType.IsNumeric, Width = 130 });
            result.Add(new OptionColumnModel() { Title = "中央補助款", Key = "BUDGET_CENTRAL", Parent = "A", Type = CellValueType.IsNumeric });
            result.Add(new OptionColumnModel() { Title = "地方自籌款", Key = "BUDGET_LOCAL", Parent = "A", Type = CellValueType.IsNumeric });
            result.Add(new OptionColumnModel() { Title = "地區別", Key = "TOWN_C", Parent = "A", Table = "M1" });
            result.Add(new OptionColumnModel() { Title = "座標X", Key = "X_COORD", Parent = "A", Table = "M1", Width = 150 });
            result.Add(new OptionColumnModel() { Title = "座標Y", Key = "Y_COORD", Parent = "A", Table = "M1", Width = 150 });
            result.Add(new OptionColumnModel() { Title = "建設類別", Key = "BUILD_KIND", Parent = "A", Table = "M1", Width = 250 });
            result.Add(new OptionColumnModel() { Title = "主要建設", Key = "MAIN_BUILD", Parent = "A", Table = "M1", Width = 250 });
            result.Add(new OptionColumnModel() { Title = "附屬設施", Key = "SUB_BUILD", Parent = "A", Table = "M1", Width = 250 });
            result.Add(new OptionColumnModel() { Title = "相關審查", Key = "REVIEWITEM", Parent = "A", Table = "M1", Width = 150 });
            result.Add(new OptionColumnModel() { Title = "特殊加註", Key = "SPEC_NOTE", Parent = "A", Width = 150 });
            result.Add(new OptionColumnModel() { Title = "列管狀態", Key = "TUBE_STATUS", Parent = "A", Table = "M1" });
            result.Add(new OptionColumnModel() { Title = "分案或併案", Key = "MERGE_STATUS", Parent = "A", Table = "M7" });
            result.Add(new OptionColumnModel() { Title = "計畫內容", Key = "ALL_JOB", Parent = "A", Table = "M1", Width = 200 });
            result.Add(new OptionColumnModel() { Title = "計畫效益", Key = "PROJECT_BENEFIT", Parent = "A", Table = "M1", Width = 200 });
            result.Add(new OptionColumnModel() { Title = "立案時間", Key = "CREATEDTIME", Parent = "A", Table = "M1", Type = CellValueType.IsDateTime, Width = 120 });
            result.Add(new OptionColumnModel() { Title = "計畫基本資料備註", Key = "MEMO", Parent = "A", Table = "M1", Width = 200 });
            result.Add(new OptionColumnModel() { Title = "廠商資訊", Key = "MANINFO", Parent = "A", Width = 250 });
            result.Add(new OptionColumnModel() { Title = "承辦人聯絡資訊姓名", Key = "REAL_CONTACT", Parent = "A", Table = "M10", Width = 160 });
            result.Add(new OptionColumnModel() { Title = "承辦人聯絡資訊電話", Key = "REAL_TEL", Parent = "A", Table = "M10", Width = 160 });
            result.Add(new OptionColumnModel() { Title = "承辦人聯絡資訊電子郵件", Key = "REAL_EMAIL", Parent = "A", Table = "M10", Width = 190 });
            #endregion

            #region 預算執行情形(B)
            result.Add(new OptionColumnModel() { Title = "預算執行情形", Key = "B", Level = LevelType.Group });
            result.Add(new OptionColumnModel() { Title = "累計預定支用", Key = "GT_EXPANDED_BUDGET", Parent = "B", Table = "M12", Type = CellValueType.IsNumeric, Width = 110 });
            result.Add(new OptionColumnModel() { Title = "累計實際完成金額", Key = "GT_FINISH_BUDGET", Parent = "B", Table = "M12", Type = CellValueType.IsNumeric, Width = 140 });
            result.Add(new OptionColumnModel() { Title = "累計各項經費支用", Key = "GT_ACT_BUDGET", Parent = "B", Table = "M12", Type = CellValueType.IsNumeric, Width = 120 });
            result.Add(new OptionColumnModel() { Title = "應付未付數", Key = "GT_AP", Parent = "B", Table = "M12", Type = CellValueType.IsNumeric });
            result.Add(new OptionColumnModel() { Title = "結餘數", Key = "GT_BALANCE", Parent = "B", Table = "M12", Type = CellValueType.IsNumeric });
            result.Add(new OptionColumnModel() { Title = "累計實際支用數", Key = "GT_TOTAL", Parent = "B", Table = "M12", Type = CellValueType.IsNumeric });
            result.Add(new OptionColumnModel() { Title = "累計執行率", Key = "GT_EXEC_RATE", Parent = "B", Table = "M12", Type = CellValueType.IsNumeric });
            result.Add(new OptionColumnModel() { Title = "本年度可支用預算數", Key = "YEAR_BUDGET_EXPANDED", Parent = "B", Table = "M12", Type = CellValueType.IsNumeric, Width = 160 });
            result.Add(new OptionColumnModel() { Title = "本年度預算分配數", Key = "YEAR_BUDGET_ALLOCATED", Parent = "B", Table = "M12", Type = CellValueType.IsNumeric, Width = 140 });
            result.Add(new OptionColumnModel() { Title = "本年度預算執行數", Key = "YEAR_EXEC_BUDGET", Parent = "B", Table = "M12", Type = CellValueType.IsNumeric, Width = 140 });
            result.Add(new OptionColumnModel() { Title = "本年度執行率", Key = "YEAR_EXEC_RATE", Parent = "B", Table = "M12", Type = CellValueType.IsNumeric, Width = 110 });
            result.Add(new OptionColumnModel() { Title = "年度預算執行率未達80%原因", Key = "IPCBGTEXECFAILED", Parent = "B", Width = 230 });
            result.Add(new OptionColumnModel() { Title = "年度預算執行率未達80%責任歸屬", Key = "IPCBGTEXECFAILEDDUTY", Parent = "B", Width = 250 });
            result.Add(new OptionColumnModel() { Title = "年度預算執行率未達80%說明", Key = "EXEC_RATE_FAILED_NOTE", Parent = "B", Table = "M12", Width = 230 });
            #endregion

            #region 經費支用(C)
            result.Add(new OptionColumnModel() { Title = "經費支用", Key = "C", Level = LevelType.Group });
            result.Add(new OptionColumnModel() { Title = "發包金額", Key = "PROCUREMENT_AMT", Parent = "C", Table = "M1", Type = CellValueType.IsNumeric });
            result.Add(new OptionColumnModel() { Title = "決標金額", Key = "TENDER_AWARDING_AMT", Parent = "C", Table = "M1", Type = CellValueType.IsNumeric });
            result.Add(new OptionColumnModel() { Title = "結案累計實際完成金額", Key = "TOTAL_ACTUAL_COMP", Parent = "C", Table = "M11", Type = CellValueType.IsNumeric, Width = 170 });
            result.Add(new OptionColumnModel() { Title = "結案累計實際支用數", Key = "ACTUAL_PAY", Parent = "C", Table = "M11", Type = CellValueType.IsNumeric, Width = 160 });
            result.Add(new OptionColumnModel() { Title = "結案應付未付數", Key = "UNPAY", Parent = "C", Table = "M11", Type = CellValueType.IsNumeric, Width = 120 });
            result.Add(new OptionColumnModel() { Title = "結案節餘數", Key = "BALANCE", Parent = "C", Table = "M11", Type = CellValueType.IsNumeric });
            result.Add(new OptionColumnModel() { Title = "結案累計經費執行率", Key = "EXACUTIVE_RATE", Parent = "C", Table = "M11", Type = CellValueType.IsNumeric, Width = 160 });
            #endregion

            #region 執行進度(D)
            result.Add(new OptionColumnModel() { Title = "執行進度", Key = "D", Level = LevelType.Group });
            result.Add(new OptionColumnModel() { Title = "計畫開始日期", Key = "CONTROL_DATE1", Parent = "D", Table = "M13", Type = CellValueType.IsDateTime, Width = 110 });
            result.Add(new OptionColumnModel() { Title = "預定完成期限", Key = "PROJECT_LAST_DATE", Parent = "D", Table = "M1", Type = CellValueType.IsDateTime, Width = 110 });
            result.Add(new OptionColumnModel() { Title = "執行方式", Key = "RUNWAY_C", Parent = "D", Table = "M1", Width = 170 });
            result.Add(new OptionColumnModel() { Title = "執行階段", Key = "PROGRESS", Parent = "D", Table = "M8", Width = 120 });
            result.Add(new OptionColumnModel() { Title = "管考進度\n(預定/實際/差異)", Key = "RDEC_RAD_PRG", Parent = "D", Width = 130 });
            result.Add(new OptionColumnModel() { Title = "管考進度(預定)", Key = "RDEC_RES_PRG", Parent = "D", Hidden = true });
            result.Add(new OptionColumnModel() { Title = "管考進度(實際)", Key = "RDEC_ACT_PRG", Parent = "D", Hidden = true });
            result.Add(new OptionColumnModel() { Title = "本月執行情形", Key = "EXECUTE_CONDITION", Parent = "D", Table = "M9", Width = 180 });
            result.Add(new OptionColumnModel() { Title = "須協辦事項", Key = "ASSISTANT_ITEM", Parent = "D", Table = "M9", Width = 180 });
            result.Add(new OptionColumnModel() { Title = "檢核點(預定/實際)", Key = "CHECKITEM", Parent = "D", Width = 250 });
            result.Add(new OptionColumnModel() { Title = "檢核點(預定)", Key = "RES_CHECKITEM", Parent = "D", Hidden = true });
            result.Add(new OptionColumnModel() { Title = "檢核點(實際)", Key = "ACT_CHECKITEM", Parent = "D", Hidden = true });
            result.Add(new OptionColumnModel() { Title = "完整檢核點(預定/實際)", Key = "ALL_CHECKITEM", Parent = "D", Width = 250 });
            result.Add(new OptionColumnModel() { Title = "預定工程標決標日期", Key = "BID_AWARD_ESTIMATED_ENDDATE", Parent = "D", Type = CellValueType.IsDateTime, Width = 160 });
            result.Add(new OptionColumnModel() { Title = "實際工程標決標日期", Key = "BID_AWARD_ACTUAL_ENDDATE", Parent = "D", Type = CellValueType.IsDateTime, Width = 160 });
            result.Add(new OptionColumnModel() { Title = "預定驗收日期", Key = "ACCEPTANCE_ESTIMATED_ENDDATE", Parent = "D", Type = CellValueType.IsDateTime, Width = 110 });
            result.Add(new OptionColumnModel() { Title = "實際驗收日期", Key = "ACCEPTANCE_ACTUAL_ENDDATE", Parent = "D", Type = CellValueType.IsDateTime, Width = 110 });
            result.Add(new OptionColumnModel() { Title = "實地查證資料【含查證日期、次數、分數】", Key = "FF_DATE_SCORE", Parent = "D", Width = 320 });
            result.Add(new OptionColumnModel() { Title = "實地查證執行機關參採情形", Key = "FF_DATE_FFREPORT", Parent = "D", Width = 210 });
            result.Add(new OptionColumnModel() { Title = "設計標流標次數", Key = "BID_01_FLOW", Parent = "D", Type = CellValueType.IsNumeric, Width = 120 });
            result.Add(new OptionColumnModel() { Title = "設計標廢標次數", Key = "BID_01_SCRAP", Parent = "D", Type = CellValueType.IsNumeric, Width = 120 });
            result.Add(new OptionColumnModel() { Title = "設計暨監造標流標次數", Key = "BID_02_FLOW", Parent = "D", Type = CellValueType.IsNumeric, Width = 170 });
            result.Add(new OptionColumnModel() { Title = "設計暨監造標廢標次數", Key = "BID_02_SCRAP", Parent = "D", Type = CellValueType.IsNumeric, Width = 170 });
            result.Add(new OptionColumnModel() { Title = "工程標流標次數", Key = "BID_03_FLOW", Parent = "D", Type = CellValueType.IsNumeric, Width = 120 });
            result.Add(new OptionColumnModel() { Title = "工程標廢標次數", Key = "BID_03_SCRAP", Parent = "D", Type = CellValueType.IsNumeric, Width = 120 });
            result.Add(new OptionColumnModel() { Title = "計畫全案實際完成日期", Key = "LAST_CHECKITEM_ACTUAL_ENDDATE", Parent = "D", Type = CellValueType.IsDateTime, Width = 170 });
            result.Add(new OptionColumnModel() { Title = "結案/撤銷日期", Key = "CLOSED_OR_REVOKE", Parent = "D", Type = CellValueType.IsDateTime, Width = 120 });
            #endregion

            #region 開竣工日(E)
            result.Add(new OptionColumnModel() { Title = "開竣工日", Key = "E", Level = LevelType.Group });
            result.Add(new OptionColumnModel() { Title = "預定開工日期", Key = "START_ESTIMATED_ENDDATE", Parent = "E", Type = CellValueType.IsDateTime, Width = 110 });
            result.Add(new OptionColumnModel() { Title = "預定竣工日期", Key = "COMPLETION_ESTIMATED_ENDDATE", Parent = "E", Type = CellValueType.IsDateTime, Width = 110 });
            result.Add(new OptionColumnModel() { Title = "實際開工日期", Key = "START_ACTUAL_ENDDATE", Parent = "E", Type = CellValueType.IsDateTime, Width = 110 });
            result.Add(new OptionColumnModel() { Title = "實際竣工日期", Key = "COMPLETION_ACTUAL_ENDDATE", Parent = "E", Type = CellValueType.IsDateTime, Width = 110 });
            result.Add(new OptionColumnModel() { Title = "標案系統預定開工日期", Key = "START_PCC_ESTIMATED_ENDDATE", Parent = "E", Type = CellValueType.IsDateTime, Width = 170 });
            result.Add(new OptionColumnModel() { Title = "標案系統預定竣工日期", Key = "COMPLETION_PCC_ESTIMATED_ENDDATE", Parent = "E", Type = CellValueType.IsDateTime, Width = 170 });
            result.Add(new OptionColumnModel() { Title = "標案系統實際開工日期", Key = "START_PCC_ACTUAL_ENDDATE", Parent = "E", Type = CellValueType.IsDateTime, Width = 170 });
            result.Add(new OptionColumnModel() { Title = "標案系統實際竣工日期", Key = "COMPLETION_PCC_ACTUAL_ENDDATE", Parent = "E", Type = CellValueType.IsDateTime, Width = 170 });
            #endregion

            #region 工程進度(F)
            result.Add(new OptionColumnModel() { Title = "工程進度", Key = "F", Level = LevelType.Group });
            result.Add(new OptionColumnModel() { Title = "工程進度(預定/實際/差異)", Key = "IPC_RAD_PRG", Parent = "F", Table = "M9", Width = 200 });
            result.Add(new OptionColumnModel() { Title = "預定工程進度", Key = "IPC_RES_PRG", Parent = "F", Table = "M9", Type = CellValueType.IsNumeric, Width = 110 });
            result.Add(new OptionColumnModel() { Title = "實際工程進度", Key = "IPC_ACT_PRG", Parent = "F", Table = "M9", Type = CellValueType.IsNumeric, Width = 110 });
            result.Add(new OptionColumnModel() { Title = "工程進度差異", Key = "IPC_DIFF_PRG", Parent = "F", Table = "M9", Type = CellValueType.IsNumeric, Width = 110 });
            result.Add(new OptionColumnModel() { Title = "標案系統工程進度(預定/實際/差異)", Key = "TEN_RAD_PRG", Parent = "F", Width = 270 });
            result.Add(new OptionColumnModel() { Title = "標案系統預定工程進度", Key = "TEN_RES_PRG", Parent = "F", Table = "M9", Type = CellValueType.IsNumeric, Width = 170 });
            result.Add(new OptionColumnModel() { Title = "標案系統實際工程進度", Key = "TEN_ACT_PRG", Parent = "F", Table = "M9", Type = CellValueType.IsNumeric, Width = 170 });
            result.Add(new OptionColumnModel() { Title = "標案系統工程進度差異", Key = "TEN_DIFF_PRG", Parent = "F", Type = CellValueType.IsNumeric, Width = 170 });
            #endregion

            #region 落後分析(G)
            result.Add(new OptionColumnModel() { Title = "落後分析", Key = "G", Level = LevelType.Group });
            result.Add(new OptionColumnModel() { Title = "落後類型", Key = "DELAY_KIND", Parent = "G", Table = "M16", Width = 150 });
            result.Add(new OptionColumnModel() { Title = "落後類別", Key = "DELAY_CLASS_C", Parent = "G", Table = "M16" });
            result.Add(new OptionColumnModel() { Title = "落後項目", Key = "DELAY_SUBCLASS_C", Parent = "G", Table = "M16", Width = 130 });
            result.Add(new OptionColumnModel() { Title = "責任歸屬", Key = "DELAY_RESPON", Parent = "G", Table = "M16" });
            result.Add(new OptionColumnModel() { Title = "落後原因", Key = "DELAY_CAUSAL", Parent = "G", Table = "M16", Width = 180 });
            result.Add(new OptionColumnModel() { Title = "解決對策", Key = "SOLUTION", Parent = "G", Table = "M16", Width = 180 });
            result.Add(new OptionColumnModel() { Title = "須協調事項", Key = "COORDINATION", Parent = "G", Table = "M16", Width = 180 });
            result.Add(new OptionColumnModel() { Title = "改進完成期限", Key = "DEADLINES", Parent = "G", Table = "M16", Type = CellValueType.IsDateTime, Width = 110 });
            result.Add(new OptionColumnModel() { Title = "連續月份落後", Key = "DELAY_MONTH", Parent = "G", Width = 110, Type = CellValueType.IsNumeric });
            result.Add(new OptionColumnModel() { Title = "進度落後天數", Key = "DELAY_DAY", Parent = "G", Width = 110, Type = CellValueType.IsNumeric });

            // 控制檢核點填報項目的 預定完成日期/實際完成日期
            IList<IPCSetParamModel> data = await iIPCSetParamService.GetSysParams("CTRL_CHK_POINT_TYPE",false);
            foreach (IPCSetParamModel item in data)
            {
                int width = (item.SET_TYPE == "E" || item.SET_TYPE == "F") ? 150 : 110;
                item.SET_VALUE = item.SET_VALUE.Replace("/", "/\n");
                result.Add(new OptionColumnModel() { Title = $"{item.SET_VALUE}\n預定完成日期", Key = $"{item.SET_TYPE}_ESTIMATED_ENDDATE", Parent = "G", Type = CellValueType.IsDateTime, Width = width });
                result.Add(new OptionColumnModel() { Title = $"{item.SET_VALUE}\n實際完成日期", Key = $"{item.SET_TYPE}_ACTUAL_ENDDATE", Parent = "G", Type = CellValueType.IsDateTime, Width = width });
            }
            #endregion

            #region 調整撤銷(H)
            result.Add(new OptionColumnModel() { Title = "調整撤銷", Key = "H", Level = LevelType.Group });
            result.Add(new OptionColumnModel() { Title = "基本資料調整", Key = "ADJ_AW01_INFO", Parent = "H", Width = 600 });
            result.Add(new OptionColumnModel() { Title = "總期程調整歷程", Key = "ADJ_AW02_Y_INFO", Parent = "H", Width = 600 });
            result.Add(new OptionColumnModel() { Title = "分月期程調整歷程", Key = "ADJ_AW02_M_INFO", Parent = "H", Width = 600 });
            result.Add(new OptionColumnModel() { Title = "計畫撤銷", Key = "ADJ_AW03_INFO", Parent = "H", Width = 600 });
            #endregion

            #region 其他項目(I)
            result.Add(new OptionColumnModel() { Title = "其他項目", Key = "I", Level = LevelType.Group });
            result.Add(new OptionColumnModel() { Title = "會議列管", Key = "CONFERENCE_INFO", Parent = "I", Width = 250 });
            result.Add(new OptionColumnModel() { Title = "資料逾期繳交或填報", Key = "FILL_REASON", Parent = "I", Width = 250 });
            result.Add(new OptionColumnModel() { Title = "相關活動", Key = "PROJECT_ACTIVITY", Parent = "I", Width = 270 });
            #endregion

            #region 管考意見(J)
            result.Add(new OptionColumnModel() { Title = "管考意見", Key = "J", Level = LevelType.Group });
            result.Add(new OptionColumnModel() { Title = "立案審核意見", Key = "MEMO_EVALUATION", Parent = "J", Table = "M1", Width = 180 });
            result.Add(new OptionColumnModel() { Title = "平時管考意見", Key = "AUDIT_OPINION", Parent = "J", Table = "M19", Width = 180 });
            result.Add(new OptionColumnModel() { Title = "平時管考意見備註", Key = "COM_IPCMEMO", Parent = "J", Table = "M19", Width = 180 });
            result.Add(new OptionColumnModel() { Title = "實地查證管考說明", Key = "FF_DATE_COMMENT", Parent = "J", Width = 250 });
            result.Add(new OptionColumnModel() { Title = "年終考核意見備註", Key = "NOTES_FOR_BUDGET", Parent = "J", Table = "M1", Width = 180 });
            if (isRdec)
            {
                result.Add(new OptionColumnModel() { Title = "其他管考備註【管考權限】", Key = "NOTES_FOR_SCHEDULE", Parent = "J", Table = "M1", Width = 210 });
            }
            #endregion
            #endregion

            return result;
        }

        /// <summary>
        /// 取得綜合查詢結果
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public async Task<List<IDictionary<string, object>>> GetUnitingQuery(Dictionary<string, object> condition)
        {
            List<OptionColumnModel> selectedColumns = JsonConvert.DeserializeObject<List<OptionColumnModel>>(condition["SELECTED_COLUMN"].ToString());
            SetParam(condition);
            (List<IDictionary<string, object>> dacResult, List<string> combineFields) = await dac.GetUnitingQuery(condition, selectedColumns);

            List<IDictionary<string, object>> result = new List<IDictionary<string, object>>();
            foreach (IDictionary<string, object> x in dacResult)
            {
                Dictionary<string, object> item = x.ToDictionary(k => k.Key,
                    k => ConvertValue(k)
                );

                string content = string.Empty;
                foreach (string combineField in combineFields)
                {
                    switch (combineField)
                    {
                        case "BUILD_KIND": //建設類別
                            content = $"主要建設：{item["MAIN_BUILD"]}\n附屬設施：{item["SUB_BUILD"]}";
                            break;
                        case "RDEC_RAD_PRG": //管考進度(預定/實際/差異)
                            content = $"預定：{item["RDEC_RES_PRG"]}\n實際：{item["RDEC_ACT_PRG"]}\n差異：{SetPercent(Calculate(item["RDEC_ACT_PRG"], item["RDEC_RES_PRG"]))}";
                            break;
                        case "CHECKITEM": //檢核點(預定/實際)
                            content = $"預定：\n{item["RES_CHECKITEM"]}\n實際：\n{item["ACT_CHECKITEM"]}";
                            break;
                        case "IPC_RAD_PRG": //工程進度(預定/實際/差異)
                            content = $"預定：{item["IPC_RES_PRG"]}\n實際：{item["IPC_ACT_PRG"]}\n差異：{item["IPC_DIFF_PRG"]}";
                            break;
                        case "TEN_RAD_PRG": //標案系統工程進度(預定/實際/差異)
                            content = $"預定：{item["TEN_RES_PRG"]}\n實際：{item["TEN_ACT_PRG"]}\n差異：{item["TEN_DIFF_PRG"]}";
                            break;
                    }
                    item.Add(combineField, content);
                }

                // 工程進度(預定/實際/差異)、預定工程進度、實際工程進度、工程進度差異、標案系統工程進度(預定/實際/差異) 、標案系統預定工程進度、標案系統實際工程進度、標案系統工程進度差異
                // 如果是非工程類的計畫顯示空白
                List<string> columns = new List<string> { "IPC_RAD_PRG", "IPC_RES_PRG", "IPC_ACT_PRG", "IPC_DIFF_PRG", "TEN_RAD_PRG", "TEN_RES_PRG", "TEN_ACT_PRG", "TEN_DIFF_PRG" };
                if (item["CP_KIND"].ToString() == "1")
                {
                    columns.ForEach(x =>
                    {
                        if (item.ContainsKey(x))
                        {
                            item[x] = string.Empty;
                        }
                    });
                }

                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// 設定參數
        /// </summary>
        /// <param name="condition"></param>
        private void SetParam(Dictionary<string, object> condition)
        {
            condition.Remove("SELECTED_COLUMN");

            int year = Convert.ToInt32(condition["STATISTICS_YEAR"].ToString()) + 1911;
            int month = Convert.ToInt32(condition["STATISTICS_MONTH"].ToString());
            DateTime firstDate = new DateTime(year, month, 1);
            // 查詢年度+查詢月份組合的第一天 ex:2022/04/01
            condition["STATISTICS_AD_YEAR_MONTH_START"] = firstDate.ToString("yyyy/MM/dd");
            DateTime lastDate = firstDate.AddMonths(1).AddDays(-1);
            // 查詢年度+查詢月份組合的最後一天 ex:2022/04/30
            condition["STATISTICS_AD_YEAR_MONTH_LAST"] = lastDate.ToString("yyyy/MM/dd");
            // 統計西元年 ex:2022
            condition["STATISTICS_AD_YEAR"] = year.ToString();
            // 統計西元年月 ex:2022/04
            condition["STATISTICS_AD_YEAR_MONTH"] = lastDate.ToString("yyyy/MM");
            // 統計西元年月(月份+1) ex:2022/05
            condition["STATISTICS_AD_YEAR_NEXT_MONTH"] = firstDate.AddMonths(1).ToString("yyyy/MM");
        }

        private object ConvertValue(KeyValuePair<string, object> obj)
        {
            if(obj.Value is null)
            {
                return string.Empty;
            }

            string value = obj.Value.ToString(), key = obj.Key.ToString();
            string[] ignoreFormatNumberFields = new string[] { "REAL_TEL", "X_COORD", "Y_COORD" };
            string[] percentColumns = new string[] { 
                "GT_EXEC_RATE", "YEAR_EXEC_RATE", "EXACUTIVE_RATE", 
                "IPC_RES_PRG", "IPC_ACT_PRG", "IPC_DIFF_PRG", 
                "TEN_RES_PRG", "TEN_ACT_PRG", "TEN_DIFF_PRG", 
                "RDEC_RES_PRG", "RDEC_ACT_PRG" };
            
            object result;
            if (long.TryParse(value, out long temp64))
            {
                if (ignoreFormatNumberFields.Contains(key))
                {
                    //不轉為數字
                    result = value;
                }
                else
                {
                    result = temp64.ToString("N0");
                }
            }
            else if (double.TryParse(value, out double num))
            {
                if (ignoreFormatNumberFields.Contains(key))
                {
                    //不轉為數字
                    result = value;
                }
                else if (percentColumns.Contains(obj.Key.ToString()))
                {
                    result = SetPercent(value);
                }
                else
                {
                    // db有些欄位型態是decimal，但不顯示小數位，故作此處理
                    result = num.ToString("N0");
                }
            }
            else if (DateTime.TryParse(value, out DateTime date))
            {
                string format = obj.Key == "CREATEDTIME" ? "yyy/MM/dd HH:mm" : "yyy/MM/dd";
                result = value.ToTwDateString(format);
            }
            else
            {
                result = value.Replace("\\n", "\n");
            }

            return result;
        }

        private object SetPercent(string value)
        {
            if (double.TryParse(value, out double x))
            {
                value = $"{x.ToString("N2")}%";
            }
            return value;
        }

        private string Calculate(object x, object y)
        {
            if (double.TryParse(x.ToString(), out double a) && double.TryParse(y.ToString(), out double b))
            {
                return (a - b).ToString();
            }
            return string.Empty;
        }
        #endregion


        #region 表10: 選項列管案件計畫歷次調整審查表

        /// <summary>
        /// 取得 表10選項列管案件計畫歷次調整審查表(詳版) 資料
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <returns></returns>
        public async Task<List<ProjectAdjustDetailedModel>> GetProjAdjDetailed(string PROJECT_NO)
        {
            List<AdjustScheHistoryModel> historyModels = await adjustDac.GetProjAdjScheHistory(new List<string> { PROJECT_NO });
            List<ProjectAdjustDetailedModel> result = await dac.GetProjAdjDetailed(PROJECT_NO);

            if (!historyModels.Any())
            {
                return result;
            }

            foreach (ProjectAdjustDetailedModel item in result)
            {
                item.HistoryModels = historyModels;
                // 取檢核點
                item.CusChkpt = await projectDac.GetProjectCusCheckpoint(PROJECT_NO, item.LOG_ID);
                item.AdjustCusChkpt = await adjustDac.GetAdjustCusCheckPoint(item.PROJ_ADJ_ID);
            }

            return result;
        }

        /// <summary>
        /// 取得屬於工程類的計畫
        /// </summary>
        /// <returns>屬於工程類的計畫清單(PROJECT_NO: 計畫編號、PROJ_ADJ_ID: 最近一次的調整流水號)</returns>
        public async Task<List<object>> GetEngineeringProjects()
        {
            return await dac.GetEngineeringProjects();
        }

        /// <summary>
        /// 取得 表10:簡版 資料 (檢核點只取CTRL_POINT = A ~ F的資料)
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <returns></returns>
        public async Task<List<ProjectAdjustDetailedModel>> GetProjAdjShort(string PROJECT_NO)
        {
            List<AdjustScheHistoryModel> historyModels = await adjustDac.GetProjAdjScheHistory(new List<string> { PROJECT_NO });
            List<ProjectAdjustDetailedModel> result = await dac.GetProjAdjDetailed(PROJECT_NO);

            if (!historyModels.Any())
            {
                return result;
            }

            List<string> ctrlPoints = new() { "A", "B", "C", "D", "E", "F" };

            foreach (ProjectAdjustDetailedModel item in result)
            {
                item.HistoryModels = historyModels;
                // 取第一筆期程調整的 LOG_ID 在歷程檔的資料當作原定列管期程
                if (item == result.First())
                {
                    List<ProjectCusCheckpointModel> allCusChkpt = await projectDac.GetProjectCusCheckpoint(PROJECT_NO, item.LOG_ID);
                    item.CusChkpt = allCusChkpt.Where(x => ctrlPoints.Contains(x.CTRL_POINT)).ToList();
                }

                // 取各次調整的檢核點
                List<AdjustCusCheckPointModel> allAdjustCusChkpt = await adjustDac.GetAdjustCusCheckPoint(item.PROJ_ADJ_ID);
                item.AdjustCusChkpt = allAdjustCusChkpt.Where(x => ctrlPoints.Contains(x.CTRL_POINT)).ToList();
            }

            return result;
        }

        #endregion 表10: 選項列管案件計畫歷次調整審查表
    }
}
