using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 傳輸數據Model
    /// </summary>
    public class MailQueueMdfModel: MailQueueModel
    {
        /// <summary>
        /// 郵件排程寄件者
        /// </summary>
        [Required]
        [Display(Name = "MailQueue_MAIL_FROM", ResourceType = typeof(i18N.Label))]
        public MailAddressModel MAIL_ADDRESS_FROM { get; set; }

        /// <summary>
        /// 郵件排程收件者
        /// </summary>
        [Required]
        [Display(Name = "MailQueue_MAIL_TO", ResourceType = typeof(i18N.Label))]
        public IList<MailAddressModel> MAIL_ADDRESS_TO { get; set; }

        /// <summary>
        /// 轉回JsonStr儲存到DB
        /// </summary>
        /// <returns></returns>
        public MailQueueQryResultModel ToQueryData()
        {
            ParseUtil.Parse(this, out MailQueueQryResultModel queryData);
            queryData.MAIL_FROM = JsonConvert.SerializeObject(MAIL_ADDRESS_FROM);
            queryData.MAIL_TO = JsonConvert.SerializeObject(MAIL_ADDRESS_TO);

            return queryData;
        }
    }
}
