using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDO.Services;
using Microsoft.AspNetCore.Authorization;
using SDO.Models;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class SetFunctionController : ControllerBase
    {
        private readonly ISetFunctionService setFunctionService;
        public SetFunctionController(ISetFunctionService setFunctionService)
        {
            this.setFunctionService = setFunctionService;
        }

        [HttpGet("[action]/{userId}")]//GET: api/SetFunction/ReadByUser/userId
        public async Task<IList<SetFunctionModel>> ReadByUser(string userId, string apid)
        {
            return await setFunctionService.ReadByUser(userId, apid);
        }

        [HttpGet("[action]")]//GET: api/SetFunction/ReadByGroup
        public async Task<IList<SetFunctionModel>> ReadByGroup([FromQuery] SetFunctionQryModel queryModel)
        {
            return await setFunctionService.ReadByGroup(queryModel);
        }

        [HttpGet("[action]")]//GET: api/ReadByFunctionRoot
        public async Task<IList<SetFunctionModel>> ReadByFunctionRoot()
        {
            return await setFunctionService.ReadByFunctionRoot();
        }

        [HttpGet("[action]")]
        public async Task<IList<SetFunctionListModel>> ReadFunctionList(string apid)
        {
            return await setFunctionService.ReadFunctionList(apid);
        }

        [HttpGet]
        public async Task<IList<SetFunctionModel>> Read()
        {
            return await setFunctionService.Read();
        }

        [HttpPost]
        public async Task<RtnResultModel> Create(SetFunctionModel function)
        {
            return await setFunctionService.Create(function);
        }

        [HttpPut]
        public async Task<RtnResultModel> Update(SetFunctionModel function)
        {
            return await setFunctionService.Update(function);
        }

        /// <summary>
        /// 設定排序
        /// 傳入範例: Json格式 "["a","b","c"]"
        /// </summary>
        /// <param name="functionIds"></param>
        /// <returns></returns>
        [HttpPut("[action]")]
        public async Task<RtnResultModel> SetSortorder(string[] functionIds)
        {
            return await setFunctionService.SetSortorder(functionIds);
        }

        [HttpDelete("{functionId}")]
        public async Task<RtnResultModel> Delete(string functionId)
        {
            return await setFunctionService.Delete(functionId);
        }

        [HttpGet("{functionId}")]
        public async Task<SetFunctionModel> ReadById(string functionId)
        {
            return await setFunctionService.ReadById(functionId);
        }

        [HttpGet("[action]/{rightId}")]
        public async Task<IList<SetFunctionModel>> ReadByRight(string rightId)
        {
            return await setFunctionService.ReadByRight(rightId);
        } 
    }
}