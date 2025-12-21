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
    /// 預覽列印 三、執行情形 （二）落後原因分析
    /// </summary>
    public class ProjectDelay : WContentBuilder
    {
        private readonly IProjectExecuteService projectExecuteService;
        // 落後原因分析資料
        private List<ProjectDelayCausalModel> projectDelayData;

        public ProjectDelay(IComponentContext coms) : base(coms)
        {
            this.projectExecuteService = coms.Resolve<IProjectExecuteService>();
        }

        protected override async Task GetData()
        {
            projectDelayData = await projectExecuteService.GetProjectFillDelayList(Parameter.PROJECT_NO, "1");
        }

        protected override void Header()
        {
            Table = Builder.StartTable();

            List<string> headers = new List<string> { "年份", "月份", "落後類型", "落後類別", "落後項目", "責任歸屬", "落後原因", "解決對策", "須協調事項", "改進完成期限" };
            foreach (string header in headers)
            {
                SetThColumn(header, backGroundColor: Color.FromArgb(222, 234, 246));
            }
            Builder.EndRow();
        }

        protected override void Content()
        {
            if (!projectDelayData.Any())
            {
                SetTdColumn("無資料", alignment: AlignmentEnum.Center, hMergeCnt: 10);
                Builder.EndRow();
            }
            foreach (ProjectDelayCausalModel item in projectDelayData)
            {
                Builder.RowFormat.HeadingFormat = false;
                SetTdColumn(item.DATA_YEAR, alignment: AlignmentEnum.Center);
                SetTdColumn(item.DATA_MONTH, alignment: AlignmentEnum.Center);
                SetTdColumn(item.DELAY_KIND, alignment: AlignmentEnum.Center);
                SetTdColumn(item.DELAY_CLASS_C, alignment: AlignmentEnum.Center);
                SetTdColumn(item.DELAY_SUBCLASS_C, alignment: AlignmentEnum.Center);
                SetTdColumn(item.DELAY_RESPON);
                SetTdColumn(item.DELAY_CAUSAL);
                SetTdColumn(item.SOLUTION);
                SetTdColumn(item.COORDINATION);
                SetTdColumn(item.DEADLINES.ToTwDateString());
                Builder.EndRow();
            }

            Builder.EndTable();
            InitTable(Table, new List<double> { 5.7, 5.7, 5.7, 10, 14.9, 6.7, 16.3, 13.6, 11.8, 9.6 });

            SetRepeatHeader(new List<int> { 0 });
        }
    }
}
