using Aspose.Words;
using Autofac;
using SDO.ReportBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    public class RPTExportWord : WordBuilder
    {
        public RPTExportWord(IComponentContext coms) : base(coms)
        {
        }

        protected override async Task<bool> MakeContent()
        {
            InitBuilder();

            object[] objData = new object[] { Parameter, Builder };
            await CreateService<RPTExportWord>(objData, "ExportWord").MakeContent();
            //await CreateService<RPTExportWord>(objData, "ExportWordTemplate").MakeContenByTemplate();
            return true;
        }
    }
}
