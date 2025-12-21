using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 新表二、表三 每月案件地區/機關統計表(簡版)
    /// </summary>
    public class ProjectAreaDeptShortModel
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
        /// 計畫年度
        /// </summary>
        public int PROJECT_YEAR { get; set; }

        /// <summary>
        /// 計畫狀態
        /// </summary>
        public string PROJECT_STATUS { get; set; }

        /// <summary>
        /// 紀錄狀態
        /// </summary>
        public string STATUS
        {
            get
            {
                string result = "";
                switch (PROJECT_STATUS)
                {
                    case "7": result = "B"; break;
                    case "8": result = "E"; break;
                    default:
                        switch (DELAY_TYPE)
                        {
                            case "": result = "C"; break;
                            case "D1": result = "D1"; break;
                            case "D2": result = "D2"; break;
                            case "D3": result = "D3"; break;
                        }
                        break;
                }
                return result;
            }
        }

        /// <summary>
        /// 主管機關
        /// </summary>
        public string MASTER_ORGAN_C { get; set; }

        /// <summary>
        /// 主管機關中文
        /// </summary>
        public string MASTER_DEPT { get; set; }

        /// <summary>
        /// 執行機關
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }

        /// <summary>
        /// 執行機關中文
        /// </summary>
        public string EXEC_DEPT { get; set; }

        /// <summary>
        /// 機關排序
        /// </summary>
        public string OU_SORT_ORDER { get; set; }

        /// <summary>
        /// 計畫地區
        /// </summary>
        public string TOWN_C { get; set; }

        /// <summary>
        /// 地區名稱
        /// </summary>
        public string TOWNNAME { get; set; }

        /// <summary>
        /// 0工程類、1非工程類
        /// </summary>
        public string CP_KIND { get; set; }

        /// <summary>
        /// 計畫總經費
        /// </summary>
        public long PROJECT_EXS { get; set; }

        /// <summary>
        /// 落後類別
        /// </summary>
        public string DELAY_TYPE { get; set; }

        /// <summary>
        /// 預定開工日期
        /// </summary>
        public DateTime? EST_START { get; set; }

        /// <summary>
        /// 實際開工日期
        /// </summary>
        public DateTime? ACT_START { get; set; }

        /// <summary>
        /// 預定竣工日期
        /// </summary>
        public DateTime? EST_COM { get; set; }

        /// <summary>
        /// 實際竣工日期
        /// </summary>
        public DateTime? ACT_COM { get; set; }

        /// <summary>
        /// 預定完成日期
        /// </summary>
        public DateTime? EST_ACPT { get; set; }

        /// <summary>
        /// 實際完成日期
        /// </summary>
        public DateTime? ACT_ACPT { get; set; }

        /// <summary>
        /// 地區/機關件數
        /// </summary>
        public int NUM { get; set; }

        /// <summary>
        /// 地區/機關總經費(億元)
        /// </summary>
        public double EXS { get; set; }

        /// <summary>
        /// 地區/機關落後件數
        /// </summary>
        public int DELAY_NUM { get; set; }

        /// <summary>
        /// 鄉鎮排序
        /// </summary>
        public int SORT_ORDER { get; set; }

        /// <summary>
        /// 結案時間
        /// </summary>
        public DateTime? FINISH_DATE { get; set; }
    }
}
