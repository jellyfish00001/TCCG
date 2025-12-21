using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.Web;
using System.Data.Odbc;
using Microsoft.Extensions.Configuration;
using SDO.Utils;
namespace SDO.Dac
{
    public class TraceDac : IDisposable, ITraceDac
    {
        private string strConnMain;
        private IConnectionControlCenter cnFac;
        private IParameterAdaptor adaptor;


        public TraceDac(IConfiguration configuration,IConnectionControlCenter cnFac ,IParameterAdaptor adaptor)
        {
           // this.strConnMain = configuration.GetConnectionString("MainDBConnection");
            this.cnFac = cnFac;
            this.adaptor = adaptor;
        }

        public void SetConnectionString(string strConnMain)
        {
            this.strConnMain = strConnMain;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="trace">是否紀錄</param>
        /// <returns></returns>
        public IList<T> ExecuteQuery<T>(string sql, object param = null)
        {
            using (var conn = cnFac.GetConnection())
            {
                sql = adaptor.ConvertSql(sql, param);
                return ParseXSSResult(conn.Query<T>(sql, param));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="trace">是否紀錄</param>
        /// <returns></returns>
        public async Task<IList<T>> ExecuteQueryAsync<T>(string sql, object param = null)
        {
            using (var conn = cnFac.GetConnection())
            {
                sql = adaptor.ConvertSql(sql, param);
                return ParseXSSResult(await conn.QueryAsync<T>(sql, param));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="trace">是否紀錄</param>
        /// <returns></returns>
        public bool ExecuteCommand(string sql, object param = null)
        {
            using (var conn = cnFac.GetConnection())
            {
                sql = adaptor.ConvertSql(sql, param);
                return conn.Execute(sql, param) > 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="trace">是否紀錄</param>
        /// <returns></returns>
        public async Task<bool> ExecuteCommandAsync(string sql, object param = null, string DB = "")
        {
            if (!string.IsNullOrEmpty(DB))
                cnFac.SetConnection(DB);
            using (var conn = cnFac.GetConnection())
            {
                sql = adaptor.ConvertSql(sql, param);
                return (await conn.ExecuteAsync(sql, param)) > 0;
            }
        }


        /// <summary>
        /// 防止 Stored XSS 攻擊
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        private IList<T> ParseXSSResult<T>(IEnumerable<T> result)
        {
            if (typeof(T) == typeof(string))
            {
                return result.AsParallel().AsOrdered().Select(p => {
                    if (p == null)
                    {
                        return p;
                    }
                    return (T)Convert.ChangeType(HttpUtility.HtmlEncode(p.ToString().Trim()), typeof(T));
                }).ToList();
            }
            else if (typeof(T) == typeof(IDictionary<string, object>))
            {
                return result.AsParallel().AsOrdered().Select(p =>
                {
                    foreach (KeyValuePair<string, object> keyValue in p as IDictionary<string, object>)
                    {
                        if (keyValue.Key.ToUpper() != keyValue.Key)
                        {
                            (p as IDictionary<string, object>).Add(keyValue.Key.ToUpper(), (keyValue.Value != null && keyValue.Value is string) ? HttpUtility.HtmlEncode(keyValue.Value.ToString().Trim()) : keyValue.Value);
                            (p as IDictionary<string, object>).Remove(keyValue.Key);
                        }
                        else
                        {
                            if (keyValue.Value != null && keyValue.Value is string)
                            {
                                (p as IDictionary<string, object>)[keyValue.Key] = HttpUtility.HtmlEncode(keyValue.Value.ToString().Trim());
                            }
                        }
                    }
                    return (T)p;
                }).ToList();
            }
            else
            {
                return result.AsParallel().AsOrdered().Select(p =>
                {
                    if (p != null)
                    {
                        foreach (var prop in p.GetType().GetProperties())
                        {
                            if (prop.PropertyType == typeof(string) && prop.GetValue(p) != null)
                            {
                                prop.SetValue(p, HttpUtility.HtmlEncode(prop.GetValue(p).ToString().Trim()));
                            }
                        }
                    }
                    return p;
                }).ToList();
            }
        }
    }
}
