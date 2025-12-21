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
    /// 參與提案人
    /// </summary>
    public class InnPartnerModel : DbEditor
    {
        /// <summary>
        /// 提案編號
        /// </summary>
        public string INN_PLAN_NO { get; set; }

        /// <summary>
        /// 機關
        /// </summary>
        public string PARTNER_ORG { get; set; }

        /// <summary>
        /// 所屬單位
        /// </summary>
        public string PARTNER_UNIT { get; set; }

        /// <summary>
        /// 職稱
        /// </summary>
        public string PARTNER_TITLE { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string PARTNER_NAME { get; set; }

    }
}
