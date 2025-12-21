using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class ListExecDac : Dac, IListExecDac
    {
        public ListExecDac(
            IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 查詢先期計畫資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ListExecModel>> GetPWSProjectList(ListExecQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"
                           SELECT 
                                PM.PLANNO,
                                PM.OU_ID,
                                dbo.FN_GetOuName(PM.OU_ID, 3) AS OU_NAME, --主管機關名稱
                                PM.IS_SEND,
                                PM.PLANYEAR,
                                PM.PLANNO,
                                PM.PLANKIND,
                                SP.SET_VALUE AS PLANKINDNAME,
                                SP2.SET_VALUE AS SEND_TYPE,
                                PM.PLANNAME,
                                PM.CREATEUNITOUID,
                                PM.CREATEORGOUID,
                                dbo.FN_GetOuName(PM.CREATEUNITOUID, 3) AS UNITOUNAME, --提報單位名稱
                                dbo.FN_GetOuName(PM.CREATEORGOUID, 3) AS ORGOUNAME --提報機關名稱
                            FROM 
                                PWSSDPLANMAIN PM (NOLOCK)
                            LEFT JOIN 
                                SET_PARAM SP ON PM.PLANKIND = SP.SET_TYPE AND SP.SET_ITEM = 'PLAN_KIND'
                            LEFT JOIN 
                                SET_PARAM SP2 ON PM.IS_SEND = SP2.SET_TYPE AND SP2.SET_ITEM = 'PROJECT_STATUS'
                            WHERE 
                                1=1
                            ");

            if (model.PLANYEAR > 0)
            {
                sql.AppendLine(" AND PLANYEAR = @PLANYEAR");
            }
            // 模糊分段查詢部分
            sql.AppendLine(FuzzySearch(model.PLANNO, "PLANNO"));
            sql.AppendLine(FuzzySearch(model.PLANNAME, "PLANNAME"));
            sql.AppendLine(FuzzySearch(model.UNITOUNAME, "dbo.FN_GetOuName(CREATEUNITOUID, 3)"));
            sql.AppendLine(FuzzySearch(model.ORGOUNAME, "dbo.FN_GetOuName(CREATEORGOUID, 3)"));

            if (!string.IsNullOrEmpty(model.PLANKIND))
            {
                sql.AppendLine(" AND PLANKIND = @PLANKIND");
            }
            if(!string.IsNullOrEmpty(model.OU_ID))
            {
                sql.AppendLine(" AND OU_ID = @OU_ID");
            }
            if (!string.IsNullOrEmpty(model.CREATEORGOUID))
            {
                sql.AppendLine(" AND CREATEORGOUID = @CREATEORGOUID");
            }
            var result = (await ExecuteQueryAsync<ListExecModel>(sql.ToString(), model, PWSDBKey)).ToList();
            return result;
        }

        /// <summary>
        /// 查詢機關是否截止
        /// </summary>
        /// <param name="OU_ID"></param>
        /// <returns></returns>
        public async Task<bool> GetOrgDeadline(string OU_ID)
        {
            string sql = @"
                            SELECT 
                                   HANDDATEEND
                              FROM PWSSDASSIGNMENT (NOLOCK)
                              WHERE OU_ID = @OU_ID 
                              AND PLANYEAR = YEAR(GETDATE()) - 1911
                              AND HANDDATEEND > GETDATE()
                           ";
            var result = await ExecuteQueryFirstOrDefaultAsync<string>(sql, new { OU_ID }, PWSDBKey);
            return result == null;
        }

        /// <summary>
        /// 刪除計畫主檔
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task DeletePWSSDPLANMAIN(List<string> PLANNO)
        {
            string sql = @"DELETE FROM PWSSDPLANMAIN WHERE PLANNO = @PLANNO";
            await ExecuteCommandAsync(sql, new { PLANNO }, PWSDBKey);
        }
        /// <summary>
        /// 刪除跨年度預算
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task DeletePROJECT_BUDGET_SOURCE_G(List<string> PLANNO)
        {
            string sql = @"DELETE FROM PROJECT_BUDGET_SOURCE_G WHERE PLANNO = @PLANNO";
            await ExecuteCommandAsync(sql, new { PLANNO }, PWSDBKey);
        }
        /// <summary>
        /// 刪除歷年執行情形
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task DeletePWSSDHISTORYEXE(List<string> PLANNO)
        {
            string sql = @"DELETE FROM PWSSDHISTORYEXE WHERE PLANNO = @PLANNO";
            await ExecuteCommandAsync(sql, new { PLANNO }, PWSDBKey);
        }
        /// <summary>
        /// 刪除近三年相關研究
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task DeletePWSSDTREEYEARPLAN(List<string> PLANNO)
        {
            string sql = @"DELETE FROM PWSSDTREEYEARPLAN WHERE PLANNO = @PLANNO";
            await ExecuteCommandAsync(sql, new { PLANNO }, PWSDBKey);
        }
        /// <summary>
        /// 刪除經費需求細項
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task DeletePWSSDPLANFUND(List<string> PLANNO)
        {
            string sql = @"DELETE FROM PWSSDPLANFUND WHERE PLANNO = @PLANNO";
            await ExecuteCommandAsync(sql, new { PLANNO }, PWSDBKey);
        }
        /// <summary>
        /// 刪除小組審核紀錄
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task DeletePWSSDVIEW(List<string> PLANNO)
        {
            string sql = @"DELETE FROM PWSSDVIEW WHERE PLANNO = @PLANNO";
            await ExecuteCommandAsync(sql, new { PLANNO }, PWSDBKey);
        }
        /// <summary>
        /// 刪除檔案上傳
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task DeletePROJECT_ATTACHMENT(List<string> PLANNO)
        {
            string sql = @"DELETE FROM PROJECT_ATTACHMENT WHERE PROJECT_NO = @PLANNO";
            await ExecuteCommandAsync(sql, new { PLANNO }, PWSDBKey);
        }


    }
}
