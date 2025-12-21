using Autofac;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    /// <summary>
    /// 表1 每月案件統計表
    /// </summary>
    public class RPTProjectStatistics : WordBuilder
    {
        public RPTProjectStatistics(IComponentContext coms) : base(coms)
        {

        }

        protected override async Task<bool> MakeContent()
        {
            InitBuilder(new DocConfigModel
            {
                IsShowPager = true,
            });

            object[] objData = new object[] { Parameter, Builder };
            switch (Parameter.Type)
            {
                case "Statistics": // 案件統計表
                    await CreateService<RPTProjectStatistics>(objData, "ProjectStatistics").MakeContenByTemplate();
                    break;
                case "Status": // 案件狀態表
                case "Delay": // 落後案件表
                    Builder.PageSetup.TopMargin = 57.14;
                    Builder.PageSetup.BottomMargin = 57.14;
                    Builder.PageSetup.LeftMargin = 57.14;
                    Builder.PageSetup.RightMargin = 57.14;
                    await CreateService<RPTProjectStatistics>(objData, "ProjectStatus").MakeContent();
                    break;
            }
            return true;
        }
    }
}
