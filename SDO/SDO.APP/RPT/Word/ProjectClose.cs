using Aspose.Words;
using Aspose.Words.Tables;
using Autofac;
using SDO.Base.RPT.Enums;
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
    /// 預覽列印 三、執行情形 （六）結案資料
    /// </summary>
    public class ProjectClose : WContentBuilder
    {
        private readonly IProjectClosedService projectClosedService;
        // 結案資料
        ProjectFillCloseModel projectCloseData;

        public ProjectClose(IComponentContext coms) : base(coms)
        {
            this.projectClosedService = coms.Resolve<IProjectClosedService>();
        }

        protected override async Task GetData()
        {
            projectCloseData = await projectClosedService.GetProjectFillClose(Parameter.PROJECT_NO);
        }

        protected override void Content()
        {
            Table = Builder.StartTable();
            Builder.Font.Size = 11;
            SetThColumn("經費支用情形", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center, vMerge: CellMerge.First);
            SetThColumn("月份", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("計畫總經費(元)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("累計實際完成金額(元)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("累計實際支用(元)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("應付未付數(元)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("節餘數(元)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("填報日期", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            Builder.EndRow();

            SetThColumn("", vMerge: CellMerge.Previous);
            SetTdColumn(projectCloseData.DATA_DATE_FORMAT, alignment: AlignmentEnum.Center);
            SetTdColumn($"{projectCloseData.TOTAL_BUDGET:N0}", alignment: AlignmentEnum.Right);
            SetTdColumn($"{projectCloseData.TOTAL_ACTUAL_COMP:N0}", alignment: AlignmentEnum.Right);
            SetTdColumn($"{projectCloseData.ACTUAL_PAY:N0}", alignment: AlignmentEnum.Right);
            SetTdColumn($"{projectCloseData.UNPAY:N0}", alignment: AlignmentEnum.Right);
            SetTdColumn($"{projectCloseData.BALANCE:N0}", alignment: AlignmentEnum.Right);
            SetTdColumn(projectCloseData.MDF_DATE.ToTwDateString(), alignment: AlignmentEnum.Center);
            Builder.EndRow();

            SetThColumn("佐證資料", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetTdColumn(projectCloseData.ProjAttachments.Any() ? string.Join("\n", projectCloseData.ProjAttachments.Select(x => x.FILE_NAME)) : "", hMergeCnt: 7, fontColor: Color.Blue);
            Builder.EndRow();

            Builder.EndTable();
            InitTable(Table, new List<double> { 15.5, 9, 12.5, 13, 13, 12.5, 12.5, 12 });
        }
    }
}
