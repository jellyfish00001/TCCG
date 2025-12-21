using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using SDO.Dac;
using SDO.Models;
using SDO.Services;

namespace SDO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "handUser")]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService tokenService;

        public TokenController(ITokenService tokenService)
        {
            this.tokenService = tokenService;
        }

        /// <summary>
        /// 取得全部token資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IList<TokenModel>> GetToken()
        {
            return await tokenService.Read();
        }

        /// <summary>
        /// 取得token By tokenId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<TokenModel> GetTokenById(string id)
        {
            return await tokenService.ReadById(id);
        }

        /// <summary>
        /// 取得token By tokenId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("[action]/{tokenType}")]
        public async Task<IList<TokenModel>> GetTokenByType(string tokenType)
        {
            return await tokenService.ReadByType(tokenType);
        }

        /// <summary>
        /// 新增token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<RtnResultModel> InsertToken(TokenModel model)
        {
            return await tokenService.InsertToken(model);
        }

        /// <summary>
        /// 更新token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<RtnResultModel> UpdateToken(TokenModel model)
        {
            return await tokenService.UpdateToken(model);
        }

        /// <summary>
        /// 刪除token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<RtnResultModel> DeleteToken(TokenModel model)
        {
            return await tokenService.DeleteToken(model);
        }
    }
}
