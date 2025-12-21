using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class ListExecModel 
    {
        /// <summary>
        /// 主管機關ID
        /// </summary>
        public string OU_ID { get; set; }

        /// <summary>
        /// 主管機關名稱
        /// </summary>
        public string OU_NAME { get; set; }

        /// <summary>
        /// 計畫年度
        /// </summary>
        public int PLANYEAR { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLANNO { get; set; }

        /// <summary>
        /// 計畫類別ID
        /// </summary>
        public int PLANKIND { get; set; }

        /// <summary>
        /// 計畫類別名稱
        /// </summary>
        public string PLANKINDNAME { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 提報單位ID
        /// </summary>
        public string CREATEUNITOUID { get; set; }

        /// <summary>
        /// 提報機關ID
        /// </summary>
        public string CREATEORGOUID { get; set; }

        /// <summary>
        /// 提報單位
        /// </summary>
        public string UNITOUNAME { get; set; }

        /// <summary>
        /// 提報單位
        /// </summary>
        public string ORGOUNAME { get; set; }

        /// <summary>
        /// 計畫是否送出
        /// </summary>
        public bool IS_SEND { get; set; }

        /// <summary>
        /// 計畫狀態
        /// </summary>
        public string SEND_TYPE { get; set; }

    }
}
