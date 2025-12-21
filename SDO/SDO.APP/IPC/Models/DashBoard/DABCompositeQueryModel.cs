using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class DABCompositeQueryModel
    {
        /// <summary>
        /// 民國年度
        /// </summary>
        public string DAB_YEAR_YYY { get; set; }
        /// <summary>
        /// 西元年
        /// </summary>
        private int DabYearInt
        {
            get { return Convert.ToInt32(this.DAB_YEAR_YYY) + 1911; }
        } 
        /// <summary>
        /// 月份
        /// </summary>
        public string DAB_MONTH { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        private int DabMonthInt
        {
            get { return  Convert.ToInt32(this.DAB_MONTH); }
        }
        /// <summary>
        /// 報表類別
        /// </summary>
        public List<string> DabKinds { get; set; } = new();
        /// <summary>
        /// 代碼
        /// 執行機關、行政區、建設類別
        /// </summary>
        public string SET_TYPE { get; set; }
        /// <summary>
        /// 名稱
        /// 執行機關、行政區、建設類別
        /// </summary>
        public List<string> SET_VALUE { get; set; }

        private DateTime _QueryPeriodSt { get; set; }
        private DateTime _QueryPeriodEnd { get; set; }
        /// <summary>
        /// 查詢區間(起)
        /// </summary>
        public DateTime QueryPeriodSt 
        { 
            get 
            {
                if (this.IsQueryFullYear)
                {
                    return new DateTime(this.DabYearInt, this.DabMonthInt, 1)
                        .AddYears(-1).AddMonths(1);
                }
                else
                {
                    return this._QueryPeriodSt;
                }
            } 
            set 
            {
                this._QueryPeriodSt = value;            
            }
        }
        /// <summary>
        /// 查詢區間(迄)
        /// </summary>
        public DateTime QueryPeriodEnd
        {
            get
            {
                return this.IsQueryFullYear ? new DateTime(this.DabYearInt, this.DabMonthInt, 1) 
                    : this._QueryPeriodEnd;
            }
            set
            {
                this._QueryPeriodEnd = value;
            }
        }

        /// <summary>
        /// 是否查詢整年
        /// </summary>
        public bool IsQueryFullYear { get; set; }
        /// <summary>
        /// 是否為執行中列管情形資料
        /// </summary>
        public bool IS_IN_PROGRESS_DATA { get; set; }
    }
}
