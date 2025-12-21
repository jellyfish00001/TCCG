using Aspose.Cells;
using Autofac;
using SDO.Dac;
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

namespace SDO.APP.RPT.Excel
{
    /// <summary>
    /// 各年度提案資料清冊
    /// </summary>
    public class RPTPlanList : XlsBuilder
    {
        private IInnStatisticsDac statisticsDac;
        private InnStatisticsModel statistics;
        private List<PlanListModel> data;
        private List<InnProjectProposalTypeModel> ProposalTypeSub;

        public RPTPlanList(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "RPTPlanList.xlsx";
            statisticsDac = coms.Resolve<IInnStatisticsDac>();
        }

        protected override async Task GetData()
        {
            InnStatisticsModel model = (InnStatisticsModel)Parameter.ObjectModel;
            statistics = model;
            data = await statisticsDac.GetPlanList(model);
            ProposalTypeSub = await statisticsDac.GetPlanProposalTypeSub(model);
            foreach (var item in data)
            {
                var ProposalTypeSubList = ProposalTypeSub.Where(subItem => subItem.INN_PLAN_NO == item.INN_PLAN_NO).ToList();
                if (ProposalTypeSubList.Any())
                {
                    item.PROPOSALTYPE_SUB = string.Join(Environment.NewLine, ProposalTypeSubList.Select(subItem => subItem.PROPOSAL_TYPE_NAME));
                }
            }

        }

        protected override void MakeContent()
        {
            Cells cells = Sheet.Cells;
            Sheet.Name = "各年度提案清冊";

            CellReplaceByExcel(new
            {
                INN_YEAR = statistics.INN_YEAR
            });

            int row = 3;
            int index = 1;
            foreach (PlanListModel item in data)
            {
                if (index > 1)
                {
                    cells.InsertRow(row - 1);
                }

                SetColumn(cells[$"A{row}"], index);
                SetColumn(cells[$"B{row}"], item.INN_PLAN_NO);
                SetColumn(cells[$"C{row}"], item.SPONSOR_TYPE);
                SetColumn(cells[$"D{row}"], item.GROUP);
                SetColumn(cells[$"E{row}"], item.INN_PLAN_NAME);
                SetColumn(cells[$"F{row}"], item.PROPOSALTYPE_MAIN);
                SetColumn(cells[$"G{row}"], item.PROPOSALTYPE_SUB);
                SetColumn(cells[$"H{row}"], item.OU_NAME);
                SetColumn(cells[$"I{row}"], item.SPONSOR_NAME);
                SetColumn(cells[$"J{row}"], item.SPONSOR_SEX);
                SetColumn(cells[$"K{row}"], $"服務單位：{item.CONTACT_ORG}\n職稱：{item.CONTACT_TITLE}\n" +
                    $"聯絡人：{item.CONTACT_NAME}\n電話：{item.CONTACT_TEL}\n電子郵件：{item.CONTACT_EMAIL}");
                row++;
                index++;
            }

        }


    }
}
