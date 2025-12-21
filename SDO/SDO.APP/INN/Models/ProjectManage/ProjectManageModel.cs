using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Models;

namespace SDO.APP.INN.Models.ProjectManage
{
    /// <summary>
    /// 資料登錄查詢Model
    /// </summary>
    public class ProjectManageModel : DbEditor
    {

        /// <summary>
        /// 提案編號
        /// </summary>
        public string INN_PLAN_NO { get; set; }

        /// <summary>
        /// 提案編號
        /// </summary>
        public string INN_YEAR { get; set; }

        /// <summary>
        /// 提案名稱
        /// </summary>
        public string INN_PLAN_NAME { get; set; }

        /// <summary>
        /// 主要提案類別
        /// </summary>
        public string CODE_VALUE { get; set; }

        /// <summary>
        /// 主要提案人
        /// </summary>
        public string SPONSOR_NAME { get; set; }

        /// <summary>
        /// 主要提案機關
        /// </summary>
        public string SPONSOR_ORG { get; set; }

        /// <summary>
        /// 主要提案人所屬單位
        /// </summary>
        public string SPONSOR_UNIT { get; set; }

        /// <summary>
        ///  主要提案機關(單位)
        /// </summary>
        public string SPONSOR_ORG_UNIT { get;set; }

        public string OU_ID { get; set; }

        public string OU_NAME { get; set; }

    }
}
