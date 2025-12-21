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
    public class ScPolicyDac : Dac, IScPolicyDac
    {
        public ScPolicyDac(IConnectionControlCenter connectionControlCenter,
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
                WHERE POLICY_ID = @POLICY_ID AND POLICY_COMP_ID = @POLICY_COMP_ID";
            return (await ExecuteQueryAsync<ScPolicyModel>(sql, new { POLICY_ID = policyId, POLICY_COMP_ID = policyCompId })).FirstOrDefault();
        }

        public async Task<ScPolicyModel> CheckExist(string policyId, string policyCompId)
        {
            string sql = @"
                SELECT POLICY_ID, POLICY_COMP_ID
                FROM   dbo.SCPOLICYM (NOLOCK)
                WHERE  POLICY_ID = @POLICY_ID 
                AND    POLICY_COMP_ID = @POLICY_COMP_ID";
            return (await ExecuteQueryAsync<ScPolicyModel>(sql, new { POLICY_ID = policyId, POLICY_COMP_ID = policyCompId })).FirstOrDefault();
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
                    ?policy_id?,
                    ?policy_comp_id?,
                    ?id_min_len?,
                    ?pass_min_len?,
                    ?pass_no_same_id_name?,
                    ?pass_no_spec_char?,
                    ?pass_mix_char_num?,
                    ?pass_no_same_2?,
                    ?pass_no_cont_3?,
                    ?pass_no_same_pass_times?,                                       
                    ?pass_at_least_special_chars?,
                    ?id_no_close?,                                               
                    ?id_disable_in_cont?,
                    ?id_disable_in_cont_times?,
                    ?id_disable_in_noncont?,
                    ?id_disable_in_noncont_times?)";
            await ExecuteCommandAsync(
                sql,
                new
                {
                    policy_id = model.POLICY_ID,
                    policy_comp_id = model.POLICY_COMP_ID,
                    id_min_len = model.ID_MIN_LEN,
                    pass_min_len = model.PASS_MIN_LEN,
                    pass_no_same_id_name = model.PASS_NO_SAME_ID_NAME,
                    pass_no_spec_char = model.PASS_NO_SPEC_CHAR,
                    pass_mix_char_num = model.PASS_MIX_CHAR_NUM,
                    pass_no_same_2 = model.PASS_NO_SAME_2,
                    pass_no_cont_3 = model.PASS_NO_CONT_3,
                    pass_no_same_pass_times = model.PASS_NO_SAME_PASS_TIMES,
                    pass_at_least_special_chars = model.PASS_AT_LEAST_SPECIAL_CHARS,
                    id_no_close = model.ID_NO_CLOSE,
                    id_disable_in_cont = model.ID_DISABLE_IN_CONT,
                    id_disable_in_cont_times = model.ID_DISABLE_IN_CONT_TIMES,
                    id_disable_in_noncont = model.ID_DISABLE_IN_NONCONT,
                    id_disable_in_noncont_times = model.ID_DISABLE_IN_NONCONT_TIMES
                });
        }

        public async Task Update(ScPolicyModel model)
        {
            string sql = $@"
                UPDATE dbo.SCPOLICYM 
                SET ID_MIN_LEN = ?id_min_len?,
                    PASS_MIN_LEN = ?pass_min_len?,
                    PASS_NO_SAME_ID_NAME = ?pass_no_same_id_name?,
                    PASS_NO_SPEC_CHAR = ?pass_no_spec_char?,
                    PASS_MIX_CHAR_NUM = ?pass_mix_char_num?,
                    PASS_NO_SAME_2 = ?pass_no_same_2?,
                    PASS_NO_CONT_3 = ?pass_no_cont_3?,
                    PASS_NO_SAME_PASS_TIMES = ?pass_no_same_pass_times?,
                    PASS_AT_LEAST_SPECIAL_CHARS = ?pass_at_least_special_chars?,
                    ID_NO_CLOSE = ?id_no_close?,                                      
                    ID_DISABLE_IN_CONT = ?id_disable_in_cont?,
                    ID_DISABLE_IN_CONT_TIMES = ?id_disable_in_cont_times?,
                    ID_DISABLE_IN_NONCONT = ?id_disable_in_noncont?,
                    ID_DISABLE_IN_NONCONT_TIMES = ?id_disable_in_noncont_times?,
                    MDF_DATE = {DTNow},
                    MDF_USER = ?logged_user?
                WHERE POLICY_ID = ?policy_id?
                AND POLICY_COMP_ID = ?policy_comp_id?";
            await ExecuteCommandAsync(
                sql,
                new
                {
                    policy_id = model.POLICY_ID,
                    policy_comp_id = model.POLICY_COMP_ID,
                    id_min_len = model.ID_MIN_LEN,
                    pass_min_len = model.PASS_MIN_LEN,
                    pass_no_same_id_name = model.PASS_NO_SAME_ID_NAME,
                    pass_no_spec_char = model.PASS_NO_SPEC_CHAR,
                    pass_mix_char_num = model.PASS_MIX_CHAR_NUM,
                    pass_no_same_2 = model.PASS_NO_SAME_2,
                    pass_no_cont_3 = model.PASS_NO_CONT_3,
                    pass_no_same_pass_times = model.PASS_NO_SAME_PASS_TIMES,
                    pass_at_least_special_chars = model.PASS_AT_LEAST_SPECIAL_CHARS,
                    id_no_close = model.ID_NO_CLOSE,
                    id_disable_in_cont = model.ID_DISABLE_IN_CONT,
                    id_disable_in_cont_times = model.ID_DISABLE_IN_CONT_TIMES,
                    id_disable_in_noncont = model.ID_DISABLE_IN_NONCONT,
                    id_disable_in_noncont_times = model.ID_DISABLE_IN_NONCONT_TIMES,
                    logged_user = model.MDF_USER
                });
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
            string sql = $" SELECT top 1 USER_ID FROM EMPUSER (NOLOCK) WHERE USER_ID=?UID? AND dbo.fn_Decrypt(USER_PWD)=?Code?";
            var result = await ExecuteQueryAsync<string>(sql, new { UID, Code });
            return result.Any();
        }
    }
}
