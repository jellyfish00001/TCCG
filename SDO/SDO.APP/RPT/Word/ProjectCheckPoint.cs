using Aspose.Words;
using Autofac;
using SDO.APP.IPC.Models.ProjectAdjust;
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
    /// <summary>
    /// 預覽列印 二、檢核點設定 (基本資料)
    /// </summary>
    public class ProjectCheckPoint : WContentBuilder
    {
        private readonly IProjectService projectService;
        private readonly IIPCSetParamDac ipcSetparamDac;
        private readonly IIPCCodeDac ipcCodeDac;
        // 檢核點資料
        protected ProjectCheckpointModel projectCheckpointData;
        //執行方式資料
        protected IList<IPCSetParamModel> cpKindData;
        protected List<IPCCodeCheckpointModel> codeCheckpointData;

        public ProjectCheckPoint(IComponentContext coms) : base(coms)
        {
            this.projectService = coms.Resolve<IProjectService>();
            this.ipcSetparamDac = coms.Resolve<IIPCSetParamDac>();
            this.ipcCodeDac = coms.Resolve<IIPCCodeDac>();
        }

        protected override async Task GetData()
        {
            projectCheckpointData = Parameter.IsDiffCompare ?
                // 差異比對
                await projectService.GetProjectCheckpoint(Parameter.PROJECT_NO, Parameter.DiffId)
                :
                // 一般預覽
                await projectService.GetProjectCheckpoint(Parameter.PROJECT_NO);
            cpKindData = await ipcSetparamDac.GetSysParams("CP_KIND", false);
            codeCheckpointData = await ipcCodeDac.GetCodeCheckpoint(projectCheckpointData.CP_KIND, false);
        }

        protected override void Title()
        {
            SetTitle("二、檢核點設定", fontSize: 14);
        }

        protected override void Content()
        {
            Table = Builder.StartTable();
            SetThColumn("執行方式", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(ShowCpKind(), hMergeCnt: 3);
            Builder.EndRow();

            SetThColumn("計畫開始日期", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(projectCheckpointData.CONTROL_DATE1.ToTwDateString(), hMergeCnt: 3);
            Builder.EndRow();

            SetThColumn("檢核點", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center, hMergeCount: 2);
            SetThColumn("管考進度", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("預定完成日期", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            Builder.EndRow();

            if (!projectCheckpointData.CusCheckpointModels.Any())
            {
                SetTdColumn("", hMergeCnt: 2);
                SetEmptyRow(2);
                Builder.EndRow();
            }
            foreach (ProjectCusCheckpointModel item in projectCheckpointData.CusCheckpointModels)
            {
                SetTdColumn(item.CHECKITEM_NAME, hMergeCnt: 2);
                SetTdColumn($"{(double)item.PROGRESS}%", alignment: AlignmentEnum.Center);
                SetTdColumn(item.ESTIMATED_ENDDATE.ToTwDateString(), alignment: AlignmentEnum.Center);
                Builder.EndRow();
            }

            SetThColumn("預定完成期限", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(ShowEstimatedEndDate(projectCheckpointData.CusCheckpointModels), hMergeCnt: 3);
            Builder.EndRow();

            (string, string) adjustHistory = ShowAdjustHistory();
            SetThColumn("總期程調整歷程", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(adjustHistory.Item1, hMergeCnt: 3);
            Builder.EndRow();

            SetThColumn("分月期程調整歷程", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(adjustHistory.Item2, hMergeCnt: 3);
            Builder.EndRow();

            SetThColumn("備註", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(projectCheckpointData.MEMO_CHK_POINT, hMergeCnt: 3);
            Builder.EndRow();

            Builder.EndTable();
            InitTable(Table, new List<double> { 25, 15, 30, 30 }, Parameter.IsDiffCompare);
        }

        /// <summary>
        /// 顯示執行方式
        /// </summary>
        /// <returns></returns>
        private string ShowCpKind()
        {
            string ckKind = cpKindData.Where(x => x.SET_TYPE == projectCheckpointData.CP_KIND).Select(x => x.SET_VALUE).FirstOrDefault();
            string runWayC = codeCheckpointData.Where(x => x.CHECKPOINT_CLASS_ID.ToString() == projectCheckpointData.RUNWAY_C).Select(x => x.CHECKPOINT_CLASS).FirstOrDefault();
            return $"{ckKind}/{runWayC}";
        }

        /// <summary>
        /// 顯示預定完成期限
        /// </summary>
        private static string ShowEstimatedEndDate(List<ProjectCusCheckpointModel> data)
        {
            string endDate = "無";
            if (data.Any())
            {
                endDate = data.Select(x => x.ESTIMATED_ENDDATE).LastOrDefault().ToTwDateString();
            }
            return endDate;
        }

        /// <summary>
        /// 顯示總期程/分月期程調整歷程
        /// </summary>
        /// <returns>(總期程字串, 分月期程字串)</returns>
        private (string, string) ShowAdjustHistory()
        {
            (string, string) historyStr = ("", "");
            foreach (AdjustScheHistoryModel item in projectCheckpointData.AdjustScheHistoryModels)
            {
                if (item.SCHE_TYPE == "Y")
                {
                    if (historyStr.Item1 != "")
                    {
                        historyStr.Item1 += "\r\n";
                    }
                    historyStr.Item1 += $"第{item.SEQ}次：" +
                        $"原定{(item.ORI_ESTIMATED_ENDDATE != null ? item.ORI_ESTIMATED_ENDDATE.ToTwDateString() : string.Empty)}完成，" +
                        $"調整至{(item.ADJ_LAST_DATE != null ? item.ADJ_LAST_DATE.ToTwDateString() : string.Empty)}" +
                        $"(核准日期{item.APPRV_DATE.ToTwDateString()})。" +
                        $"調整原因：{item.REASON}";
                }

                else if (item.SCHE_TYPE == "M")
                {
                    if (historyStr.Item2 != "")
                    {
                        historyStr.Item2 += "\r\n";
                    }
                    historyStr.Item2 += $"第{item.SEQ}次：" +
                        (item.ADJ_LAST_DATE != null ? item.ADJ_LAST_DATE.ToTwDateString() : string.Empty) +
                        $"(核准日期：{item.APPRV_DATE.ToTwDateString()})。" +
                        $"調整原因：{item.REASON}";
                }
            }
            return historyStr;
        }
    }
}
