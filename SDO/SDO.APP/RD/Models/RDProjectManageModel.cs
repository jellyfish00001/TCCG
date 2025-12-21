using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 研究管理清單 Model
    /// </summary>
    public class RDProjectManageModel : RDProjectManageQueryModel
    {
        /// <summary>
        /// 計畫序號
        /// </summary>
        public int PLAN_ID { get; set; }

        /// <summary>
        /// 研究主持人
        /// </summary>
        public string RESEARCH_NAME { get; set; }

        /// <summary>
        /// 計畫期程年月_起
        /// </summary>
        public string PLAN_START_DATE { get; set; }

        /// <summary>
        /// 計畫期程年月_迄
        /// </summary>
        public string PLAN_END_DATE { get; set; }

        /// <summary>
        /// 聯絡人(承辦人)
        /// </summary>
        public string CONTACT_NAME { get; set; }

        /// <summary>
        /// 執行機關名稱
        /// </summary>
        public string EXEC_ORG_NAME { get; set; }

        /// <summary>
        /// 執行機關排序
        /// </summary>
        public string EXEC_ORG_ORDER { get; set; }

        /// <summary>
        /// 建立人員/提報人員
        /// </summary>
        public string CRT_USER_NAME { get; set; }
    }
}
