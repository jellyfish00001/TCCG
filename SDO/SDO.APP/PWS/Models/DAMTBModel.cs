using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class DAMTBModel : DbEditor
    {
        /// <summary>
        /// 計畫序號
        /// </summary>
        public string PLANNO { get; set; }

        /// <summary>
        /// 需求細項編號
        /// </summary>
        public int FUNDID { get; set; }

        /// <summary>
        /// 經費需求細項
        /// </summary>
        public string FUNDDESC { get; set; }

        /// <summary>
        /// 計算方式說明
        /// </summary>
        public string CALCULATIONDESC { get; set; }

        /// <summary>
        /// 單價(千元)
        /// </summary>
        public int PRICE { get; set; }

        /// <summary>
        /// 數量
        /// </summary>
        public int AMOUNT { get; set; }

        // <summary>
        /// 總計(千元)
        /// </summary>
        public int FUNDTOT { get; set; }

    }
}
