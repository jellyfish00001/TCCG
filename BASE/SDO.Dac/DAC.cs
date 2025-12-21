using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Dac.Models;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using static Dapper.SqlMapper;

namespace SDO.Dac
{
    public abstract class Dac : IDac, IDisposable
    {
        private readonly IConnectionControlCenter connectionControlCenter;
        protected HttpContext httpContext => httpContextAccessor?.HttpContext;
        protected readonly IHttpContextAccessor httpContextAccessor;

        protected readonly IParameterAdaptor ParameterAdaptor;

        private readonly ISqlTrace sqlTrace;
        private readonly IUserData userInfo;

        protected string UserId { get; set; }
        protected string UserIP { get; set; }

        protected string UserName { get; set; }
        protected string UserEmail { get; set; }

        public enum DBConnection
        {
            defaultKey = 0,
            SCDBKey = 1,
            RISDBKey = 2,
            IPCDBKey = 3,
            INNDBKey = 4,
            PWSDBKey = 5,
            RDDBKey = 6,
            MainDBKey = 7
        }

        protected const string MainDBKey = "MainDBConnection";// 桃園重大建設DB
        protected const string SCDBKey = "SCDBConnection";    // 角色人員、組織DB
        protected const string RISDBKey = "RISDBConnection";  // 桃園舊案DB
        protected const string IPCDBKey = "IPCDBConnection";  // 桃園舊案DB
        protected const string INNDBKey = "INNDBConnection";  // 創新提案DB
        protected const string PWSDBKey = "PWSDBConnection";  // 先期計畫DB
        protected const string RDDBKey = "RDDBConnection";    // 研究發展DB

        // DB Link string
        protected string RIS_M = "";
        protected string SC30_M = "";

        //現在日期
        protected readonly string DTNow = "DATEADD(HH,8,GETUTCDATE())";

        public void SetProfile(string UserId, string Ip)
        {
            this.UserId = UserId;
            this.UserIP = Ip;
        }
        //若是代理人，需調整UserId格式
        /// <summary>
        /// for api
        /// </summary>
        /// <param name="connectionControlCenter"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="trace"></param>
        /// <param name="profile"></param>
        /// <param name="ParameterAdaptor"></param>
        public Dac(IConnectionControlCenter connectionControlCenter, IHttpContextAccessor httpContextAccessor, ISqlTrace trace, IUserProfile profile, IParameterAdaptor ParameterAdaptor, IConfiguration configuration)
        {
            this.SetProfile(profile.GetLoginUser().USER_ID, profile.GetLoginUser().USER_IP);
            this.connectionControlCenter = connectionControlCenter;
            this.httpContextAccessor = httpContextAccessor;
            this.ParameterAdaptor = ParameterAdaptor;
            this.sqlTrace = trace;
            this.userInfo = profile.GetLoginUser();
            // 確保拿到正確的連線字串
            connectionControlCenter.SetConnection(MainDBKey);
            // 取得DBLink字串
            RIS_M = configuration.GetSection("DBLinks")["RIS_M"].ToString();
            SC30_M = configuration.GetSection("DBLinks")["SC30_M"].ToString();
        }

