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
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class DimRoleController : ControllerBase
    {
        private readonly IDimRoleService dimRoleService;

        public DimRoleController(IDimRoleService dimRoleService)
        {
            this.dimRoleService = dimRoleService;
        }

        [HttpGet]
        public async Task<IList<DimRoleModel>> Read()
        {
            return await dimRoleService.Read();
        }

        [HttpGet("{roleId}")]
        public async Task<DimRoleModel> ReadById(string roleId)
        {
            return await dimRoleService.ReadById(roleId);
        }

        [HttpGet("[action]/{userId}")]
        public async Task<IList<DimRoleModel>> ReadByUser(string userId)
        {
            return await dimRoleService.ReadByUser(userId);
        }

        [HttpPost]
        public async Task<RtnResultModel> Create(DimRoleMdfModel role)
        {
            return await dimRoleService.Create(role);
        }

        [HttpPut]
        public async Task<RtnResultModel> Update(DimRoleMdfModel role)
        {
            return await dimRoleService.Update(role);
        }

        [HttpDelete("{roleId}")]
        public async Task<RtnResultModel> Delete(string roleId)
        {
            return await dimRoleService.Delete(roleId);
        }
    }
}