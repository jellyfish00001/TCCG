using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表1 每月案件統計表
    /// </summary>
    public class ProjectStatisticsModel
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
        /// 落後類別
        /// </summary>
        public string DELAY_TYPE { get; set; }
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
        /// 計畫總經費
        /// </summary>
        public long BUDGET_TOTAL { get; set; }


        /// <summary>
        /// 總列管件數
        /// </summary>
        public int EXEC_DEPT_A
        {
            get
            {
                return EXEC_DEPT_B + EXEC_DEPT_C + EXEC_DEPT_D + EXEC_DEPT_E;
            }
        }
        /// <summary>
        /// 已結案件數
        /// </summary>
        public int EXEC_DEPT_B { get; set; }
        /// <summary>
        /// 進度符合或超前
        /// </summary>
        public int EXEC_DEPT_C { get; set; }
        /// <summary>
        /// 件數
        /// </summary>
        public int EXEC_DEPT_D
        {
            get
            {
                return EXEC_DEPT_D1 + EXEC_DEPT_D2 + EXEC_DEPT_D3;
            }
        }
        /// <summary>
        /// 落後件數排序
        /// </summary>
        public int EXEC_DEPT_F1 { get; set; }
        /// <summary>
        /// 比率
        /// </summary>
        public double EXEC_DEPT_F1_RATE { get; set; }
        /// <summary>
        /// 落後比率排序
        /// </summary>
        public int EXEC_DEPT_F2 { get; set; }
        /// <summary>
        /// 開工前預定檢核點進度落後
        /// </summary>
        public int EXEC_DEPT_D1 { get; set; }
        /// <summary>
        /// 施工進度落後
        /// </summary>
        public int EXEC_DEPT_D2 { get; set; }
        /// <summary>
        /// 竣工後預定檢核點進度落後
        /// </summary>
        public int EXEC_DEPT_D3 { get; set; }
        /// <summary>
        /// 撤銷列管件數
        /// </summary>
        public int EXEC_DEPT_E { get; set; }
        /// <summary>
        /// 完成率
        /// </summary>
        public double EXEC_DEPT_BA { get; set; }
        /// <summary>
        /// 落後排序合計
        /// </summary>
        public int EXEC_DEPT_F1F2
        {
            get
            {
                return EXEC_DEPT_F1 + EXEC_DEPT_F2;
            }
        }
        /// <summary>
        /// 落後綜合排名
        /// </summary>
        public int EXEC_DEPT_RANKING { get; set; }

        /// <summary>
        /// 區分一般機關與區公所
        /// 1: 一般機關(非區公所) 2: 區公所
        /// </summary>
        public int DeptType { 
            get 
            {
                return this.EXEC_DEPT.Contains("區公所") ? 2 : 1;       
            } 
        } 
    }
}
