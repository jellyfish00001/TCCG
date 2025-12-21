using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class CalendarModel: DbEditor
    {
        public int CALENDAR_ID { get; set; }
        public string CALENDAR_TITLE { get; set; }
        public DateTime CALENDAR_START_DATE { get; set; }
        public DateTime CALENDAR_END_DATE { get; set; }

        /// <summary>
        /// 狀態(0：公開 1：部門 2：私人)
        /// </summary>
        public string CALENDAR_TYPE { get; set; }
        public string CALENDAR_ORG { get; set; }
        public string CALENDAR_CONTENT { get; set; }
    }
}
