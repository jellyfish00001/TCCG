using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 展延申請 - 評核指標
    /// </summary>
    /// <remarks>這裡列出評核指標調整表有，但是評核指標沒有的欄位</remarks>
    public class ExtensionPolicyIndexModel : PolicyIndexModel
    {
        /// <summary>
        /// 展延紀錄評核指標序號
        /// </summary>
        public int POLICY_INDEX_ADJ_ID { get; set; }

        /// <summary>
        /// 展延紀錄序號
        /// </summary>
        public int EXTENSION_ID { get; set; }

        /// <summary>
        /// 展延紀錄編號
        /// </summary>
        public string EXTENSION_NO { get; set; }

        /// <summary>
        /// 評核指標調整預定完成期程
        /// </summary>
        public DateTime? POLICY_EXTP_LANEND_DATE { get; set; }

        /// <summary>
        /// 狀態（E：編輯、A：新增、D：刪除）
        /// </summary>
        public string EDIT_STATUS { get; set; }

        /// <summary>
        /// 展延計畫期程年月_迄（調整計畫期程 RES_EXTP_LANEND_DATE）???
        /// </summary>
        public DateTime? EXTP_LANEND_DATE { get; set; }
    }
}
