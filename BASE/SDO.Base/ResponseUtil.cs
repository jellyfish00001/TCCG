using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SDO.Utils
{
    public static class ResponseUtil
    {
        /// <summary>
        /// 寫入Response Result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static async Task WriteResponse<T>(HttpContext context, T result)
        {
            //XML
            if ("application/xml".Equals(context.Request.ContentType) ||
                "text/xml".Equals(context.Request.ContentType))
            {
                using System.IO.StringWriter stringwriter = new System.IO.StringWriter();
                XmlSerializer serializer = new XmlSerializer(result.GetType());
                serializer.Serialize(stringwriter, result);
                await context.Response.WriteAsync(stringwriter.ToString());
            }
            //JSON
            else
            {
                await context.Response.WriteAsJsonAsync(result);
            }
        }
    }

    public static class StatusCode
    {
        public const int Success = 200;

        public const int AccessDenide = 401;

        public const int Error = 500;
    }
}
