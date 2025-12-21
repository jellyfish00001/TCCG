using SDO.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫工程進度
    /// </summary>
    public class ProjectEngineeringProgressModel : DbEditor
    {
        /// <summary>
        /// IDENTITY_FIELD
        /// </summary>
        public int SEQ { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public string YEAR { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        public string MONTH { get; set; }

        public DateTime? PROJECT_DATE
        {
            get
            {
                if (int.TryParse(YEAR, out int year))
                {
                    return $"{year + 1911}-{MONTH}".ToDateTimeWithNull();
                }
                return null;
            }
        }

        /// <summary>
        ///  重大預定施工進度
        /// </summary>
        public decimal? IPC_RES_PRG { get; set; }

        /// <summary>
        /// 重大實際施工進度
        /// </summary>
        public decimal? IPC_ACT_PRG { get; set; }

        /// <summary>
        /// 執行情形說明
        /// </summary>
        public string EXECUTE_CONDITION { get; set; }

        /// <summary>
        /// 需協辦事項
        /// </summary>
        public string ASSISTANT_ITEM { get; set; }

        /// <summary>
        /// 否施工進度落後，0否(預設)/1是
        /// </summary>
        public bool IS_DELAY { get; set; }

        /// <summary>
        /// 執行情形是否送出
        /// </summary>
        public bool IS_SEND { get; set; }

        /// <summary>
        /// 取消當期執行情形送出
        /// </summary>
        public bool CancelSend { get; set; }

        /// <summary>
        /// 上一個月執行情形
        /// </summary>
        public string LAST_EXECUTE_CONDITION { get; set; }
}
}
