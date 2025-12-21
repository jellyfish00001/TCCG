using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class InnProjectBasicFillModel :DbEditor
    {
        /// <summary>
        /// 提案序號
        /// </summary>
        public string INN_PLAN_NO { get; set; }
        /// <summary>
        /// 創新提案基本資料
        /// </summary>
        public InnProjectBasicModel InnProjectBasic { get; set; }

        /// <summary>
        /// 提案提案類別
        /// </summary>
        public List<InnProjectProposalTypeModel> InnProjectProposalType { get; set; }

        /// <summary>
        /// 自定義欄位
        /// </summary>
        public List<ProjectCusFieldModel> InnProjectCusFields { get; set; }

        /// <summary>
        /// 自定義欄位值
        /// </summary>
        public List<InnProjectCusFieldValueModel> InnProjectCusFieldValue { get; set; }

        /// <summary>
        /// 參與提案人
        /// </summary>
        public List<InnPartnerModel> InnPartner { get; set; }

        /// <summary>
        /// 檔案上傳
        /// </summary>
        public List<ProjectAttachmentModel> Files { set; get; }
    }
}
