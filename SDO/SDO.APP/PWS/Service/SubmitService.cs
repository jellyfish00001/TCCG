using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SDO.Services
{
    public class SubmitService : Service, ISubmitService
    {
        private readonly ISubmitDac dac;
        private readonly IPlanBasicBDac planBasicBDac;
        private readonly IPlanBasicADac planBasicADac;
        private readonly IDAMTBDac DAMTBDac;
        private readonly IBudgetExecDac budgetExecDac;

        public SubmitService(
            ISubmitDac dac,
            IPlanBasicBDac planBasicBDac,
            IPlanBasicADac planBasicADac,
            IDAMTBDac DAMTBDac,
            IBudgetExecDac budgetExecDac
            )
        {
            this.dac = dac;
            this.planBasicBDac = planBasicBDac;
            this.planBasicADac = planBasicADac;
            this.DAMTBDac = DAMTBDac;
            this.budgetExecDac = budgetExecDac;
        }

        /// <summary>
        /// 計畫送出
        /// </summary>
        /// <returns></returns>
        public async Task<SubmitModel> CheckProjectFillSubmit( string PROJECT_NO, int PLANKIND)
        {
            SubmitModel submitResultModel = new();
            List<ProjectSubmitErrorModel> errModels = new();
            // 檢查當期執行情形是否已送出
            submitResultModel.IS_SEND = await dac.CheckProjectFillIsSend(PROJECT_NO);
            // 未送出才需檢驗資料
            if (!submitResultModel.IS_SEND)
            {
                if(PLANKIND == 1)
                {
                    await ChkPlanBasicAValid(PROJECT_NO, errModels);
                    await ChkFundingExecutionValid(PROJECT_NO, errModels);
                }
                else
                {
                    await ChkPlanBasicBValid(PROJECT_NO, errModels);
                    await ChkFundingExecutionValid(PROJECT_NO, errModels);
                }
            }
            submitResultModel.ErrorModels = errModels;
            return submitResultModel;
        }

        /// <summary>
        /// 重大計畫錯誤訊息
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="errModels"></param>
        /// <returns></returns>
        private async Task ChkPlanBasicAValid(string PROJECT_NO, List<ProjectSubmitErrorModel> errModels)
        {
            // 取計畫執行內容
            PlanBasicAModel model = await planBasicADac.GetPlanBasicA(PROJECT_NO);
            // 錯誤訊息
            List<string> errMsgs = new();

            if (string.IsNullOrEmpty(model.PLANNAME))
            {
                errMsgs.Add("「計畫名稱」");
            }
            if (string.IsNullOrEmpty(model.PLANDATETYPE))
            {
                errMsgs.Add("「計畫性質」");
            }
            if (model.PLANDATETYPE == "1" && string.IsNullOrEmpty(model.PLANORGINYN))
            {
                errMsgs.Add("「新興計畫是否屬市長政策或指示項目」");
            }
            // 公務預算, 基金預算, 中央預算, 其他預算(至少填一)
            if (model.PLANTOTMONEY <= 0 )
            {
                errMsgs.Add("「需求數」");
            }
            if( model.FUNDMONEY > 0 && model.FUNDNO == null)
            {
                errMsgs.Add("「基金名稱」");
            }
            if (model.APPROVEDYN =="Y" && model.CENTERMONEY > 0 && string.IsNullOrEmpty(model.APPROVEDNUMBER))
            { 
                errMsgs.Add("「核定文號」");
            }
            if (model.APPROVEDYN == "N" && model.CENTERMONEY > 0 && string.IsNullOrEmpty(model.APPLYAPPROVEDYN))
            {
                errMsgs.Add("「是否報請核定」");
            }
            if (string.IsNullOrEmpty(model.EXPLAINNECESSITY))
            {
                errMsgs.Add("「說明計畫之必要性、亮點及效益」");
            }
            if (string.IsNullOrEmpty(model.EXPLANBASICINFO))
            {
                errMsgs.Add("「說明計畫之基本資料及執行方式」");
            }
            if (string.IsNullOrEmpty(model.EXPLANIMPROVE))
            {
                errMsgs.Add("「既有例行計畫,應說明業務之精進作法」");
            }
            if (string.IsNullOrEmpty(model.EXPLANFUND))
            {
                errMsgs.Add("「說明計畫完成後,是否產生後續維護費用」");
            }
            // 取重點工作規劃及工作期程(檢核點設定)
            model.CusCheckpointModels = await planBasicADac.GetProjectCheckPoint(PROJECT_NO);

            // 檢查 CusCheckpointModels 陣列是否為空
            if (model.CusCheckpointModels == null || model.CusCheckpointModels.Count == 0)
            {
                // 加入錯誤訊息
                errMsgs.Add("「重點工作規劃及工作期程」");
            }
            else
            {
                // 檢查 CusCheckpointModels 日期欄位是否有空值
                bool hasEmptyEstimatedEndDate = false;
                foreach (var cusCheckpointModel in model.CusCheckpointModels)
                {
                    if (!cusCheckpointModel.ESTIMATED_ENDDATE.HasValue)
                    {
                        hasEmptyEstimatedEndDate = true;
                        break;
                    }
                }
                // 如果有空值，則加入錯誤訊息
                if (hasEmptyEstimatedEndDate)
                {
                    errMsgs.Add("「重點工作規劃及工作期程」中「預計完成日期」");
                }
                // 檢查 CusCheckpointModels 日期欄位是否有遞增
                var GD = await dac.ChkPlanBasicAValid(PROJECT_NO);
                if (GD.Contains("No"))
                {
                    errMsgs.Add("「檢核點日期應遞增」");
                }
            }

            if (errMsgs.Any())
            {
                string chapterId = "AddNewPlan";
                errModels.Add(new ProjectSubmitErrorModel()
                {
                    Chapter = "計畫資料",
                    ChapterId = chapterId,
                    ChapterUrl = "/ProjectChapter/AddNewPlan",
                    ErrMsg = $"{string.Join('、', errMsgs)} 尚未填報"
                });
            }
        }

        /// <summary>
        /// 委託研究錯誤訊息
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="errModels"></param>
        /// <returns></returns>
        private async Task ChkPlanBasicBValid(string PROJECT_NO, List<ProjectSubmitErrorModel> errModels)
        {
            // 取得需驗證資料(委託研究)
            PlanBasicBModel model = await planBasicBDac.GetPlanBasicB(PROJECT_NO);
            // 錯誤訊息
            List<string> errMsgs = new();

            if (string.IsNullOrEmpty(model.PLANNAME))
            {
                errMsgs.Add("「計畫名稱」");
            }
            if (string.IsNullOrEmpty(model.LABORYN))
            {
                errMsgs.Add("「計畫性質」");
            }
            // 公務預算, 基金預算, 中央預算, 其他預算(至少填一)
            if (model.PLANTOTMONEY <= 0)
            {
                errMsgs.Add("「需求數」");
            }
            if (model.FUNDMONEY > 0 && string.IsNullOrEmpty(model.FUNDNO.ToString()))
            {
                errMsgs.Add("「基金名稱」");
            }
            if (model.APPROVEDYN == "Y" && model.CENTERMONEY > 0 && string.IsNullOrEmpty(model.APPROVEDNUMBER))
            {
                errMsgs.Add("「核定文號」");
            }
            if (model.APPROVEDYN == "N" && model.CENTERMONEY > 0 && string.IsNullOrEmpty(model.APPLYAPPROVEDYN))
            {
                errMsgs.Add("「是否報請核定」");
            }
            if (string.IsNullOrEmpty(model.PLANCAUSE)) 
            {
                errMsgs.Add("「研究原因及目的」");
            }
            if (string.IsNullOrEmpty(model.PLANEXPECTED))
            {
                errMsgs.Add("「預期研究成果」");
            }
            if (errMsgs.Any())
            {
                string chapterId = "CAddNewPlan";
                errModels.Add(new ProjectSubmitErrorModel()
                {
                    Chapter = "計畫資料",
                    ChapterId = chapterId,
                    ChapterUrl = "/ProjectChapter/CAddNewPlan",
                    ErrMsg = $"{string.Join('、', errMsgs)} 尚未填報"
                });
            }
        }

        /// <summary>
        /// 經費需求驗證
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="errModels"></param>
        /// <returns></returns>
        private async Task ChkFundingExecutionValid(string PROJECT_NO, List<ProjectSubmitErrorModel> errModels)
        {
            // 取得需驗證資料(委託研究)
            FundingExecutionModel model = new();
            // 經費需求
            model.DAMTBListModel = await DAMTBDac.GetDAMTB(PROJECT_NO);
            // 經費執行情形
            model.budgetExecListModel = await budgetExecDac.GetBudgetExec(PROJECT_NO);
            // 檢查計畫經費是否一致
            var result = await dac.CheckProjectMoney(PROJECT_NO);
            // 錯誤訊息
            List<string> errMsgs = new();

            if (model.DAMTBListModel == null || model.DAMTBListModel.Count == 0)
            {
                errMsgs.Add("「經費需求」");
            }
            if (!result)
            {
                errMsgs.Add("「經費需求與計畫需求金額不同」");
            }
            if (model.budgetExecListModel == null || model.budgetExecListModel.Count == 0)
            {
                errMsgs.Add("「歷年執行情形」");
            }
            else
            {
                if (model.budgetExecListModel[0].NOBUDGETYN == 0 && model.budgetExecListModel.All(be => be.RATIO == 0))
                {
                    errMsgs.Add("「歷年執行情形」");
                }
            }
            if (errMsgs.Any())
            {
                string chapterId = "ProjectFundingExecution";
                errModels.Add(new ProjectSubmitErrorModel()
                {
                    Chapter = "經費執行情形和歷年執行情形",
                    ChapterId = chapterId,
                    ChapterUrl = "/ProjectChapter/ProjectFundingExecution",
                    ErrMsg = $"{string.Join('、', errMsgs)} 尚未填報"
                });
            }
        }   

        /// <summary>
        /// 計畫送出
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task<bool> ProjectFillSubmit(string PROJECT_NO)
        {
            await dac.ProjectFillSubmit(PROJECT_NO);
            return true;
        }
    }
}

