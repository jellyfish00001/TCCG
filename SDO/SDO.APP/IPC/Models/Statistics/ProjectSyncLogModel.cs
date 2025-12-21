using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表 13 重大建設系統介接公共工程雲雲端服務網資料統計表
    /// </summary>
    public class ProjectSyncLogModel
    {

        /// <summary>
        /// 是否使用界接（1: 使用，0: 不使用）
        /// </summary>
        public bool IS_USER_FTY_DATA { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
	    public string PROJECT_NAME { get; set; }

        /// <summary>
        /// 執行機關名稱
        /// </summary>
        public string EXEC_ORGAN_NAME { get; set; }

        /// <summary>
        /// 執行機關編號
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }

        /// <summary>
        /// 執行階段
        /// </summary>
        public string PROJECT_STAGE { get; set; }

        /// <summary>
        /// 錯誤項目 / 界接狀況
        /// </summary>
        public string GDB_ERROR_ITEM { get; set; }

        /// <summary>
        /// 工程預計進度
        /// </summary>
        public decimal? IPC_RES_PRG { get; set; }

        /// <summary>
        /// 工程預定進度
        /// </summary>
        public decimal? IPC_ACT_PRG { get; set; }
        /// <summary>
        /// 工程進度 差異
        /// </summary>
        public decimal? IPC_DIFF_PRG { 
            get 
            {  
                if(IPC_RES_PRG.HasValue && IPC_ACT_PRG.HasValue)
                {
                    return IPC_RES_PRG.Value - IPC_ACT_PRG.Value;
                }
                return null;
            } 
        }

        /// <summary>
        /// 實際竣工日期
        /// </summary>
        public string COMPLETION_ACTUAL_ENDDATE { get; set; }

        /// <summary>
        /// 格式比率
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string FormatProgress(decimal? value)
        {
            if (value.HasValue)
            {
                return $"{value.Value:N2}%";
            }
            return string.Empty;
        }
    }
}
