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
    public class EmpAgentController : ControllerBase
    {
        private readonly IEmpAgentService empAgentService;

        public EmpAgentController(IEmpAgentService empAgentService)
        {
            this.empAgentService = empAgentService;
        }

        [HttpGet]
        public async Task<IList<EmpAgentModel>> Read()
        {
            return await empAgentService.Read();
        }

        [HttpPost]
        public async Task<RtnResultModel> Create(EmpAgentMdfModel agent)
        {
            return await empAgentService.Create(agent);
        }

        [HttpDelete("{sid}")]
        public async Task<RtnResultModel> Delete(string sid)
        {
            return await empAgentService.Delete(sid);
        }
    }
}