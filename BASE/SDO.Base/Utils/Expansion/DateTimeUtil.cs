using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Utils
{
    public static class DateTimeUtil
    {
        /// <summary>
        /// 取得當下時間(server local)
        /// </summary>
        public static DateTime Now => DateTime.Now;

        /// <summary>
        ///  取得當下時間Utc
        /// </summary>
        public static DateTime UtcNow => DateTime.UtcNow;

        /// <summary>
        ///  取得台灣當下時間Utc+8
        /// </summary>
        public static DateTime GMT8 => DateTime.UtcNow.AddHours(8);

        /// <summary>
        /// server local date
        /// </summary>
        public static DateTime Today => Now.Date;

        /// <summary>
        /// utc today
        /// </summary>
        public static DateTime TodayUtc => UtcNow.Date;

        /// <summary>
        ///  Taiwan today
        /// </summary>
        public static string TodayTW => ToTwDateString(Today);

        /// <summary>
        /// Taiwan today GMT+8
        /// </summary>
        public static string TodayTWGmt => ToTwDateString(GMT8);

        /// <summary>
        /// Taiwan now time
        /// </summary>
        public static string NowtimeTW => ToTwDateString(Now, "yyy/MM/dd HH:mm:ss");

        /// <summary>
        /// taiwan gmt+8 server time
        /// </summary>
        public static string TodaytimeTWGMT => ToTwDateString(GMT8, "yyy/MM/dd HH:mm:ss");

        /// <summary>
        /// 字串轉日期(轉不成功為null)
        /// </summary>
        /// <param name="dtStr">日期字串</param>
        /// <returns></returns>
        public static DateTime? ToDateTimeWithNull(this string dtStr, bool isTwStr = false)
        {
            if (string.IsNullOrEmpty(dtStr))
            {
                return null;
            }

            DateTime dt;

            if (!isTwStr && DateTime.TryParse(dtStr, out dt))
            {
                return dt;
            }

            if (!isTwStr &&DateTime.TryParseExact(dtStr, "yyyyMMdd", null, DateTimeStyles.None, out dt))
            {
                return dt;
            }

            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            if (DateTime.TryParse(dtStr, culture, DateTimeStyles.AdjustToUniversal, out dt))
            {
                return dt;
            }

            return null;
        }

        /// <summary>
        /// 字串轉民國年
        /// </summary>
        /// <param name="dtStr">日期字串</param>
        /// <param name="format">格式
        /// yyy年MM月dd日
        /// yyy年MM月
        /// yyy.MM.dd
        /// yyy.MM
        /// </param>
        /// <returns></returns>
        public static string ToTwDateString(this string dtStr, string format = "yyy/MM/dd")
        {
            DateTime? dt = ToDateTimeWithNull(dtStr);
            if (!dt.HasValue)
            {
                return string.Empty;
            }

            return ToTwDateString(dt, format);
        }

        /// <summary>
        /// 西元年轉民國年
        /// </summary>
        /// <param name="dt">日期</param>
        /// <param name="format">格式</param>
        /// <returns></returns>
        public static string ToTwDateString(this DateTime? dt, string format = "yyy/MM/dd")
        {
            if (dt.HasValue)
            {
                return dt.Value.ToTwDateString(format);
            }
            return string.Empty;
        }

        /// <summary>
        /// 西元年轉民國年
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToTwDateString(this DateTime dt, string format = "yyy/MM/dd")
        {
            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            return dt.ToString(format, culture);
        }

        /// <summary>
        /// 計算期程
        /// EX：110年2月1日 至 110年3月1日 （共1個月）
        /// </summary>
        /// <param name="excu_st"></param>
        /// <param name="excu_end"></param>
        /// <returns></returns>
        public static string ExcuDateDisplay(string excu_st, string excu_end)
        {
            return $"{excu_st.ToTwDateString("yyy年MM月dd日")} 至 {excu_end.ToTwDateString("yyy年MM月dd日")} （共{GetExcuMonth(excu_st, excu_end)}個月）";
        }

        /// <summary>
        /// 取得期程月份
        /// </summary>
        /// <param name="excu_st">起日</param>
        /// <param name="excu_end">迄日</param>
        /// <returns></returns>
        public static int GetExcuMonth(string excu_st, string excu_end)
        {
            //計算期程月份
            int excuMonth = 0;
            DateTime? excuStDate = excu_st.ToDateTimeWithNull();
            DateTime? excuEndDate = excu_end.ToDateTimeWithNull();
            //開始、結束都有值才計算
            if (excuStDate.HasValue && excuEndDate.HasValue)
            {
                excuMonth = (excuEndDate.Value.Year - excuStDate.Value.Year) * 12 + (excuEndDate.Value.Month - excuStDate.Value.Month + 1);
            }
            return excuMonth;
        }

        /// <summary>
        /// 取得期程年份
        /// </summary>
        /// <param name="excu_st">起日</param>
        /// <param name="excu_end">迄日</param>
        /// <returns></returns>
        public static int GetExcuYear(string excu_st, string excu_end)
        {
            //計算期程月份
            int excuYear = 0;
            DateTime? excuStDate = excu_st.ToDateTimeWithNull();
            DateTime? excuEndDate = excu_end.ToDateTimeWithNull();
            //開始、結束都有值才計算
            if (excuStDate.HasValue && excuEndDate.HasValue)
            {
                excuYear = excuEndDate.Value.Year - excuStDate.Value.Year + 1;
            }
            return excuYear;
        }
    }
}
