using SDO.Models;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface ISqlTrace
    {
        void SetTrace(string sql, object param, IUserData userInfo);

        void AddLog(object sqlTrace);

        void AddLog(string sql, object param);

        Task WriteLog();
    }
}