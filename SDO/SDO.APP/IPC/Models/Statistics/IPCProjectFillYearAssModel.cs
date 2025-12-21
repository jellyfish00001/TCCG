using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表11：年終考核案件成績表
    /// </summary>
    public class IPCProjectFillYearAssModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NAME { get; set; }

        /// <summary>
        /// 計畫狀態
        /// </summary>
        public string PROJECT_STATUS { get; set; }

        /// <summary>
        /// 計畫總經費
        /// </summary>
        public long PROJECT_EXS { get; set; }

        /// <summary>
        /// 執行機關
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }

        /// <summary>
        /// 執行機關中文
        /// </summary>
        public string EXEC_DEPT { get; set; }

        /// <summary>
        /// 0工程類、1非工程類
        /// </summary>
        public string CP_KIND { get; set; }

        /// <summary>
        /// 執行方式中文
        /// </summary>
        public string CP_KIND_NAME { get; set; }

        /// <summary>
        /// 計畫全案實際完成日期
        /// </summary>
        public DateTime? ACTUAL_ENDDATE { get; set; }

        /// <summary>
        /// 結案日期
        /// </summary>
        public DateTime? FINISH_DATE { get; set; }

        /// <summary>
        /// 時間差(結案日期 - 計畫全案實際完成日期)
        /// </summary>
        public double? DIFF_MONTH { get; set; }

        /// <summary>
        /// 原始基本資料
        /// </summary>
        public int SCORE_A { get; set; }

        /// <summary>
        /// 基本資料得分
        /// </summary>
        public double SA
        {
            get
            {
                return Math.Round(SCORE_A * 0.2, 1);
            }
        }

        /// <summary>
        /// 逾期天數
        /// </summary>
        public int OVERDUE_DAY { get; set; }

        /// <summary>
        /// 報表填報得分
        /// </summary>
        public int SB
        {
            get
            {
                return 10 - OVERDUE_DAY < 0 ? 0 : 10 - OVERDUE_DAY;
            }
        }

        /// <summary>
        /// 報表品質改善次數
        /// </summary>
        public int TYPE_1_CNT { get; set; }

        /// <summary>
        /// 報表品質得分
        /// </summary>
        public int SC
        {
            get
            {
                return 5 - 3 * TYPE_1_CNT < 0 ? 0 : 5 - 3 * TYPE_1_CNT;
            }
        }

        /// <summary>
        /// 總期程調整次數
        /// </summary>
        public int SCHE_Y_TOTCNT { get; set; }

        /// <summary>
        /// 總期程調整延遲次數
        /// </summary>
        public int SCHE_Y_DELAY_CNT
        {
            get { return SCHE_Y_TOTCNT > 0 ? SCHE_Y_TOTCNT - 1 : 0; }
        }

        /// <summary>
        /// 分月期程調整次數
        /// </summary>
        public int SCHE_M_TOTCNT { get; set; }

        /// <summary>
        /// 分月期程調整延遲次數
        /// </summary>
        public int SCHE_M_DELAY_CNT
        {
            get { return SCHE_M_TOTCNT > 0 ? SCHE_M_TOTCNT - 1 : 0; }
        }

        /// <summary>
        /// 計畫調整得分
        /// </summary>
        public int SD
        {
            get
            {
                int deductScoreD = (SCHE_M_TOTCNT != 0 ? (SCHE_M_TOTCNT - 1) * 1 : 0) + (SCHE_Y_TOTCNT != 0 ? (SCHE_Y_TOTCNT - 1) * 3 : 0);
                return 10 - deductScoreD < 0 ? 0 : 10 - deductScoreD;
            }
        }

        /// <summary>
        /// 管考實際進度
        /// </summary>
        public double PROG_C { get; set; }

        /// <summary>
        /// 工程實際進度
        /// </summary>
        public double PROG_P { get; set; }

        /// <summary>
        /// 實地查證平均分數
        /// </summary>
        public double? AVG_FFSCORE { get; set; }

        /// <summary>
        /// 管考進度得分
        /// </summary>
        public double SE
        {
            get
            {
                double se = 0;
                if (CP_KIND == "1")
                {
                    double seA = Math.Round(PROG_C / 100, 2);
                    se = Math.Round(seA * 15, 1);
                }
                else if (CP_KIND == "0" && !AVG_FFSCORE.HasValue)
                {
                    double seB = Math.Round((PROG_C / 100) + (PROG_P / 100), 2);
                    se = Math.Round(seB / 2 * 15, 1);
                }
                else if (CP_KIND == "0" && AVG_FFSCORE.HasValue)
                {
                    double seC = Math.Round((PROG_C / 100) + (PROG_P / 100) + (double)(AVG_FFSCORE / 100), 2);
                    se = Math.Round(seC / 3 * 15, 1);
                }
                return se;
            }
        }

        /// <summary>
        /// 執行總期程
        /// </summary>
        public int EX_MONTH
        {
            get
            {
                return PROJECT_STATUS == "7"
                    ? (FINISH_DATE.Value.Year - CONTROL_DATE1.Year) * 12 + FINISH_DATE.Value.Month - CONTROL_DATE1.Month + 1
                    : (PROJECT_LAST_DATE.Year - CONTROL_DATE1.Year) * 12 + PROJECT_LAST_DATE.Month - CONTROL_DATE1.Month + 1;
            }
        }

        /// <summary>
        /// 落後月數
        /// </summary>
        public int DELAY_MONTH { get; set; }

        /// <summary>
        /// 最長連續落後月數
        /// </summary>
        public int MAX_DELAY_MONTH { get; set; }

        /// <summary>
        /// 落後情形得分
        /// </summary>
        public double SF
        {
            get
            {
                double sfB = Math.Round(EX_MONTH <= 0 ? 0 : ((double)DELAY_MONTH) / ((double)EX_MONTH), 2);
                double deductScoreF = Math.Round((1 - sfB) * 20 - ((double)MAX_DELAY_MONTH / 2), 1);
                return deductScoreF < 0 ? 0 : deductScoreF;
            }
        }

        /// <summary>
        /// 累計實際支付數
        /// </summary>
        public decimal ACTUAL_PAY { get; set; }

        /// <summary>
        /// 應付未付數
        /// </summary>
        public decimal UNPAY { get; set; }

        /// <summary>
        /// 結餘數
        /// </summary>
        public decimal BALANCE { get; set; }

        /// <summary>
        /// 預算達成率
        /// </summary>
        public double BUDGET_ACHIEVE_RATE
        {

            get
            {
                return PROJECT_EXS > 0
                    ? Math.Round((double)(ACTUAL_PAY + UNPAY + BALANCE) / PROJECT_EXS * 100 , 1 )
                    : 0;
            }
        }

        /// <summary>
        /// 實際支用比
        /// </summary>
        public double BUDGET_ACTUAL_RATE
        {
            get
            {
                return PROJECT_EXS > 0
                    ? Math.Round((double)ACTUAL_PAY / PROJECT_EXS * 100, 1 )
                    : 0;
            }
        }

        /// <summary>
        /// 經費執行得分
        /// </summary>
        public double SG
        {
            get
            {
                double sgL =  (double)(BUDGET_ACHIEVE_RATE / 100) > 1 ? 1 : (double)(BUDGET_ACHIEVE_RATE / 100);
                double sgM = (double)(BUDGET_ACTUAL_RATE / 100) > 1 ? 1 : (double)(BUDGET_ACTUAL_RATE / 100);
                return Math.Round((double)(sgL + sgM) / 2 * 10, 1);
            }
        }

        /// <summary>
        /// 合計得分
        /// </summary>
        public double SH
        {
            get
            {
                return (double)(SA + SB + SC + SD + SE + SF + SG);
            }
        }

        /// <summary>
        /// 未主動提報次數
        /// </summary>
        public int TYPE_2_CNT { get; set; }

        /// <summary>
        /// 未主動提報扣分
        /// </summary>
        public int DEDUCT_A
        {
            get
            {
                return TYPE_2_CNT > 0 ? 5 : 0;
            }
        }

        /// <summary>
        /// 逾期申請調整次數
        /// </summary>
        public int DELAY_APPLY_CNT { get; set; }

        /// <summary>
        /// 逾期申請調整扣分
        /// </summary>
        public int DEDUCT_B
        {
            get
            {
                return 5 * DELAY_APPLY_CNT;
            }
        }

        /// <summary>
        /// 填報不實次數
        /// </summary>
        public int TYPE_4_CNT { get; set; }

        /// <summary>
        /// 填報不實扣分
        /// </summary>
        public int DEDUCT_C
        {
            get
            {
                return 3 * TYPE_4_CNT;
            }
        }

        /// <summary>
        /// 分案次數
        /// </summary>
        public int MERGE_1_CNT { get; set; }

        /// <summary>
        /// 分案列管扣分
        /// </summary>
        public int DEDUCT_D
        {
            get
            {
                return MERGE_1_CNT > 0 ? 3 : 0;
            }
        }

        /// <summary>
        /// 合計扣分
        /// </summary>
        public int SI
        {
            get
            {
                return DEDUCT_A + DEDUCT_B + DEDUCT_C + DEDUCT_D;
            }
        }

        /// <summary>
        /// 初評分數
        /// </summary>
        public double SJ
        {
            get
            {
                return (double)(SH - SI);
            }
        }

        /// <summary>
        /// 計畫開始日期
        /// </summary>
        public DateTime CONTROL_DATE1 { get; set; }

        /// <summary>
        /// 預定完成期限
        /// </summary>
        public DateTime PROJECT_LAST_DATE { get; set; }
    }
}
