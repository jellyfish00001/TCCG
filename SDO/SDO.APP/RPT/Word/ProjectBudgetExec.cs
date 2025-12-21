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
    /// 預覽列印 三、執行情形 （三）預算執行情形
    /// </summary>
    public class ProjectBudgetExec : WContentBuilder
    {
        private readonly IProjectExecuteService projectExecuteService;
        private readonly ISysParamDac sysParamDac;
        // 預算執行情形資料
        List<ProjectBudgetExecuteModel> budgetExecuteData;
        //原因參數資料
        private IList<SetParamModel> bgtExecFailed;
        //責任歸屬參數資料
        private IList<SetParamModel> bgtExecFailedDuty;

        public ProjectBudgetExec(IComponentContext coms) : base(coms)
        {
            this.projectExecuteService = coms.Resolve<IProjectExecuteService>();
            this.sysParamDac = coms.Resolve<ISysParamDac>();
        }

        protected override async Task GetData()
        {
            ProjectFillBudgetExecModel allData = await projectExecuteService.GetProjectFillBudgetExec(Parameter.PROJECT_NO);
            budgetExecuteData = allData.ProjectBudgetExecute;
            bgtExecFailed = await sysParamDac.GetSysParams("IPCBGTEXECFAILED");
            bgtExecFailedDuty = await sysParamDac.GetSysParams("IPCBGTEXECFAILEDDUTY");
        }

        protected override void Header()
        {
            Table = Builder.StartTable();

            SetThColumn("期間", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center, vMerge: CellMerge.First);
            SetThColumn("累計執行情形", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center, hMergeCount: 7);
            SetThColumn("本年度執行情形", backGroundColor: Color.FromArgb(255, 242, 204), alignment: ParagraphAlignment.Center, hMergeCount: 7);
            SetThColumn("填報日期", backGroundColor: Color.FromArgb(255, 242, 204), alignment: ParagraphAlignment.Center, vMerge: CellMerge.First);
            Builder.EndRow();

            SetThColumn("", alignment: ParagraphAlignment.Center, vMerge: CellMerge.Previous);
            SetThColumn("累計預定支用\n(元)\n(a)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("累計實際完成金額\n(元)\n(b+c)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("累計實際支用\n(元)\n(b)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("應付未付數\n(元)\n(c)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("結餘數\n(元)\n(d)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("累計實際支用數\n(元)\n(e)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("累計執行率\n(%)\n(e/a)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);

            SetThColumn("本年度可支用預算數\n(元)\n(j)", backGroundColor: Color.FromArgb(255, 242, 204), alignment: ParagraphAlignment.Center);
            SetThColumn("本年度預算分配數\n(元)\n(k)", backGroundColor: Color.FromArgb(255, 242, 204), alignment: ParagraphAlignment.Center);
            SetThColumn("本年度預算執行數\n(元)\n(l)", backGroundColor: Color.FromArgb(255, 242, 204), alignment: ParagraphAlignment.Center);
            SetThColumn("本年度執行率\n(%)\n(l/k)", backGroundColor: Color.FromArgb(255, 242, 204), alignment: ParagraphAlignment.Center);
            SetThColumn("預算執行率未達80%原因", backGroundColor: Color.FromArgb(255, 242, 204));
            SetThColumn("責任歸屬", backGroundColor: Color.FromArgb(255, 242, 204));
            SetThColumn("說明", backGroundColor: Color.FromArgb(255, 242, 204));
            SetThColumn("", alignment: ParagraphAlignment.Center, vMerge: CellMerge.Previous);
            Builder.EndRow();
        }

        protected override void Content()
        {
            if (!budgetExecuteData.Any())
            {
                SetTdColumn("無資料", alignment: AlignmentEnum.Center, hMergeCnt: 16);
                Builder.EndRow();
            }
            foreach (ProjectBudgetExecuteModel item in budgetExecuteData)
            {
                Builder.RowFormat.HeadingFormat = false;
                SetTdColumn($"{item.EXEC_YEAR}_{item.EXEC_MONTH}", alignment: AlignmentEnum.Center);
                SetTdColumn($"{item.GT_EXPANDED_BUDGET:N0}", alignment: AlignmentEnum.Right);
                SetTdColumn($"{(item.GT_ACT_BUDGET + item.GT_AP):N0}", alignment: AlignmentEnum.Right);
                SetTdColumn($"{item.GT_ACT_BUDGET:N0}", alignment: AlignmentEnum.Right);
                SetTdColumn($"{item.GT_AP:N0}", alignment: AlignmentEnum.Right);
                SetTdColumn($"{item.GT_BALANCE:N0}", alignment: AlignmentEnum.Right);
                SetTdColumn($"{item.GT_TOTAL:N0}", alignment: AlignmentEnum.Right);
                SetTdColumn($"{(double)item.GT_EXEC_RATE}", alignment: AlignmentEnum.Right);

                SetTdColumn($"{item.YEAR_BUDGET_EXPANDED:N0}", alignment: AlignmentEnum.Right);
                SetTdColumn($"{item.YEAR_BUDGET_ALLOCATED:N0}", alignment: AlignmentEnum.Right);
                SetTdColumn($"{item.YEAR_EXEC_BUDGET:N0}", alignment: AlignmentEnum.Right);
                SetTdColumn($"{((double)item.YEAR_EXEC_RATE)}", alignment: AlignmentEnum.Right);
                SetTdColumn(ShowFailed(item.FailedMappingData, bgtExecFailed));
                SetTdColumn(ShowFailed(item.FailedDutyMappingData, bgtExecFailedDuty));
                SetTdColumn(item.EXEC_RATE_FAILED_NOTE);
                SetTdColumn(item.CRT_DATE.ToTwDateString(), alignment: AlignmentEnum.Center);
                Builder.EndRow();
            }

            Builder.EndTable();
            InitTable(Table, new List<double> { 4.4, 6.6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 8, 8, 8, 5 });

            SetRepeatHeader(new List<int> { 0 });
        }

        /// <summary>
        /// 顯示原因/責任歸屬
        /// </summary>
        /// <param name="mappingData"></param>
        /// <param name="paramData"></param>
        /// <returns></returns>
        private string ShowFailed(List<ProjectMappingDataModel> mappingData, IList<SetParamModel> paramData)
        {
            List<string> setType = mappingData.Select(x => x.SET_TYPE).ToList();
            return string.Join("、", paramData.Where(x => setType.Contains(x.SET_TYPE)).Select(x => x.SET_VALUE));
        }
    }
}
