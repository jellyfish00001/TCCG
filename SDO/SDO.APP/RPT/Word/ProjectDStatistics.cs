using Autofac;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
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
    /// 表1 案件統計表
    /// </summary>
    public class ProjectDStatistics : WContentBuilder
    {
        private readonly IStatisticsDac statisticsDac;
        private StatisticsModel model;
        // 原始SQL資料
        protected List<ProjectStatisticsModel> statisticsData;
        // 報表結構資料
        protected List<ProjectStatisticsModel> execOrganData = new();
        // DAB案件落後統計
        protected List<DABCompositeModel> DABstatisticsData;
        // DAB案件落後統計
        protected List<DABProjectDataModel> DABProjectDelyData;

        public ProjectDStatistics(IComponentContext coms) : base(coms)
        {
            statisticsDac = coms.Resolve<IStatisticsDac>();
            TemplateFileName = "IPCProjectDStatisticsRPT.doc";
        }

        protected override async Task GetData()
        {
            model = (StatisticsModel)Parameter.ObjectModel;
            model.SORT_TYPE_1 = "D";
            model.SORT_GROUPBY_DEPT = false;
            model.PROJECT_YEAR_STATUS = "B";
            // 取DAB案件落後統計
            DABstatisticsData = await statisticsDac.GetDABCompositeData(model);
            // 取落案計畫明細資料
            DABProjectDelyData = await statisticsDac.GetDABProjectDataModel(model);

            // 參數替換
            SetDABStatisticsData();
        }

        /// <summary>
        /// DAB案件落後統計
        /// </summary>
        private void SetDABStatisticsData()
        {
            int sumA = (int)DABstatisticsData.Sum(x => x.TOTAL_NUM);
            int sumB = DABstatisticsData.Sum(x => x.CLOSE_NUM);
            int sumC = DABstatisticsData.Sum(x => x.CONFORM_NUM);
            int sumD = (int)DABstatisticsData.Sum(x => x.DELAY_NUM);
            int sumD1 = DABstatisticsData.Sum(x => x.DELAY_NUM_D1);
            int sumD2 = DABstatisticsData.Sum(x => x.DELAY_NUM_D2);
            int sumD3 = DABstatisticsData.Sum(x => x.DELAY_NUM_D3);
            int sumE = DABstatisticsData.Sum(x => x.CANCEL_NUM);
            double rateB = Math.Round((double)sumB / (double)sumA * 100, 1);
            double rateC = Math.Round((double)sumC / (double)sumA * 100, 1);
            double rateD = Math.Round((double)sumD / (double)sumA * 100, 1);
            double rateE = Math.Round((double)sumE / (double)sumA * 100, 1);

            BasicData = new
            {
                TITLE = Parameter.FileName,
                SUM_A = sumA,
                SUM_B = sumB,
                SUM_C = sumC,
                SUM_D = sumD,
                SUM_D1 = sumD1,
                SUM_D2 = sumD2,
                SUM_D3 = sumD3,
                SUM_E = sumE,
                SUM_BA = Math.Round((double)sumB / (double)sumA * 100, 1),
                RATE_A = Math.Round((double)sumA / (double)sumA * 100, 1),
                RATE_B = rateB,
                RATE_C = rateC,
                RATE_D = rateD,
                RATE_E = rateE
            };

            ListData = new List<ITableData>();
            var item = DABstatisticsData.Select(x => new
            {
                x.SET_VALUE,
                x.TOTAL_NUM,
                x.CLOSE_NUM,
                x.CONFORM_NUM,
                x.DELAY_NUM,
                x.F1,
                DELAY_RATE = Math.Round((double)x.DELAY_NUM / (x.DELAY_NUM + x.CONFORM_NUM) * 100, 1),
                x.F2,
                x.DELAY_NUM_D1,
                x.DELAY_NUM_D2,
                x.DELAY_NUM_D3,
                x.CANCEL_NUM,
                COMPLETE_RATE = Math.Round(x.COMPLETE_RATE, 1),
                x.DELAY_F1F2,
                x.DELAY_RANKING,
                // 依據不同排序條件決定哪個排序條件前3須加上網底
                BgColor = (x.DELAY_RANKING <= 3 ? Color.FromArgb(255, 204, 153) : Color.White)

            });
            if (item.Any())
            {
                ListData.Add(new WordTableData { LIST_DATA = item });
            }
        }
    }
}
