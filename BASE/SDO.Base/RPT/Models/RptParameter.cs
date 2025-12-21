using SDO.ReportBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Models
{
    /// <summary>
    /// 報表額外參數
    /// </summary>
    public class RptParameter : RptBaseParameter
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        public string YEAR { get; set; }

        public List<string> PROJECT_NO_DATA { get; set; }  

        public RptParameter()
        {
            PROJECT_NO_DATA = new List<string>();
        }
      
        /// <summary>
        /// 調整流水號
        /// </summary>
        public int PROJ_ADJ_ID { get; set; }
        /// <summary>
        /// 區分報表類型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 是否為管考功能
        /// </summary>
        public bool IsRdecFun { get; set; }

        /// <summary>
        /// 報表所需參數，用來放model
        /// </summary>
        public object ObjectModel { get; set; }

        /// <summary>
        /// 是否為差異比對
        /// </summary>
        public bool IsDiffCompare { get; set; }

        /// <summary>
        /// 當下的比對編號
        /// </summary>
        public int DiffId { get; set; }
    }
}
