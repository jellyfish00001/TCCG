using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class VUserFunctionDac : Dac, IVUserFunctionDac
    {
        public VUserFunctionDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public async Task<bool> CheckControllerAccess(string userId, string functionController)
        {
            string sql = @"
                SELECT 
	                CASE WHEN COUNT(1) = 0 THEN 0
	                ELSE 1 END
                FROM [VW_USER_FUNCTION] (NOLOCK)
                WHERE USER_ID = ?USER_ID? and FUNCTION_CONTROLLER = ?FUNCTION_CONTROLLER?";
            return (await ExecuteQueryAsync<bool>(sql, new { USER_ID = userId, FUNCTION_CONTROLLER = functionController })).First();
        }
    }
}
