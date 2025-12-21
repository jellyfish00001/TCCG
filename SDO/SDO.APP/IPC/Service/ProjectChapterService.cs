using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{

    public class ProjectChapterService : Service, IProjectChapterService
    {
        private readonly IProjectService projectService;
        private readonly IProjectExecuteService projectExecuteService;
        public ProjectChapterService(IProjectService projectService, IProjectExecuteService projectExecuteService)
        {
            this.projectService = projectService;
            this.projectExecuteService = projectExecuteService;
        }

        #region 計畫章節
        /// <summary>
        /// 取得計畫章節表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectChapterListModel>> GetProjectChapter(ProjectChapterQueryModel model)
        {

            List<ProjectChapterListModel> projectChapter = await projectService.GetProjectChapter(model);

            if(model.ApId == "IPC3") { 
                if (model.OPERATION == "P1")
                {
                    // 資料先排除 立案送審、立案審核、執行情形送出、結案審核
                    List<ProjectChapterListModel> result =
                        projectChapter.Where(x => x.CHAPTER_ID != "ProjectFillAddSubmit" &&
                                            x.CHAPTER_ID != "ProjectFillAddAudit" &&
                                            x.CHAPTER_ID != "ProjectFillExecuteSubmit" &&
                                            x.CHAPTER_ID != "ProjectFillCloseAudit").ToList();

                    string projectStatus = await projectService.GetProjectStatus(model.PROJECT_NO);
                    ProjectChapterListModel data = new();
                    // 根據projectStatus，決定應該要顯示哪些章節
                    switch (projectStatus)
                    {
                        // 顯示立案送審
                        case "1":
                        case "3":
                            data = projectChapter.Where(x => x.CHAPTER_ID == "ProjectFillAddSubmit").FirstOrDefault();
                            break;
                        // 顯示立案審核
                        case "2":
                            data = projectChapter.Where(x => x.CHAPTER_ID == "ProjectFillAddAudit").FirstOrDefault();
                            break;
                        // 顯示執行情形送出
                        case "4":
                        case "6":
                            data = projectChapter.Where(x => x.CHAPTER_ID == "ProjectFillExecuteSubmit").FirstOrDefault();
                            break;
                        // 顯示結案審核
                        case "5":
                            data = projectChapter.Where(x => x.CHAPTER_ID == "ProjectFillCloseAudit").FirstOrDefault();
                            break;
                    }
                    if (data != null)
                    {
                        result.Add(data);
                    }

                    //「落後原因分析」：判斷有當期落後或已有落後紀錄才顯示。
                    string delayKind = await projectExecuteService.GetDelayKind(model.PROJECT_NO);
                    List<ProjectDelayCausalModel> delayData = await projectExecuteService.GetProjectFillDelayList(model.PROJECT_NO, "1");
                    if (string.IsNullOrEmpty(delayKind) && delayData.Count() == 0)
                    {
                        result = result.Where(x => x.CHAPTER_ID != "ProjectFillDelay").ToList();
                    }
                    //「實地查證情形」：判斷計畫「管考備註」之「實地查證意見」有查證紀錄才顯示。
                    List<ProjectFactFindingModel> factFindingData = await projectExecuteService.GetProjectFactFinding(model.PROJECT_NO);
                    if (factFindingData.Count() == 0)
                    {
                        result = result.Where(x => x.CHAPTER_ID != "ProjectFillField").ToList();
                    }
                    // 「結案資料」：判斷計畫最後一個檢核點有填實際完成日期才顯示。
                    bool lasttActualEnddate = await projectService.IsLasttActualEnddate(model.PROJECT_NO);
                    if (!lasttActualEnddate)
                    {
                        result = result.Where(x => x.CHAPTER_ID != "ProjectFillClose").ToList();
                    }

                    projectChapter = result.OrderBy(x => x.SORT_ORDER).ToList();
                }
            }

            SetChapterName(projectChapter, model.OPERATION);

            return projectChapter;
        }

        /// <summary>
        /// 設定章節表名稱 顯示樣式
        /// </summary>
        /// <param name="data"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public void SetChapterName(List<ProjectChapterListModel> data, string operation)
        {
            int count = 1;
            string currentStage = string.Empty;
            foreach (ProjectChapterListModel item in data)
            {
                switch (item.SYMBOL)
                {
                    case "A":
                        if (operation == "P1" && currentStage != item.STAGE)
                        {
                            count = 1;
                        }
                        item.title = string.Format("{0}、{1}", count, item.title);
                        currentStage = item.STAGE;
                        count++;
                        break;
                    case "B":
                        item.title = string.Format("￭　{1}", count, item.title);
                        break;
                    case "C":
                        item.title = string.Format("–　{1}", count, item.title);
                        break;
                }
            }
        }
        #endregion
    }
}
