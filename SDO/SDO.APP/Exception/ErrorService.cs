using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SDO.Models;

namespace SDO.Services
{
    public class ErrorService : Service, IErrorService
    {
        private readonly bool showErrorMsg;
        private readonly IHttpContextAccessor httpContextAccessor;
        public ErrorService(IWebHostEnvironment environment, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            //測試環境 或 從設定檔設定(ShowErrorMsg = true) 皆會顯示錯誤訊息
            this.showErrorMsg = environment.IsDevelopment() || Convert.ToBoolean(configuration["ShowErrorMsg"].ToString());
            this.httpContextAccessor = httpContextAccessor;
        }

        public (int statusCode, IRtnResult returnObject) ErrorHandler()
        {
            IRtnResult message;

            if (showErrorMsg)
            {
                IExceptionHandlerFeature error = httpContextAccessor.HttpContext.Features.Get<IExceptionHandlerFeature>();
                message = ChangeResult(false, error?.Error?.ToString());
            }
            else
            {
                message = ChangeResult(false, i18N.Message.R01);
            }


            return (statusCode: (int)HttpStatusCode.InternalServerError, returnObject: message);
        }
    }
}
