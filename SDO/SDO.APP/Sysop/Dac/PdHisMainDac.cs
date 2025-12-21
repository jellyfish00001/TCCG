using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class PdHisMainDac : Dac, IPdHisMainDac
    {
        public PdHisMainDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public async Task<IList<ScPswdHismModel>> ReadById(string userId)
        {
            string sql = @"
                SELECT
                    [USER_ID]
                    ,[PSWD]
                    ,[CRT_DATE]
                FROM[dbo].[SCPSWD_HISM](NOLOCK)
                WHERE USER_ID = ?user_id? ";
            return await ExecuteQueryAsync<ScPswdHismModel>(sql, new { user_id = userId });
        }

        public async Task<bool> CheckPdSamePassTime(string userId, string userPd, int sameTime, string TABLE)
        {
            string UserCol = "USER_ID";
            if (TABLE == "WAPL_USER")
                UserCol = "WID";
            if (TABLE == "RVW_USER")
                UserCol = "RVWUSER_ID";

            string sql = $@"
                SELECT 
	                CASE COUNT(*) WHEN @sameTime THEN 1
	                ELSE 0 END
                FROM(
                    SELECT TOP @sameTime
                        sc.PSWD
                        ,sc.CRT_DATE
                    FROM SCPSWD_HISM AS sc (NOLOCK) INNER JOIN @TABLE AS emp (NOLOCK)
                        ON sc.USER_ID = emp.{UserCol}
                    WHERE sc.USER_ID = @user_id
	                and dbo.fn_Decrypt(PSWD)= @user_pd  
                    ORDER BY CRT_DATE DESC
                ) AS pdHis";
            return (await ExecuteQueryAsync<bool>(sql, 
                new 
                { 
                    user_id = userId, 
                    user_pd = userPd, 
                    sameTime, 
                    TABLE
                })).Single();
        }

        public async Task Create(string userId, string userPd)
        {
            string sql = $@"
                INSERT INTO SCPSWD_HISM (
                    USER_ID, 
                    PSWD, 
                    CRT_DATE)
                VALUES (
                    ?user_id?,
                    dbo.fn_Encrypt( ?user_pd? ),
                    {DTNow} 
                )";
            await ExecuteCommandAsync(sql, new { user_id = userId, user_pd = userPd });
        }
    }
}
