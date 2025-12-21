using Aspose.Words;
using Aspose.Words.Tables;
using Autofac;
using SDO.Base.RPT.Enums;
using SDO.Base.Utils.Models;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SDO.APP.RPT.Word
{
    /// <summary>
    /// 創新提案提案報表
    /// </summary>
    public class InnProjectFillBasic : WContentBuilder
    {
        private readonly IInnProjectService innProjectService;
        private readonly ISysParamDac sysParamDac;
        private readonly IDropDownDac dropDownDac;

        //計畫基本資料
        protected InnProjectBasicFillModel InnProjectBasicFill;
        //參數資料
        protected IList<SetParamModel> sysParamData;
        //機關資料
        protected List<DropDownListModel> organData;
        //主管機關/人員
        protected List<DropDownListModel> masterUserData;
        //執行機關/人員
        protected List<DropDownListModel> execUserData;
        //代辦機關/人員
        protected List<DropDownListModel> proposalTypeData;


        public InnProjectFillBasic(IComponentContext coms) : base(coms)
        {
            this.innProjectService = coms.Resolve<IInnProjectService>();
            this.sysParamDac = coms.Resolve<ISysParamDac>();
            this.dropDownDac = coms.Resolve<IDropDownDac>();

        }

        protected override async Task GetData()
        {
            InnProjectBasicFill = await innProjectService.GetInnBasic(Parameter.PROJECT_NO);
            sysParamData = await sysParamDac.GetSysParams(4);
            proposalTypeData = await dropDownDac.GetInnPropsalType(Parameter.YEAR);
            organData = await dropDownDac.GetOrganList();
        }

        protected override void Content()
        {
            Table = Builder.StartTable();
            SetThColumn("提案人數", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(sysParamData.Where(x => x.SET_ITEM == "SPONSORTYPE" && x.SET_TYPE == InnProjectBasicFill.InnProjectBasic.SPONSOR_TYPE)
                .Select(x => x.SET_VALUE).FirstOrDefault());
            Builder.EndRow();

            SetThColumn("參加組別", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(sysParamData.Where(x => x.SET_ITEM == "GROUP" && x.SET_TYPE == InnProjectBasicFill.InnProjectBasic.GROUP)
               .Select(x => x.SET_VALUE).FirstOrDefault());
            Builder.EndRow();

            SetThColumn("提案名稱", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(InnProjectBasicFill.InnProjectBasic.INN_PLAN_NAME);
            Builder.EndRow();

            SetThColumn("提案是否有不受理範圍之情形？", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(InnProjectBasicFill.InnProjectBasic.REJECT_YN == "1" ? "是" : "否");
            Builder.EndRow();

            SetThColumn("主要提案類別", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(proposalTypeData.Where(x => x.value == InnProjectBasicFill.InnProjectBasic.PROPOSAL_TYPE)
                .Select(x => x.text).FirstOrDefault());
            Builder.EndRow();

            SetThColumn("涉及其他提案類別", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(ShowSubProposalType());
            Builder.EndRow();

            SetThColumn("問題描述", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(InnProjectBasicFill.InnProjectBasic.INN_DESCRIPTION);
            Builder.EndRow();

            SetThColumn("提案構想", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(InnProjectBasicFill.InnProjectBasic.IDEA_CONTENT);
            Builder.EndRow();

            SetThColumn("預期效益", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(InnProjectBasicFill.InnProjectBasic.EXPECT_BENEFIT);
            Builder.EndRow();

            SetThColumn("提案人員", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(ShowSponsor(InnProjectBasicFill.InnProjectBasic.SPONSOR_ORG));
            Builder.EndRow();

            SetThColumn("主要提案人性別", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(sysParamData.Where(x => x.SET_ITEM == "SPONSOR_SEX" && x.SET_TYPE == InnProjectBasicFill.InnProjectBasic.SPONSOR_SEX)
              .Select(x => x.SET_VALUE).FirstOrDefault());
            Builder.EndRow();

            SetThColumn("是否為全國、本府或本市首創？", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(InnProjectBasicFill.InnProjectBasic.ORIGINATE_YN == "1" ||
                InnProjectBasicFill.InnProjectBasic.SPREAD_IDEA_YN == "1" ? "屬本府或本市首創。" : "非本府或本市首創。");
            Builder.EndRow();

            SetThColumn("主要提案人聯絡方式", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(ShowContact());

            Builder.EndRow();

            Builder.EndTable();
            InitTable(Table, new List<double> { 30, 70 }, Parameter.IsDiffCompare);
        }

        private string ShowSubProposalType()
        {
            string result = "";
            if (InnProjectBasicFill.InnProjectProposalType.Any())
            {
                foreach (InnProjectProposalTypeModel item in InnProjectBasicFill.InnProjectProposalType)
                {
                    result += proposalTypeData.Where(x => x.value == item.PROPOSAL_TYPE_ID.ToString())
                .Select(x => x.text).FirstOrDefault() + " ";
                }
            }
            return result;
        }

        private string ShowContact()
        {
            string result = "姓名：" + InnProjectBasicFill.InnProjectBasic.CONTACT_NAME +
                 "\n電話：" + InnProjectBasicFill.InnProjectBasic.CONTACT_TEL +
                 "\nE-mail：" + InnProjectBasicFill.InnProjectBasic.CONTACT_EMAIL;
            return result;
        }
        private string ShowSponsor(string orgid)
        {

            string result = "主要提案人：\n" + organData.Where(x => x.value == orgid).Select(x => x.text).FirstOrDefault() +
                "/" + InnProjectBasicFill.InnProjectBasic.SPONSOR_TITLE + "/" + InnProjectBasicFill.InnProjectBasic.SPONSOR_NAME;

            if (InnProjectBasicFill.InnPartner.Any())
            {
                result += "\n參與提案人：\n";
                foreach (InnPartnerModel item in InnProjectBasicFill.InnPartner)
                {
                    result += item.PARTNER_ORG + "/" + item.PARTNER_TITLE + "/" + item.PARTNER_NAME;
                }
            }
            return result;
        }


    }
}
