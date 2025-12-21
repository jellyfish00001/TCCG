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
    public class ProjectManageQueryModel : DbEditor
    {
        /// <summary>
        /// 提案年度
        /// </summary>
        public string INN_YEAR { get; set; }

        /// <summary>
        /// 提案編號
        /// </summary>
        public string INN_PLAN_NO { get; set; }

        /// <summary>
        /// 提案名稱
        /// </summary>
        public string INN_PLAN_NAME { get; set; }

        /// <summary>
        /// 主要提案類別
        /// </summary>
        public string PROPOSAL_TYPE { get; set; }

        /// <summary>
        /// 組別
        /// </summary>
        public string GROUP { get; set; }

        /// <summary>
        /// 主要提案人性別
        /// </summary>
        public string SPONSOR_SEX { get; set; }

        public string OU_ID { get; set; }

        public string CONTACT_NAME { get; set; }

        /// <summary>
        /// 是否為提案頁面
        /// </summary>
        public bool isProjectProposal { get; set; }

    }
}
