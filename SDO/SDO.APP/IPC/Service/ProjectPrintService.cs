using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class ProjectPrintService : Service, IProjectPrintService
    {
        private readonly IProjectService projectService;
        private readonly IProjectExecuteService projectExecuteService;
        private readonly IProjectClosedService projectClosedService;
        public ProjectPrintService(IProjectService projectService, IProjectExecuteService projectExecuteService,
            IProjectClosedService projectClosedService
            )
        {
            this.projectService = projectService;
            this.projectExecuteService = projectExecuteService;
            this.projectClosedService = projectClosedService;
        }

        /// <summary>
        /// 取得計畫預覽資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<ProjectPrintModel> GetProjectPrint(string PROJECT_NO, string type)
        {
            ProjectFillBudgetExecModel ProjectFillBudgetExec = new();
            if (type != "A")
            {
                ProjectFillBudgetExec = await projectExecuteService.GetProjectFillBudgetExec(PROJECT_NO);
            }
            ProjectPrintModel result = new ProjectPrintModel()
            {
                ProjectBasicFill = await projectService.GetProjectBasicFill(PROJECT_NO),
                ProjectCheckpoint = await projectService.GetProjectCheckpoint(PROJECT_NO),
                ProjectCkptCom = await projectExecuteService.GetProjectFillCkptCom(PROJECT_NO),
                CheckProjectEngineeringProgress = type == "A" ? new() : await projectExecuteService.GetProjecFillExecute(PROJECT_NO, string.Empty),
                ProjectEngineeringProgress = type == "A" ? new() : await projectExecuteService.GetProjecFillExecuteList(PROJECT_NO, "1"),
                ProjectDelay = type == "A" ? new() : await projectExecuteService.GetProjectFillDelayList(PROJECT_NO, "1"),
                ProjectBudgetExecute = type == "A" ? new() : ProjectFillBudgetExec.ProjectBudgetExecute,
                ProjectFactFinding = type == "A" ? new() : await projectExecuteService.GetProjectFactFinding(PROJECT_NO),
                ProjectOther = type == "A" ? new() : await projectExecuteService.GetProjectFillOther(PROJECT_NO),
                ProjectClose = type == "A" ? new() : await projectClosedService.GetProjectFillClose(PROJECT_NO),
                ProjectAudit = await projectExecuteService.GetProjectFillAudit(PROJECT_NO),
                ShowList = type == "A" ? new() : await GetProjectPrintShowData(PROJECT_NO),
            };
            return result;
        }

        /// <summary>
        /// 計畫預覽依特定條件顯示填報資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, object>> GetProjectPrintShowData(string PROJECT_NO)
        {
            string[] number = new string[] { "一", "二", "三", "四", "五", "六" };
            Dictionary<string, object> result = new();
            int count = 0;

            //「每月辦理情形」：判斷有資料才顯示。
            List<ProjectEngineeringProgressGridModel> executeData = await projectExecuteService.GetProjecFillExecuteList(PROJECT_NO, "1");
            if (executeData.Any())
            {
                result.Add("ProjectExecute", $"（{number[count++]}）每月辦理情形");
            }

            //「落後原因分析」：判斷有落後資料才顯示。
            List<ProjectDelayCausalModel> delayData = await projectExecuteService.GetProjectFillDelayList(PROJECT_NO, "1");
            if (delayData.Any())
            {
                result.Add("ProjectDelay", $"（{number[count++]}）落後原因分析");
            }

            //「預算執行情形」：判斷主辦有輸入資料才顯示。
            List<ProjectBudgetExecuteModel> budgetExecData = (await projectExecuteService.GetProjectFillBudgetExec(PROJECT_NO)).ProjectBudgetExecute;
            if (budgetExecData.Any())
            {
                result.Add("ProjectBudgetExec", $"（{number[count++]}）預算執行情形");
            }

            //「實地查證情形」：判斷計畫「管考備註」之「實地查證意見」有查證紀錄才顯示。
            List<ProjectFactFindingModel> factFindingData = await projectExecuteService.GetProjectFactFinding(PROJECT_NO);
            if (factFindingData.Any())
            {
                result.Add("ProjectField", $"（{number[count++]}）實地查證情形");
            }

            //「其它資訊」：各子項目主辦有輸入資料才顯示。
            ProjectFillOtherModel otherData = await projectExecuteService.GetProjectFillOther(PROJECT_NO);
            List<bool> checkList = CheckProjectOtherData(otherData);
            if (checkList.Contains(true))
            {
                result.Add("ProjectOther", $"（{number[count++]}）其它資訊");
            }
            result.Add("SubProjectOther", checkList);

            // 「結案資料」：判斷計畫最後一個檢核點有填實際完成日期才顯示。
            bool lasttActualEnddate = await projectService.IsLasttActualEnddate(PROJECT_NO);
            if (lasttActualEnddate)
            {
                result.Add("ProjectClose", $"（{number[count++]}）結案資料");
            }
            return result;
        }

        /// <summary>
        /// 檢查其他資訊各子項目有無資料
        /// </summary>
        /// <param name="otherData"></param>
        /// <returns></returns>
        private List<bool> CheckProjectOtherData(ProjectFillOtherModel otherData)
        {
            List<bool> result = new();
            // 招標情形有無資料
            bool hasBidData = false;
            foreach (ProjectBidModel item in otherData.ProjectBid)
            {
                if (otherData.ProjectBidDetail.Where(x => x.BID_KIND == item.BID_KIND).Any() ||
                    item.AWARD_BID_DATE.HasValue ||
                    !string.IsNullOrEmpty(item.BID_TENDER))
                {
                    hasBidData = true;
                    break;
                }
            }
            result.Add(hasBidData);

            // 相關活動有無資料
            result.Add(otherData.ProjectActivity.Where(x => x.IS_ACTIVITY.HasValue).Any());

            // 相關審查有無資料
            result.Add(otherData.ProjectReview.Where(x => x.IS_REVIEW.HasValue).Any());

            // 廠商資訊有無資料
            result.Add(otherData.ProjectTender.Any());
            return result;
        }
    }
}
