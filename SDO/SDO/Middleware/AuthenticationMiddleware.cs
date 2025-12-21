using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SDO.Models;
using SDO.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SDO.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization.Policy;

namespace SDO.Authentication
{

    public class AuthorizationHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler
        DefaultHandler = new AuthorizationMiddlewareResultHandler();

        public async Task HandleAsync(RequestDelegate next,
                                HttpContext context,
                                AuthorizationPolicy policy,
                                PolicyAuthorizationResult authorizeResult)
        {
            if(!authorizeResult.Succeeded && authorizeResult.Forbidden)
            {
                //await ResponseUtil.WriteResponse(context, new AuthorizationFailureResultModel());
                context.Response.StatusCode = 401;
                return;
            }

            // Fallback to the default implementation.
            await DefaultHandler.HandleAsync(next, context, policy,
                                   authorizeResult);
        }
    }
}