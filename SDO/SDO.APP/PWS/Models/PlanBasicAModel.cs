using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class PlanBasicAModel : DbEditor
    {
        
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 計畫ID
        /// </summary>
        public int PLANID { get; set; }

        /// <summary>
        /// 局處ID
        /// </summary>
        public string OU_ID { get; set; }

        /// <summary>
        /// 局處名稱
        /// </summary>
        public string OU_NAME { get; set; } 

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLANNO { get; set; }

        /// <summary>
        /// 計畫類別 1.重要施政計畫;2.委託研究計畫
        /// </summary>
        public string PLANKIND { get; set; }

        /// <summary>
        /// 計畫年度
        /// </summary>
        public int PLANYEAR { get; set; }

        /// <summary>
        /// 提報機關
        /// </summary>
        public string CREATEORGOUID { get; set; }

        /// <summary>
        /// 機關名稱
        /// </summary>
        public string ORGOUNAME { get; set; }

        /// <summary>
        /// 提報單位
        /// </summary>
        public string CREATEUNITOUID { get; set; }

        /// <summary>
        /// 單位名稱
        /// </summary>
        public string UNITOUNAME { get; set; }

        /// <summary>
        /// 計畫起始日期
        /// </summary>
        public string PLANSTARTDATE { get; set; }

        /// <summary>
        /// 計畫結束日期
        /// </summary>
        public string PLANENDDATE { get; set; }

        /// <summary>
        /// 工程/非工程
        /// </summary>
        public string CP_KIND { get; set; }

        /// <summary>
        /// 計畫性質 1.新興計畫 2.延續性計畫 3. 單一年度計畫 4.跨年度計畫
        /// 3,4 for委託選項
        /// </summary>
        public string PLANDATETYPE { get; set; }

        /// <summary>
        /// 是否為市長政策
        /// </summary>
        public string PLANORGINYN { get; set; }

        /// <summary>
        /// 公務預算千元
        /// </summary>
        public int? PUBLICMONEY { get; set; }

        /// <summary>
        /// 基金預算千元
        /// </summary>
        public int? FUNDMONEY { get; set; }

        /// <summary>
        /// 基金序號
        /// </summary>
        public int? FUNDNO { get; set; }

        /// <summary>
        /// 中央預算千元
        /// </summary>
        public int? CENTERMONEY { get; set; }

        /// <summary>
        /// 是公務預算或基金預算
        /// </summary>
        public string BUDGETTYPE { get; set; }

        /// <summary>
        /// 未核定與否
        /// </summary>
        public string APPROVEDYN { get; set; }

        /// <summary>
        /// 是否已報核定(未核定)
        /// </summary>
        public string APPLYAPPROVEDYN { get; set; }

        /// <summary>
        /// 核定文號
        /// </summary>
        public string APPROVEDNUMBER { get; set; }

        /// <summary>
        /// 其他預算千元
        /// </summary>
        public int? OTHERMONEY { get; set; }

        /// <summary>
        /// 其他預算說明
        /// </summary>
        public string OTHERDESC { get; set; }

        /// <summary>
        /// 計畫需求總數
        /// </summary>
        public int PLANTOTMONEY { get; set; }

        /// <summary>
        /// 是否涉及建築裝修工程
        /// </summary>
        public int PLANCONTENTENGINE { get; set; }

        /// <summary>
        /// 是否涉及用地取得
        /// </summary>
        public int PLANCONTENTLAND { get; set; }

        /// <summary>
        /// 是否涉及資訊費用
        /// </summary>
        public int PLANCONTENTINFO { get; set; }

        /// <summary>
        /// 是否涉及區公所養護專案
        /// </summary>
        public int PLANMAINTAIN { get; set; }

        /// <summary>
        /// 說明計畫之必要性
        /// </summary>
        public string EXPLAINNECESSITY { get; set; }

        /// <summary>
        /// 說明計畫之基本資料
        /// </summary>
        public string EXPLANBASICINFO { get; set; }

        /// <summary>
        /// 說明計畫之執行方式
        /// </summary>
        public string EXPLANEXECUTE { get; set; }

        /// <summary>
        /// 說明經費成長幅度及原因
        /// </summary>
        public string EXPLANIMPROVE { get; set; }

        /// <summary>
        /// 是否產生後續維護費用
        /// </summary>
        public string EXPLANFUND { get; set; }

        /// <summary>
        /// 重點工作規劃及工作期程
        /// </summary>
        public int? RUNWAY_C { get; set; }

        /// <summary>
        /// 重點工作規劃及工作期程
        /// </summary>
        public int? OLD_RUNWAY_C { get; set; }

        /// <summary>
        /// 是否送出(計畫送出)
        /// </summary>
        public int IS_SEND { get; set; }

        /// <summary>
        /// 是否規劃辦理性別影響評估
        /// </summary>
        public int GENDER_ANALYST_YN { get; set; }

        /// <summary>
        /// 是否送出(優先順序)
        /// </summary>
        public int SEND_STATUS { get; set; }

        /// <summary>
        /// 是否須檢附成本效益分析報告
        /// </summary>
        public int ATTA_COST_YN { get; set; }

        /// <summary>
        /// 計畫是否送出過
        /// </summary>
        public bool IS_SEND_ONTIME { get; set; }

        /// <summary>
        /// 存檔訊息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 舊計畫編號
        /// </summary>
        public string OldPlanNo { get; set; }

        /// <summary>
        /// 跨年度經費Model
        /// </summary>
        public List<PlanCrossAMTAModel> PlanCrossAMTA { get; set; }

        /// <summary>
        /// 自訂檢核點資料
        /// </summary>
        public List<ProjectCusCheckpointModel> CusCheckpointModels { get; set; }

        /// <summary>
        /// 存入檔案資料
        /// </summary>
        public List<ProjectAttachmentModel> Files { set; get; }
    }
}
