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
    public class Sc30PolicyDac : Dac, IScPolicyDac
    {
        public Sc30PolicyDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public async Task<ScPolicyModel> GetPolicy(string policyId, string policyCompId)
        {
            string sql = @"
                SELECT 
                    [POLICY_ID]
                    ,[POLICY_COMP_ID]
                    ,[ID_MIN_LEN]
                    ,[PASS_MIN_LEN]
                    ,[PASS_NO_SAME_ID_NAME]
                    ,[PASS_NO_SPEC_CHAR]
                    ,[PASS_MIX_CHAR_NUM]
                    ,[PASS_NO_SAME_2]
                    ,[PASS_NO_CONT_3]
                    ,[PASS_NO_SAME_PAST_TIMES]
                    ,[PASS_AT_LEAST_SPECIAL_CHARS]
                    ,[ID_NO_CLOSE]
                    ,[ID_DISABLE_IN_CONT_TIMES]
                    ,[ID_DISABLE_IN_NONCONT_TIMES]
                FROM SCPOLICYM (NOLOCK)
                WHERE POLICY_ID = ?POLICY_ID? AND POLICY_COMP_ID = ?POLICY_COMP_ID?";
            return
                await ExecuteQueryFirstOrDefaultAsync<ScPolicyModel>(sql, new { POLICY_ID = policyId, POLICY_COMP_ID = policyCompId }, SCDBKey);
        }

        public async Task<ScPolicyModel> CheckExist(string policyId, string policyCompId)
        {
            string sql = @"
                SELECT POLICY_ID, POLICY_COMP_ID
                FROM   dbo.SCPOLICYM (NOLOCK)
                WHERE  POLICY_ID = ?POLICY_ID? 
                AND    POLICY_COMP_ID = ?POLICY_COMP_ID?";
            return (await ExecuteQueryAsync<ScPolicyModel>(sql, new { POLICY_ID = policyId, POLICY_COMP_ID = policyCompId }, SCDBKey)).FirstOrDefault();
        }

        public async Task Insert(ScPolicyModel model)
        {
            string sql = @"
                INSERT INTO dbo.SCPOLICYM(
                    POLICY_ID,
                    POLICY_COMP_ID,
                    ID_MIN_LEN,
                    PASS_MIN_LEN,
                    PASS_NO_SAME_ID_NAME,
                    PASS_NO_SPEC_CHAR,
                    PASS_MIX_CHAR_NUM,
                    PASS_NO_SAME_2,
                    PASS_NO_CONT_3,
                    PASS_NO_SAME_PASS_TIMES,
                    PASS_AT_LEAST_SPECIAL_CHARS,
                    ID_NO_CLOSE,                                        
                    ID_DISABLE_IN_CONT,
                    ID_DISABLE_IN_CONT_TIMES,
                    ID_DISABLE_IN_NONCONT,
                    ID_DISABLE_IN_NONCONT_TIMES
                )
                VALUES(
                    ?POLICY_ID?,
                    ?POLICY_COMP_ID?,
                    ?ID_MIN_LEN?,
                    ?PASS_MIN_LEN?,
                    ?PASS_NO_SAME_ID_NAME?,
                    ?PASS_NO_SPEC_CHAR?,
                    ?PASS_MIX_CHAR_NUM?,
                    ?PASS_NO_SAME_2?,
                    ?PASS_NO_CONT_3?,
                    ?PASS_NO_SAME_PASS_TIMES?,                                       
                    ?PASS_AT_LEAST_SPECIAL_CHARS?,
                    ?ID_NO_CLOSE?,                                               
                    ?ID_DISABLE_IN_CONT?,
                    ?ID_DISABLE_IN_CONT_TIMES?,
                    ?ID_DISABLE_IN_NONCONT?,
                    ?ID_DISABLE_IN_NONCONT_TIMES?)";
            await ExecuteCommandAsync(
                sql,
                new
                {
                    model.POLICY_ID,
                    model.POLICY_COMP_ID,
                    model.ID_MIN_LEN,
                    model.PASS_MIN_LEN,
                    model.PASS_NO_SAME_ID_NAME,
                    model.PASS_NO_SPEC_CHAR,
                    model.PASS_MIX_CHAR_NUM,
                    model.PASS_NO_SAME_2,
                    model.PASS_NO_CONT_3,
                    model.PASS_NO_SAME_PASS_TIMES,
                    model.PASS_AT_LEAST_SPECIAL_CHARS,
                    model.ID_NO_CLOSE,
                    model.ID_DISABLE_IN_CONT,
                    model.ID_DISABLE_IN_CONT_TIMES,
                    model.ID_DISABLE_IN_NONCONT,
                    model.ID_DISABLE_IN_NONCONT_TIMES
                }, SCDBKey);
        }

        public async Task Update(ScPolicyModel model)
        {
            string sql = $@"
                UPDATE dbo.SCPOLICYM 
                SET ID_MIN_LEN = ?ID_MIN_LEN?,
                    PASS_MIN_LEN = ?PASS_MIN_LEN?,
                    PASS_NO_SAME_ID_NAME = ?PASS_NO_SAME_ID_NAME?,
                    PASS_NO_SPEC_CHAR = ?PASS_NO_SPEC_CHAR?,
                    PASS_MIX_CHAR_NUM = ?PASS_MIX_CHAR_NUM?,
                    PASS_NO_SAME_2 = ?PASS_NO_SAME_2?,
                    PASS_NO_CONT_3 = ?PASS_NO_CONT_3?,
                    PASS_NO_SAME_PASS_TIMES = ?PASS_NO_SAME_PASS_TIMES?,
                    PASS_AT_LEAST_SPECIAL_CHARS = ?PASS_AT_LEAST_SPECIAL_CHARS?,
                    ID_NO_CLOSE = ?ID_NO_CLOSE?,                                      
                    ID_DISABLE_IN_CONT = ?ID_DISABLE_IN_CONT?,
                    ID_DISABLE_IN_CONT_TIMES = ?ID_DISABLE_IN_CONT_TIMES?,
                    ID_DISABLE_IN_NONCONT = ?ID_DISABLE_IN_NONCONT?,
                    ID_DISABLE_IN_NONCONT_TIMES = ?ID_DISABLE_IN_NONCONT_TIMES?,
                    MDF_DATE = {DTNow},
                    MDF_USER = ?LOGGED_USER?
                WHERE POLICY_ID = ?POLICY_ID?
                AND POLICY_COMP_ID = ?POLICY_COMP_ID?";
            await ExecuteCommandAsync(
                sql,
                new
                {
                    model.POLICY_ID,
                    model.POLICY_COMP_ID,
                    model.ID_MIN_LEN,
                    model.PASS_MIN_LEN,
                    model.PASS_NO_SAME_ID_NAME,
                    model.PASS_NO_SPEC_CHAR,
                    model.PASS_MIX_CHAR_NUM,
                    model.PASS_NO_SAME_2,
                    model.PASS_NO_CONT_3,
                    model.PASS_NO_SAME_PASS_TIMES,
                    model.PASS_AT_LEAST_SPECIAL_CHARS,
                    model.ID_NO_CLOSE,
                    model.ID_DISABLE_IN_CONT,
                    model.ID_DISABLE_IN_CONT_TIMES,
                    model.ID_DISABLE_IN_NONCONT,
                    model.ID_DISABLE_IN_NONCONT_TIMES,
                    LOGGED_USER = model.MDF_USER
                }, SCDBKey);
        }
        /// <summary>
        /// 檢查密碼是否與上次一致一致
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="UID"></param>
        /// <param name="Code"></param>
        /// <returns></returns>
        public async Task<bool> CheckPassSame(string UID, string Code)
        {
            string sql = $" SELECT top 1 USR_ID FROM SCUSERM (NOLOCK) WHERE USR_ID=?UID?" +
                $" And (PASSWORD = '#ENC5#'+replace(sys.fn_varbintohexstr(hashbytes('MD5',USR_ID+USR_COMP_ID+?Code?)),'0x','') )";
            var result = await ExecuteQueryAsync<string>(sql, new { UID, Code }, SCDBKey);
            return result.Any();
        }
    }
}
