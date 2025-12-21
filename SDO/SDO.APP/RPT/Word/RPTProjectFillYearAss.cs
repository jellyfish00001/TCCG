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
    public class RPTProjectFillYearAss : WordBuilder
    {
        public RPTProjectFillYearAss(IComponentContext coms) : base(coms)
        {
        }

        protected override async Task<bool> MakeContent()
        {
            InitBuilder(new DocConfigModel
            {
                IsShowPager = true,
            });
            object[] objData = new object[] { Parameter, Builder };
            return await CreateService<RPTProjectFillYearAss>(objData, "ProjectFillYearAss").MakeContenByTemplate();
        }
    }
}
