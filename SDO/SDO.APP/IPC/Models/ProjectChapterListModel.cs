using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫章節表(配合前端功能列所需結構)
    /// </summary>
    public class ProjectChapterListModel 
    {
        /// <summary>
        /// 計畫章節 名稱
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 計畫章節代碼
        /// </summary>
        public string CHAPTER_ID { get; set; }

        /// <summary>
        /// 階段
        /// </summary>
        public string STAGE { get; set; }

        /// <summary>
        /// 程式路徑
        /// </summary>
        public string SOURCE_PATH { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int SORT_ORDER { get; set; }

        /// <summary>
        /// 處理CHAPTER前面要串上的字，A:數字、B:審核類￭、C:其他–
        /// </summary>
        public string SYMBOL { get; set; }
    }
}
