using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static SDO.Utils.CallAPI;

namespace SDO.Utils
{
    public interface ICallAPI
    {
        /// <summary>
        /// 設定http headders
        /// </summary>
        /// <param name="Headers"></param>
        void SetupHeader(Dictionary<string, string> Headers);

        /// <summary>
        /// Send API Request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="requestURL"></param>
        /// <param name="requestObj"></param>
        /// <param name="isFormData"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> SendRequestList<T>(MethodEnum method, string requestURL, dynamic requestObj = null, bool isFormData = false,long timeout = 0);

        /// <summary>
        /// Send API Request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="requestURL"></param>
        /// <param name="requestObj"></param>
        /// <param name="isFormData"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task<T> SendRequest<T>(MethodEnum method, string requestURL, dynamic requestObj = null, bool isFormData = false,long timeout = 0);

        /// <summary>
        /// 處理response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> ProcessResultList<T>(HttpResponseMessage response);

        /// <summary>
        /// 處理response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        Task<T> ProcessResult<T>(HttpResponseMessage response);
         
        /// <summary>
        /// 呼叫WebAPI的動作
        /// </summary>
        /// <param name="httpMethod">Http Method</param>
        /// <param name="content">Post 資料</param>
        /// <param name="code">回傳的HttpStatusCode</param>
        /// <returns></returns>
        string Call(string url, MethodEnum httpMethod, string content, out HttpStatusCode code);
    }
}
