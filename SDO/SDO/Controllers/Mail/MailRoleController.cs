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
    public class MailRoleController : ControllerBase
    {

        private readonly IMailRoleService mailRoleService;

        public MailRoleController(IMailRoleService mailRoleService)
        {
            this.mailRoleService = mailRoleService;
        }

        [HttpGet]
        public async Task<IList<UserCountModel>> Read(){
            return await mailRoleService.Read();
        }

        [HttpGet("{roleId}")]
        public async Task<MailRoleModel> ReadById(string roleId)
        {
            return await mailRoleService.ReadById(roleId);
        }

        [HttpGet("[action]/{roleId}")]
        public async Task<IList<MailRoleuUserModel>> ReadUsers(string roleId)
        {
            return await mailRoleService.ReadUsers(roleId);
        }

        [HttpPost]
        public async Task<RtnResultModel> Create(MailRoleMdfModel role)
        {
            await mailRoleService.Create(role);
            return new(true, "存檔成功");
        }

        [HttpPut]
        public async Task<RtnResultModel> Update(MailRoleMdfModel role)
        {
             await mailRoleService.Update(role);
            return new(true, "存檔成功");
        }
        
        [HttpDelete("{roleId}")]
        public async Task<RtnResultModel> Delete(string roleId)
        {
            await mailRoleService.Delete(roleId);
            return new(true, "刪除成功");
        }
    }
}