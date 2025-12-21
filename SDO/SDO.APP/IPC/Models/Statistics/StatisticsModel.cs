using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 統計報表model
    /// </summary>
    public class StatisticsModel
    {
        /// <summary>
        /// 報表編號
        /// </summary>
        public int STATISTICS_ID { get; set; }
        /// <summary>
        /// 報表名稱
        /// </summary>
        public string STATISTICS_NAME { get; set; }
        /// <summary>
        /// 計畫年度
        /// </summary>
        public string PROJECT_YEAR { get; set; }
        /// <summary>
        /// 計畫年度-結束
        /// </summary>
        public string PROJECT_YEAR_E { get; set; }
        /// <summary>
        /// 計畫年度狀態 Null:無 A:含之前所有案件、B:含之前未結案件
        /// </summary>
        public string PROJECT_YEAR_STATUS { get; set; }
        /// <summary>
        /// 統計年月-年
        /// </summary>
        public string STATISTICS_YEAR { get; set; }
        /// <summary>
        /// 統計年月-月
        /// </summary>
        public int STATISTICS_MONTH { get; set; }
        /// <summary>
        /// 統計年月
        /// </summary>
        public DateTime? YEAR_MONTH_START
        {
            get
            {
                int year;
                if (string.IsNullOrEmpty(STATISTICS_YEAR) || !int.TryParse(STATISTICS_YEAR, out year))
                {
                    return null;
                }
                return $"{year + 1911}/{STATISTICS_MONTH}/1".ToDateTimeWithNull();
            }
        }
        /// <summary>
        /// 統計年月-結束
        /// </summary>
        public DateTime? YEAR_MONTH_END
        {
            get
            {
                int year;
                if (string.IsNullOrEmpty(STATISTICS_YEAR) || !int.TryParse(STATISTICS_YEAR, out year))
                {
                    return null;
                }
                return YEAR_MONTH_START.Value.AddMonths(1).AddDays(-1);
            }
        }
        /// <summary>
        /// 結案年月-開始年
        /// </summary>
        public string CLOSE_S_YEAR { get; set; }
        /// <summary>
        /// 結案年月-開始月
        /// </summary>
        public int? CLOSE_S_MONTH { get; set; }
        /// <summary>
        /// 結案年月-開始
        /// </summary>
        public DateTime? CLOSE_START
        {
            get
            {
                int year;
                if (string.IsNullOrEmpty(CLOSE_S_YEAR) || !int.TryParse(CLOSE_S_YEAR, out year))
                {
                    return null;
                }
                return $"{year + 1911}/{CLOSE_S_MONTH}/1".ToDateTimeWithNull();
            }
        }
        /// <summary>
        /// 結案年月-結束年
        /// </summary>
        public string CLOSE_E_YEAR { get; set; }
        /// <summary>
        /// 結案年月-結束月
        /// </summary>
        public int? CLOSE_E_MONTH { get; set; }
        /// <summary>
        /// 結案年月-結束
        /// </summary>
        public DateTime? CLOSE_END
        {
            get
            {
                int year;
                if (string.IsNullOrEmpty(CLOSE_E_YEAR) || !int.TryParse(CLOSE_E_YEAR, out year))
                {
                    return null;
                }
                DateTime firstDate = $"{year + 1911}/{CLOSE_E_MONTH}/1".ToDateTimeWithNull().Value;
                return firstDate.AddMonths(1).AddDays(-1);
            }
        }
        /// <summary>
        /// 主管機關
        /// </summary>
        public string MASTER_DEPT { get; set; }
        /// <summary>
        /// 執行機關
        /// </summary>
        public string EXEC_DEPT { get; set; }
        /// <summary>
        /// 協辦機關
        /// </summary>
        public string ASS_DEPT { get; set; }
        /// <summary>
        /// 代辦機關
        /// </summary>
        public string AGCY_DEPT { get; set; }
        /// <summary>
        /// 列管狀態 S1:未結案 S2:已結案 S3:撤銷列管
        /// </summary>
        public string TUBE_STATUS { get; set; }
        /// <summary>
        /// 特殊加註
        /// </summary>
        public List<string> SPEC_NOTE { get; set; }
        /// <summary>
        /// 計畫總經費(元) 開始
        /// </summary>
        public int? PROJECT_EXS_S { get; set; }
        /// <summary>
        /// 計畫總經費(元) 結束
        /// </summary>
        public int? PROJECT_EXS_E { get; set; }
        /// <summary>
        /// 執行率 S1:全部 S2:當年度執行率未達80% S3:累計執行率未達80%
        /// </summary>
        public string EXEC_RATE { get; set; }
        /// <summary>
        /// 執行落後類型
        /// </summary>
        public List<string> DELAY_TYPE { get; set; }
        /// <summary>
        /// 列管編號(計畫編號)
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 工作項目
        /// </summary>
        public List<string> CTRL_CHK_POINT_TYPE { get; set; }
        /// <summary>
        /// 排序1
        /// </summary>
        public string SORT_TYPE_1 { get; set; }
        /// <summary>
        /// 是否依機關群組
        /// </summary>
        public bool SORT_GROUPBY_DEPT { get; set; }
        /// <summary>
        /// 排序2
        /// </summary>
        public string SORT_TYPE_2 { get; set; }
        /// <summary>
        /// 排序3
        /// </summary>
        public string SORT_TYPE_3 { get; set; }
        /// <summary>
        /// 要下載哪一種報表
        /// ( 表4 ：{1: 統計表、2: 挑案列表})
        /// ( 表10：{Short: 簡版、Detailed: 詳版})
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 是否為郵件夾帶
        /// </summary>
        public bool? ForMail { get; set; }
        /// <summary>
        /// 計畫狀態
        /// </summary>
        public List<string> ProjectStatuses { get; set; }
    }
}
