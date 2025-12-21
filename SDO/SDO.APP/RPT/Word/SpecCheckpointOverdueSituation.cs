using Aspose.Words;
using Aspose.Words.Tables;
using Autofac;
using SDO.Base.RPT.Enums;
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

namespace SDO.APP.RPT.Word
{
    public class SpecCheckpointOverdueSituation : WContentBuilder
    {
        private IStatisticsDac statisticsDac;
        private IIPCSetParamService sysParamDac;
        private StatisticsModel statistics;
        private List<SpecChkPointOverdueSituationModel> data;
        private List<OrgProjectCntModel> orgProjectCntModels;
        private Dictionary<string, string> ctrlPointDic;

        private Color backGroundColor = Color.FromArgb(198, 217, 241);

        public SpecCheckpointOverdueSituation(IComponentContext coms) : base(coms)
        {
            statisticsDac = coms.Resolve<IStatisticsDac>();
            sysParamDac = coms.Resolve<IIPCSetParamService>();
        }

        protected override async Task GetData()
        {
            statistics = Parameter.ObjectModel is null ? new() : (StatisticsModel)(Parameter.ObjectModel);
            List<string> projStatuses = new List<string> { null, "1", "2", "3", "8" };
            data = (await statisticsDac.GetSpecCheckpointOverdueSituation(statistics)).Where(x => !projStatuses.Contains(x.PROJECT_STATUS)).ToList();
            // 取得機關列管計畫件數
            orgProjectCntModels = await statisticsDac.GetExecOrgProjectCnt((StatisticsModel)Parameter.ObjectModel);

            ctrlPointDic = (await sysParamDac.GetSysParams("CTRL_CHK_POINT_TYPE", false))
                .Where(x => statistics.CTRL_CHK_POINT_TYPE.Contains(x.SET_TYPE))
                .OrderBy(x => x.SORT_ORDER)
                .ToDictionary(x => x.SET_TYPE, y => y.SET_VALUE);
        }

        protected override void Title()
        {
            int newLineIndex = Parameter.FileName.IndexOf("重大建設計畫");
            string title = Parameter.FileName.Substring(0, newLineIndex) + "\n" + Parameter.FileName.Substring(newLineIndex);
            SetTitle(title, fontSize: 18, alignment: ParagraphAlignment.Center);

            SetSubTitle(
                $"統計日期：{DateTime.Now.ToTwDateString()}",
                fontSize: 12,
                alignment: ParagraphAlignment.Right
            );
        }

        protected override void Footer()
        {
            SetRemark("備註：逾期係統計上月份以前應達成之工作項目，且未填報實際完成日期者。", fontSize: 12);
        }

        protected override void Content()
        {
            int tableIndex = 1;

            // 設定資料統計表
            tableIndex = SetDataStat(tableIndex);

            // 設定工作項目
            foreach (KeyValuePair<string, string> ctrlPoint in ctrlPointDic)
            {
                tableIndex = SetCtrlPointData(tableIndex, ctrlPoint);
            }

            // 設定明細
            SetDetailData(tableIndex);
        }

        private Dictionary<string, string> GetExecOrganDic(IEnumerable<SpecChkPointOverdueSituationModel> specCheckpointExpiries)
        {
            return specCheckpointExpiries
               .OrderBy(x => x.OU_SORT_ORDER)
               .ThenBy(x=>x.PROJECT_NO)
               .Select(x => new { x.EXEC_ORGAN_C, x.EXEC_ORGAN_NAME })
               .Distinct()
               .ToDictionary(x => x.EXEC_ORGAN_C, y => y.EXEC_ORGAN_NAME);
        }

