using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface ITokenDac : IDac
    {
        Task<TokenModel> AuthenticationToken(string token, string mdfIp,string Sys="APL");
        Task Delete(TokenModel model);
        Task Insert(TokenModel model);
        Task<IList<TokenModel>> Read();
        Task<TokenModel> ReadById(string tokenId);
        TokenModel ReadByToken(string token);
        Task<IList<TokenModel>> ReadByType(string tokenType);
        Task<TokenModel> ReadLoginToken(string userId, string tokenType);
        Task Update(TokenModel model);
    }
}