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
    /// 預覽列印 三、執行情形 （一）每月辦理情形
    /// </summary>
    public class ProjectFillExecute : WContentBuilder
    {
        private readonly IProjectExecuteService projectExecuteService;
        // 每月辦理情形資料(單筆)
        ProjectEngineeringProgressTableModel checkProjecExecuteData;
        // 每月辦理情形資料
        List<ProjectEngineeringProgressGridModel> projecExecuteData;
        // 是否顯示施工進度欄位
        bool isShow = false;

        public ProjectFillExecute(IComponentContext coms) : base(coms)
        {
            this.projectExecuteService = coms.Resolve<IProjectExecuteService>();
        }

        protected override async Task GetData()
        {
            checkProjecExecuteData = await projectExecuteService.GetProjecFillExecute(Parameter.PROJECT_NO, string.Empty);
            projecExecuteData = await projectExecuteService.GetProjecFillExecuteList(Parameter.PROJECT_NO, "1");
            // 確認施工方式為"工程類"，且辦理開工的實際完成日期已填寫
            isShow = checkProjecExecuteData == null ? false : checkProjecExecuteData.IsEngStartWork;
        }

        protected override void Header()
        {
            Table = Builder.StartTable();

            SetThColumn("期間", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center, vMerge: CellMerge.First);
            //需判斷是否呈現
            if (isShow)
            {
                SetThColumn("重大建設系統", backGroundColor: Color.FromArgb(255, 242, 204), alignment: ParagraphAlignment.Center, hMergeCount: 2);
                SetThColumn("標案管理系統", backGroundColor: Color.FromArgb(255, 242, 204), alignment: ParagraphAlignment.Center, hMergeCount: 2);
            }
            SetThColumn("執行情況", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center, vMerge: CellMerge.First);
            SetThColumn("須協辦事項", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center, vMerge: CellMerge.First);
            SetThColumn("填報日期\n(逾期天數)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center, vMerge: CellMerge.First);
            Builder.EndRow();
            SetThColumn("", alignment: ParagraphAlignment.Center, vMerge: CellMerge.Previous);
            //需判斷是否呈現
            if (isShow)
            {
                SetThColumn("累計預定施工進度", backGroundColor: Color.FromArgb(255, 242, 204), alignment: ParagraphAlignment.Center);
                SetThColumn("累計實際施工進度", backGroundColor: Color.FromArgb(255, 242, 204), alignment: ParagraphAlignment.Center);
                SetThColumn("累計預定施工進度", backGroundColor: Color.FromArgb(255, 242, 204), alignment: ParagraphAlignment.Center);
                SetThColumn("累計實際施工進度", backGroundColor: Color.FromArgb(255, 242, 204), alignment: ParagraphAlignment.Center);
            }
            SetThColumn("", alignment: ParagraphAlignment.Center, vMerge: CellMerge.Previous);
            SetThColumn("", alignment: ParagraphAlignment.Center, vMerge: CellMerge.Previous);
            SetThColumn("", alignment: ParagraphAlignment.Center, vMerge: CellMerge.Previous);
            Builder.EndRow();
        }

        protected override void Content()
        {
            foreach (ProjectEngineeringProgressGridModel item in projecExecuteData)
            {
                Builder.RowFormat.HeadingFormat = false;
                SetTdColumn($"{item.YEAR}_{item.MONTH}", alignment: AlignmentEnum.Center);
                if (isShow)
                {
                    SetTdColumn(item.IPC_RES_PRG.HasValue? item.IPC_RES_PRG.Value.ToString():string.Empty, alignment: AlignmentEnum.Right);
                    SetTdColumn(item.IPC_ACT_PRG.HasValue? item.IPC_ACT_PRG.Value.ToString():string.Empty, alignment: AlignmentEnum.Right);
                    SetTdColumn(item.TEN_RES_PRG.HasValue? item.TEN_RES_PRG.Value.ToString():string.Empty, alignment: AlignmentEnum.Right);
                    SetTdColumn(item.TEN_ACT_PRG.HasValue? item.TEN_ACT_PRG.Value.ToString():string.Empty, alignment: AlignmentEnum.Right);
                }
                SetTdColumn(item.EXECUTE_CONDITION);
                SetTdColumn(item.ASSISTANT_ITEM);
                SetTdColumn($"{ShowDelayDay(item)}", alignment: AlignmentEnum.Center);
                Builder.EndRow();
            }

            Builder.EndTable();
            List<double> pageWidth = new() { 15, 50, 20, 15 };
            if (isShow)
            {
                pageWidth = new() { 10, 10, 10, 10, 10, 24, 14, 12 };
            }
            InitTable(Table, pageWidth);

            SetRepeatHeader(new List<int> { 0,1 });
        }

        /// <summary>
        /// 顯示填報日期(逾期天數)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static string ShowDelayDay(ProjectEngineeringProgressGridModel item)
        {
            string result = "";
            result += item.SEND_DATE.HasValue ? $"{item.SEND_DATE.ToTwDateString()}\n" : "";
            result += $"({(item.DISREGARD ? "不計算" : item.OVERDUE_DAY == 0 ? "無" : item.OVERDUE_DAY)})";
            return result;
        }
    }
}
