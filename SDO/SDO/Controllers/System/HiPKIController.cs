using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class HiPKIController : ControllerBase
    {
        private IHiPKIService hiPKIService;

        public HiPKIController(IHiPKIService hiPKIService)
        {
            this.hiPKIService = hiPKIService;
        }

        [HttpPost]
        public HiPKIModel CheckSignature([FromBody] string sigResult)
        {
            return hiPKIService.CheckSignature(sigResult);
        }
    }
}
