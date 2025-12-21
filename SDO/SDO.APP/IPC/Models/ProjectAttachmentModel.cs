using SDO.Base.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫檔案資料
    /// </summary>
    public class ProjectAttachmentModel : DbEditor
    {
        /// <summary>
        /// 識別欄位
        /// </summary>
        public int IDENTITY_FIELD { get; set; }

        /// <summary>
        /// 計畫編號 (參考資料為0)
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 檔案類型 ref SET_PARAM.SET_ITEM='FILE_KIND'
        /// </summary>
        public string FILE_KIND { get; set; }

        /// <summary>
        /// 檔案類型中文
        /// </summary>
        public string NAME { get; set; }

        /// <summary>
        /// 檔案名稱
        /// </summary>
        public string FILE_NAME { get; set; }

        /// <summary>
        /// 檔案描述
        /// </summary>
        public string FILE_MEMO { get; set; }

        /// <summary>
        /// 檔案路徑
        /// </summary>
        public string FILE_PATH { get; set; }

        /// <summary>
        /// 檔案上傳來源 01：相關檔案上傳  02：其他地方上傳
        /// </summary>
        public string FILE_UP_SOURCE { get; set; }

        /// <summary>
        /// 是否同步顯示於主辦畫面 (NULL為主辦上傳、0不顯示、1顯示)
        /// </summary>
        public bool? IS_DISPLAY { get; set; }

        /// <summary>
        /// 上傳來源流水號
        /// </summary>
        public int? SOURCE_ID { get; set; }

        /// <summary>
        /// 異動檔案
        /// </summary>
        public List<UploadTempFileModel> EditFiles { get; set; }
        /// <summary>
        /// 上傳日期
        /// </summary>
        public DateTime CRT_DATE { get; set; }

        /// <summary>
        /// 是否多檔上傳
        /// </summary>
        public bool IsMultiple { get; set; }

        public int? SORT_ORDER { get; set; }

        /// <summary>
        /// 判斷存入的DB
        /// </summary>
        public int DB { get; set; }

        /// <summary>
        /// 檔案存放資料夾名稱
        /// </summary>
        public string FOLDER_NAME { get; set; }
    }
}
