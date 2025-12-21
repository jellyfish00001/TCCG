using System;
using System.ComponentModel.DataAnnotations;

namespace SDO.Models
{
    public class ApiTraceModel
    {
        /// <summary>
        /// ApiTrace計數器
        /// </summary>
        public int DATA_COUNT { get; set; }

        /// <summary>
        /// ApiTrace代碼
        /// </summary>
        public int SID { get; set; }

        /// <summary>
        /// 呼叫API的IP位置
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// Api REQUEST_HEADER
        /// </summary>
        public string REQUEST_HEADER { get; set; }

        /// <summary>
        /// Api REQUEST_BODY
        /// </summary>
        public string REQUEST_BODY { get; set; }

        /// <summary>
        /// API REQUEST_URL
        /// </summary>
        public string REQUEST_URL { get; set; }

        /// <summary>
        /// API REQUEST_TYPE
        /// </summary>
        public string REQUEST_TYPE { get; set; }

        /// <summary>
        /// API RESPONSE_HEADER
        /// </summary>
        public string RESPONSE_HEADER { get; set; }

        /// <summary>
        /// API RESPONSE_BODY
        /// </summary>
        public string RESPONSE_BODY { get; set; }

        /// <summary>
        /// API RESPONSE_CODE
        /// </summary>
        public int RESPONSE_CODE { get; set; }

        /// <summary>
        /// APITrace LOG_DATE
        /// </summary>
        public string LOG_DATE { get; set; }

        public string UserId { get; set; }

        public string request_desc { get; set; }
    }
}
