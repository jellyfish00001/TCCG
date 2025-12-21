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
    public class SCRoleDac : Dac, IDimRoleDac
    {
        public SCRoleDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }
        public async Task<List<DimRoleModel>> ReadListByUser(string userId, string apId)
        {
            string sql = $@"select AP_ID
                                ,USR_ID
                                ,ROL_ID ROLE_ID
                                ,ROL_DOMAIN_ID 
                                ,ROL_COMP_ID 
                                ,ROL_NAME ROLE_NAME 
                                ,ROL_TYPE 
                                ,ROL_KIND 
                                ,ROL_DESC 
                                ,ROL_SORT_ORDER 
                                ,REL_KIND
                            from SCREL_ROL_USRMV
                            where REL_KIND = 1
                                and USR_ID = @UserId 
                                {(string.IsNullOrEmpty(apId) ? string.Empty : "and AP_ID = @AP_ID")}";
            return (await ExecuteQueryAsync<DimRoleModel>(sql, new { UserId = userId, AP_ID = apId }, SCDBKey)).ToList();
        }
    }
}
