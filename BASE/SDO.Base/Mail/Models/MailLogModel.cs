using System;

namespace SDO.Models
{
    public class MailLogModel : DbEditor
    {
        public int DATA_COUNT { get; set; }
        public string MAIL_SENDER { get; set; }
        public string MAIL_RECEIVER { get; set; }
        public string CC_RECEIVER { get; set; }
        public string BCC_RECEIVER { get; set; }
        public string MAIL_SUBJECT { get; set; }
        public string MAIL_CONTENT { get; set; }
        public string MAIL_ID  { get; set; }
        public string PROJECT_NO { get; set; }
        public bool SEND_FLG { get; set; }
        public DateTime LOG_DATE { get; set; }
    }
}
