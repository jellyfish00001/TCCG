using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDO.Services;

namespace SDO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CryptController : ControllerBase
    {
        private readonly ICryptService cryptService;

        public CryptController(ICryptService cryptService)
        {
            this.cryptService = cryptService;
        }

        [HttpGet("[action]")]
        public string EnCryptAES256([FromQuery]string data, [FromQuery]string key)
        {
            return cryptService.EncryptAES256(data, key);
        }


        [HttpGet("[action]")]
        public string DeCryptAES256([FromQuery] string data, [FromQuery] string key)
        {
            return cryptService.DecryptAES256(data, key);
        }
    }
}