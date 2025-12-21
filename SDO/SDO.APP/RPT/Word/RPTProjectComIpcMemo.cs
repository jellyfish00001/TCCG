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
    public class RPTProjectComIpcMemo : WordBuilder
    {
        public RPTProjectComIpcMemo(IComponentContext coms) : base(coms)
        {
        }

        protected override async Task<bool> MakeContent()
        {
            InitBuilder(new DocConfigModel
            {
                IsShowPager = true
            });
            
            Builder.PageSetup.TopMargin = 42;
            Builder.PageSetup.BottomMargin = 42;
            Builder.PageSetup.LeftMargin = 42;
            Builder.PageSetup.RightMargin = 42;

            object[] objData = new object[] { Parameter, Builder };
            
            await CreateService<RPTProjectComIpcMemo>(objData, "ProjectComIpcMemo").MakeContenByTemplate();
            return true;
        }
    }
}
