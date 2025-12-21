using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 提案提案類別
    /// </summary>
    public class InnProjectProposalTypeModel : DbEditor
    {
        /// <summary>
        /// 序號
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 提案編號
        /// </summary>
        public string INN_PLAN_NO { get; set; }

        /// <summary>
        /// 主要/次要(涉及)
        /// </summary>
        public int PROPOSAL_KIND { get; set; }

        /// <summary>
        /// 類別ID
        /// </summary>
        public int PROPOSAL_TYPE_ID { get; set; }

        public string PROPOSAL_TYPE_NAME { get; set; }

    }
}