        #region 統計表
        /// <summary>
        /// 設定資料統計表
        /// </summary>
        /// <param name="tableIndex">表格索引</param>
        /// <returns></returns>
        private int SetDataStat(int tableIndex)
        {
            SetSubTitle($"表{tableIndex}  特定工作項目逾期未完成件數統計表");

            Table = Builder.StartTable();
            SetStatHeader();

            Dictionary<string, int> totalDic = GetTotalDic();
           
            foreach(OrgProjectCntModel model in orgProjectCntModels)
            {
                // 取得此機關資料 條件: 執行機關 = 此機關
                IEnumerable<SpecChkPointOverdueSituationModel> details = data.Where(x => x.EXEC_ORGAN_C == model.OrgId);

                SetTdColumn(model.OrgName, alignment: AlignmentEnum.Center);
                SetTdColumn(model.ProjectCnt, alignment: AlignmentEnum.Center);
                totalDic["totalCnt"] += model.ProjectCnt;

                foreach (KeyValuePair<string, string> ctrlPoint in ctrlPointDic)
                {
                    // 取得此工作項目數量 條件: 工作項目 = 此項目 & 預定完成日期 <= 統計年月(填報周期結束日) 且 無實際完成日期    
                    int cnt = details.Where(x => x.CTRL_POINT == ctrlPoint.Key && !x.ACTUAL_ENDDATE.HasValue && (x.ESTIMATED_ENDDATE <= statistics.YEAR_MONTH_END.Value)).Count();
                    SetTdColumn(cnt, alignment: AlignmentEnum.Center);

                    totalDic[$"{ctrlPoint.Key}"] += cnt;
                }

                Builder.EndRow();
            }

            // 合計
            SetTdColumn("合計", alignment: AlignmentEnum.Center);
            foreach (KeyValuePair<string, int> total in totalDic)
            {
                SetTdColumn(total.Value, alignment: AlignmentEnum.Center);
            }
            Builder.EndRow();

            Builder.EndTable();

            List<double> colWidths = new List<double>() { 14, 8 };
            int ctrlPointCol = (100 - colWidths.Sum(x => (int)x)) / ctrlPointDic.Count;
            for (int i = 0; i < ctrlPointDic.Count; i++)
            {
                colWidths.Add(ctrlPointCol);
            }
            InitTable(Table, colWidths);

            SetRepeatHeader(new List<int> { 0, 1, 2 });

            Builder.Writeln(string.Empty);
            return tableIndex + 1;
        }

        /// <summary>
        /// 設定統計表標題
        /// </summary>
        private void SetStatHeader()
        {
            SetThColumn("執行機關", backGroundColor: backGroundColor, vMerge: CellMerge.First);
            SetThColumn("列管\n件數", backGroundColor: backGroundColor, vMerge: CellMerge.First);
            SetThColumn("工作項目類型", backGroundColor: backGroundColor, hMergeCount: ctrlPointDic.Count);
            Builder.EndRow();

            SetThColumn(string.Empty, backGroundColor: backGroundColor, vMerge: CellMerge.Previous);
            SetThColumn(string.Empty, backGroundColor: backGroundColor, vMerge: CellMerge.Previous);
            foreach (KeyValuePair<string, string> ctrlPoint in ctrlPointDic)
            {
                SetThColumn($"{ctrlPoint.Value}\n逾期未完成件數", backGroundColor: backGroundColor);
            }
            Builder.EndRow();
        }

        /// <summary>
        /// 取得合計 DIC
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, int> GetTotalDic()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            result.Add("totalCnt", 0);
            foreach (KeyValuePair<string, string> ctrlPoint in ctrlPointDic)
            {
                result.Add($"{ctrlPoint.Key}", 0);
            }
            return result;
        }
        #endregion 統計表

        /// <summary>
        /// 設定工作項目資料
        /// </summary>
        /// <param name="tableIndex">表格索引</param>
        /// <param name="ctrlPoint">工作項目</param>
        /// <returns></returns>
        private int SetCtrlPointData(int tableIndex, KeyValuePair<string, string> ctrlPoint)
        {
            // 取得此工作項目清單 條件: 工作項目 = 此項目 & 預定完成日期 <= 統計年月(填報周期結束日) 且 無實際完成日期    
            IEnumerable<SpecChkPointOverdueSituationModel> ctrlPointData = data.Where(x => x.CTRL_POINT == ctrlPoint.Key
                && !x.ACTUAL_ENDDATE.HasValue && (x.ESTIMATED_ENDDATE <= statistics.YEAR_MONTH_END.Value));

            Dictionary<string, string> execOrganDic = GetExecOrganDic(ctrlPointData);
            if (!execOrganDic.Any())
            {
                return tableIndex;
            }

            SetSubTitle($"表{tableIndex}  「{ctrlPoint.Value}」逾期明細表");

            Table = Builder.StartTable();
            SetThColumn("執行機關", backGroundColor: backGroundColor);
            SetThColumn("件數", backGroundColor: backGroundColor);
            SetThColumn("計畫名稱", backGroundColor: backGroundColor);
            SetThColumn("逾期檢核點名稱", backGroundColor: backGroundColor);
            SetThColumn("預定完成日期", backGroundColor: backGroundColor);
            Builder.EndRow();

            foreach (KeyValuePair<string, string> execOrgan in execOrganDic)
            {
                // 取得此機關清單 條件: 執行機關 = 此機關
                IEnumerable<SpecChkPointOverdueSituationModel> details = ctrlPointData.Where(x => x.EXEC_ORGAN_C == execOrgan.Key);

                int detailCnt = details.Count();
                foreach (SpecChkPointOverdueSituationModel item in details)
                {
                    CellMerge vMerge = item.Equals(details.First()) ? CellMerge.First : CellMerge.Previous;
                    SetTdColumn(execOrgan.Value, alignment: AlignmentEnum.Center, vMerge: vMerge);
                    SetTdColumn(detailCnt, alignment: AlignmentEnum.Center, vMerge: vMerge);
                    SetTdColumn(item.PROJECT_NAME);
                    SetTdColumn(item.CHECKITEM_NAME);
                    SetTdColumn(item.ESTIMATED_ENDDATE.ToTwDateString(), alignment: AlignmentEnum.Center);
                    Builder.EndRow();
                }
            }

            Builder.EndTable();
            InitTable(Table, new List<double> { 15, 8, 37, 20, 20 });

            SetRepeatHeader(new List<int> { 0 });

            Builder.Writeln(string.Empty);
            return tableIndex + 1;
        }

