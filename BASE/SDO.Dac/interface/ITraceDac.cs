using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface ITraceDac
    {
        void Dispose();
        bool ExecuteCommand(string sql, object param = null);
        Task<bool> ExecuteCommandAsync(string sql, object param = null, string DB = "");
        IList<T> ExecuteQuery<T>(string sql, object param = null);
        Task<IList<T>> ExecuteQueryAsync<T>(string sql, object param = null);
    }
}