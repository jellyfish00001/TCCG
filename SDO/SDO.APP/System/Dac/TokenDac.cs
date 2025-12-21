using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class TokenDac : Dac, ITokenDac
    {
        public TokenDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public async Task<IList<TokenModel>> Read()
        {
            string sql = @"SELECT 
                              TOKEN_ID
                              ,TOKEN
                              ,TOKEN_TYPE
                              ,USER_ID
                              ,EXPIRE_DATE
                              ,CRT_DATE
                              ,CRT_USER
                              ,MDF_DATE
                              ,MDF_USER
                          FROM TOKEN (NOLOCK) ";
            return await ExecuteQueryAsync<TokenModel>(sql);
        }

        public TokenModel ReadByToken(string token)
        {
            string sql = @"SELECT
                              TOKEN_ID
                              ,TOKEN
                              ,TOKEN_TYPE
                              ,USER_ID
                              ,EXPIRE_DATE
                              ,CRT_DATE
                              ,CRT_USER
                              ,MDF_DATE
                              ,MDF_USER
                          FROM TOKEN (NOLOCK)
                          WHERE TOKEN = ?TOKEN?";
            return (ExecuteQuery<TokenModel>(sql, new { TOKEN = token })).FirstOrDefault();
        }

        public async Task<IList<TokenModel>> ReadByType(string tokenType)
        {
            string sql = @"SELECT 
                               TOKEN_ID
                               ,TOKEN
                               ,TOKEN_TYPE
                               ,USER_ID
                               ,EXPIRE_DATE
                               ,CRT_DATE
                               ,CRT_USER
                               ,MDF_DATE
                               ,MDF_USER
                          FROM TOKEN (NOLOCK)
                          WHERE TOKEN_TYPE = ?TOKEN_TYPE?";
            return await ExecuteQueryAsync<TokenModel>(sql, new { TOKEN_TYPE = tokenType });
        }

        public async Task<TokenModel> ReadById(string tokenId)
        {
            string sql = @"SELECT 
                               TOKEN_ID
                               ,TOKEN
                               ,TOKEN_TYPE
                               ,USER_ID
                               ,EXPIRE_DATE
                               ,CRT_DATE
                               ,CRT_USER
                               ,MDF_DATE
                               ,MDF_USER
                          FROM TOKEN (NOLOCK)
                          WHERE TOKEN_ID = ?TOKEN_ID?";
            return (await ExecuteQueryAsync<TokenModel>(sql, new { TOKEN_ID = tokenId })).FirstOrDefault();
        }

        public async Task<TokenModel> AuthenticationToken(string token, string mdfIp,string Sys= "SDO")
        {
            string UsrJoin = " INNER JOIN EMP_USER U  (nolock) on T.USER_ID = U.USER_ID and U.DEL_FLG = 0 ";
            if(Sys == "WAPL")
                UsrJoin = " inner join WAPL_USER U  (nolock) on T.user_id = U.WID and U.IS_ENABLED = 'Y' ";
            if (Sys == "RVW")
                UsrJoin = " inner join RVW_USER U  (nolock) on T.user_id = U.RVWUSER_ID and U.DEL_FLG =0 ";
            string sql = $@"SELECT 
                                T.TOKEN_ID
                                ,T.TOKEN
                                ,T.TOKEN_TYPE
                                ,T.USER_ID
                                ,T.EXPIRE_DATE
                                ,T.CRT_DATE
                                ,T.CRT_USER
                                ,T.MDF_DATE
                                ,T.MDF_USER
                            FROM TOKEN T (nolock)
                            {UsrJoin}
                            WHERE TOKEN = ?TOKEN?
                             --   AND T.MDF_IP = ?MDF_IP? --比對IP
                             --   AND T.TOKEN_TYPE = 'L' --login token
                                AND T.DEL_FLG = 0  --未停用
                                AND T.EXPIRE_DATE > {DTNow}  --有效期限內";

            return (await ExecuteQueryAsync<TokenModel>(sql, new { TOKEN = token, MDF_IP = mdfIp }, trace:false)).FirstOrDefault();
        }

        public async Task<TokenModel> ReadLoginToken(string userId, string tokenType)
        {
            string sql = @"SELECT 
                              TOKEN_ID
                              ,TOKEN
                              ,TOKEN_TYPE
                              ,USER_ID
                              ,EXPIRE_DATE
                              ,CRT_DATE
                              ,CRT_USER
                              ,MDF_DATE
                              ,MDF_USER
                          FROM TOKEN (NOLOCK)
                          WHERE USER_ID = ?user_id? 
                          AND TOKEN_TYPE = ?token_type?";
            return (await ExecuteQueryAsync<TokenModel>(sql, new { user_id = userId, token_type = tokenType })).FirstOrDefault();
        }

        public async Task Insert(TokenModel model)
        {
            string sql = $@"INSERT INTO TOKEN
                                   (TOKEN
                                   ,TOKEN_TYPE
                                   ,USER_ID
                                   ,EXPIRE_DATE
                                   ,DEL_FLG
                                   ,CRT_DATE
                                   ,CRT_USER
                                   ,MDF_DATE
                                   ,MDF_USER)
                             VALUES
                                   (?TOKEN?
                                   ,?TOKEN_TYPE?
                                   ,?USER_ID?
                                   ,?EXPIRE_DATE?
                                   ,0
                                   ,{DTNow}
                                   ,?CRT_USER?
                                   ,{DTNow}
                                   ,?MDF_USER?)";
            await ExecuteCommandAsync(sql, model);
        }

        public async Task Update(TokenModel model)
        {
            string sql = $@"UPDATE TOKEN
                            SET 
                                TOKEN = ?TOKEN?
                                ,EXPIRE_DATE = ?EXPIRE_DATE?
                                ,MDF_DATE = {DTNow}
                                ,MDF_USER = ?MDF_USER?
                            WHERE TOKEN_ID = ?TOKEN_ID?";
            await ExecuteCommandAsync(sql, model);
        }

        public async Task Delete(TokenModel model)
        {
            string sql = $@"UPDATE TOKEN
                            SET 
                                DEL_FLG = 1
                                ,MDF_DATE = {DTNow}
                                ,MDF_USER = ?MDF_USER?
                            WHERE TOKEN_ID = ?TOKEN_ID?";
            await ExecuteCommandAsync(sql, model);
        }
    }
}
