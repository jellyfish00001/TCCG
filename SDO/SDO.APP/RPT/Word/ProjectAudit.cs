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
    /// 預覽列印 四、管考備註 （一）平時管考意見
    /// </summary>
    public class ProjectAudit : WContentBuilder
    {
        private readonly IProjectExecuteService projectExecuteService;
        private readonly ISysParamDac sysParamDac;
        // 管考備註資料
        ProjectFillAuditModel projectAuditData;
        //參數資料
        private IList<SetParamModel> sysParamData;

        public ProjectAudit(IComponentContext coms) : base(coms)
        {
            this.projectExecuteService = coms.Resolve<IProjectExecuteService>();
            this.sysParamDac = coms.Resolve<ISysParamDac>();
        }

        protected override async Task GetData()
        {
            projectAuditData = await projectExecuteService.GetProjectFillAudit(Parameter.PROJECT_NO);
            sysParamData = await sysParamDac.GetSysParams();
        }

        protected override void Content()
        {
            Table = Builder.StartTable();
            SetThColumn("期間", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("意見", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            SetThColumn("備註", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Center);
            Builder.EndRow();

            if (!projectAuditData.ProjectEngineeringAuditOpinion.Any())
            {
                SetTdColumn("無資料", alignment: AlignmentEnum.Center, hMergeCnt: 3);
                Builder.EndRow();
            }
            foreach (ProjectEngineeringAuditOpinionModel item in projectAuditData.ProjectEngineeringAuditOpinion)
            {
                SetTdColumn($"{item.YEAR}_{item.MONTH}", alignment: AlignmentEnum.Center);
                SetTdColumn(item.AUDIT_OPINION);
                SetTdColumn(ShowMemo(item.ComIPCMemoMappingData));
                Builder.EndRow();
            }
            Builder.EndTable();
            InitTable(Table, new List<double> { 12, 58, 30 });
            Builder.Writeln(string.Empty);

            Table = Builder.StartTable();
            SetThColumn("特殊加註", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(ShowSpecNote(projectAuditData.SpecNoteMappingData, sysParamData.Where(x => x.SET_ITEM == "SPEC_NOTE").ToList()));
            Builder.EndRow();

            SetThColumn("列管會議", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(ShowConference());
            Builder.EndRow();

            SetThColumn("資料逾期繳交或填報", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(ShowDelay());
            Builder.EndRow();

            SetThColumn("分案或併案", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(ShowMerge());
            Builder.EndRow();

            SetThColumn("撤銷資料", backGroundColor: Color.FromArgb(222, 234, 246), alignment: ParagraphAlignment.Left);
            SetTdColumn(ShowRevokeData());
            Builder.EndRow();

            Builder.EndTable();
            InitTable(Table, new List<double> { 25, 75 });
        }

        /// <summary>
        /// 顯示 平時管考意見-備註
        /// </summary>
        /// <returns></returns>
        private string ShowMemo(List<ProjectMappingDataModel> mappingData)
        {
            int count = 1;
            string result = "";
            foreach (ProjectMappingDataModel item in mappingData)
            {
                result += $"{count}.{sysParamData.Where(x => x.SET_ITEM == "COM_IPCMEMO" && x.SET_TYPE == item.SET_TYPE).Select(x => x.SET_VALUE).FirstOrDefault()}";
                count++;
                if (!item.Equals(mappingData.LastOrDefault()))
                {
                    result += "\n";
                }
            }
            return result;
        }

        /// <summary>
        /// 顯示特殊加註
        /// </summary>
        /// <param name="mappingData"></param>
        /// <param name="paramData"></param>
        /// <returns></returns>
        private string ShowSpecNote(List<ProjectMappingDataModel> mappingData, IList<SetParamModel> paramData)
        {
            List<string> setType = mappingData.Select(x => x.SET_TYPE).ToList();
            return string.Join("、", paramData.Where(x => setType.Contains(x.SET_TYPE)).Select(x => x.SET_VALUE));
        }

        /// <summary>
        /// 顯示列管會議
        /// </summary>
        /// <returns></returns>
        private string ShowConference()
        {
            int count = 1;
            string result = "";
            foreach (ProjectConferenceModel item in projectAuditData.ProjectConference)
            {
                string setValue = sysParamData.Where(x => x.SET_ITEM == "COM_CONFERENCEGENRE" && x.SET_TYPE == item.CONFERENCE_GENRE)
                    .Select(x => x.SET_VALUE).FirstOrDefault();
                result += $"{count}.{setValue}（{item.CONFERENCE_TIME.ToTwDateString()} 第{item.CONFERENCE_NUM}次會議）";
                count++;
                if (!item.Equals(projectAuditData.ProjectConference.LastOrDefault()))
                {
                    result += "\n";
                }
            }
            return result;
        }

        /// <summary>
        /// 顯示資料逾期繳交或填報
        /// </summary>
        /// <returns></returns>
        private string ShowDelay()
        {
            int count = 1;
            string result = "";
            foreach (ProjectDelayfillModel item in projectAuditData.ProjectDelayfill)
            {
                result += $"{count}.{item.FILL_TIME.ToTwDateString()} {item.FILL_REASON}";
                count++;
                if (!item.Equals(projectAuditData.ProjectDelayfill.LastOrDefault()))
                {
                    result += "\n";
                }
            }
            return result;
        }

        /// <summary>
        /// 顯示分案或併案
        /// </summary>
        /// <returns></returns>
        private string ShowMerge()
        {
            string result = "";
            foreach (ProjectMergeLogModel item in projectAuditData.ProjectMergeLog)
            {
                foreach (var file in item.File)
                {
                    result += $"{item.PROMERGE_DATE.ToTwDateString()} {(item.MERGE_STATUS == "01" ? "簽准分案" : "簽准併案")}（{file.FILE_NAME}）";
                    if (!item.Equals(projectAuditData.ProjectMergeLog.LastOrDefault()))
                    {
                        result += "\n";
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 顯示撤銷資料
        /// </summary>
        /// <returns></returns>
        private string ShowRevokeData()
        {
            string result = "";
            AdjustAuditModel revokeData = projectAuditData.RevokeData;
            if (revokeData != null)
            {
                result = $"核准日期：{revokeData.APPRV_DATE.ToTwDateString()}\n" +
                    $"撤銷原因：{revokeData.OTHER_REASON}\n" +
                    $"撤銷原因說明：{revokeData.ADJUST_REASON}";
            }
            return result;
        }
    }
}
