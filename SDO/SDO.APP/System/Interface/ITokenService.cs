using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface ITokenService
    {
        /// <summary>
        /// 讀取全部Token
        /// </summary>
        /// <returns></returns>
        Task<IList<TokenModel>> Read();

        /// <summary>
        /// 以token搜尋token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        TokenModel ReadByToken(string token);

        /// <summary>
        /// 以id搜尋token
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        Task<TokenModel> ReadById(string tokenId);

        /// <summary>
        /// 驗證LoginToken是否有效
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<TokenModel> AuthenticationToken(string token);

        /// <summary>
        /// 新增Tokne
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<RtnResultModel> InsertToken(TokenModel model);

        /// <summary>
        /// 產生Token
        /// </summary>
        /// <returns></returns>
        string TokenGenerator();

        /// <summary>
        /// 以UserId搜尋LoginToken(token_type = 'L')
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<TokenModel> ReadLoginToken(string userId);

        /// <summary>
        /// 更新Token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<RtnResultModel> UpdateToken(TokenModel model);

        /// <summary>
        /// 刪除Token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<RtnResultModel> DeleteToken(TokenModel model);
        Task<IList<TokenModel>> ReadByType(string tokenType);
        string HashTokenGenerator(UserDataModel user);
        HashTokenModel ParseHashToken(string hashToken);
        string GenJWTToken(SCUserModel user);
    }
}