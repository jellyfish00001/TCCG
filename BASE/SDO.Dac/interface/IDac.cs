using SDO.Models;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IDac
    {
        void Dispose();
        void SetProfile(string UserId, string Ip);
        IList<T> ExecuteQuery<T>(string sql, object param = null, string DB = "", bool trace = true, bool IsRaw = true, bool isSetUSER = false);
        Task<IList<T>> ExecuteQueryAsync<T>(string sql, object param = null, string DB = "",bool trace = true,  bool IsRaw = true);
        Task<IList<IDictionary<string, object>>> ExecuteQueryDictAsync(string sql, object param = null, string DB = "",bool trace = true,  bool IsRaw = true);
        Task<IEnumerable<dynamic>> ExecuteQueryAsyncToDynamic(string sql, object param = null, string DB = "", bool trace = true);
        bool ExecuteCommand(string sql, object param = null, string DB = "", bool trace = true);
        bool ExecuteCommand(string sql, IEnumerable<object> param = null, string DB = "", bool trace = true);
        Task<bool> ExecuteCommandAsync(string sql, IEnumerable<object> param = null, string DB = "", bool trace = true);
        Task<(IList<T1> result1, IList<T2> result2)> ExecuteQueryMultipleAsync<T1, T2>(string sql, object param = null, string DB = "", bool trace = false);
        void BeginTransaction();
        void Commit();
        void Rollback();
    }
}