using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Utils
{
    public class CallAPI : ICallAPI
    {
        private readonly IHttpClientFactory clientFactory;
        protected Dictionary<string, string> Headers;
        public CallAPI(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
            Headers = new Dictionary<string, string>();
        }

        public void SetupHeader(Dictionary<string, string> Headers)
        {
            this.Headers = Headers;
        }

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
        public virtual async Task<IEnumerable<T>> SendRequestList<T>(MethodEnum method, string requestURL, dynamic requestObj = null, bool isFormData = false, long timeout = 0)
        {
            HttpResponseMessage response = new();
            // Passing Data From Body
            if (!isFormData)
            {
                //包裝 request 物件
                HttpRequestMessage request = new(new HttpMethod(method.ToString()), requestURL);
                foreach (var item in Headers)
                {
                    request.Headers.Add(item.Key,item.Value);
                }
                //Make HttpContent
                request.Content = MakeHttpContent(requestObj);
                response = await SendAPI(request);
            }
            // Passing Data From Form
            else
            {
                HttpRequestMessage request = new();
                foreach (var item in Headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
                request.Content = MakeHttpContent(requestObj, true);
                response = await SendAPI(request, requestURL, true);
            }

            return await ProcessResultList<T>(response);
        }

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
        public virtual async Task<T> SendRequest<T>(MethodEnum method, string requestURL, object requestObj = null, bool isFormData = false,long timeout = 0)
        {
            HttpResponseMessage response;
            // Passing Data From Body
            if (!isFormData)
            {
                //包裝 request 物件
                HttpRequestMessage request = new(new HttpMethod(method.ToString()), requestURL);
                foreach (var item in Headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
                //Make HttpContent
                if(requestObj!=null)
                    request.Content = MakeHttpContent(requestObj);
                response = await SendAPI(request,timeout:timeout);
            }
            // Passing Data From Form
            else
            {
                HttpRequestMessage request = new();
                foreach (var item in Headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
                request.Content = MakeHttpContent(requestObj, true);
                response = await SendAPI(request, requestURL, true,timeout:timeout);
            }

            if (typeof(T) == typeof(HttpResponseMessage))
                return (T)Convert.ChangeType(response, typeof(T));

            return await ProcessResult<T>(response);
        }

        /// <summary>
        /// 處理response (回傳格式為陣列)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> ProcessResultList<T>(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // response => Json 
                string json = await response.Content.ReadAsStringAsync();
                // Json => IEnumerable<T>
                return JsonConvert.DeserializeObject<IEnumerable<T>>(json);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 處理response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task<T> ProcessResult<T>(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // response => Json 
                string json = await response.Content.ReadAsStringAsync();
                // Json => T
                return JsonConvert.DeserializeObject<T>(json);
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// 製作httpContent
        /// </summary>
        /// <param name="requestObj"></param>
        /// <param name="isFormData"></param>
        /// <returns></returns>
        private HttpContent MakeHttpContent(dynamic requestObj, bool isFormData = false)
        {
            // from body
            if (!isFormData)
            {
                return new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");
            }
            // from form
            else
            {
                // 建立 formCotent 物件
                MultipartFormDataContent content = new();
                // 取得 Request 物件所有屬性資料
                var props = requestObj.GetType().GetProperties();
                // 將得到的屬性資料加入 content 中
                foreach (PropertyInfo prop in props)
                {
                    string propName = prop.Name;
                    object value = prop.GetValue(requestObj, null);
                    content.Add(new StringContent(value.ToString()), propName);
                }
                return content;
            }
        }

        /// <summary>
        /// Call API
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestUrl"></param>
        /// <param name="isFormData"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> SendAPI(HttpRequestMessage request, string requestUrl = "", bool isFormData = false, long timeout = 0)
        {
            // 建立 http 物件
            HttpClient httpClient = clientFactory.CreateClient();
            if (timeout > 0)
                httpClient.Timeout = new TimeSpan(timeout * 1000);
            // from body
            if (!isFormData)
                return await httpClient.SendAsync(request);
            // from form
            else
            {
                HttpContent content = request.Content;
                return await httpClient.PostAsync(requestUrl, content);
            }
        }

        /// <summary>
        /// 呼叫WebAPI的動作
        /// </summary>
        /// <param name="url">呼叫的Url</param>
        /// <param name="httpMethod">Http Method</param>
        /// <param name="content">Post 資料</param>
        /// <param name="code">回傳的HttpStatusCode</param>
        /// <returns></returns>
        public string Call(string url, MethodEnum httpMethod, string content, out HttpStatusCode code)
        {
            GC.Collect();
            var strReturn = string.Empty;

            code = HttpStatusCode.OK;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = httpMethod.ToString();
            request.Timeout = (int)TimeSpan.FromHours(6).TotalMilliseconds;

            if (!string.IsNullOrEmpty(content))
            {
                request.KeepAlive = false;
                request.ContentType = "application/json; charset=utf-8";
                if (httpMethod != MethodEnum.GET)
                {
                    Stream reqStream = request.GetRequestStream();
                    var bytes = Encoding.UTF8.GetBytes(content);
                    reqStream.Write(bytes, 0, content.Length);
                }
            }

            try
            {
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                }

                using (var response = request.GetResponse())
                {
                    using (var respStream = response.GetResponseStream())
                    {
                        strReturn = new StreamReader(respStream, Encoding.UTF8).ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                strReturn = e.Message;
                code = HttpStatusCode.NotFound;
            }
            finally
            {
                request.Abort();
                request = null;
            }
            return strReturn;
        }
    }
}
