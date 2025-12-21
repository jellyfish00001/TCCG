using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SDO.Services;
using SDO.Models;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.Eventing.Reader;
using System.Collections.Generic;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class EmpUserController : ControllerBase
    {
        private readonly IEmpUserService empUserService;

        public EmpUserController(IEmpUserService empUserService)
        {
            this.empUserService = empUserService;
        }
        
        [HttpPost("[action]")]
        public async Task<IList<UserDataModel>> Read(EmpUserReadModel model)
        {
            return await empUserService.Read(model);
        }

        [HttpGet("{userId}")]
        public async Task<UserDataModel> ReadById(string userId)
        {
            return await empUserService.GetUserById(userId);
        }

        [HttpGet("[action]/{orgId}")]
        public async Task<IList<UserDataModel>> ReadByOrg(string orgId)
        {
            return await empUserService.GetUserByOrg(orgId);
        }

        [HttpGet("[action]")]
        public async Task<IList<UserDataModel>> QueryUsers(string userIds)
        {
            return await empUserService.GetUserByIds(userIds);
        }

        [HttpPost]
        public async Task<RtnResultModel> Create(EmpUserModel empUser)
        {
            return await empUserService.Create(empUser);
        }

        [HttpPut]
        public async Task<RtnResultModel> Update(EmpUserModel empUser)
        {
            return await empUserService.Update(empUser);
        }

        [HttpDelete("{userId}")]
        public async Task<RtnResultModel> Delete(string userId, [FromForm] string del_reason)
        {
            return await empUserService.Delete(userId, del_reason);
        }

        [HttpGet("[action]")]
        public async Task<IList<SCUserModel>> ReadUserAgent()
        {
            return await empUserService.ReadUserAgent();
        }
    }
}
