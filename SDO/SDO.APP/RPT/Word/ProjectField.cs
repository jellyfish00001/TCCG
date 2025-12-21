using Aspose.Words;
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
    /// 預覽列印 三、執行情形 （四）實地查證情形
    /// </summary>
    public class ProjectField : WContentBuilder
    {
        private readonly IProjectExecuteService projectExecuteService;
        // 實地查證情形資料
        private List<ProjectFactFindingModel> projectFactFindingData;

        public ProjectField(IComponentContext coms) : base(coms)
        {
            this.projectExecuteService = coms.Resolve<IProjectExecuteService>();
        }

        protected override async Task GetData()
        {
            projectFactFindingData = await projectExecuteService.GetProjectFactFinding(Parameter.PROJECT_NO);
        }

        protected override void Header()
        {
            Table = Builder.StartTable();

            SetThColumn("次數", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("分數", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("查證日期", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("回覆期限", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("管考說明", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("查證記錄", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("執行機關\n參採情形", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("參採資料", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("填報\n日期", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            Builder.EndRow();
        }

        protected override void Content()
        {
            int count = 1;
            if (!projectFactFindingData.Any())
            {
                SetTdColumn("無資料", alignment: AlignmentEnum.Center, hMergeCnt: 9);
                Builder.EndRow();
            }
            foreach (ProjectFactFindingModel item in projectFactFindingData)
            {
                Builder.RowFormat.HeadingFormat = false;
                SetTdColumn(count, alignment: AlignmentEnum.Center);
                SetTdColumn($"{(item.FFSCORE.HasValue ? (double)item.FFSCORE : "無")}", alignment: AlignmentEnum.Right);
                SetTdColumn(item.FFDATE.ToTwDateString(), alignment: AlignmentEnum.Center);
                SetTdColumn(item.COMPLETEREPLYDATE.ToTwDateString(), alignment: AlignmentEnum.Center);
                SetTdColumn(item.FFCOMMENT);
                SetTdColumn(item.RdecFile == null ? "" : string.Join(", ", item.RdecFile.Select(file => file.FILE_NAME)), fontColor: Color.Blue);
                SetTdColumn(item.FFREPORT);
                SetTdColumn(item.HandFile == null ? "" : string.Join(", ", item.HandFile.Select(file => file.FILE_NAME)), fontColor: Color.Blue); SetTdColumn(item.FFREPORT_DATE.ToTwDateString());
                Builder.EndRow();
                count++;
            }

            Builder.EndTable();
            InitTable(Table, new List<double> { 4, 7, 10, 10, 15, 15, 15, 15, 9 });

            SetRepeatHeader(new List<int> { 0 });
        }
    }
}
