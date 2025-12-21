using Microsoft.AspNetCore.Http;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Web;

namespace SDO.Services
{
    public abstract class Service
    {
        /// <summary>
        /// 將IList<Dictionary> 轉為 IList<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        protected IList<T> Mapping<T>(IEnumerable<IEnumerable<KeyValuePair<string, object>>> dictionary) where T : new()
        {
            if (dictionary == null || !dictionary.Any())
                return null;

            IList<T> results = new List<T>();

            foreach (IEnumerable<KeyValuePair<string, object>> fields in dictionary)
            {
                results.Add(Mapping<T>(fields));
            }

            return results;
        }

        /// <summary>
        /// Dictionary 轉 T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        protected T Mapping<T>(IEnumerable<KeyValuePair<string, object>> dictionary) where T : new()
        {
            //建立T實體
            T obj = Activator.CreateInstance<T>();

            foreach (KeyValuePair<string, object> field in dictionary)
            {
                //以Dictionary的Key值取的T的同名屬性(不分大小寫)，設定該屬性值為Dictionary的Value值
                typeof(T)
                    .GetProperty(field.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                    .SetValue(obj, field.Value);
            }

            return obj;
        }

        /// <summary>
        /// Object 轉 Json 字串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected string JsonSerialize(object obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        /// <summary>
        /// Object 轉 Json 字串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected string JsonSerialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        /// <summary>
        /// Json 轉 Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        protected T JsonDeserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// HtmlEncode
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected string HtmlEncode(string str)
        {
            return HttpUtility.HtmlEncode(str);
        }

        /// <summary>
        /// HtmlDecode
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected string HtmlDecode(string str)
        {
            return HttpUtility.HtmlDecode(str);
        }

        /// <summary>
        /// 產生回傳資料異動結果Model
        /// </summary>
        /// <param name="resultType">新增、修改、刪除</param>
        /// <param name="InsertFailPK">新增時重複的PK值</param>
        /// <returns></returns>
        protected RtnResultModel ChangeResult(ResultType resultType, string InsertFailPK = "")
        {
            bool success = (resultType & ResultType.Success) == ResultType.Success;
            string message = i18N.Message.R01;

            resultType &= ResultType.Insert | ResultType.Update | ResultType.Delete;

            if (success)
            {
                switch (resultType)
                {
                    case ResultType.Insert:
                        message = i18N.Message.R04;
                        break;
                    case ResultType.Update:
                        message = i18N.Message.R05;
                        break;
                    case ResultType.Delete:
                        message = i18N.Message.R06;
                        break;
                }
            }
            else
            {
                if (resultType == ResultType.Insert && string.IsNullOrEmpty(InsertFailPK))
                    message = string.Format(i18N.Message.R07, InsertFailPK);
            }

            return ChangeResult(success, message);
        }

        /// <summary>
        /// 產生回傳資料異動結果Model
        /// </summary>
        /// <param name="success"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected RtnResultModel ChangeResult(bool success, string message = "")
        {
            return new RtnResultModel(success, message);
        }
        /// <summary>
        /// 產生回傳資料異動結果Model(登入時具有雙因子登入功能)
        /// </summary>
        /// <param name="success"></param>
        /// <param name="message"></param>
        /// <param name="isSend2AuthMail"></param>
        /// <returns></returns>
        protected ObjectResultModel<bool> LoginChangeResult(bool success, string message = "", bool isSend2AuthMail = false)
        {
            return new ObjectResultModel<bool> { success = success, message = message, data = isSend2AuthMail };
        }

        /// <summary>
        /// 取得當下時間
        /// </summary>
        /// <returns></returns>
        protected DateTime Now => DateTimeUtil.Now;

        /// <summary>
        /// 取得當日日期
        /// </summary>
        protected DateTime Today => DateTimeUtil.Today;

        /// <summary>
        /// 字串 轉 Byte Array
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected byte[] StringToBytes(string str)
        {
            return StringToBytes(Encoding.UTF8, str);
        }

        /// <summary>
        /// 字串 轉 Byte Array
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        protected byte[] StringToBytes(Encoding encoding, string str)
        {
            return encoding.GetBytes(str);
        }

        /// <summary>
        /// Byte Array 轉 字串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        protected string BytesToString(byte[] bytes)
        {
            return BytesToString(Encoding.UTF8, bytes);
        }

        /// <summary>
        /// Byte Array 轉 字串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        protected string BytesToString(Encoding encoding, byte[] bytes)
        {
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// Stream 轉 Byte Array
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected byte[] StreamToBytes(Stream stream)
        {
            long position = stream.Position;

            byte[] bytes = new byte[stream.Length - position];

            stream.Read(bytes, 0, bytes.Length);

            if (stream.CanSeek)
                stream.Seek(position, SeekOrigin.Begin);

            return bytes;
        }

        /// <summary>
        /// 讀取 RequestBody 內字串
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected string ReadRequestBody(HttpContext context)
        {
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            return BytesToString(StreamToBytes(context.Request.Body));
        }

        /// <summary>
        /// 讀取 RequestBody 並 mapping 至 model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        protected T ReadRequestBody<T>(HttpContext context)
        {
            return JsonDeserialize<T>(ReadRequestBody(context));
        }

        /// <summary>
        /// 取得報表執行各體
        /// </summary>
        /// <param name="PConstruct">建構所需物件參數</param>
        /// <param name="ClassName">製表clasName</param>
        /// <returns></returns>
        public T CreateService<T>(object[] PConstruct, string ClassName ,string TGTNameSpace= "Services")
        {
            var theAssembly = Assembly.GetAssembly(typeof(T));
            string Namespace = typeof(T).Namespace;
            Type theType = theAssembly.GetType($"{Namespace.Replace("Interface", TGTNameSpace)}.{ClassName}");
            return (T)Activator.CreateInstance(theType,
                    BindingFlags.Instance | BindingFlags.Public, null, PConstruct,
                    System.Globalization.CultureInfo.InvariantCulture, null);
        }
    }
}
