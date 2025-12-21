using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class ReviewListQueryModel
    {
        /// <summary>
        /// 計畫年度
        /// </summary>
        public int PLANYEAR { get; set; }

        /// <summary>
        /// 計畫年度
        /// </summary>
        public string PLANNO { get; set; }

        /// <summary>
        /// 計畫狀態0:未送出 1:送出 
        /// </summary>
        public int? SEND_STATUS { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 提報單位(ID)
        /// </summary>
        public string CREATEUNITOUID { get; set; }

        /// <summary>
        /// 提報機關(ID)
        /// </summary>
        public string CREATEORGOUID { get; set; }

        /// <summary>
        /// 提報單位
        /// </summary>
        public string UNITOUNAME { get; set; }

        /// <summary>
        /// 提報機關
        /// </summary>
        public string ORGOUNAME { get; set; }

        /// <summary>
        /// 計畫機關
        /// </summary>
        public string OU_ID { get; set; }

        /// <summary>
        /// 計畫類別
        /// </summary>
        public string PLANKIND { get; set; }
    }
}
