using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class EmpUserDac : Dac, IEmpUserDac
    {
        public EmpUserDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 以USER_ID搜尋
        /// </summary>
        /// <param name="id"></param>
        /// <param name="delFlg">是否停用</param>
        /// <returns></returns>
        public virtual async Task<UserDataModel> GetUserById(string id, bool delFlg = false)
        {
            string sql = @"
                SELECT 
                    empUser.USER_ID,
                    empUser.USER_NAME,
                    empUser.USER_EMAIL,
                    empUser.DEL_FLG,
                    empUser.LAST_SUCCLOGIN,
                    empUser.LAST_FAILLOGIN,
                    empUser.CNT_FAILLOGIN,
                    empUser.CON_FAULT,
                    empUser.NONCON_FAULT,
                    orgUser.ORG_ID
                FROM dbo.EMP_USER empUser (NOLOCK)
                LEFT JOIN dbo.MAP_ORG_USER orgUser (NOLOCK) ON empUser.USER_ID = orgUser.USER_ID
                WHERE empUser.DEL_FLG=?DEL_FLG? AND empUser.USER_ID=?USER_ID?";
            return (await ExecuteQueryAsync<UserDataModel>(sql, new { USER_ID = id, DEL_FLG = delFlg ? 1 : 0 })).FirstOrDefault();
        }
        public virtual async Task<IList<UserDataModel>> GetUserByIds(IEnumerable<string> Ids)
        {
            string sql = @"
                SELECT 
                    USER_ID,
                    USER_NAME,
                    USER_EMAIL,
                    DEL_FLG
                FROM dbo.EMP_USER (NOLOCK)
                WHERE DEL_FLG=0 AND USER_ID IN ?USER_ID?";
            return await ExecuteQueryAsync<UserDataModel>(sql, new { USER_ID = Ids });
        }

        public virtual async Task<IList<UserDataModel>> GetUserByOrg(string orgId = "")
        {
            string sql = @"
                SELECT 
                    EMP_USER.USER_ID,
                    USER_NAME,
                    USER_EMAIL,
                    DEL_FLG
                FROM dbo.EMP_USER (NOLOCK)
                INNER JOIN dbo.MAP_ORG_USER (NOLOCK) ON MAP_ORG_USER.USER_ID = EMP_USER.USER_ID
                WHERE EMP_USER.DEL_FLG = 0 AND MAP_ORG_USER.ORG_ID = ?ORG_ID?";
            return await ExecuteQueryAsync<UserDataModel>(sql, new { ORG_ID = orgId });
        }

        public virtual async Task UpdateLoginInfo(string userId, bool login, int conFault = 0, int nonconFault = 0, bool flag = false, string Table = "EMP_USER")
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@$"UPDATE {Table} SET");
            if (login)
            {
                sql.AppendLine($@"LAST_SUCCLOGIN = {DTNow},
                           CON_FAULT = 0" + (flag ? "," : ""));
                if (flag)
                {
                    sql.AppendLine("NONCON_FAULT = 0");
                }
            }
            else
            {
                sql.AppendLine($@"last_faillogin = {DTNow},
                           CNT_FAILLOGIN = CNT_FAILLOGIN + 1,");
                sql.AppendFormat(@"CON_FAULT = {0},
                           NONCON_FAULT = {1}",
                           flag ? "1" : "?CON_FAULT?",
                           flag ? "1" : "?NONCON_FAULT?");
                sql.AppendLine();
            }

            object obj = new
            {
                USER_ID = userId
            };
            if (!flag)
            {
                obj = new
                {
                    USER_ID = userId,
                    CON_FAULT = conFault + 1,
                    NONCON_FAULT = nonconFault + 1
                };
            }
            await ExecuteCommandAsync(sql.ToString(), obj);
        }
        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="Login"></param>
        /// <returns></returns>
        public virtual async Task<UserDataModel> Login(LoginModel Login)
        {
            string sql = @"
                SELECT USER_ID
                FROM EMP_USER (NOLOCK)
                WHERE USER_ID = ?USER_ID?
                AND dbo.fn_Decrypt(USER_PWD)= ?USER_PD?";
            return (await ExecuteQueryAsync<UserDataModel>(sql, Login)).FirstOrDefault();
        }

        public virtual async Task InsertLoginLog(LoginLogModel loginLogModel)
        {
            string sql = $@"
                INSERT INTO dbo.LOGIN_LOG( 
                        [USER_ID],
		                USER_IP,
		                LOG_TIME,
                        LOG_AP,
                        MSG_ID,
                        MSG_CONTENT,
                        MSG_DETAIL)
                VALUES (?USER_ID?,
		                ?USER_IP?,
		                {DTNow},
                        ?LOG_AP?,
                        ?MSG_ID?,
                        ?MSG_CONTENT?,
                        ?MSG_DETAIL?)";
            await ExecuteCommandAsync(sql, loginLogModel);
        }

        public virtual async Task<IList<UserDataModel>> Read(EmpUserReadModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"SELECT 
                                empUser.USER_ID,
                                empUser.USER_NAME,
                                empUser.USER_EMAIL,
                                empUser.DEL_FLG,
								empUser.LAST_SUCCLOGIN,
                                empOrg.ORG_NAME
                           FROM dbo.EMP_USER empUser (NOLOCK)
                           LEFT JOIN dbo.MAP_ORG_USER orgUser (NOLOCK)  on empUser.USER_ID = orgUser.USER_ID
                           LEFT JOIN VW_USER_ORG empOrg (NOLOCK) on orgUser.USER_ID = empOrg.USER_ID");
            if (!string.IsNullOrWhiteSpace(model.USER_ID))
                sql.AppendLine(@" AND empUser.USER_ID like '%' + ?USER_ID? + '%'");
            if (!string.IsNullOrWhiteSpace(model.USER_NAME))
                sql.AppendLine(@" AND empUser.USER_NAME like '%' + ?USER_NAME? + '%'");
            if (!string.IsNullOrWhiteSpace(model.ORG_ID))
                sql.AppendLine(@" AND orgUser.ORG_ID = ?ORG_ID?");
            if (model.DEL_FLG != 2)
                sql.AppendLine(@" AND empUser.DEL_FLG = ?DEL_FLG?");
            if ((!string.IsNullOrWhiteSpace(model.IS_LAST_SUCCLOGIN) && model.IS_LAST_SUCCLOGIN == "1"))
                sql.AppendLine(@" AND (empUser.LAST_SUCCLOGIN IS NULL OR DATEDIFF(day, empUser.LAST_SUCCLOGIN, GETDATE()) > 90)");

            return await ExecuteQueryAsync<UserDataModel>(
                sql.ToString(),
                new
                {
                    USER_ID = model.USER_ID,
                    USER_NAME = model.USER_NAME,
                    ORG_ID = model.ORG_ID,
                    DEL_FLG = model.DEL_FLG
                });
        }

        public virtual async Task Insert(EmpUserModel createModel)
        {
            string sql = $@"
                INSERT INTO dbo.EMP_USER( 
                        USER_ID,
                        USER_NAME,
                        USER_EMAIL,
                        CRT_DATE,
                        CRT_USER,
                        MDF_DATE,
                        MDF_USER) 
                VALUES( ?USER_ID?,
                        ?USER_NAME?,
                        ?USER_EMAIL?,
                        {DTNow},
                        ?CRT_USER?,
                        {DTNow},
                        ?MDF_USER?)";
            await ExecuteCommandAsync(sql, createModel);
        }

        public virtual async Task Update(EmpUserModel createModel)
        {
            string sql = $@"
                UPDATE dbo.EMP_USER 
                SET DEL_FLG=0,
                    USER_NAME=?USER_NAME?,
                    USER_EMAIL=?USER_EMAIL?,
                    MDF_DATE={DTNow},
                    MDF_USER=?MDF_USER?
                WHERE USER_ID=?USER_ID?";
            await ExecuteCommandAsync(sql, createModel);
        }

        public virtual async Task Delete(string userId, string del_reason)
        {
            string sql = $@"
                UPDATE dbo.EMP_USER 
                SET DEL_FLG=1
                    MDF_DATE={DTNow},
                    MDF_USER=?MDF_USER?
                WHERE USER_ID=?USER_ID?";
            await ExecuteCommandAsync(sql, new EmpUserModel() { USER_ID = userId });
        }


        public virtual async Task<UserDataModel> CheckExists(string userId, bool delFlg)
        {
            string sql = @"
                SELECT 
                    USER_ID,
                    USER_NAME,
                    USER_EMAIL,
                    DEL_FLG  
                FROM  EMP_USER (NOLOCK)
                WHERE USER_ID=?USER_ID?";
            return (
                await ExecuteQueryAsync<UserDataModel>(sql, new { USER_ID = userId, DEL_FLG = delFlg })).SingleOrDefault();
        }

        public virtual async Task UpdatePassword(string userId, string userPd)
        {
            string sql = @"
                UPDATE dbo.EMP_USER 
                SET USER_PWD= dbo.fn_Encrypt(?USER_PD?) 
                WHERE USER_ID=?USER_ID?";
            await ExecuteCommandAsync(sql, new { USER_ID = userId, USER_PD = userPd });
        }

        /// <summary>
        /// 取得使用者可代理的人員
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public virtual async Task<IList<SCUserModel>> ReadUserAgent(string userId)
        {
            string sql = $@"
                SELECT 
                    EMP_USER.USER_ID,
                    USER_NAME,
                    USER_EMAIL,
                    EMP_USER.DEL_FLG
                FROM EMP_USER (NOLOCK) EMP_USER
                INNER JOIN EMP_AGENT (NOLOCK) ON EMP_AGENT.USER_ID=EMP_USER.USER_ID
                WHERE EMP_USER.DEL_FLG=0 
                AND EMP_AGENT.DEL_FLG=0 
                AND EMP_AGENT.AGENT_ID=?USER_ID?
                AND {DTNow} BETWEEN EMP_AGENT.AGENT_FROM 
                AND DATEADD(DAY,1,EMP_AGENT.AGENT_TO )";
            return await ExecuteQueryAsync<SCUserModel>(sql, new { USER_ID = userId });
        }

        public virtual async Task DeleteMap(string userId)
        {
            string sql = @"
                DELETE FROM dbo.MAP_USER_ROLE 
                WHERE USER_ID = ?USER_ID?";
            await ExecuteCommandAsync(sql, new { USER_ID = userId });
        }

        public virtual async Task InsertMap(IEnumerable<MapUserRoleModel> maps)
        {
            string sql = $@"
                INSERT INTO dbo.MAP_USER_ROLE( 
                        USER_ID,
                        ROLE_ID,
                        CRT_DATE,
                        CRT_USER )
                VALUES( ?USER_ID?,
                        ?ROLE_ID?,
                        {DTNow},
                        ?CRT_USER? )";
            await ExecuteCommandAsync(sql, maps);
        }

        public virtual async Task DeleteMapOrgUser(string userId)
        {
            string sql = @"
                DELETE FROM dbo.MAP_ORG_USER 
                WHERE USER_ID = ?USER_ID?";
            await ExecuteCommandAsync(sql, new { USER_ID = userId });
        }

        public virtual async Task InsertMapOrgUser(string orgId, string userId, string crtUser)
        {
            string sql = $@"
                INSERT INTO dbo.MAP_ORG_USER (
                    ORG_ID, 
                    USER_ID, 
                    CRT_DATE, 
                    CRT_USER)
                VALUES(
                    ?ORG_ID?, 
                    ?USER_ID?,
                    {DTNow},
                    ?CRT_USER?)";
            await ExecuteCommandAsync(sql, new
            {
                ORG_ID = orgId,
                USER_ID = userId,
                CRT_USER = crtUser
            });
        }

        /// <summary>
        /// 讀取登入log
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<LoginLogModel>> GetLoginLog(string USER_ID)
        {
            string sql = @"
                SELECT TOP 10 USER_IP, LOG_TIME, MSG_CONTENT, MSG_DETAIL
                FROM LOGIN_LOG (NOLOCK)
                WHERE USER_ID = ?USER_ID?
                ORDER BY LOG_TIME DESC";
            return (await ExecuteQueryAsync<LoginLogModel>(sql, new { USER_ID = USER_ID })).ToList();
        }
    }
}
