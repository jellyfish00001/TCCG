using Aspose.Words;
using Autofac;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    /// <summary>
    /// 計畫調整內容
    /// </summary>
    public class RPTProjectAdjustDiff : WordBuilder
    {
        public RPTProjectAdjustDiff(IComponentContext coms) : base(coms)
        {
        }

        protected override async Task<bool> MakeContent()
        {
            InitBuilder(new DocConfigModel
            {
                IsShowPager = Parameter.Extension == "html" ? false : true,
                PagerNumberType = "2"
            });
            Builder.PageSetup.TopMargin = 36;
            Builder.PageSetup.BottomMargin = 36;
            Builder.PageSetup.LeftMargin = 36;
            Builder.PageSetup.RightMargin = 36;
            SetTitle(Parameter.FileName, fontSize: 14, bold: true, alignment: ParagraphAlignment.Center);
            SetSubTitle($"資料日期：{DateTime.Now.ToTwDateString()}", fontSize: 12, alignment: ParagraphAlignment.Right);
            object[] objData = new object[] { Parameter, Builder };
            
            // 調整基本資料
            if (Parameter.Type == "AW01")
            {
                await CreateService<RPTProjectPrint>(objData, "ProjectBasicAdjDiff").MakeContent();
            }
            // 調整期程
            else if(Parameter.Type == "AW02")
            {
                await CreateService<RPTProjectPrint>(objData, "ProjectCheckPointAdjDiff").MakeContent();
            }
            return true;
        }
    }
}
