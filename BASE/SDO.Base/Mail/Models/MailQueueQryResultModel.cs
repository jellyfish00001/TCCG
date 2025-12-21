using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SDO.Models
{
    /// <summary>
    /// 資料庫數據Model
    /// </summary>
    public class MailQueueQryResultModel : MailQueueModel, IDbEditor
    {
        /// <summary>
        /// 寄件人
        /// </summary>
        public string MAIL_FROM { get; set; }

        /// <summary>
        /// 收件人
        /// </summary>
        public string MAIL_TO { get; set; }

        /// <summary>
        /// 建立使用者
        /// </summary>
        public string CRT_USER { get; set; }

        /// <summary>
        /// 修改使用者
        /// </summary>
        public string MDF_USER { get; set; }

        /// <summary>
        /// 解碼功能
        /// </summary>
        /// <returns></returns>
        public MailQueueQryResultModel Decode()
        {
            MAIL_FROM = HttpUtility.HtmlDecode(MAIL_FROM);
            MAIL_TO = HttpUtility.HtmlDecode(MAIL_TO);
            MAIL_CONTENT = HttpUtility.HtmlDecode(HttpUtility.HtmlDecode(MAIL_CONTENT));
            return this;
        }

        /// <summary>
        /// 日期格式從JsonStr deserialize成MailAddress類
        /// </summary>
        /// <returns></returns>
        public MailQueueMdfModel ToEditView()
        {
            ParseUtil.Parse(this, out MailQueueMdfModel editView);
            editView.MAIL_ADDRESS_FROM = JsonConvert.DeserializeObject<MailAddressModel>(MAIL_FROM);
            editView.MAIL_ADDRESS_TO = MAIL_TO == null ? null : JsonConvert.DeserializeObject<IList<MailAddressModel>>(MAIL_TO);

            return editView;
        }
    }
}
