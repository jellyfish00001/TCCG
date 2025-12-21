using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class SCOrgDac : Dac, ISCOrgDac
    {
        public SCOrgDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 透過OU類別取得OU資料 
        /// </summary>
        /// <param name="OU_KIND"></param>
        /// <returns></returns>
        public List<SCOrgModel> GetOUByOUKind(string OU_KIND)
        {
            string sql = @"select OU_ID, OU_NAME 
                           from SCORG_UNITM (nolock)　
                           where OU_KIND  = @OU_KIND";
            return (ExecuteQuery<SCOrgModel>(sql,new { OU_KIND }, SCDBKey)).ToList();
        }
    }
}
