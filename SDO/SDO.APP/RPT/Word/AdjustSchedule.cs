using Aspose.Words;
using Aspose.Words.Tables;
using Autofac;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Base.RPT.Enums;
using SDO.Base.Utils.Models;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    public class AdjustSchedule : WContentBuilder
    {
        private readonly IProjectAdjustService service;
        private readonly IProjectService projectService;
        private readonly IDropDownService dropDownService;
        private readonly ISetParamService setParamService;

        private RPTAdjustScheduleModel rptData;
        private AdjustReasonScheduleViewModel adjustReasonData;
        private List<ProjectCusCheckpointModel> checkPoint;
        private List<AdjustCusCheckPointModel> adjustCheckPoint;
        private IList<SetParamModel> setParamModels;
        private List<ProjectAttachmentModel> finishedEngineerFile;
        public AdjustSchedule(IComponentContext coms) : base(coms)
        {
            this.service = coms.Resolve<IProjectAdjustService>();
            this.projectService = coms.Resolve<IProjectService>();
            this.dropDownService = coms.Resolve<IDropDownService>();
            this.setParamService = coms.Resolve<ISetParamService>();
        }

        protected override async Task GetData()
        {
            // 取報表所需資料
            rptData = await service.GetRPTAdjustSchedule(Parameter.PROJ_ADJ_ID);
            adjustReasonData = await service.GetExecReasonSchedule(Parameter.PROJ_ADJ_ID);
            setParamModels = await setParamService.GetSysParams("ADJUST_REASON");
            // 取執行機關文字
            List<DropDownListModel> orgList = await dropDownService.GetOrganList();
            DropDownListModel execOrganCModel = orgList.Where(x => x.value == rptData.EXEC_ORGAN_C).FirstOrDefault();
            rptData.EXEC_ORGAN_C = execOrganCModel.text;
            // 取檢核點
            ProjectCheckpointModel projectCheckpointModel = await projectService.GetProjectCheckpoint(Parameter.PROJECT_NO);
            checkPoint = projectCheckpointModel.CusCheckpointModels;
            AdjustCheckPointModel adjustCheckPointModel = await service.GetAdjustCheckPoint(Parameter.PROJ_ADJ_ID);
            adjustCheckPoint = adjustCheckPointModel.CusCheckpointModels;
            finishedEngineerFile = adjustCheckPointModel.Files;
        }

        /// <summary>
        /// 標題
        /// </summary>
        protected override void Title()
        {
            SetTitle("桃園市政府重大建設計畫選項列管案件計畫調整申請表", alignment: ParagraphAlignment.Center);
            SetSubTitle($"填表日期：{DateTimeUtil.TodayTW}", alignment: ParagraphAlignment.Right, fontSize: 12);
        }

        /// <summary>
        /// 內容
        /// </summary>
        protected override void Content()
        {
            // 基本資料欄位
            ProjectInfo();

            Builder.Writeln(string.Empty);

            // 修正計畫檢核點進度
            CheckPointClassData();

            Builder.Writeln(string.Empty);

            // 檢附說明事證資料
            AttachmentFile();

            Builder.Writeln(string.Empty);

            // 簽章
            Signature();
        }

        /// <summary>
        /// 基本資料欄位
        /// </summary>
        private void ProjectInfo()
        {
            Table = Builder.StartTable();

            SetThColumn("計畫編號", isBold: true, hMergeCount: 2);
            SetTdColumn(Parameter.PROJECT_NO);
            SetThColumn("執行機關", isBold: true);
            SetTdColumn(rptData.EXEC_ORGAN_C);
            SetThColumn("調整次數\n及項目", isBold: true);
            string scheType = rptData.SCHE_TYPE;
            SetTdColumn($"第{(scheType == "Y" ? rptData.SCHE_Y_CNT : rptData.SCHE_M_CNT) + 1}次{(scheType == "Y" ? "總期程" : "分月")}調整，" +
                $"曾辦理{(scheType == "Y" ? rptData.SCHE_M_CNT : rptData.SCHE_Y_CNT)}次{(scheType == "Y" ? "分月" : "總期程")}調整。");
            Builder.EndRow();

            SetThColumn("計畫名稱", isBold: true, hMergeCount: 2);
            SetTdColumn(rptData.PROJECT_NAME, hMergeCnt: 5);
            Builder.EndRow();

            // 計畫基本資料
            SetProjectData();

            SetThColumn("中央補助款", isBold: true, hMergeCount: 2);
            SetTdColumn(GetBudgetCentral(), hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn("目前\n執行情形", isBold: true, hMergeCount: 2);
            SetTdColumn(adjustReasonData.CUR_EXECUTION, hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn("一、計畫調整原因說明：", alignment: ParagraphAlignment.Left, isBold: true, hMergeCount: 7);
            Builder.EndRow();

            SetTdColumn(adjustReasonData.ADJUST_REASON, hMergeCnt: 7);
            Builder.EndRow();

            Builder.EndTable();
            InitTable(Table, new List<double> { 4, 13, 19, 16, 16, 16, 16 });
        }

        /// <summary>
        /// 設定計畫基本資料
        /// </summary>
        private void SetProjectData()
        {
            SetThColumn("計畫基本資料", alignment: ParagraphAlignment.Center, isBold: true, vMerge: CellMerge.First);
            SetTdColumn("計畫\n金額", alignment: AlignmentEnum.Center);
            SetTdColumn(GetProjectAmt(), hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn(string.Empty, vMerge: CellMerge.Previous);
            SetTdColumn("承包\n廠商", alignment: AlignmentEnum.Center, vMerge: CellMerge.First);
            SetTdColumn("專案管理", alignment: AlignmentEnum.Center);
            SetTdColumn(adjustReasonData.TENDER_PROJ);
            SetTdColumn("設計單位", alignment: AlignmentEnum.Center);
            SetTdColumn(adjustReasonData.TENDER_DESIGN, hMergeCnt: 2);
            Builder.EndRow();
            
            SetThColumn(string.Empty, vMerge: CellMerge.Previous);
            SetTdColumn(string.Empty, vMerge: CellMerge.Previous);
            SetTdColumn("監造單位", alignment: AlignmentEnum.Center);
            SetTdColumn(adjustReasonData.TENDER_SUPV);
            SetTdColumn("施工單位", alignment: AlignmentEnum.Center);
            SetTdColumn(adjustReasonData.TENDER_CONST, hMergeCnt: 2);
            //SetTdColumn("專案管理：_______________，設計單位：_______________，\n監造單位：_______________，施工單位：_______________。（機關填寫）", hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn(string.Empty, vMerge: CellMerge.Previous);
            SetTdColumn("基地\n位置", alignment: AlignmentEnum.Center);
            SetTdColumn(rptData.PROJECT_LOCATION, hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn(string.Empty, vMerge: CellMerge.Previous);
            SetTdColumn("計畫\n內容\n及效益", alignment: AlignmentEnum.Center);
            SetTdColumn($"計畫內容：\n{rptData.ALL_JOB}\n計畫效益：\n{rptData.PROJECT_BENEFIT}", hMergeCnt: 5);
            Builder.EndRow();
        }

        /// <summary>
        /// 取得計畫金額
        /// </summary>
        /// <returns></returns>
        private string GetProjectAmt()
        {
            string amtStr = $"總預算經費：{adjustReasonData.BUDGET:N0}元";
            if (rptData.CP_KIND == "0")
            {
                amtStr += $"，發包金額：{adjustReasonData.PROCUREMENT_AMT:N0}元，決標金額：{adjustReasonData.TENDER_AWARDING_AMT:N0}元。";
            }
            return amtStr;
        }

        /// <summary>
        /// 取得中央補助款
        /// </summary>
        /// <returns></returns>
        private string GetBudgetCentral()
        {
            bool isBudgetCentral = adjustReasonData.IS_BUDGET_CENTRAL == 1;
            bool isEffectBudget = adjustReasonData.IS_EFFECT_BUDGET == 1;

            string budgetCentralQuestion = $"{(isBudgetCentral ? "1.": string.Empty)}有無獲得中央補助款：";
            string effectBudgetQuestion = isBudgetCentral ? "2.本次調整有無影響補助經費之請領：" : string.Empty;

            // 核定函描述
            string isBudgetCentralDesc = string.Empty;
            if (isBudgetCentral)
            {
                // 組字串上傳核定函
                string assessFiles = string.Join("\n", adjustReasonData.Files2.Select(x => x.FILE_NAME).ToList());
                if (!string.IsNullOrEmpty(assessFiles))
                {
                    isBudgetCentralDesc = $"\n中央同意展延期程文件：\n{assessFiles}";
                }
                else if (!string.IsNullOrEmpty(adjustReasonData.NO_OD_REASON))
                {
                    isBudgetCentralDesc = $"\n無中央同意展延期程文件原因：{adjustReasonData.NO_OD_REASON}";
                }
            }

            string result = string.Empty;
            // 有無獲得中央補助款
            result += $"{budgetCentralQuestion}{(isBudgetCentral ? "有" + (isBudgetCentral ? isBudgetCentralDesc : string.Empty) : "無")}";
            if (isBudgetCentral)
            {
                result += "\n";
                // 本次調整有無影響補助經費之請領
                result += $"{effectBudgetQuestion}{(isEffectBudget ? "有" : "無")}";
                // 影響經費請領說明
                result += !string.IsNullOrEmpty(adjustReasonData.EFFECT_BUDGET_MEMO) ? $"\n{adjustReasonData.EFFECT_BUDGET_MEMO}" : string.Empty;
            }
            
            return result;
        }

        /// <summary>
        /// 修正計畫檢核點進度
        /// </summary>
        private void CheckPointClassData()
        {
            Table = Builder.StartTable();

            if (rptData.RUNWAY_C == rptData.RUNWAY_C_ADJ)
            {
                SetTdColumn("二、修正計畫檢核點進度：", isBold: true, hMergeCnt: 3);
                Builder.EndRow();

                SetThColumn("檢核點名稱", isBold: true);
                SetThColumn("原預定完成日期", isBold: true);
                SetThColumn("修正後預定完成日期", isBold: true);
                Builder.EndRow();

                foreach (AdjustCusCheckPointModel adjustItem in adjustCheckPoint)
                {
                    ProjectCusCheckpointModel oriCheckPointItem = checkPoint.Where(x => x.CHECKITEM_NAME == adjustItem.CHECKITEM_NAME).FirstOrDefault();
                    
                    // 檢核點名稱
                    SetTdColumn(adjustItem.CHECKITEM_NAME);
                    // 原預定完成日期
                    string estimatedEndDate = oriCheckPointItem != null ? oriCheckPointItem.ESTIMATED_ENDDATE.ToTwDateString() : string.Empty;
                    SetTdColumn(estimatedEndDate, alignment: AlignmentEnum.Center);
                    
                    // 修正後預定完成日期
                    bool estimatedEndDateAdj = !string.IsNullOrEmpty(adjustItem.ESTIMATED_ENDDATE.ToTwDateString());
                    string adjustDate = estimatedEndDateAdj ? adjustItem.ESTIMATED_ENDDATE.ToTwDateString() : string.Empty;
                    // 若有ACTUAL_ENDDATE就顯示已完成
                    string actualEndDate = oriCheckPointItem != null && oriCheckPointItem.ACTUAL_ENDDATE != null ? "(已完成)" : string.Empty;
                    SetTdColumn(adjustDate + actualEndDate, alignment: AlignmentEnum.Center);

                    Builder.EndRow();
                }

                // 執行方式為工程類者，已填報實際"竣工"日期再出現「工程竣工報告表或函報竣工文件」
                // 執行方式為工程類者，已填報實際"開工"日期，且未填報實際"竣工"日期，再出現「核章版工程預定進度網圖」
                AdjustCusCheckPointModel checkPointFinish = adjustCheckPoint.Where(x => x.CTRL_POINT == "B").FirstOrDefault();
                AdjustCusCheckPointModel checkPointStart = adjustCheckPoint.Where(x => x.CTRL_POINT == "A").FirstOrDefault();
                DateTime? actualEndDateFinish = checkPointFinish != null ? checkPointFinish.ACTUAL_ENDDATE : null;
                DateTime? actualEndDateStart = checkPointStart != null ? checkPointStart.ACTUAL_ENDDATE : null;
                if (rptData.CP_KIND_ADJ == "0" && actualEndDateFinish != null)
                {
                    string fileNames = string.Empty;
                    foreach (ProjectAttachmentModel file in finishedEngineerFile)
                    {
                        if (file != finishedEngineerFile.First())
                        {
                            fileNames += "、";
                        }
                        fileNames += file.FILE_NAME;
                    }
                    SetTdColumn($"工程竣工報告表或函報竣工文件：{fileNames}", hMergeCnt: 3);
                    Builder.EndRow();
                }
                else if (rptData.CP_KIND_ADJ == "0" && actualEndDateStart != null)
                {
                    string fileNames = string.Empty;
                    foreach (ProjectAttachmentModel file in finishedEngineerFile)
                    {
                        if (file != finishedEngineerFile.First())
                        {
                            fileNames += "、";
                        }
                        fileNames += file.FILE_NAME;
                    }
                    SetTdColumn($"核章版工程預定進度網圖或預定竣工日展延核定文件：{fileNames}", hMergeCnt: 3);
                    Builder.EndRow();
                }

                Builder.EndTable();
                InitTable(Table, new List<double> { 50, 25, 25 });
            }
            else
            {
                SetTdColumn("二、修正計畫執行方式及檢核點進度：", isBold: true, hMergeCnt: 4);
                Builder.EndRow();

                SetThColumn("原檢核點", isBold: true, hMergeCount: 2);
                SetThColumn("修正後檢核點", isBold: true, hMergeCount: 2);
                Builder.EndRow();

                SetThColumn("檢核點名稱", isBold: true);
                SetThColumn("預定完成日期", isBold: true);
                SetThColumn("檢核點名稱", isBold: true);
                SetThColumn("預定完成日期", isBold: true);
                Builder.EndRow();

                // checkPoint vs adjustCheckPoint 選擇較大長度的來跑迴圈
                int checkPointCntMax = checkPoint.Count > adjustCheckPoint.Count ? checkPoint.Count : adjustCheckPoint.Count;
                for (int i = 0; i < checkPointCntMax; i++)
                {
                    // 原檢核點名稱
                    SetTdColumn(i < checkPoint.Count ? checkPoint[i].CHECKITEM_NAME : string.Empty);
                    // 原檢核點預定完成日期
                    string estimatedEndDate = i < checkPoint.Count ? checkPoint[i].ESTIMATED_ENDDATE.ToTwDateString() : string.Empty;
                    SetTdColumn(estimatedEndDate, alignment: AlignmentEnum.Center);
                    
                    // 修正後檢核點名稱
                    SetTdColumn(i < adjustCheckPoint.Count ? adjustCheckPoint[i].CHECKITEM_NAME : string.Empty);
                    // 修正後檢核點預定完成日期
                    string estimatedEndDateAdj = i < adjustCheckPoint.Count ? adjustCheckPoint[i].ESTIMATED_ENDDATE.ToTwDateString() : string.Empty;
                    SetTdColumn(estimatedEndDateAdj, alignment: AlignmentEnum.Center);
                    
                    Builder.EndRow();
                }

                Builder.EndTable();
                InitTable(Table, new List<double> { 30, 15, 30, 15 });
            }
        }

        /// <summary>
        /// 檢附說明事證資料
        /// </summary>
        private void AttachmentFile()
        {
            Table = Builder.StartTable();

            SetTdColumn($"三、檢附說明事證資料：", isBold: true);
            Builder.EndRow();

            string attachments = string.Empty;
            for (int i = 0; i < adjustReasonData.Files.Count; i++)
            {
                if (i != 0)
                {
                    attachments += "\n";
                }
                attachments += $"附件{i+1}：{adjustReasonData.Files[i].FILE_NAME}";
            }
            SetTdColumn(attachments, isBold: true);
            Builder.EndRow();

            Builder.EndTable();
        }

        /// <summary>
        /// 簽章：承辦人、業務主管、機管首長
        /// </summary>
        private void Signature()
        {
            Table = Builder.StartTable();

            SetTdColumn("承辦人：", isBold: true);
            SetTdColumn("業務主管：", isBold: true);
            SetTdColumn("機關首長：", isBold: true);
            Builder.EndRow();

            Builder.EndTable();
            InitTable(Table);
            Table.ClearBorders();
        }

    }
}
