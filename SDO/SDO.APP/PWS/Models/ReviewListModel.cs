using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class ReviewListModel : DbEditor
    {
        /// <summary>
        /// 計畫序號
        /// </summary>
        public int PLANID { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLANNO { get; set; }

        /// <summary>
        /// 機關ID
        /// </summary>
        public string OU_ID { get; set; }

        /// <summary>
        /// 計畫年度
        /// </summary>
        public int PLANYEAR { get; set; }

        /// <summary>
        /// 計畫順序
        /// </summary>
        public int PLANORDERNUMBER { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 計畫狀態ID
        /// </summary>
        public int IS_SEND { get; set; }


        /// <summary>
        /// 計畫狀態代號
        /// </summary>
        public string SEND_STATUS { get; set; }

        /// <summary>
        /// 計畫狀態
        /// </summary>
        public string SENDTYPE { get; set; }

        /// <summary>
        /// 計畫類別ID
        /// </summary>
        public string PLANKIND { get; set; }

        /// <summary>
        /// 計畫類別
        /// </summary>
        public string PLANKINDNAME { get; set; }

        /// <summary>
        /// 計畫性質
        /// </summary>
        public string PLANDATETYPE { get; set; }

        /// <summary>
        /// 提報單位ID
        /// </summary>
        public string CREATEUNITOUID { get; set; }

        /// <summary>
        /// 提報機關ID
        /// </summary>
        public string CREATEORGOUID { get; set; }

        /// <summary>
        /// 提報機關
        /// </summary>
        public string UNITOUNAME { get; set; }

        /// <summary>
        /// 提報單位
        /// </summary>
        public string ORGOUNAME { get; set; }

    }
}
