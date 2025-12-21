using Microsoft.Extensions.Configuration;
using SDO.Dac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Utils;

namespace SDO.Services
{
    
    public class SystemInfoService : SetParamService, ISystemInfoService
    {
        private IConfiguration configuration;

        /// <summary>
        /// 繼承setParam
        /// </summary>
        /// 提供Constructor參數 dac setParam userProfile
        /// <param name="dac"></param>
        public SystemInfoService(ISetParamDac dac, ISysParam sysParam, IConfiguration configuration) : base(dac, sysParam)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// 取得版本號
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetVersionInfo()
        {
            return string.Format("({0}/{1})", (await GetSysParam("Version", "AP")).SET_VALUE, (await GetSysParam("Version", "DB")).SET_VALUE);
        }

        public string GetCacheExpireTime()
        {
            return configuration.GetValue<int>("CacheSetting:CacheExpireTime").ToString();
        }
    }
}
