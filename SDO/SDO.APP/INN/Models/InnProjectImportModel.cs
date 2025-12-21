using Microsoft.AspNetCore.Http;
using SDO.Base.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class InnProjectImportModel : InnProjectBasicModel

    {
        /// <summary>
        /// 提案類別代號列
        /// </summary>
        public string codeList { get; set; }
        /// <summary>
        /// 主要/次要(涉及)
        /// </summary>
        public int PROPOSAL_KIND { get; set; }
        /// <summary>
        /// 提案人機關ID
        /// </summary>
        public string SPONSOR_ORG_NAME { get; set; }
        /// <summary>
        /// 參與提案人ID
        /// </summary>
        public string PARTNER_USERID_1 { get; set; }
        /// <summary>
        /// 參與提案人ID
        /// </summary>
        public string PARTNER_USERID_2 { get; set; }
        /// <summary>
        /// 參與提案人ID
        /// </summary>
        public string PARTNER_USERID_3 { get; set; }
        /// <summary>
        /// 參與提案人ID
        /// </summary>
        public string PARTNER_USERID_4 { get; set; }
        /// <summary>
        /// 參與提案人ID
        /// </summary>
        public string PARTNER_USERID_5 { get; set; }
        /// <summary>
        /// 記數
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public List<string> ErrMsgs { get; set; }
        /// <summary>
        /// 匯入年度
        /// </summary>
        public string PlanYear { get; set; }
        /// <summary>
        /// 匯入檔案
        /// </summary>
        public IFormFile ImportFile { get; set; }
        /// <summary>
        /// 參與提案人
        /// </summary>
        public List<InnPartnerModel> PartnerModels { get; set; }
        /// <summary>
        /// 提案類別
        /// </summary>
        public List<InnProjectProposalTypeModel> ProposalTypeModels { get; set; }
    }
}
