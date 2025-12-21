using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;

namespace SDO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PerformanceController : ControllerBase
    {
        private readonly IPerformanceService performanceService;

        public PerformanceController(IPerformanceService performanceService)
        {
            this.performanceService = performanceService;
        }

        [HttpGet]
        public IList<PerformanceModel> Read()
        {
            return performanceService.GetPerformance();
        }
    }
}