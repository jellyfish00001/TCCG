using Autofac;
using SDO.Models;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    public class RPTDelayStatistics : WordBuilder
    {
        public RPTDelayStatistics(IComponentContext coms) : base(coms)
        {
        }

        protected override async Task<bool> MakeContent()
        {
            InitBuilder(new DocConfigModel
            {
                IsShowPager = true,
            });

            object[] objData = new object[] { Parameter, Builder };
            
            await CreateService<RPTDelayStatistics>(objData, "DelayStatistics").MakeContenByTemplate();
            return true;
        }
    }
}
