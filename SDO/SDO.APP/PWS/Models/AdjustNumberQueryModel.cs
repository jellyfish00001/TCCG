using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class AdjustNumberQueryModel : DbEditor
    {
        /// <summary>
        /// 計畫序號
        /// </summary>
        public int PLANID { get; set; }

        /// <summary>
        /// 計畫機關
        /// </summary>
        public string OU_ID { get; set; }

        /// <summary>
        /// 提報機關
        /// </summary>
        public string CREATEORGOUID { get; set; }

        /// <summary>
        /// 計畫年度
        /// </summary>
        public int PLANYEAR { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLANNO { get; set; }

        /// <summary>
        /// 計畫類別
        /// </summary>
        public string PLANKIND { get; set; }

        /// <summary>
        /// 計畫是否送出
        /// </summary>
        public string IS_SEND { get; set; }

        /// <summary>
        /// 基金序號
        /// </summary>
        public int? FUNDNO { get; set; }

        /// <summary>
        /// 公務預算or基金預算
        /// </summary>
        public string BUDGETTYPE { get; set; }

        /// <summary>
        /// 送出狀態
        /// </summary>
        public string SENDTYPE { get; set; }

        /// <summary>
        /// 存檔或審核狀態
        /// </summary>
        public string SaveType { get; set; }

        /// <summary>
        /// 優先順序排序資料
        /// </summary>

        public List<AdjustNumberModel> AdjustNumberModels { get; set; }

    }
}