        /// <summary>
        /// for 排程開發使用
        /// </summary>
        /// <param name="connectionControlCenter"></param>
        /// <param name="trace"></param>
        /// <param name="ParameterAdaptor"></param>
        /// <param name="userData"></param>
        /// <param name="configuration"></param>
        public Dac(IConnectionControlCenter connectionControlCenter, ISqlTrace trace, IParameterAdaptor ParameterAdaptor, IUserData userData, IConfiguration configuration)
        {
            this.connectionControlCenter = connectionControlCenter;
            this.ParameterAdaptor = ParameterAdaptor;
            this.sqlTrace = trace;
            this.userInfo = userData;
            this.UserId = userData.USER_ID;
            // 確保拿到正確的連線字串
            connectionControlCenter.SetConnection(MainDBKey);
            // 取得DBLink字串
            RIS_M = configuration.GetSection("DBLinks")["RIS_M"].ToString();
            SC30_M = configuration.GetSection("DBLinks")["SC30_M"].ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 是否紀錄
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="DB"></param>
        /// <param name="trace">查詢</param>
        /// <param name="IsRaw"></param>
        /// <param name="isSetUSER">是否設定userId(基本上是CUD會用到)，預設false(For R)</param>
        /// <returns></returns>
        public IList<T> ExecuteQuery<T>(string sql, object param = null, string DB = MainDBKey, bool trace = true, bool IsRaw = true, bool isSetUSER = false)
        {
            sql = ParameterAdaptor.ConvertSql(sql, param);

            if (param is IDbEditor && isSetUSER)
            {
                ((IDbEditor)param).CRT_USER ??= this.UserId;
                ((IDbEditor)param).MDF_USER ??= this.UserId;
            }

            if (trace && isSetUSER)
            {
                sqlTrace.SetTrace(sql, param, userInfo);
            }

            connectionControlCenter.SetConnection(DB);
            if (connectionControlCenter.IsTran)
            {
                var conn = connectionControlCenter.GetConnection();
                return ParseXSSResult(conn.Query<T>(sql, param, connectionControlCenter.GetTransaction()), IsRaw);
            }
            else
            {
                using (DbConnection conn = connectionControlCenter.GetConnection())
                {
                    return ParseXSSResult(conn.Query<T>(sql, param, connectionControlCenter.GetTransaction()), IsRaw);
                }
            }
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="DB"></param>
        /// <param name="trace">是否紀錄</param>
        /// <returns></returns>
        public async Task<IList<T>> ExecuteQueryAsync<T>(string sql, object param = null, string DB = MainDBKey, bool trace = false, bool IsRaw = true)
        {
            sql = ParameterAdaptor.ConvertSql(sql, param);
            if (trace)
            {
                sqlTrace.SetTrace(sql, param, userInfo);
            }

            return ParseXSSResult(await QueryAsync<T>(sql, param, DB, IsRaw), IsRaw);
        }

        public async Task<IEnumerable<dynamic>> ExecuteQueryAsyncToDynamic(string sql, object param = null, string DB = MainDBKey, bool trace = true)
        {
            sql = ParameterAdaptor.ConvertSql(sql, param);
            if (trace)
            {
                sqlTrace.SetTrace(sql, param, userInfo);
            }

            return await QueryAsync(sql, param, DB, trace);
        }

        private async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param, string DB, bool IsRaw)
        {
            connectionControlCenter.SetConnection(DB);
            if (connectionControlCenter.IsTran)
            {
                var conn = connectionControlCenter.GetConnection();
                return ParseXSSResult(await conn.QueryAsync<T>(sql, param, connectionControlCenter.GetTransaction()), IsRaw);
            }
            else
            {
                using (DbConnection conn = connectionControlCenter.GetConnection())
                {
                    return ParseXSSResult(await conn.QueryAsync<T>(sql, param), IsRaw);
                }
            }
        }

        private async Task<IEnumerable<dynamic>> QueryAsync(string sql, object param, string DB, bool IsRaw)
        {
            connectionControlCenter.SetConnection(DB);
            if (connectionControlCenter.IsTran)
            {
                var conn = connectionControlCenter.GetConnection();
                return ParseXSSResult(await conn.QueryAsync(sql, param, connectionControlCenter.GetTransaction()), IsRaw);
            }
            else
            {
                using (DbConnection conn = connectionControlCenter.GetConnection())
                {
                    return ParseXSSResult(await conn.QueryAsync(sql, param), IsRaw);
                }
            }
        }

        /// <summary>
        /// 查詢單一model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="DB"></param>
        /// <param name="trace">是否紀錄</param>
        /// <returns></returns>
        public async Task<T> ExecuteQueryFirstOrDefaultAsync<T>(string sql, object param = null, string DB = MainDBKey, bool trace = false)
        {
            sql = ParameterAdaptor.ConvertSql(sql, param);
            if (trace)
            {
                sqlTrace.SetTrace(sql, param, userInfo);
            }

            connectionControlCenter.SetConnection(DB);
            if (connectionControlCenter.IsTran)
            {
                var conn = connectionControlCenter.GetConnection();
                return await conn.QueryFirstOrDefaultAsync<T>(sql, param, connectionControlCenter.GetTransaction());
            }
            else
            {
                using (DbConnection conn = connectionControlCenter.GetConnection())
                {
                    return await conn.QueryFirstOrDefaultAsync<T>(sql, param);
                }
            }
        }


        /// <summary>
        /// 查詢單一model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="DB"></param>
        /// <param name="trace">是否紀錄</param>
        /// <param name="isSetUSER">是否設定userId(基本上是CUD會用到)，預設false(For R)</param>
        /// <returns></returns>
        public T ExecuteQueryFirstOrDefault<T>(string sql, object param = null, string DB = MainDBKey, bool trace = false, bool isSetUSER = false)
        {
            sql = ParameterAdaptor.ConvertSql(sql, param);

            if (param is IDbEditor && isSetUSER)
            {
                ((IDbEditor)param).CRT_USER ??= this.UserId;
                ((IDbEditor)param).MDF_USER ??= this.UserId;
            }

            if (trace && isSetUSER)
            {
                sqlTrace.SetTrace(sql, param, userInfo);
            }

            connectionControlCenter.SetConnection(DB);
            if (connectionControlCenter.IsTran)
            {
                var conn = connectionControlCenter.GetConnection();
                return conn.QueryFirstOrDefault<T>(sql, param, connectionControlCenter.GetTransaction());
            }
            else
            {
                using (DbConnection conn = connectionControlCenter.GetConnection())
                {
                    return conn.QueryFirstOrDefault<T>(sql, param, connectionControlCenter.GetTransaction());
                }
            }
        }

        public async Task<GridModel<T>> ExecuteQueryMultipleAsync<T>(string sql, object param = null, string DB = MainDBKey, bool trace = false) where T : class
        {
            sql = ParameterAdaptor.ConvertSql(sql, param);
            GridModel<T> gridModel = new();

            if (trace)
            {
                sqlTrace.SetTrace(sql, param, userInfo);
            }

            connectionControlCenter.SetConnection(DB);
            using (DbConnection conn = connectionControlCenter.GetConnection())
            {
                var results = await conn.QueryMultipleAsync(sql, param, connectionControlCenter.GetTransaction());
                var GenericTypeData = await results.ReadAsync<T>();
                var TotalCount = await results.ReadAsync<int>();
                gridModel.Data = GenericTypeData.ToList();
                gridModel.TotalCount = TotalCount.FirstOrDefault();
                return gridModel;
            }
        }

        public async Task<(IList<T1> result1, IList<T2> result2)> ExecuteQueryMultipleAsync<T1, T2>(string sql, object param = null, string DB = MainDBKey, bool trace = false)
        {
            sql = ParameterAdaptor.ConvertSql(sql, param);

            if (trace)
            {
                sqlTrace.SetTrace(sql, param, userInfo);
            }

            connectionControlCenter.SetConnection(DB);
            using (DbConnection conn = connectionControlCenter.GetConnection())
            {
                using (var results = await conn.QueryMultipleAsync(sql, param, connectionControlCenter.GetTransaction()))
                {
                    var resultSet1 = await results.ReadAsync<T1>();
                    var resultSet2 = await results.ReadAsync<T2>();
                    return (resultSet1.ToList(), resultSet2.ToList());
                }
            }
        }

        public async Task<GridReader> ExecuteQueryMultipleAsync<T1, T2, T3, T4, T5>(string sql, object param = null, string DB = MainDBKey, bool trace = false)
        {
            sql = ParameterAdaptor.ConvertSql(sql, param);
            if (trace)
            {
                sqlTrace.SetTrace(sql, param, userInfo);
            }

            connectionControlCenter.SetConnection(DB);
            using (DbConnection conn = connectionControlCenter.GetConnection())
            {
                using (var results = await conn.QueryMultipleAsync(sql, param, connectionControlCenter.GetTransaction()))
                {
                    return results;
                }
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
        public async Task<IList<IDictionary<string, object>>> ExecuteQueryDictAsync(string sql, object param = null, string DB = MainDBKey, bool trace = false, bool IsRaw = true)
        {
            sql = ParameterAdaptor.ConvertSql(sql, param);
            if (trace)
            {
                sqlTrace.SetTrace(sql, param, userInfo);
            }

            connectionControlCenter.SetConnection(DB);
            using (DbConnection conn = connectionControlCenter.GetConnection())
            {
                return ParseXSSResult((await conn.QueryAsync(sql, param, connectionControlCenter.GetTransaction())).Select(x => (IDictionary<string, object>)x), IsRaw);
            }
        }

        public bool ExecuteCommand(string sql, object param = null, string DB = MainDBKey, bool trace = true)
        {
            sql = ParameterAdaptor.ConvertSql(sql, param);
            if (param == null)
                return Execute(sql, param, DB, trace);
            else if (param is IDbEditor)
            {
                ((IDbEditor)param).CRT_USER ??= this.UserId;
                ((IDbEditor)param).MDF_USER ??= this.UserId;
            }
            return Execute(sql, param, DB, trace);
        }

        public bool ExecuteCommand(string sql, IEnumerable<object> param = null, string DB = MainDBKey, bool trace = true)
        {
            sql = ParameterAdaptor.ConvertSql(sql, param);
            if (param == null)
                return Execute(sql, param, DB, trace);

            Parallel.ForEach(param, o =>
            {
                if (o is IDbEditor)
                {
                    ((IDbEditor)o).CRT_USER ??= this.UserId;
                    ((IDbEditor)o).MDF_USER ??= this.UserId;
                }
            });

            return Execute(sql, param, DB, trace);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="trace">是否紀錄</param>
        /// <returns></returns>
        private bool Execute(string sql, object param, string DB, bool trace)
        {
            sql = ParameterAdaptor.ConvertSql(sql, param);
            if (trace)
            {
                sqlTrace.SetTrace(sql, param, userInfo);
            }

            connectionControlCenter.SetConnection(DB);
            if (connectionControlCenter.IsTran)
            {
                var conn = connectionControlCenter.GetConnection();
                return conn.Execute(sql, param, connectionControlCenter.GetTransaction()) > 0;
            }
            else
            {
                using (DbConnection conn = connectionControlCenter.GetConnection())
                {
                    return conn.Execute(sql, param, connectionControlCenter.GetTransaction()) > 0;
                }
            }
        }

        public async Task<bool> ExecuteCommandAsync(string sql, object param = null, string DB = MainDBKey, bool trace = true)
        {
            if (param == null)
                return await ExecuteAsync(sql, param, DB, trace);
            else if (param is IDbEditor)
            {
                ((IDbEditor)param).CRT_USER ??= this.UserId;
                ((IDbEditor)param).MDF_USER ??= this.UserId;
            }

            return await ExecuteAsync(sql, param, DB, trace);
        }

        public async Task<bool> ExecuteCommandAsync(string sql, IEnumerable<object> param = null, string DB = MainDBKey, bool trace = true)
        {
            sql = ParameterAdaptor.ConvertSql(sql, param);
            if (param == null)
                return await ExecuteAsync(sql, param, DB, trace);

            Parallel.ForEach(param, o =>
            {
                if (o is IDbEditor)
                {
                    ((IDbEditor)o).CRT_USER ??= this.UserId;
                    ((IDbEditor)o).MDF_USER ??= this.UserId;
                }
            });

            return await ExecuteAsync(sql, param, DB, trace);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="trace">是否紀錄</param>
        /// <returns></returns>
        private async Task<bool> ExecuteAsync(string sql, object param, string DB, bool trace)
        {
            sql = ParameterAdaptor.ConvertSql(sql, param);
            if (trace)
            {
                sqlTrace.SetTrace(sql, param, userInfo);
            }

            connectionControlCenter.SetConnection(DB);
            if (connectionControlCenter.IsTran)
            {
                var conn = connectionControlCenter.GetConnection();
                return await conn.ExecuteAsync(sql, param, connectionControlCenter.GetTransaction()) > 0;
            }
            else
            {
                using (DbConnection conn = connectionControlCenter.GetConnection())
                {
                    return await conn.ExecuteAsync(sql, param, connectionControlCenter.GetTransaction()) > 0;
                }
            }
        }

        public void BeginTransaction()
        {
            connectionControlCenter.BeginTransaction();
        }

        public void Commit()
        {
            connectionControlCenter.Commit();
        }

        public void Rollback()
        {
            connectionControlCenter.Rollback();
        }

        /// <summary>
        /// 防止 Stored XSS 攻擊
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        private IList<T> ParseXSSResult<T>(IEnumerable<T> result, bool IsRaw)
        {
            if (IsRaw)
                return result.ToList();

            if (typeof(T) == typeof(string))
            {
                return result.AsParallel().AsOrdered().Select(p => (T)Convert.ChangeType(HttpUtility.HtmlEncode(p.ToString().Trim()), typeof(T))).ToList();
            }
            else if (typeof(T) == typeof(IDictionary<string, object>))
            {
                return result.Select(p =>
                {
                    foreach (KeyValuePair<string, object> keyValue in p as IDictionary<string, object>)
                    {

                        if (keyValue.Value != null && keyValue.Value is string)
                        {
                            (p as IDictionary<string, object>)[keyValue.Key] = HttpUtility.HtmlEncode(keyValue.Value.ToString().Trim());
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
                        Parallel.ForEach(p.GetType().GetProperties(), prop =>
                        {
                            if (prop.PropertyType == typeof(string) &&
                                (prop.CanRead && prop.GetValue(p) != null) &&
                                prop.CanWrite)
                            {
                                prop.SetValue(p, HttpUtility.HtmlEncode(prop.GetValue(p).ToString().Trim()));
                            }
                        }
                        );
                    }
                    return p;
                }).ToList();
            }
        }

        /// <summary>
        /// 組模糊查詢SQL
        /// </summary>
        /// <param name="str"></param> 需處理模糊查詢的字串
        /// <param name="field"></param> 對應資料庫的欄位
        /// <returns></returns>
        protected string FuzzySearch(string str, string field)
        {
            // 空字串不處理
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            var values = HttpUtility.HtmlEncode(str)
                .Split(new char[] { ',', ' ', '　' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => $"{field} like '%{x}%'")
                .ToList();

            return values.Any() ? $" AND ({string.Join(" OR ", values)})" : "";
        }

        /// <summary>
        /// AP對應DB連線Key
        /// </summary>
        /// <param name="ApId"></param>
        /// <returns></returns>
        public string GetAPDBKey(string ApId)
        {
            switch(ApId )
            {
                case "IPC3":
                    return MainDBKey;
                case "PWS":
                    return PWSDBKey;
                case "INN":
                    return INNDBKey;
                case "RD2":
                    return RDDBKey;
                default:return MainDBKey;
            }
        }
    }
}
