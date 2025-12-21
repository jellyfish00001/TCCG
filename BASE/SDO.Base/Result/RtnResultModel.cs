using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SDO.Models
{
    public class RtnResultModel : IRtnResult
    {
        /// <summary>
        /// execute success?
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private string _message;

        /// <summary>
        /// return message
        /// </summary>
        public virtual string message
        {
            get
            {
                return HttpUtility.HtmlEncode(_message);
            }
            set
            {
                this._message = value;
            }
        }

        public int statusCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="success"></param>
        /// <param name="message"></param>
        /// <param name="result"></param>
        public RtnResultModel(bool success, string message = "")
        {
            this.success = success;
            this.message = message;
            this.statusCode = success ? StatusCode.Success : StatusCode.Error;
        }

        public RtnResultModel(int statusCode,bool success, string message = "")
        {
            this.statusCode = statusCode;
            this.success = success;
            this.message = message;
        }

        public T ToChildResultModel<T>()where T: RtnResultModel
        {
            T resultModel = (T)Activator.CreateInstance(typeof(T), new object[]{ success, message });
            return resultModel;
        }
    }

    public class ObjectResultModel<T> : RtnResultModel
    { 
        public ObjectResultModel(bool success = true, string message = "") : base(success, message)
        {
        }

        public T data { get; set; }
    }

    public class AuthorizationFailureResultModel : RtnResultModel
    {
        public AuthorizationFailureResultModel() : base(StatusCode.AccessDenide, false, "Access Denide")
        {}
    }

    [Flags]
    public enum ResultType
    {   
        Success = 0b_0000_0001,
        Fail = 0b_0000_0010,
        Insert = 0b_0000_0100,
        Update = 0b_0000_1000,
        Delete = 0b_0001_0000
    }

    public class RtnExChgResultModel
    {
        /// <summary>
        /// execute success?
        /// </summary>
        public bool success { get; set; }

        private string _token;

        /// <summary>
        /// return message
        /// </summary>
        public virtual string token
        {
            get { return HttpUtility.HtmlEncode(_token); }
            set { this._token = value; }
        }

        public int statusCode { get; set; }

        public RtnExChgResultModel(bool success, string token)
        {
            this.success = success;
            this.token = token;
            this.statusCode = success ? StatusCode.Success : StatusCode.Error;
        }
    }
}
