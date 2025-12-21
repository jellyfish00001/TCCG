using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SDO.Dac;
using SDO.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using SDO.CryptSet;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using SDO.Utils;

namespace SDO.Services
{
    public class TokenService : Service, ITokenService
    {
        private readonly ITokenDac dac;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly int tokenExpireTime;
        private readonly IEncryptService encryptService;
        private readonly IAuthorizationKeyProvider authorizationKeyProvider;

        public TokenService(ITokenDac dac, 
            IHttpContextAccessor httpContextAccessor, 
            IEncryptService encryptService,
            IAuthorizationKeyProvider authorizationKeyProvider,
            IOptions<TokenSetting> tokenSettingOptions)
        {
            this.dac = dac;
            this.httpContextAccessor = httpContextAccessor;
            this.encryptService = encryptService;
            this.tokenExpireTime = tokenSettingOptions.Value.TokenExpireTime;
            this.authorizationKeyProvider = authorizationKeyProvider;
        }

        public async Task<IList<TokenModel>> Read()
        {
            return await dac.Read();
        }

        public TokenModel ReadByToken(string token)
        {
            return dac.ReadByToken(token);
        }

        public async Task<IList<TokenModel>> ReadByType(string tokenType)
        {
            return await dac.ReadByType(tokenType);
        }

        public async Task<TokenModel> ReadById(string tokenId)
        {
            return await dac.ReadById(tokenId);
        }

        public async Task<TokenModel> AuthenticationToken(string token)
        {
            return await dac.AuthenticationToken(token, httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString());
        }

        public async Task<RtnResultModel> InsertToken(TokenModel model)
        {
            //model.CRT_IP = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            //model.MDF_IP = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            await dac.Insert(model);
            return ChangeResult(ResultType.Success | ResultType.Insert);
        }

        public string TokenGenerator()
        {
            return Guid.NewGuid().ToString();
        }

        public string HashTokenGenerator(UserDataModel user)
        {
            StringBuilder hashToken = new StringBuilder();

            HashTokenModel payload = new HashTokenModel() {
                USER_ID = user.USER_ID,
                EXPIRE_DATE = Now.AddSeconds(tokenExpireTime),
                IP = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                HASH = Guid.NewGuid().ToString(),
                IsAgent = !string.IsNullOrWhiteSpace(user.AGENT_ID),
                AGENT_ID =  user.AGENT_ID 
            };

            string payloadJson = JsonSerialize(payload);//Model to String
            byte[] payloadBytes = StringToBytes(payloadJson);//String to Bytes
            string base64Payload = Convert.ToBase64String(payloadBytes);//Bytes to Base64String
            hashToken.Append(base64Payload + '.');

            VerifySignatureModel verifySignature = new VerifySignatureModel()
            {
                BASE64_PAYLOAD = base64Payload,
                SECRET = encryptService.SHA512(authorizationKeyProvider.AuthorizationKey).encryptedString//PrivateKey to SHA512String
            };

            string verifySignatureJson = JsonSerialize(verifySignature);//Model to String
            string verifySignatureSHA512 = encryptService.SHA512(verifySignatureJson).encryptedString;//String to SHA512String
            hashToken.Append(verifySignatureSHA512);//hashToken = Base64Paload + '.' + VerifySignature

            return hashToken.ToString();
        }

        public HashTokenModel ParseHashToken(string hashToken)
        {
            if (string.IsNullOrWhiteSpace(hashToken) && hashToken.Split('.').Length != 2)
                return null;

            string base64Payload = hashToken.Split('.')[0];
            string verifySignatureString = hashToken.Split('.')[1];

            VerifySignatureModel verifySignature = new VerifySignatureModel()
            {
                BASE64_PAYLOAD = base64Payload,
                SECRET = encryptService.SHA512(authorizationKeyProvider.AuthorizationKey).encryptedString
            };
            string verifySignatureJson = JsonSerialize(verifySignature);
            string verifySignatureSHA512 = encryptService.SHA512(verifySignatureJson).encryptedString;
            //檢驗是否遭異動
            if (verifySignatureString != verifySignatureSHA512)
                return null;

            string payloadJson = BytesToString(Convert.FromBase64String(base64Payload));
            HashTokenModel payload = JsonDeserialize<HashTokenModel>(payloadJson);
            if (payload.EXPIRE_DATE >= Now && //驗證有效期限
                payload.IP == httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString()) //驗證IP
                return payload;

            return null;
        }

        public async Task<TokenModel> ReadLoginToken(string userId)
        {
            return await dac.ReadLoginToken(userId, "L");
        }

        public async Task<RtnResultModel> UpdateToken(TokenModel model)
        {
            if(!model.TOKEN_ID.HasValue)
                return ChangeResult(false, string.Format(i18N.Message.R13, "TOKEN_ID"));

            //更新
            //model.CRT_IP = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            //model.MDF_IP = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            await dac.Update(model);
            return ChangeResult(ResultType.Success | ResultType.Update);
        }

        public async Task<RtnResultModel> DeleteToken(TokenModel model)
        {
            if (!model.TOKEN_ID.HasValue)
                return ChangeResult(false, string.Format(i18N.Message.R13, "TOKEN_ID"));

            //刪除
            //model.MDF_IP = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            await dac.Delete(model);
            return ChangeResult(ResultType.Success | ResultType.Delete);
        }

        public string GenJWTToken(SCUserModel user)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SymmetricSecurityKey key = new SymmetricSecurityKey(authorizationKeyProvider.AuthorizationKey);

            IList<Claim> userClaims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.USER_ID),
                new Claim(ClaimTypes.Role, "handUser") // fix Cx : Missing Object Level Authorization
            };

            if (!string.IsNullOrWhiteSpace(user.AGENT_ID))
            {
                userClaims.Add(new Claim("Agent", user.AGENT_ID));
            }

            JwtSecurityToken jwt = new JwtSecurityToken(
                  claims: userClaims,
                  notBefore: DateTime.UtcNow,
                  expires: DateTime.UtcNow.AddSeconds(tokenExpireTime), 
                  signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return tokenHandler.WriteToken(jwt);
        }
    }
}
