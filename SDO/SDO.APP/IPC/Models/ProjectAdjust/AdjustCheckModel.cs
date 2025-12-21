using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.ProjectAdjust
{
    public class AdjustCheckModel
    {
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NAME { get; set; }
        /// <summary>
        /// 需上傳的檔案類型  (SET_PARAM.SET_ITEM = 'FILE_KIND')
        /// </summary>
        public string FILE_KIND { get; set; }
        /// <summary>
        /// 計畫調整原因說明
        /// </summary>
        public string ADJUST_REASON { get; set; }
        /// <summary>
        /// 調整類別 (Y: 總期程調整/ M: 分月調整)
        /// </summary>
        public string SCHE_TYPE { get; set; }
        /// <summary>
        /// 已上傳的檔案個數
        /// </summary>
        public int CNT_FILE { get; set; }
    }
}
