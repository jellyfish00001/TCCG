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
    /// 預覽列印 二、檢核點 (執行情形)
    /// </summary>
    public class ProjectCkptCom : WContentBuilder
    {
        private readonly IProjectExecuteService projectExecuteService;
        private readonly IProjectService projectService;
        private readonly IIPCSetParamDac ipcSetparamDac;
        private readonly IIPCCodeDac ipcCodeDac;
        // 檢核點資料(執行情形)
        ProjectFillCkptComModel projectCkptComData;
        // 檢核點資料(基本資料)
        ProjectCheckpointModel projectCheckpointData;
        //執行方式資料
        IList<IPCSetParamModel> cpKindData;
        List<IPCCodeCheckpointModel> codeCheckpointData;

        public ProjectCkptCom(IComponentContext coms) : base(coms)
        {
            this.projectExecuteService = coms.Resolve<IProjectExecuteService>();
            this.projectService = coms.Resolve<IProjectService>();
            this.ipcSetparamDac = coms.Resolve<IIPCSetParamDac>();
            this.ipcCodeDac = coms.Resolve<IIPCCodeDac>();
        }

        protected override async Task GetData()
        {
            projectCkptComData = await projectExecuteService.GetProjectFillCkptCom(Parameter.PROJECT_NO);
            projectCheckpointData = await projectService.GetProjectCheckpoint(Parameter.PROJECT_NO);
            cpKindData = await ipcSetparamDac.GetSysParams("CP_KIND", false);
            codeCheckpointData = await ipcCodeDac.GetCodeCheckpoint(projectCheckpointData.CP_KIND, false);
        }

        protected override void Title()
        {
            SetTitle("二、檢核點", fontSize: 14);
        }

        protected override void Content()
        {
            ContactData();
            Builder.Writeln(string.Empty);
            CkptComData();
        }

        /// <summary>
        /// 聯繫資訊
        /// </summary>
        private void ContactData()
        {
            SetTitle("（一）聯繫資訊", fontSize: 14);
            Table = Builder.StartTable();
            SetThColumn("計畫實際承辦人", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(projectCkptComData.REAL_CONTACT);
            Builder.EndRow();

            SetThColumn("計畫承辦人電話", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(projectCkptComData.REAL_TEL);
            Builder.EndRow();

            SetThColumn("計畫承辦人信箱", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(projectCkptComData.REAL_EMAIL);
            Builder.EndRow();

            Builder.EndTable();
            InitTable(Table, new List<double> { 20, 80 });

            if (projectCkptComData.CP_KIND == "0")
            {
                Builder.Writeln(string.Empty);
                Table = Builder.StartTable();
                SetThColumn("工程會標案編號", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                SetTdColumn(projectCkptComData.PCC_PROJECT_NO);
                Builder.EndRow();

                SetThColumn("工程會標案名稱", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                SetTdColumn(projectCkptComData.PCC_PROJECT_NAME);
                Builder.EndRow();

                SetThColumn("標案承辦人", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                SetTdColumn(projectCkptComData.FACTORY_CONTACT);
                Builder.EndRow();

                SetThColumn("標案承辦人電話", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                SetTdColumn(projectCkptComData.FACTORY_TEL);
                Builder.EndRow();

                SetThColumn("以界接資料填報", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                SetTdColumn(projectCkptComData.IS_USER_FTY_DATA == true ? "是" : "否");
                Builder.EndRow();

                Builder.EndTable();
                InitTable(Table, new List<double> { 20, 80 });
            }
        }

        /// <summary>
        /// 檢核點完成日期
        /// </summary>
        private void CkptComData()
        {
            SetTitle("（二）檢核點完成日期", fontSize: 14);
            Table = Builder.StartTable();

            SetThColumn("執行方式", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(ShowCpKind(), hMergeCnt: 4);
            Builder.EndRow();

            SetThColumn("計畫開始日期", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(projectCheckpointData.CONTROL_DATE1.ToTwDateString(), hMergeCnt: 4);
            Builder.EndRow();

            SetThColumn("檢核點", backGroundColor: Color.FromArgb(222, 234, 246), hMergeCount: 2);
            SetThColumn("管考進度", backGroundColor: Color.FromArgb(222, 234, 246));
            SetThColumn("預定完成日期", backGroundColor: Color.FromArgb(222, 234, 246));
            SetThColumn("實際完成日期", backGroundColor: Color.FromArgb(222, 234, 246));
            Builder.EndRow();

            if (!projectCkptComData.CustomChkItemModels.Any())
            {
                SetEmptyRow(5);
            }
            foreach (ProjectCusCheckpointModel item in projectCkptComData.CustomChkItemModels)
            {
                SetTdColumn(item.CHECKITEM_NAME, hMergeCnt: 2);
                SetTdColumn($"{(double)item.PROGRESS}%", alignment: AlignmentEnum.Center);
                SetTdColumn(item.ESTIMATED_ENDDATE.ToTwDateString(), alignment: AlignmentEnum.Center);
                SetTdColumn(item.ACTUAL_ENDDATE.ToTwDateString(), alignment: AlignmentEnum.Center);
                Builder.EndRow();
            }

            SetThColumn("預定完成期限", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(ShowEstimatedEndDate(projectCkptComData.CustomChkItemModels), hMergeCnt: 4);
            Builder.EndRow();

            (string, string) adjustHistory = ShowAdjustHistory();
            SetThColumn("總期程調整歷程", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(adjustHistory.Item1, hMergeCnt: 4);
            Builder.EndRow();

            SetThColumn("分月期程調整歷程", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(adjustHistory.Item2, hMergeCnt: 4);
            Builder.EndRow();

            SetThColumn("備註", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(projectCheckpointData.MEMO_CHK_POINT, hMergeCnt: 4);
            Builder.EndRow();

            if (projectCkptComData.CP_KIND == "0")
            {
                SetThColumn("契約預定竣工日", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                SetTdColumn(projectCkptComData.CONTRACT_FINISH_DATE.ToTwDateString(), hMergeCnt: 4);
                Builder.EndRow();

                SetThColumn("工程預定進度表", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                SetTdColumn(ShowFile(), hMergeCnt: 4);
                Builder.EndRow();

                SetThColumn("發包金額(元)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                SetTdColumn($"{projectCkptComData.PROCUREMENT_AMT:N0}", hMergeCnt: 4);
                Builder.EndRow();

                SetThColumn("決標金額(元)", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
                SetTdColumn($"{projectCkptComData.TENDER_AWARDING_AMT:N0}", hMergeCnt: 4);
                Builder.EndRow();
            }

            Builder.EndTable();
            InitTable(Table, new List<double> { 25, 15, 15, 20, 25 });

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
        /// 顯示工程預定進度表
        /// </summary>
        /// <returns></returns>
        private string ShowFile()
        {
            string result = "";
            foreach (ProjectAttachmentModel item in projectCkptComData.FileModels)
            {
                result += $"{item.CRT_DATE.ToTwDateString()} {item.FILE_NAME}";
                if (item.Equals(projectCkptComData.FileModels.LastOrDefault()))
                {
                    result += "\n";
                }
            }
            return result;
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
