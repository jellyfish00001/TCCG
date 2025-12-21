using Aspose.Pdf.Operators;
using Aspose.Words.Themes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using SDO.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class PWSRPTDac : Dac, IPWSRPTDac
    {
        public PWSRPTDac(
            IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }


        /// <summary>
        /// 取得計畫小組審查資料 (2Excal)
        /// </summary>
        /// <param name="model"></param>
        public async Task<List<RPTBudgeReviewModel>> GetPlanReview(PWSReportModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"
                            SELECT 
                                pm.PLANNO,
                                pm.OU_ID,
                                dbo.FN_GetOuName(pm.OU_ID, 3) AS OU_NAME,
                                pm.PLANNAME,
                                pm.PLANYEAR,
                                pm.PLANORDERNUMBER,
                                pm.CREATEUNITOUID,
                                dbo.FN_GetOuName(pm.CREATEUNITOUID, 3) AS UNITOUNAME,
                                pm.CREATEORGOUID,
                                dbo.FN_GetOuName(pm.CREATEORGOUID, 3) AS ORGOUNAME,
                                fo.FUNDNO,
                                fo.FUNDNAME,
                                COALESCE(pbsg.CROSS_PUBLICMONEY, 0) AS CROSS_PUBLICMONEY,
                                COALESCE(pbsg.CROSS_FUNDMONEY, 0) AS CROSS_FUNDMONEY,
                                pbsg.BUDGETTYPE,
                                pm.PUBLICMONEY,
                                pm.FUNDMONEY,
                                pv.PUBLIC1,
                                pv.FUND1,
                                pv.PUBLIC2,
                                pv.FUND2,
                                pv.ADVIEWDESC
                            FROM PWSSDPLANMAIN pm (NOLOCK)
                            LEFT JOIN (
                                SELECT 
                                    PLANNO,
                                    SUM(CASE WHEN SOURCEKIND = '1' THEN BUDGETCENTRAL ELSE 0 END) AS CROSS_PUBLICMONEY,
                                    SUM(CASE WHEN SOURCEKIND = '3' THEN BUDGETLOCAL ELSE 0 END) AS CROSS_FUNDMONEY,
                                    MAX(BUDGETTYPE) AS BUDGETTYPE 
                                FROM PROJECT_BUDGET_SOURCE_G (NOLOCK)
                                GROUP BY PLANNO
                            ) pbsg ON pm.PLANNO = pbsg.PLANNO
                            LEFT JOIN FUNDORG fo ON pm.FUNDNO = fo.FUNDNO
                            LEFT JOIN PWSSDVIEW pv ON pm.PLANNO = pv.PLANNO
                            WHERE 1=1");

            if (!string.IsNullOrEmpty(model.PWS_YEAR))
            {
                sql.AppendLine(" AND pm.PLANYEAR = @PWS_YEAR");
            }
            if (!string.IsNullOrEmpty(model.BUDGETTYPE))
            {
                sql.AppendLine(" AND pm.BUDGETTYPE = @BUDGETTYPE");
            }
            if (model.IS_SEND.HasValue)
            {
                sql.AppendLine(" AND pm.IS_SEND = @IS_SEND");
            }
            if (model.SEND_STATUS.HasValue)
            {
                sql.AppendLine(" AND pm.SEND_STATUS = @SEND_STATUS");
            }
            if (model.AUDIT_STATUS.HasValue)
            {
                sql.AppendLine(" AND pv.AUDIT_STATUS = @AUDIT_STATUS");
            }
            if (!string.IsNullOrEmpty(model.FUND_OU_ID))
            {
                sql.AppendLine(" AND pm.OU_ID = @FUND_OU_ID");
            }
            if (!string.IsNullOrEmpty(model.OU_ID))
            {
                sql.AppendLine(" AND pm.OU_ID = @OU_ID");
            }
            if (!string.IsNullOrEmpty(model.CREATEUNITOUID))
            {
                sql.AppendLine(" AND pm.CREATEUNITOUID = @CREATEUNITOUID");
            }
            var result = await ExecuteQueryAsync<RPTBudgeReviewModel>(sql.ToString(), model, PWSDBKey);
            return result.ToList();
        }

        /// <summary>
        /// 取得計畫局處全部彙整表 (Excal)
        /// </summary>
        /// <param name="model"></param>
        public async Task<List<RPTBudgeReviewModel>> GetALLORGPlan(PWSReportModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"
                            SELECT 
                                pm.OU_ID,
                                dbo.FN_GetOuName(pm.OU_ID, 3) AS OU_NAME,
	                            COUNT(DISTINCT pm.PLANNO) AS PLANCOUNT,
                                MAX(fo.FUNDNO) AS FUNDNO, 
                                MAX(fo.FUNDNAME) AS FUNDNAME, 
                                SUM(COALESCE(pbsg.CROSS_PUBLICMONEY, 0)) AS CROSS_PUBLICMONEY,
                                SUM(COALESCE(pbsg.CROSS_FUNDMONEY, 0)) AS CROSS_FUNDMONEY,
                                MAX(pbsg.BUDGETTYPE) AS BUDGETTYPE, 
                                SUM(pm.PUBLICMONEY) AS PUBLICMONEY,
                                SUM(pm.FUNDMONEY) AS FUNDMONEY,
                                SUM(pv.PUBLIC1) AS PUBLIC1,
                                SUM(pv.FUND1) AS FUND1,
                                SUM(pv.PUBLIC2) AS PUBLIC2,
                                SUM(pv.FUND2) AS FUND2,
                                MAX(pv.ADVIEWDESC) AS ADVIEWDESC 
                            FROM PWSSDPLANMAIN pm (NOLOCK)
                            LEFT JOIN (
                                SELECT 
                                    PLANNO,
                                    SUM(CASE WHEN SOURCEKIND = '1' THEN BUDGETCENTRAL ELSE 0 END) AS CROSS_PUBLICMONEY,
                                    SUM(CASE WHEN SOURCEKIND = '3' THEN BUDGETLOCAL ELSE 0 END) AS CROSS_FUNDMONEY,
                                    MAX(BUDGETTYPE) AS BUDGETTYPE 
                                FROM PROJECT_BUDGET_SOURCE_G (NOLOCK)
                                GROUP BY PLANNO
                            ) pbsg ON pm.PLANNO = pbsg.PLANNO
                            LEFT JOIN FUNDORG fo ON pm.FUNDNO = fo.FUNDNO
                            LEFT JOIN PWSSDVIEW pv ON pm.PLANNO = pv.PLANNO
                            WHERE 1=1 
                           ");
                            
            if (!string.IsNullOrEmpty(model.PWS_YEAR))
            {
                sql.AppendLine(" AND pm.PLANYEAR = @PWS_YEAR");
            }
            if (!string.IsNullOrEmpty(model.BUDGETTYPE))
            {
                sql.AppendLine(" AND pm.BUDGETTYPE = @BUDGETTYPE");
            }
            if (model.IS_SEND.HasValue)
            {
                sql.AppendLine(" AND pm.IS_SEND = @IS_SEND");
            }
            if (model.SEND_STATUS.HasValue)
            {
                sql.AppendLine(" AND pm.SEND_STATUS = @SEND_STATUS");
            }
            if (model.AUDIT_STATUS.HasValue)
            {
                sql.AppendLine(" AND pv.AUDIT_STATUS = @AUDIT_STATUS");
            }
            sql.AppendLine(@" GROUP BY pm.OU_ID, 
                                       dbo.FN_GetOuName(pm.OU_ID, 3)
                           ");
            var result = await ExecuteQueryAsync<RPTBudgeReviewModel>(sql.ToString(), model, PWSDBKey);
            return result.ToList();
        }

        /// <summary>
        /// 取得計畫基金全部彙整表 (Excal)
        /// </summary>
        /// <param name="model"></param>
        public async Task<List<RPTBudgeReviewModel>> GetALLFUNDPlan(PWSReportModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"
                            SELECT 
	                            COUNT(DISTINCT pm.PLANNO) AS PLANCOUNT,
                                fo.FUNDNO, 
                                fo.FUNDNAME, 
                                SUM(COALESCE(pbsg.CROSS_PUBLICMONEY, 0)) AS CROSS_PUBLICMONEY,
                                SUM(COALESCE(pbsg.CROSS_FUNDMONEY, 0)) AS CROSS_FUNDMONEY,
                                MAX(pbsg.BUDGETTYPE) AS BUDGETTYPE, 
                                SUM(pm.PUBLICMONEY) AS PUBLICMONEY,
                                SUM(pm.FUNDMONEY) AS FUNDMONEY,
                                SUM(pv.PUBLIC1) AS PUBLIC1,
                                SUM(pv.FUND1) AS FUND1,
                                SUM(pv.PUBLIC2) AS PUBLIC2,
                                SUM(pv.FUND2) AS FUND2,
                                MAX(pv.ADVIEWDESC) AS ADVIEWDESC 
                            FROM PWSSDPLANMAIN pm (NOLOCK)
                            LEFT JOIN (
                                SELECT 
                                    PLANNO,
                                    SUM(CASE WHEN SOURCEKIND = '1' THEN BUDGETCENTRAL ELSE 0 END) AS CROSS_PUBLICMONEY,
                                    SUM(CASE WHEN SOURCEKIND = '3' THEN BUDGETLOCAL ELSE 0 END) AS CROSS_FUNDMONEY,
                                    MAX(BUDGETTYPE) AS BUDGETTYPE 
                                FROM PROJECT_BUDGET_SOURCE_G (NOLOCK)
                                GROUP BY PLANNO
                            ) pbsg ON pm.PLANNO = pbsg.PLANNO
                            LEFT JOIN FUNDORG fo ON pm.FUNDNO = fo.FUNDNO
                            LEFT JOIN PWSSDVIEW pv ON pm.PLANNO = pv.PLANNO
                            WHERE fo.FUNDNO IS NOT NULL ");

            if (!string.IsNullOrEmpty(model.PWS_YEAR))
            {
                sql.AppendLine(" AND pm.PLANYEAR = @PWS_YEAR");
            }
            if (!string.IsNullOrEmpty(model.BUDGETTYPE))
            {
                sql.AppendLine(" AND pm.BUDGETTYPE = @BUDGETTYPE");
            }
            if (model.IS_SEND.HasValue)
            {
                sql.AppendLine(" AND pm.IS_SEND = @IS_SEND");
            }
            if (model.SEND_STATUS.HasValue)
            {
                sql.AppendLine(" AND pm.SEND_STATUS = @SEND_STATUS");
            }
            if (model.AUDIT_STATUS.HasValue)
            {
                sql.AppendLine(" AND pv.AUDIT_STATUS = @AUDIT_STATUS");
            }
            sql.AppendLine(@" GROUP BY fo.FUNDNO, 
                               fo.FUNDNAME
                          ");
            var result = await ExecuteQueryAsync<RPTBudgeReviewModel>(sql.ToString(), model, PWSDBKey);
            return result.ToList();
        }

        /// <summary>
        /// 取性別辦理評估計畫清單 Word
        /// </summary>
        /// <param name="model"></param>
        public async Task<List<RPTProjectReviewList>> GetPlanGenderAnalyst(PWSReportModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"
                            SELECT
                                  pm.OU_ID,
                                  dbo.FN_GetOuName(OU_ID, 3) AS OU_NAME,
                                  pm.PLANNAME,
                                  pm.PLANYEAR,
                                  pv.AUDIT_STATUS
                            FROM  PWSSDPLANMAIN pm (NOLOCK)
                            LEFT JOIN PWSSDVIEW pv ON pm.PLANNO = pv.PLANNO
                            WHERE GENDER_ANALYST_YN = 1
                          ");
            if(!string.IsNullOrEmpty(model.PWS_YEAR))
            {
                sql.AppendLine(" AND pm.PLANYEAR = @PWS_YEAR");
            }
            if (model.IS_SEND.HasValue)
            {
                sql.AppendLine(" AND pm.IS_SEND = @IS_SEND");
            }
            if (model.SEND_STATUS.HasValue)
            {
                sql.AppendLine(" AND pm.SEND_STATUS = @SEND_STATUS");
            }
            if (model.AUDIT_STATUS.HasValue)
            {
                sql.AppendLine(" AND pv.AUDIT_STATUS = @AUDIT_STATUS");
            }
            var result = await ExecuteQueryAsync<RPTProjectReviewList>(sql.ToString(), model, PWSDBKey);
            return result.ToList();
        }

        /// <summary>
        /// 取重大計畫審查NO表 (Word)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<string>> GetPlanReviewNOList(PWSReportModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"
                            SELECT 
                                 pm.PLANNO
                            FROM PWSSDPLANMAIN pm (NOLOCK)
                            LEFT JOIN PWSSDVIEW pv ON pm.PLANNO = pv.PLANNO
                            WHERE 1=1
                           ");
            if (!string.IsNullOrEmpty(model.PWS_YEAR))
            {
                sql.AppendLine(" AND pm.PLANKIND = @PLANKIND");
            }
            if (!string.IsNullOrEmpty(model.PWS_YEAR))
            {
                sql.AppendLine(" AND pm.PLANYEAR = @PWS_YEAR");
            }
            if (model.IS_SEND.HasValue)
            {
                sql.AppendLine(" AND pm.IS_SEND = @IS_SEND");
            }
            if (model.SEND_STATUS.HasValue)
            {
                sql.AppendLine(" AND pm.SEND_STATUS = @SEND_STATUS");
            }
            if (model.AUDIT_STATUS.HasValue)
            {
                sql.AppendLine(" AND pv.AUDIT_STATUS = @AUDIT_STATUS");
            }
            if (!string.IsNullOrEmpty(model.OU_ID))
            {
                sql.AppendLine(" AND pm.OU_ID = @OU_ID");
            }
            if (!string.IsNullOrEmpty(model.CREATEUNITOUID))
            {
                sql.AppendLine(" AND pm.CREATEUNITOUID = @CREATEUNITOUID");
            }
            if (!string.IsNullOrEmpty(model.PLANNO))
            {
                sql.AppendLine(" AND pm.PLANNO) = @PLANNO)");
            }
            var result = await ExecuteQueryAsync<string>(sql.ToString(), model, PWSDBKey);
            return result.ToList();
        }

        /// <summary>
        /// 取重大計畫審查 Word
        /// </summary>
        /// <param name="model"></param>
        public async Task<RPTProjectReviewList> GetPlanReviewList(PWSReportModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"
                            SELECT 
                                PLANNO,
                                PLANNAME,
                                PLANORDERNUMBER,
                                CREATEUNITOUID,
                                dbo.FN_GetOuName(CREATEUNITOUID, 3) AS UNITOUNAME,
                                CREATEORGOUID,
                                dbo.FN_GetOuName(CREATEORGOUID, 3) AS ORGOUNAME,
                                PLANDATETYPE,
                                BUDGETTYPE,
                                PUBLICMONEY,
                                FUNDMONEY,
                                PLANTOTMONEY,
                                PLANORGINYN,
                                PLANCONTENTENGINE,
                                PLANCONTENTLAND,
                                PLANCONTENTINFO,
                                PLANSTARTDATE,
                                PLANENDDATE
                            FROM PWSSDPLANMAIN (NOLOCK)
                            WHERE PLANKIND = '1'
                           ");
            if (!string.IsNullOrEmpty(model.PLANNO))
            {
                sql.AppendLine(" AND PLANNO = @PLANNO");
            }

            var result = await ExecuteQueryFirstOrDefaultAsync<RPTProjectReviewList>(sql.ToString(), model, PWSDBKey);
            return result;
        }

        /// <summary>
        /// 先期審查-委託研究計畫先期審查計畫表(管考+機關) (Word)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RPTProjectEntListModel> GetEntPlanReviewList(PWSReportModel model)
        {
           string sql = @"
                            SELECT    
                                    p.PLANNO,
                                    p.PLANNAME,
                                    p.OU_ID,
                                    dbo.FN_GetOuName(p.OU_ID, 3) AS OU_NAME,
                                    p.PLANYEAR,
                                    p.LABORYN,
                                    p.PLANSTARTDATE,
                                    p.PLANENDDATE,
                                    p.AWARDYM,
                                    p.MIDREPORTYM,
                                    p.FINAKREPORTYM,
                                    p.CLOSEYM,
                                    p.PLANORDERNUMBER,
                                    p.CREATEUNITOUID,
                                    dbo.FN_GetOuName(p.CREATEUNITOUID, 3) AS UNITOUNAME,
                                    p.CREATEORGOUID,
                                    dbo.FN_GetOuName(p.CREATEORGOUID, 3) AS ORGOUNAME,
                                    p.PLANDATETYPE,
                                    p.PUBLICMONEY,
                                    p.FUNDMONEY,
                                    SUM(CASE WHEN b.PLANYEAR = YEAR(GETDATE()) - 1911 THEN b.BUDGETCENTRAL + b.BUDGETLOCAL END) AS NYMoney,
                                    SUM(CASE WHEN b.PLANYEAR < YEAR(GETDATE()) - 1911 THEN b.BUDGETCENTRAL + b.BUDGETLOCAL END) AS BYMoney,
                                    SUM(CASE WHEN b.PLANYEAR > YEAR(GETDATE()) - 1911 THEN b.BUDGETCENTRAL + b.BUDGETLOCAL END) AS AYMoney,
                                    p.CENTERMONEY,
                                    p.OTHERMONEY,
                                    p.OTHERDESC,
                                    p.APPROVEDNUMBER,
                                    p.APPROVEDYN,
                                    p.APPLYAPPROVEDYN,
                                    p.PLANCAUSE,
                                    p.PLANEXPECTED,
                                    p.PLANTOTMONEY
                            FROM PWSSDPLANMAIN p(NOLOCK)
                            LEFT JOIN[TYCG_PWS_M].[dbo].[PROJECT_BUDGET_SOURCE_G] b ON p.PLANNO = b.PLANNO
                            WHERE 1 = 1 
                        ";
            if (!string.IsNullOrEmpty(model.PLANNO))
            {
                sql += " AND p.PLANNO = @PLANNO";
            }
            sql += @"           
                    GROUP BY 
                        p.PLANNO,
                        p.PLANNAME,
                        p.OU_ID,
                        p.PLANYEAR,
                        p.LABORYN,
                        p.PLANSTARTDATE,
                        p.PLANENDDATE,
                        p.AWARDYM,
                        p.MIDREPORTYM,
                        p.FINAKREPORTYM,
                        p.CLOSEYM,
                        p.PLANORDERNUMBER,
                        p.CREATEUNITOUID,
                        p.CREATEORGOUID,
                        p.PLANDATETYPE,
                        p.PUBLICMONEY,
                        p.FUNDMONEY,
                        p.CENTERMONEY,
                        p.OTHERMONEY,
                        p.OTHERDESC,
                        p.APPROVEDNUMBER,
                        p.APPROVEDYN,
                        p.APPLYAPPROVEDYN,
                        p.PLANCAUSE,
                        p.PLANEXPECTED,
                        p.PLANTOTMONEY
                    ";
            var result = await ExecuteQueryFirstOrDefaultAsync<RPTProjectEntListModel>(sql, model, PWSDBKey);
            return result;
        }

        /// <summary>
        /// 先期審查-委託研究計畫審查結果彙整表(管考) (Excal)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<RPTBudgeReviewModel>> GetEntPlanMg(PWSReportModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@" SELECT 
                                pm.PLANNO,
                                pm.PLANNAME,
                                pm.PLANYEAR,
                                pm.PLANORDERNUMBER,
                                pm.CREATEUNITOUID,
                                dbo.FN_GetOuName(pm.CREATEUNITOUID, 3) AS UNITOUNAME,
                                pm.CREATEORGOUID,
                                dbo.FN_GetOuName(pm.CREATEORGOUID, 3) AS ORGOUNAME,
                                fo.FUNDNO,
                                fo.FUNDNAME,
                                COALESCE(pbsg.CROSS_PUBLICMONEY, 0) AS CROSS_PUBLICMONEY,
                                COALESCE(pbsg.CROSS_FUNDMONEY, 0) AS CROSS_FUNDMONEY,
                                pbsg.BUDGETTYPE,
                                pm.PUBLICMONEY,
                                pm.FUNDMONEY,
                                pv.PUBLIC1,
                                pv.FUND1,
                                pv.PUBLIC2,
                                pv.FUND2,
                                pv.ADVIEWDESC
                            FROM PWSSDPLANMAIN pm (NOLOCK)
                            LEFT JOIN (
                                SELECT 
                                    PLANNO,
                                    SUM(CASE WHEN SOURCEKIND = '1' THEN BUDGETCENTRAL ELSE 0 END) AS CROSS_PUBLICMONEY,
                                    SUM(CASE WHEN SOURCEKIND = '3' THEN BUDGETLOCAL ELSE 0 END) AS CROSS_FUNDMONEY,
                                    MAX(BUDGETTYPE) AS BUDGETTYPE 
                                FROM PROJECT_BUDGET_SOURCE_G (NOLOCK)
                                GROUP BY PLANNO
                            ) pbsg ON pm.PLANNO = pbsg.PLANNO
                            LEFT JOIN FUNDORG fo ON pm.FUNDNO = fo.FUNDNO
                            LEFT JOIN PWSSDVIEW pv ON pm.PLANNO = pv.PLANNO
                            WHERE PLANKIND = '2'
                          ");

            if (!string.IsNullOrEmpty(model.PWS_YEAR))
            {
                sql.AppendLine(" AND pm.PLANYEAR = @PWS_YEAR");
            }
            if (model.IS_SEND.HasValue)
            {
                sql.AppendLine(" AND pm.IS_SEND = @IS_SEND");
            }
            if (model.SEND_STATUS.HasValue)
            {
                sql.AppendLine(" AND pm.SEND_STATUS = @SEND_STATUS");
            }
            if (model.AUDIT_STATUS.HasValue)
            {
                sql.AppendLine(" AND pv.AUDIT_STATUS = @AUDIT_STATUS");
            }
            var result = await ExecuteQueryAsync<RPTBudgeReviewModel>(sql.ToString(), model, PWSDBKey);
            return result.ToList();
        }

        /// <summary>
        /// 先期審查-委託研究計畫審查結果彙整表(機關) (Excal)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<RPTBudgeReviewModel>> GetEntPlanOrg(PWSReportModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"
                            SELECT 
                                pm.PLANNO,
                                pm.PLANNAME,
                                pm.PLANYEAR,
                                pm.PLANORDERNUMBER,
                                pm.CREATEUNITOUID,
                                dbo.FN_GetOuName(pm.CREATEUNITOUID, 3) AS UNITOUNAME,
                                pm.CREATEORGOUID,
                                dbo.FN_GetOuName(pm.CREATEORGOUID, 3) AS ORGOUNAME,
                                fo.FUNDNO,
                                fo.FUNDNAME,
                                COALESCE(pbsg.CROSS_PUBLICMONEY, 0) AS CROSS_PUBLICMONEY,
                                COALESCE(pbsg.CROSS_FUNDMONEY, 0) AS CROSS_FUNDMONEY,
                                pbsg.BUDGETTYPE,
                                pm.PUBLICMONEY,
                                pm.FUNDMONEY,
                                pv.PUBLIC1,
                                pv.FUND1,
                                pv.PUBLIC2,
                                pv.FUND2,
                                pv.ADVIEWDESC
                            FROM PWSSDPLANMAIN pm (NOLOCK)
                            LEFT JOIN (
                                SELECT 
                                    PLANNO,
                                    SUM(CASE WHEN SOURCEKIND = '1' THEN BUDGETCENTRAL ELSE 0 END) AS CROSS_PUBLICMONEY,
                                    SUM(CASE WHEN SOURCEKIND = '3' THEN BUDGETLOCAL ELSE 0 END) AS CROSS_FUNDMONEY,
                                    MAX(BUDGETTYPE) AS BUDGETTYPE 
                                FROM PROJECT_BUDGET_SOURCE_G (NOLOCK)
                                GROUP BY PLANNO
                            ) pbsg ON pm.PLANNO = pbsg.PLANNO
                            LEFT JOIN FUNDORG fo ON pm.FUNDNO = fo.FUNDNO
                            LEFT JOIN PWSSDVIEW pv ON pm.PLANNO = pv.PLANNO
                            WHERE PLANKIND = '2'
                           ");

            if (!string.IsNullOrEmpty(model.PWS_YEAR))
            {
                sql.AppendLine(" AND pm.PLANYEAR = @PWS_YEAR");
            }
            if (model.IS_SEND.HasValue)
            {
                sql.AppendLine(" AND pm.IS_SEND = @IS_SEND");
            }
            if (model.SEND_STATUS.HasValue)
            {
                sql.AppendLine(" AND pm.SEND_STATUS = @SEND_STATUS");
            }
            if (model.AUDIT_STATUS.HasValue)
            {
                sql.AppendLine(" AND pv.AUDIT_STATUS = @AUDIT_STATUS");
            }
            var result = await ExecuteQueryAsync<RPTBudgeReviewModel>(sql.ToString(), model, PWSDBKey);
            return result.ToList();
        }

    }
}
