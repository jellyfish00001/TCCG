using Aspose.Words;
using Aspose.Words.Tables;
using Autofac;
using SDO.Base.RPT.Enums;
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
    /// <summary>
    /// 預覽列印 四、管考備註 （三）年終考核意見
    /// </summary>
    public class ProjectAuditClose : WContentBuilder
    {
        private readonly IProjectExecuteService projectExecuteService;
        // 管考備註資料
        ProjectFillAuditModel projectAuditData;
        // 年終考核type資料
        List<ProjectCloseDetailsModel> type1Data;
        List<ProjectCloseDetailsModel> type2Data;
        List<ProjectBasicAdjForDelayApply> type3Data;
        List<ProjectCloseDetailsModel> type4Data;
        List<ProjectMergeLogModel> type5Data;
        int totalScore = 0;
        Dictionary<int, string> titleNumberList = new() { { 2, "二" }, { 3, "三" }, { 4, "四" } };
        //title number
        int number;

        public ProjectAuditClose(IComponentContext coms) : base(coms)
        {
            this.projectExecuteService = coms.Resolve<IProjectExecuteService>();
        }

        protected override async Task GetData()
        {
            projectAuditData = await projectExecuteService.GetProjectFillAudit(Parameter.PROJECT_NO);
            type1Data = projectAuditData.ProjectCloseDetails.Where(x => x.CLOSE_DETAILS_TYPE == "1").ToList();
            type2Data = projectAuditData.ProjectCloseDetails.Where(x => x.CLOSE_DETAILS_TYPE == "2").ToList();
            type3Data = projectAuditData.DelayApply;
            type4Data = projectAuditData.ProjectCloseDetails.Where(x => x.CLOSE_DETAILS_TYPE == "4").ToList();
            type5Data = projectAuditData.ProjectMergeLog.Where(x => x.MERGE_STATUS == "01").ToList();
            totalScore = (type2Data.Count > 0 ? 5 : 0) + (5 * type3Data.Count) + (3 * type4Data.Count) + (type5Data.Count > 0 ? 3 : 0);
            number = (int)Parameter.ObjectModel;
        }

        protected override void Title()
        {
            SetTitle($"（{titleNumberList[number]}）年終考核意見", fontSize: 14);
        }

        protected override void Content()
        {
            Table = Builder.StartTable();
            SetThColumn("結案或撤銷日期", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            string date = !projectAuditData.ProjectBasic.FINISH_DATE.HasValue ? "" :
                projectAuditData.ProjectBasic.FINISH_DATE.ToTwDateString() + projectAuditData.ProjectBasic.PROJECT_STATUS;
            SetTdColumn($"{date}", hMergeCnt: 2);
            Builder.EndRow();

            SetThColumn("基本資料", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn($"{projectAuditData.ProjectBasic.SCORE_A} 分", hMergeCnt: 2);
            Builder.EndRow();

            SetThColumn("報表品質", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(ShowType1(), hMergeCnt: 2);
            Builder.EndRow();

            SetThColumn("特殊扣分", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, vMerge: CellMerge.First);
            SetTdColumn("符合列管標準，卻未依第4點規定，主動提報智發會列管，考核分數再扣減5分。");
            SetTdColumn(ShowType(type2Data));
            Builder.EndRow();

            SetThColumn("", vMerge: CellMerge.Previous);
            SetTdColumn($"申請計畫調整，未依第9點規定於期限內提出，按次扣減考核分數5分。共{type3Data.Count}次，扣減{5 * type3Data.Count}分。");
            SetTdColumn(ShowType3());
            Builder.EndRow();

            SetThColumn("", vMerge: CellMerge.Previous);
            SetTdColumn($"經查證填報不實者，按次扣減該計畫年終考核分數3分。共{type4Data.Count}次，扣減{3 * type4Data.Count}分。");
            SetTdColumn(ShowType(type4Data));
            Builder.EndRow();

            SetThColumn("", vMerge: CellMerge.Previous);
            SetTdColumn("計畫於管考期間，申請分案列管，考核分數再扣減3分。");
            SetTdColumn(ShowType5());
            Builder.EndRow();

            SetThColumn("", vMerge: CellMerge.Previous);
            SetTdColumn($"合計扣減 {totalScore} 分", hMergeCnt: 2);
            Builder.EndRow();

            SetThColumn("年終考核備註", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(projectAuditData.ProjectBasic.NOTES_FOR_BUDGET, hMergeCnt: 2);
            Builder.EndRow();

            Builder.EndTable();
            InitTable(Table, new List<double> { 18, 41, 41 });

            // 管考才顯示、主辦不顯示
            ShowOtherMemo();
        }

        /// <summary>
        /// （四）其他管考備註
        /// </summary>
        private void ShowOtherMemo()
        {
            if (Parameter.IsRdecFun)
            {
                Builder.Writeln(string.Empty);
                SetTitle($"（{titleNumberList[number+1]}）其他管考備註", fontSize: 14);
                Table = Builder.StartTable();
                SetThColumn("其他管考備註", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                SetTdColumn(projectAuditData.ProjectBasic.NOTES_FOR_SCHEDULE);
                Builder.EndRow();
                Builder.EndTable();
                InitTable(Table, new List<double> { 18, 82 });
            }
        }

        /// <summary>
        /// 顯示報表品質
        /// </summary>
        /// <returns></returns>
        private string ShowType1()
        {
            string result = $"各項報表資料內容欠周詳，內容過於簡略，經智發會通知改善{type1Data.Count}次";
            foreach (ProjectCloseDetailsModel item in type1Data)
            {
                result += $"\n{item.REF_DATE.ToTwDateString()}：{item.REF_MEMO}";
            }
            return result;
        }

        /// <summary>
        /// 特殊扣分 第一項、第三項
        /// </summary>
        /// <returns></returns>
        private static string ShowType(List<ProjectCloseDetailsModel> data)
        {
            string result = "";
            foreach (ProjectCloseDetailsModel item in data)
            {
                result += $"{item.REF_DATE.ToTwDateString()}：{item.REF_MEMO}";
                if (!item.Equals(data.LastOrDefault()))
                {
                    result += "\n";
                }
            }
            return result;
        }

        /// <summary>
        /// 特殊扣分 第二項
        /// </summary>
        /// <returns></returns>
        private string ShowType3()
        {
            string result = type3Data.Any() ? "未於期限內提出之期程調整：" : "";
            foreach (ProjectBasicAdjForDelayApply item in type3Data)
            {
                result += $"\n{item.APPRV_DATE.ToTwDateString()} {item.SCHE_TYPE}";
            }
            return result;
        }

        /// <summary>
        /// 特殊扣分 第四項
        /// </summary>
        /// <returns></returns>
        private string ShowType5()
        {
            string result = "";
            foreach (ProjectMergeLogModel item in type5Data)
            {
                result += $"{item.PROMERGE_DATE.ToTwDateString()} 簽准分案";
                if (!item.Equals(type5Data.LastOrDefault()))
                {
                    result += "\n";
                }
            }
            return result;
        }
    }
}