        #region 明細資料
        /// <summary>
        /// 設定明細資料
        /// </summary>
        /// <param name="tableIndex">表格索引</param>
        /// <returns></returns>
        private void SetDetailData(int tableIndex)
        {
            SetSubTitle($"表{tableIndex}  計畫逾期工作項目類型明細表");

            Table = Builder.StartTable();
            SetDetailHeader();

            // 取得逾期未完成清單 條件: 預定完成日期 <= 統計年月(填報周期結束日) 且 無實際完成日期    
            IEnumerable<SpecChkPointOverdueSituationModel> SpecCheckpointExpirys = data.Where(x => !x.ACTUAL_ENDDATE.HasValue && (x.ESTIMATED_ENDDATE <= statistics.YEAR_MONTH_END.Value));

            foreach (KeyValuePair<string, string> execOrgan in GetExecOrganDic(SpecCheckpointExpirys))
            {
                // 取得此機關清單 條件: 執行機關 = 此機關
                IEnumerable<SpecChkPointOverdueSituationModel> items = data.Where(x => x.EXEC_ORGAN_C == execOrgan.Key);

                items = items.Where(x => !x.ACTUAL_ENDDATE.HasValue && (x.ESTIMATED_ENDDATE <= statistics.YEAR_MONTH_END.Value));

                List<string> projNos = items.Select(x => x.PROJECT_NO).Distinct().ToList();
                foreach (string projNo in projNos)
                {
                    // 取得此計畫清單 條件: 計畫編號 = 此編號
                    IEnumerable<SpecChkPointOverdueSituationModel> details = items.Where(x => x.PROJECT_NO == projNo);

                    SetTdColumn(execOrgan.Value, alignment: AlignmentEnum.Center, vMerge: projNo.Equals(projNos.First()) ? CellMerge.First : CellMerge.Previous);
                    SetTdColumn(details.First().PROJECT_NAME);

                    foreach (KeyValuePair<string, string> ctrlPoint in ctrlPointDic)
                    {
                        SpecChkPointOverdueSituationModel ctrlPointItem = details.FirstOrDefault(x => x.CTRL_POINT == ctrlPoint.Key);
                        string value = ctrlPointItem != null ? ctrlPointItem.ESTIMATED_ENDDATE.ToTwDateString() : string.Empty;
                        SetTdColumn(value, alignment: AlignmentEnum.Center);
                    }

                    Builder.EndRow();
                }
            }

            Builder.EndTable();

            int ctrlPointWidth = 16;
            List<double> colWidths = new List<double>() { 14 };
            colWidths.Add(100 - colWidths[0] - ctrlPointDic.Count * ctrlPointWidth);
            for (int i = 0; i < ctrlPointDic.Count; i++)
            {
                colWidths.Add(ctrlPointWidth);
            }
            InitTable(Table, colWidths);

            SetRepeatHeader(new List<int> { 0, 1 });

            Builder.Writeln(string.Empty);
        }

        /// <summary>
        /// 設定明細資料
        /// </summary>
        private void SetDetailHeader()
        {
            SetThColumn("執行機關", backGroundColor: backGroundColor, vMerge: CellMerge.First);
            SetThColumn("計畫名稱", backGroundColor: backGroundColor, vMerge: CellMerge.First);
            SetThColumn("預定完成日期", backGroundColor: backGroundColor, hMergeCount: ctrlPointDic.Count);
            Builder.EndRow();

            SetThColumn(string.Empty, backGroundColor: backGroundColor, vMerge: CellMerge.Previous);
            SetThColumn(string.Empty, backGroundColor: backGroundColor, vMerge: CellMerge.Previous);
            foreach (KeyValuePair<string, string> ctrlPoint in ctrlPointDic)
            {
                SetThColumn(ctrlPoint.Value, backGroundColor: backGroundColor);
            }
            Builder.EndRow();
        }
        #endregion 明細資料
    }
}
