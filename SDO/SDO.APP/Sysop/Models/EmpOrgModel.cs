using System.ComponentModel.DataAnnotations;

namespace SDO.Models
{
    public class EmpOrgModel : DbEditor
    {
        /// <summary>
        /// 機關代碼
        /// </summary>
        public virtual string ORG_ID { get; set; }

        /// <summary>
        /// 機關名稱
        /// </summary>
        public virtual string ORG_NAME { get; set; }

        public string ORG_DISPLAY
        {
            get
            {
                return string.Format("{0}-{1}", this.ORG_ID, this.ORG_NAME);
            }
            private set { }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string PARENT_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool DEL_FLG { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool HASCHILDREN { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string[] USERS { get; set; }

        /// <summary>
        /// 機關電話
        /// </summary>
        public string TEL { get; set; }


    }
}
