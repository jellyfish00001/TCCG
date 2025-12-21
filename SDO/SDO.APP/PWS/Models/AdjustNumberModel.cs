using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class AdjustNumberModel : DbEditor
    {

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLANNO { get; set; }

        /// <summary>
        /// 計畫年度
        /// </summary>
        public int PLANYEAR { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 主管機關ID
        /// </summary>
        public string OU_ID { get; set; }

        /// <summary>
        /// 主管機關
        /// </summary>
        public string OU_NAME { get; set; }
        /// <summary>
        /// 基金序號
        /// </summary>
        public int FUNDNO { get; set; }

        /// <summary>
        /// 基金序號
        /// </summary>
        public string FUNDNAME { get; set; }

        /// <summary>
        /// 計畫類別
        /// </summary>
        public string PLANKIND { get; set; }

        /// <summary>
        /// 計畫類別名稱
        /// </summary>
        public string PLANKINDNAME { get; set; }

        /// <summary>
        /// 提報機關ID
        /// </summary>
        public string CREATEORGOUID { get; set; }

        /// <summary>
        /// 提報機關
        /// </summary>
        public string ORGOUNAME { get; set; }

        /// <summary>
        /// 提報單位ID
        /// </summary>
        public string CREATEUNITOUID { get; set; }

        /// <summary>
        /// 提報單位
        /// </summary>
        public string UNITOUNAME { get; set; }

        /// <summary>
        /// 優先順序
        /// </summary>
        public int? PLANORDERNUMBER { get; set; }

        /// <summary>
        /// 送出狀態代碼
        /// </summary>
        public int IS_SEND { get; set; }

        /// <summary>
        /// 送出狀態
        /// </summary>
        public string SENDTYPE { get; set; }

        /// <summary>
        /// 存檔或審核狀態
        /// </summary>
        public string SAVETYPE { get; set; }

        /// <summary>
        /// 是否已審核
        /// </summary>
        public int SEND_STATUS { get; set; }

        /// <summary>
        /// 公務 OR 基金
        /// </summary>
        public string BUDGETTYPE { get; set; }

        /// <summary>
        /// 順序是否有效
        /// </summary>
        public int IsValid { get; set; }
    }
}
