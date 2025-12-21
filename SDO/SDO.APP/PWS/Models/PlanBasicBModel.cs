using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class PlanBasicBModel : DbEditor
    { 
        /// <summary>
        /// 計畫類別 1.重要施政計畫 2.委託研究計畫
        /// </summary>
        public string PLANKIND { get; set; }


        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLANNO { get; set; }

        /// <summary>
        /// 機關ID
        /// </summary>
        public string OU_ID { get; set; }

        /// <summary>
        /// 機關名稱
        /// </summary>
        public string OU_NAME { get; set; } 

        /// <summary>
        /// 提報機關
        /// </summary>
        public string CREATEORGOUID { get; set; }

        /// <summary>
        /// 提報單位
        /// </summary>
        public string CREATEUNITOUID { get; set; }
        

        /// <summary>
        /// 計畫性值
        /// </summary>
        public string LABORYN { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 計畫年度
        /// </summary>
        public int PLANYEAR { get; set; }

        /// <summary>
        /// 計畫起始日期
        /// </summary>
        public string PLANSTARTDATE { get; set; }

        /// <summary>
        /// 計畫結束日期
        /// </summary>
        public string PLANENDDATE { get; set; }

        /// <summary>
        /// 決標年月
        /// </summary>
        public string AWARDYM { get; set; }

        /// <summary>
        /// 期中報告年月
        /// </summary>
        public string MIDREPORTYM { get; set; }

        /// <summary>
        /// 期末報告年月
        /// </summary>
        public string FINAKREPORTYM { get; set; }

        /// <summary>
        /// 結案年月
        /// </summary>
        public string CLOSEYM { get; set; }

        /// <summary>
        /// 計畫性質 3.單一  4.跨年度
        /// </summary>
        public string PLANDATETYPE { get; set; }

        /// <summary>
        /// 是否為市長政策
        /// </summary>
        public string PLANORGINYN { get; set; }

        /// <summary>
        /// 公務預算千元
        /// </summary>
        public int PUBLICMONEY { get; set; }

        /// <summary>
        /// 基金預算千元
        /// </summary>
        public int FUNDMONEY { get; set; }

        /// <summary>
        /// 基金序號
        /// </summary>
        public int? FUNDNO { get; set; }

        /// <summary>
        /// 中央預算千元
        /// </summary>
        public int CENTERMONEY { get; set; }

        /// <summary>
        /// 核定否
        /// </summary>
        public string APPROVEDYN { get; set; }

        /// <summary>
        /// 年度經費總計
        /// </summary>
        public int PLANTOTMONEY { get; set; }

        /// <summary>
        /// 未核定是否已報核定
        /// </summary>
        public string APPLYAPPROVEDYN { get; set; }

        /// <summary>
        /// 核定文號
        /// </summary>
        public string APPROVEDNUMBER { get; set; }

        /// <summary>
        /// 其他預算千元
        /// </summary>
        public int OTHERMONEY { get; set; }

        /// <summary>
        /// 其他預算來源說明
        /// </summary>
        public string OTHERDESC { get; set; }
        
        /// <summary>
        /// 其他預算來源說明
        /// </summary>
        public string BUDGETTYPE { get; set; }

        /// <summary>
        /// 研究原因及目的
        /// </summary>
        public string PLANCAUSE { get; set; }

        /// <summary>
        /// 預期研究成果
        /// </summary>
        public string PLANEXPECTED { get; set; }

        /// <summary>
        /// 姿樺是否送出修先順序
        /// </summary>
        public int IS_SEND { get; set; }

        /// <summary>
        /// 計畫是否送出過
        /// </summary>
        public bool IS_SEND_ONTIME { get; set; }

        /// <summary>
        /// 跨年度經費Model
        /// </summary>
        public List<PlanCrossAMTAModel> PlanCrossAMTB { get; set; }

        /// <summary>
        /// 檔案資料
        /// <summary>
        public List<ProjectAttachmentModel> Files { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; }
    }
}
