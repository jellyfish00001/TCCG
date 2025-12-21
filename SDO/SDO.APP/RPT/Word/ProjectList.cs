using Aspose.Pdf;
using Aspose.Words;
using Aspose.Words.Tables;
using Autofac;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.APP.IPC.Models.Statistics;
using SDO.Base.RPT.Enums;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Table = Aspose.Words.Tables.Table;

namespace SDO.APP.RPT.Word
{
    public class ProjectList : WContentBuilder
    {
        private IPWSRPTDac pwsRptDac;
        private PWSReportModel pwsRptModel;
        private List<RPTProjectReviewList> data;

        /// <summary>
        /// 取TempFile檔案
        /// </summary>
        /// <param name="coms"></param>
        public ProjectList(IComponentContext coms) : base(coms)
        {
            pwsRptDac = coms.Resolve<IPWSRPTDac>();
            TemplateFileName = "ProjectList.doc";
        }

        /// <summary>
        /// 取得表單資料
        /// </summary>
        /// <returns></returns>
        protected override async Task<Task> GetData()
        {
            PWSReportModel model = (PWSReportModel)Parameter.ObjectModel;
            pwsRptModel = model;
            data = await pwsRptDac.GetPlanGenderAnalyst(pwsRptModel);

            // 資料文字替換
            BasicData = new
            {
                YEAR = pwsRptModel.PWS_YEAR,
            };

            // 資料表格
            ListData = new List<ITableData>();
            List<object> listData = new();
            var index = 1;
            if (data.Count != 0)
            {
                foreach (RPTProjectReviewList item in data)
                {
                    listData.Add(new
                    {
                        index = index++,
                        OU_NAME = item.OU_NAME,
                        PLANNAME = item.PLANNAME,
                    });
                }
            }   

            ListData.Add(new WordTableData { LIST_DATA = listData });
            return Task.CompletedTask;
        }

    }
}
