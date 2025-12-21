using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class DimRightController : ControllerBase
    {
        private readonly IDimRightService dimRightService;
        public DimRightController(IDimRightService dimRightService)
        {
            this.dimRightService = dimRightService;
        }

        [HttpGet]
        public async Task<IList<DimRightModel>> Read()
        {
            return await dimRightService.Read();
        }


        [HttpGet("{rightId}")]
        public async Task<DimRightModel> ReadById(string rightId)
        {
            return await dimRightService.ReadById(rightId);
        }


        [HttpGet("[action]/{roleId}")]
        public async Task<IList<DimRightModel>> ReadByRole(string roleId)
        {
            return await dimRightService.ReadByRole(roleId);
        }


        [HttpPost]
        public async Task<RtnResultModel> Create(DimRightModel right)
        {
            return await dimRightService.Create(right);
        }

        [HttpPut]
        public async Task<RtnResultModel> Update(DimRightModel right)
        {
            return await dimRightService.Update(right);
        }


        [HttpDelete("{rightId}")]
        public async Task<RtnResultModel> Delete(string rightId)
        {
            return await dimRightService.Delete(rightId);
        }
    }
}