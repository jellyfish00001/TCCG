using Aspose.Words;
using Aspose.Words.Tables;
using Autofac;
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
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    /// <summary>
    /// 預覽列印 一、基本資料
    /// </summary>
    public class ProjectFillBasic : WContentBuilder
    {
        private readonly IProjectService projectService;
        private readonly ISysParamDac sysParamDac;
        private readonly IIPCCodeDac ipcCodeDac;
        private readonly IDropDownDac dropDownDac;
        //計畫基本資料
        protected ProjectBasicFillModel projectFillBasicData;
        //參數資料
        protected IList<SetParamModel> sysParamData;
        //中央預算來源
        protected List<IPCCodePlanItemModel> planItemCData;
        //本府預算來源
        protected List<IPCCodePlanItemModel> planItemLData;
        //機關資料
        protected List<DropDownListModel> organData;
        //主管機關/人員
        protected List<DropDownListModel> masterUserData;
        //執行機關/人員
        protected List<DropDownListModel> execUserData;
        //代辦機關/人員
        protected List<DropDownListModel> budgetUserData;
        //地區資料
        protected List<DropDownListModel> townData;

        public ProjectFillBasic(IComponentContext coms) : base(coms)
        {
            this.projectService = coms.Resolve<IProjectService>();
            this.sysParamDac = coms.Resolve<ISysParamDac>();
            this.ipcCodeDac = coms.Resolve<IIPCCodeDac>();
            this.dropDownDac = coms.Resolve<IDropDownDac>();
        }

        protected override async Task GetData()
        {
            projectFillBasicData = Parameter.IsDiffCompare ?
                // 差異比對
                await projectService.GetProjectBasicFill(Parameter.PROJECT_NO, Parameter.DiffId)
                :
                // 一般預覽
                await projectService.GetProjectBasicFill(Parameter.PROJECT_NO);
            sysParamData = await sysParamDac.GetSysParams();
            planItemCData = await ipcCodeDac.GetCodePlanItem("2", false);
            planItemLData = await ipcCodeDac.GetCodePlanItem("1", false);
            organData = await dropDownDac.GetOrganList();
            townData = await dropDownDac.GetCodeTownByCityId("H");
            masterUserData = await dropDownDac.GetUserByOrg(projectFillBasicData.ProjectBasic.MASTER_ORGAN_C, 1, true);
            execUserData = await dropDownDac.GetUserByOrg(projectFillBasicData.ProjectBasic.EXEC_ORGAN_C, 2, true);
            budgetUserData = await dropDownDac.GetUserByOrg(projectFillBasicData.ProjectBasic.BUDGET_HOLD_ORGAN_C, 4, true);
        }

        protected override void Title()
        {
            SetTitle("一、基本資料", fontSize: 14);
        }

        protected override void Content()
        {
            Table = Builder.StartTable();
            SetThColumn("計畫年度", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn(projectFillBasicData.ProjectBasic.PROJECT_YEAR, hMergeCnt: 3);
            SetThColumn("列管編號", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(projectFillBasicData.ProjectBasic.PROJECT_NO);
            Builder.EndRow();

            SetThColumn("計畫名稱", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn(projectFillBasicData.ProjectBasic.PROJECT_NAME, hMergeCnt: 5);
            Builder.EndRow();

            if (!Parameter.IsDiffCompare)
            {
                SetThColumn("列管狀態", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
                string tubeStatus = projectFillBasicData.ProjectBasic.TUBE_STATUS_DESC;
                SetTdColumn(tubeStatus, hMergeCnt: 5);
                Builder.EndRow();
            }

            SetThColumn("計畫總經費(元)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn($"{projectFillBasicData.ProjectBudgetSourceG.Sum(x => x.BUDGET_CENTRAL + x.BUDGET_LOCAL):N0}", hMergeCnt: 5);
            Builder.EndRow();

            #region 經費來源
            SetThColumn("經費來源", backGroundColor: Color.FromArgb(222, 234, 246), hMergeCount: 7);
            Builder.EndRow();

            SetThColumn("年度", backGroundColor: Color.FromArgb(222, 234, 246));
            SetThColumn("預算類型", backGroundColor: Color.FromArgb(222, 234, 246));
            SetThColumn("中央\n預算來源", backGroundColor: Color.FromArgb(222, 234, 246));
            SetThColumn("中央\n補助款(元)", backGroundColor: Color.FromArgb(222, 234, 246));
            SetThColumn("本府\n預算來源", backGroundColor: Color.FromArgb(222, 234, 246));
            SetThColumn("本府\n預算金額(元)", backGroundColor: Color.FromArgb(222, 234, 246));
            SetThColumn("核定函", backGroundColor: Color.FromArgb(222, 234, 246));
            Builder.EndRow();
            if (!projectFillBasicData.ProjectBudgetSourceG.Any())
            {
                SetEmptyRow(7);
                Builder.EndRow();
            }
            foreach (ProjectBudgetSourceGModel item in projectFillBasicData.ProjectBudgetSourceG)
            {
                SetTdColumn(item.PLAN_YEAR, alignment: AlignmentEnum.Center);
                SetTdColumn(sysParamData.Where(x => x.SET_ITEM == "BUDGETCLASS" && x.SET_TYPE == item.BUDGET_CLASS).Select(x => x.SET_VALUE).FirstOrDefault(), alignment: AlignmentEnum.Center);
                SetTdColumn(planItemCData.Where(x => x.PLAN_ITEM_ID == item.PLAN_ITEM_C).Select(x => x.PLAN_ITEM_NAME).FirstOrDefault(), alignment: AlignmentEnum.Center);
                SetTdColumn($"{item.BUDGET_CENTRAL:N0}", alignment: AlignmentEnum.Right);
                SetTdColumn(planItemLData.Where(x => x.PLAN_ITEM_ID == item.PLAN_ITEM_L).Select(x => x.PLAN_ITEM_NAME).FirstOrDefault(), alignment: AlignmentEnum.Center);
                SetTdColumn($"{item.BUDGET_LOCAL:N0}", alignment: AlignmentEnum.Right);
                SetTdColumn(item.FILE != null && item.FILE.Count > 0 ? string.Join(", ", item.FILE.Select(file => file.FILE_NAME)) : "");
                Builder.EndRow();
            }
            #endregion

            SetThColumn("建設類別", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn(ShowBiuldKind(), hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn("相關審查", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn(ShowReview(), hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn("特殊加註", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn(projectFillBasicData.ProjectBasic.SPEC_NOTE, hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn("主管機關/人員", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn(ShowOrganPerson(projectFillBasicData.ProjectBasic.MASTER_ORGAN_C, masterUserData, projectFillBasicData.ProjectBasic.MASTER_UNDERTAKER_C), hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn("執行機關/人員", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn(ShowOrganPerson(projectFillBasicData.ProjectBasic.EXEC_ORGAN_C, execUserData, projectFillBasicData.ProjectBasic.EXEC_UNDERTAKER_C), hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn("協辦機關/人員", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn(ShowAssistant(), hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn("代辦機關/人員", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn(ShowOrganPerson(projectFillBasicData.ProjectBasic.BUDGET_HOLD_ORGAN_C, budgetUserData, projectFillBasicData.ProjectBasic.BUDGET_HOLD_UNDERTAKER_C), hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn("辦理地點", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn(ShowTown(projectFillBasicData.ProjectBasic.TOWN_C), hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn("位置說明", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn(projectFillBasicData.ProjectBasic.PROJECT_LOCATION, hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn("單點地圖定位", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            if (Parameter.Extension == "html")
            {
                SetTdColumn("", hMergeCnt: 5);
            }
            else
            {
                ShowMap(projectFillBasicData.ProjectBasic);
            }
            Builder.EndRow();

            SetThColumn("多點地圖定位", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn("檢視地點", hMergeCnt: 5, fontColor: Color.Blue);
            Builder.EndRow();

            SetThColumn("計畫內容", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn(projectFillBasicData.ProjectBasic.ALL_JOB, hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn("計畫效益", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn(projectFillBasicData.ProjectBasic.PROJECT_BENEFIT, hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn("備註", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn(projectFillBasicData.ProjectBasic.MEMO, hMergeCnt: 5);
            Builder.EndRow();

            SetThColumn("立案時間", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
            SetTdColumn(projectFillBasicData.ProjectBasic.CREATEDTIME.ToTwDateString("yyy/MM/dd HH:mm"), hMergeCnt: 5);
            Builder.EndRow();

            if (!Parameter.IsDiffCompare)
            {
                SetThColumn("立案審核意見", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
                SetTdColumn(GetProjLog(projectFillBasicData.ProjectBasic, new List<string> { "3", "4" }), hMergeCnt: 5);
                Builder.EndRow();

                SetThColumn("基本資料成績", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
                SetTdColumn(projectFillBasicData.ProjectBasic.SCORE_A, hMergeCnt: 5);
                Builder.EndRow();

                SetThColumn("結案審核意見", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left, hMergeCount: 2);
                SetTdColumn(GetProjLog(projectFillBasicData.ProjectBasic, new List<string> { "6", "7", "8" }), hMergeCnt: 5);
                Builder.EndRow();
            }

            Builder.EndTable();
            InitTable(Table, new List<double> { 7, 12.5, 15, 14, 15, 17.5, 19 }, Parameter.IsDiffCompare);
        }

        /// <summary>
        /// 顯示建設類別
        /// </summary>
        /// <returns></returns>
        private string ShowBiuldKind()
        {
            IList<SetParamModel> buildKindType = sysParamData.Where(x => x.SET_ITEM == "BUILD_KIND_TYPE").ToList();
            string result = "";
            foreach (SetParamModel item in buildKindType)
            {
                List<string> buildKind = projectFillBasicData.ProjectBuildKind.Where(x => x.BUILD_KIND_TYPE == item.SET_TYPE).Select(x => x.BUILD_KIND).ToList();
                result += $"{item.SET_VALUE}：{string.Join("、", sysParamData.Where(x => x.SET_ITEM == "COM_PLANKIND" && buildKind.Contains(x.SET_TYPE)).Select(x => x.SET_VALUE))}";
                if (!item.Equals(buildKindType.LastOrDefault()))
                {
                    result += "\n";
                }
            }
            return result;
        }

        /// <summary>
        /// 顯示相關審查
        /// </summary>
        /// <returns></returns>
        private string ShowReview()
        {
            if (projectFillBasicData.ProjectBasic.REVIEWITEM == null)
            {
                return "";
            }
            string[] review = projectFillBasicData.ProjectBasic.REVIEWITEM.Split(",");
            return string.Join(",", sysParamData.Where(x => x.SET_ITEM == "COM_REVIEWITEM" && review.Contains(x.SET_TYPE)).Select(x => x.SET_VALUE));
        }

        /// <summary>
        /// 顯示機關/人員
        /// </summary>
        /// <param name="organ"></param>
        /// <returns></returns>
        private string ShowOrganPerson(string organ, List<DropDownListModel> userListData, string user)
        {
            string result = "無";
            if (!string.IsNullOrEmpty(organ))
            {
                result = $"{organData.Where(x => x.value == organ).Select(x => x.text).FirstOrDefault()}/{userListData.Where(x => x.value == user).Select(x => x.text).FirstOrDefault()}";
            }
            return result;
        }

        /// <summary>
        /// 顯示協辦機關/人員
        /// </summary>
        /// <returns></returns>
        private string ShowAssistant()
        {
            string result = "";
            if (projectFillBasicData.ProjectAsstOrg.Any())
            {
                foreach (ProjectAsstOrgModel item in projectFillBasicData.ProjectAsstOrg)
                {
                    result += $"{organData.Where(x => x.value == item.ASSISTANT_ORGAN_C).Select(x => x.text).FirstOrDefault()}/{item.ASSISTANT_UNDERTAKER_C_NAME}";
                    if (!item.Equals(projectFillBasicData.ProjectAsstOrg.LastOrDefault()))
                    {
                        result += "\n";
                    }
                }
            }
            else
            {
                result = "無";
            }
            return result;
        }

        /// <summary>
        /// 顯示辦理地點
        /// </summary>
        /// <param name="town"></param>
        /// <returns></returns>
        private string ShowTown(string town)
        {
            return townData.Where(x => x.value == town).Select(x => x.text).FirstOrDefault();
        }

        /// <summary>
        /// 顯示地圖
        /// </summary>
        /// <param name="model"></param>
        private void ShowMap(ProjectBasicModel model)
        {
            #region 預設位置
            // //預設地址為 桃園區縣府路1號 - 桃園市政府
            if (string.IsNullOrEmpty(model.X_COORD)){
                model.X_COORD = "24.993098524588383";
            }
            if (string.IsNullOrEmpty(model.Y_COORD))
            {
                model.Y_COORD = "121.30101509392262";
            }
            #endregion

            // Google static map
            string urlFormat = "https://maps.googleapis.com/maps/api/staticmap?center={0},{1}&zoom=12&size=466x233&markers=color:red%7Csize:mid%7C{0},{1}&key={2}";
            string url = String.Format(urlFormat, model.X_COORD, model.Y_COORD, "AIzaSyA3i_vmmHzSA0NRT777jEkDEErysjfoyrQ");

            Cell cell;
            cell = Builder.InsertCell();
            cell.CellFormat.HorizontalMerge = CellMerge.First;
            Builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            // 插入圖片
            Builder.InsertImage(url);

            for (int i = 0; i < 4; i++)
            {
                cell = Builder.InsertCell();
                cell.CellFormat.HorizontalMerge = CellMerge.Previous;
            }
        }

        private string GetProjLog(ProjectBasicModel projectBasic, List<string> logStatuses)
        {
            if (projectBasic.ProjLogs == null || !projectBasic.ProjLogs.Any())
            {
                return string.Empty;
            }

            List<string> result = projectBasic.ProjLogs
                .Where(x => logStatuses.Contains(x.LOG_STATUS_C))
                .Select((x, index) => $"{index + 1}.{x.LOG_DATE.ToTwDateString()} {x.LOG_STATUS}{(!string.IsNullOrEmpty(x.MEMO) ? ":" : string.Empty)}{x.MEMO}。")
                .ToList();

            return string.Join("\n", result);
        }
    }
}
