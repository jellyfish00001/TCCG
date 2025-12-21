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
    public class SCUserDac : Dac, ISCUserDac
    {
        public SCUserDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 由帳號取得使用者資料
        /// </summary>
        /// <param name="id"></param>
        /// <param name="delFlg"></param>
        /// <returns></returns>
        public async Task<SCUserModel> GetUserById(string id, bool delFlg = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetSelectUserSQL(false));

            if (!string.IsNullOrEmpty(id))
            {
                sb.Append(" AND A.USR_ID = @USR_ID ");
            }
            return (await ExecuteQueryAsync<SCUserModel>(sb.ToString(), new { USR_ID = id }, SCDBKey)).FirstOrDefault();
        }

        /// <summary>
        /// 多筆帳號取得使用者資料
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<IList<SCUser>> GetUserByIds(IEnumerable<string> ids)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetSelectUserSQL(false));

            if (ids.Any())
            {
                sb.Append(" AND A.USR_ID in @USR_ID ");
            }

            var objParam = new
            {
                USR_ID = ids,
            };
            return (await ExecuteQueryAsync<SCUser>(sb.ToString(), objParam, SCDBKey)).ToList();
        }

        /// <summary>
        /// 取得機關下的使用者帳號
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public async Task<IList<SCUser>> GetUserByOrg(string orgId = "")
        {
            string sql = @"
                    SELECT ORG.USR_ID AS UsrID,ORG.USR_NAME AS UsrName 
                     From SCREL_ORG_USRMV ORG 
                       inner  JOIN SCREL_ROL_USRMV ROL ON ORG.USR_ID = ROL.USR_ID 
                       WHERE OU_ID = @OU_ID AND ROL_ID = @ROL_ID
                       AND ROL.ID_ENABLE = 'Y' 
                       AND ROL.REL_KIND = '1' AND ROL.LINK_KIND = '1' 
                       AND ORG.REL_KIND = '1'";

            return await ExecuteQueryAsync<SCUser>(
                sql,
                new
                {
                    ORG_ID = orgId
                }, SCDBKey);
        }

        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="login"></param>
        /// <param name="isSSOLogin">是否是單一入口登入</param>
        /// <returns></returns>
        public async Task<SCUserModel> Login(LoginModel login, bool isSSOLogin = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetSelectUserSQL(false));

            if (login != null)
            {
                sb.Append(" AND A.USR_ID = @USER_ID ");
                if (isSSOLogin)
                {
                    sb.Append(" AND A.PASSWORD = @USER_PD ");
                }
                else
                {
                    sb.Append(" AND (A.PASSWORD = '#ENC5#'+ replace(sys.fn_varbintohexstr(hashbytes('MD5', A.USR_ID + A.USR_COMP_ID + @USER_PD)), '0x', '')) ");
                }
            }
            return (await ExecuteQueryAsync<SCUserModel>(sb.ToString(), login, SCDBKey)).FirstOrDefault();
        }

        public async Task<IList<SCUser>> Read(EmpUserReadModel model)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetSelectUserSQL(false));
            if (!string.IsNullOrWhiteSpace(model.USER_ID))
            {
                sb.Append(" AND A.USR_ID = @USER_ID ");
            }
            if (!string.IsNullOrWhiteSpace(model.USER_NAME))
            {
                sb.Append(" AND A.USR_NAME = @USER_NAME ");
            }
            if (!string.IsNullOrWhiteSpace(model.ORG_ID))
            {
                sb.Append(" AND C.OU_ID = @ORG_ID ");
            }
            if (!string.IsNullOrWhiteSpace(model.USER_EMAIL))
            {
                sb.Append(" AND A.USR_EMAIL = @USER_EMAIL ");
            }
            if (!string.IsNullOrWhiteSpace(model.IS_LAST_SUCCLOGIN) && model.IS_LAST_SUCCLOGIN == "1")
            {
                sb.Append(" AND (A.LAST_SUCCLOGIN is null or DATEDIFF(day, A.LAST_SUCCLOGIN, GETDATE()) > 90) ");
            }
            if (model.DEL_FLG == 1)
            {
                sb.Append(" AND (A.ID_ENABLE='Y') ");
            }
            return await ExecuteQueryAsync<SCUser>(
                sb.ToString(),
                new
                {
                    USER_ID = model.USER_ID,
                    USER_NAME = model.USER_NAME,
                    ORG_ID = model.ORG_ID,
                    USER_EMAIL = model.USER_EMAIL,
                    DEL_FLG = model.DEL_FLG,
                    DAYCOUNT = model.DAY_COUNT ?? 0
                }, SCDBKey);
        }

        public async Task<bool> CheckExists(string userId, bool delFlg)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetSelectUserSQL(false));
            sb.Append(" AND A.USR_ID = @USER_ID ");
            if (delFlg)
                sb.Append(" AND A.ID_ENABLE != 'Y' ");
            else
                sb.Append(" AND A.ID_ENABLE = 'Y' ");
            return (
                await ExecuteQueryAsync<SCUser>(
                    sb.ToString(),
                    new
                    {
                        USER_ID = userId,
                        DEL_FLG = delFlg
                    }, SCDBKey)
                ).Any();
        }

        /// <summary>
        /// 更新密碼
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userPd"></param>
        /// <param name="needChanPswd"></param>
        /// <returns></returns>
        public async Task UpdatePassword(string userId, string userPd, string needChanPswd = "N")
        {
            string sql = @"insert into SCPSWD_HISM (USR_ID, USR_COMP_ID, PASSWORD, CHANPSWD_DATE)
                            select USR_ID, USR_COMP_ID, PASSWORD, getdate() CHANPSWD_DATE from SCUSERM where USR_ID = @USR_ID
                            ;
                            update SCUSERM
                            set 
                                PASSWORD = '#ENC5#' + replace(sys.fn_varbintohexstr(hashbytes('MD5', USR_ID + USR_COMP_ID + @USER_PD)), '0x', ''), 
                                NEED_CHANPSWD = @NEED_CHANPSWD, 
                                LAST_CHANPSWD = getdate()
                            where USR_ID = @USR_ID";
            var objParam = new
            {
                USR_ID = userId,
                USER_PD = userPd,
                NEED_CHANPSWD = needChanPswd
            };
            await ExecuteCommandAsync(sql, objParam, SCDBKey);
        }

        /// <summary>
        /// 取得查詢 userinfo的 SQL字串
        /// </summary>
        /// <param name="isSimpleOuName">機關名稱呈現，isSimpleOuName => true：三個字，false：完整</param>
        /// <returns></returns>
        protected virtual string GetSelectUserSQL(bool isSimpleOuName)
        {
            string sqlUserQuery = @"
                      SELECT distinct A.USR_ID  UsrID
                            ,A.USR_COMP_ID UsrCompID
                            ,A.USR_NAME UsrName
                            ,A.USR_TITLE UsrTitle
                            ,A.USR_TYPE UsrType
                            ,A.USR_DESC UsrDesc
                            ,A.USR_GRADE UsrGrade
                            ,A.ID_ENABLE IDEnable
                            ,isnull(A.NEED_CHANPSWD, 'N') NeedChanpswd
                            ,A.CUR_LOCATION CurLocation
                            ,A.SDATE SDate 
                            ,A.EDATE EDate
                            ,A.LAST_CHANPSWD 
                            ,A.LAST_SUCCLOGIN 
                            ,A.LAST_FAILLOGIN 
                            ,A.CNT_FAILLOGIN 
                            ,A.CON_FAULT 
                            ,A.NONCON_FAULT 
                            ,A.COUNTER 
                            ,A.PSWD_ALLRIGHT PswdAllright
                            ,A.EMP_ID EMPID
                            ,A.USR_NICKNAME UsrNickname
                            ,A.USR_EMAIL UsrEmail 
                            ,A.USR_CUSTOM1 UsrCustiom1
                            ,A.USR_CUSTOM2 UsrCustiom2
                            ,A.USR_CUSTOM3 UsrCustiom3
                            ,A.PSWD_Q as LIMIT_LOCATION 
                            ,B.OU_KIND OuKind
                            ,C.OU_ID AS OrgOuID --主管機關ID
                            ,CASE WHEN IsNull(D.OU_ID,'')='' THEN C.OU_ID ELSE D.OU_ID END AS UnitOuID --主辦機關ID
                            ,B.OU_ID OuID --主辦單位ID
                            ,B.OU_NAME ORG_NAME
                            [OU_NAME_AREA]
                            FROM SCUSERM A (NOLOCK)
                            JOIN SCREL_ORG_USRMV B (NOLOCK) ON A.USR_ID=B.USR_ID  
                                                        AND B.REL_KIND = '1'--避免抓到附屬組織
                                                        AND B.LINK_KIND = '1'
                            LEFT JOIN (
	                            select F_GROUP_ID,OU_ID,OU_NAME from GPREL_GRP_GRPM GRP (NOLOCK)
	                            JOIN SCORG_UNITM UNITM (NOLOCK) ON GRP.T_GROUP_ID=UNITM.OU_ID And LEFT(UNITM.OU_KIND,1)='1' AND OU_ID <> 'GSSROOT'
                            ) C ON B.OU_ID=C.F_GROUP_ID
                            LEFT JOIN (
	                            select F_GROUP_ID,OU_ID,OU_NAME from GPREL_GRP_GRPM GRP (NOLOCK)
	                            JOIN SCORG_UNITM UNITM (NOLOCK) ON GRP.T_GROUP_ID=UNITM.OU_ID And UNITM.OU_KIND='2'
                            ) D ON B.OU_ID=D.F_GROUP_ID
                            where 1=1 ";
            sqlUserQuery = sqlUserQuery.Replace("[OU_NAME_AREA]", isSimpleOuName
                ? @" ,dbo.FN_GetOuName(C.OU_ID, 3) AS OrgOuName
                     ,dbo.FN_GetOuName((CASE WHEN IsNull(D.OU_ID,'')='' THEN C.OU_ID ELSE D.OU_ID END), 3) AS UnitOuName
                     ,dbo.FN_GetOuName(B.OU_ID, 3) AS OuName "
                : @" ,C.OU_NAME AS OrgOuName
                     ,CASE WHEN IsNull(D.OU_NAME,'')='' THEN C.OU_NAME ELSE D.OU_NAME END AS UnitOuName
                     ,B.OU_NAME OuName ");
            return sqlUserQuery;
        }

        public virtual async Task UpdateLoginInfo(string userId, bool login, int conFault = 0, int nonconFault = 0, bool flag = false)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("UPDATE SCUSERM SET");
            if (login)
            {
                sql.AppendLine("last_succlogin = DATEADD(HH,8,GETUTCDATE()),");
                sql.AppendLine($"con_fault = 0{(flag ? "," : string.Empty)}");
                if (flag)
                {
                    sql.AppendLine("noncon_fault = 0");
                }
            }
            else
            {
                sql.AppendLine("last_faillogin = DATEADD(HH,8,GETUTCDATE()),");
                sql.AppendLine("cnt_faillogin = cnt_faillogin + 1,");
                sql.AppendFormat("con_fault = {0}, noncon_fault = {1}",
                           flag ? "1" : "?CON_FAULT?",
                           flag ? "1" : "?NONCON_FAULT?");
            }

            sql.AppendLine("WHERE USR_ID = @USER_ID");

            object obj = new
            {
                USER_ID = userId,
                CON_FAULT = conFault + 1,
                NONCON_FAULT = nonconFault + 1
            };
            await ExecuteCommandAsync(sql.ToString().ToUpper(), obj, SCDBKey);
        }

        /// <summary>
        /// 取得SC密碼規則
        /// </summary>
        /// <returns></returns>
        public async Task<SCPolicyModel> GetSCPolicy()
        {
            string sql = @"select 
	                            POLICY_ID,
	                            ID_MIN_LEN,
	                            PASS_MIN_LEN,
	                            PASS_NO_SAME_ID_NAME,
	                            PASS_MIX_CHAR_NUM,
	                            PASS_NO_SPEC_CHAR,
	                            PASS_AT_LEAST_SPECIAL_CHARS,
	                            PASS_NO_SAME_2,
	                            PASS_NO_CONT_3,
	                            PASS_NO_SAME_PAST_TIMES,
	                            PASS_CHANGE_IN_DAYS
                            from SCPOLICYM where POLICY_ID = 'GSS'";
            return (await ExecuteQueryAsync<SCPolicyModel>(sql, DB: SCDBKey)).FirstOrDefault();
        }

        /// <summary>
        /// 檢查帳戶是否存在
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public async Task<bool> CheckExists(string userId, string userName, string userEmail)
        {
            string sql = @"select 
	                            USR_ID 
                            from SCUSERM (nolock)
                            where USR_ID = @USR_ID
	                            and USR_NAME = @USR_NAME
	                            and USR_EMAIL = @USR_EMAIL";

            object param = new
            {
                USR_ID = userId,
                USR_NAME = userName,
                USR_EMAIL = userEmail
            };

            return (await ExecuteQueryAsync<string>(sql, param: param, DB: SCDBKey)).FirstOrDefault() != null;
        }

        /// <summary>
        /// 取得密碼變更記錄檔清單
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="topCnt">筆數</param>
        /// <returns></returns>
        public async Task<IList<SCPswdHismModel>> GetPswdHishs(string userId, int topCnt)
        {
            string sql = $@"select top {topCnt}
	                            USR_ID,
	                            PASSWORD,
	                            CHANPSWD_DATE
                            from SCPSWD_HISM 
                            where USR_COMP_ID = 'GSS'
	                            and USR_ID = @userId";
            return await ExecuteQueryAsync<SCPswdHismModel>(sql, new { userId }, DB: SCDBKey);
        }

        /// <summary>
        /// 檢查 token 是否有對應到 SCSESSIONM.SESSION_ID
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<string> CheckToken(string token)
        {
            string sql = @"
                SELECT USR_ID
                FROM SCSESSIONM
                WHERE AP_ID = 'IPC3'
                AND SESSION_ID = @SESSION_ID 

               DELETE SCSESSIONM
               WHERE AP_ID = 'IPC3'
               AND SESSION_ID = @SESSION_ID ";
            return (await ExecuteQueryAsync<string>(sql, new { SESSION_ID = token }, DB: SCDBKey)).FirstOrDefault();
        }
    }
}
