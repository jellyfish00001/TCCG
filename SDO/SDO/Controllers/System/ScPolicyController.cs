using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;
using System.Threading.Tasks;

namespace SDO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "handUser")]
    public class ScPolicyController : ControllerBase
    {
        private readonly IScPolicyService scPolicyService;

        public ScPolicyController(IScPolicyService scPolicyService)
        {
            this.scPolicyService = scPolicyService;
        }

        [HttpPost("[action]")]
        public async Task<ScPolicyModel> GetPolicy([FromForm] string policyId, [FromForm] string policyCompId)
        {
            return await scPolicyService.GetPolicy(policyId, policyCompId);
        }

        [HttpPut]
        public async Task<RtnResultModel> UpdatePolicy(ScPolicyModel model)
        {
            return await scPolicyService.UpdatePolicy(model);
        }
    }
}