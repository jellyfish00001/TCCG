using System.Data.Common;

namespace SDO.Utils
{
    public interface IConnectionControlCenter
    {
        void SetConnection(string Key);
        void BeginTransaction();
        void Commit();
        void Dispose();
        DbConnection GetConnection();
        DbTransaction GetTransaction();
        void Rollback();
        /// <summary>
        /// 是否開啟交易
        /// </summary>
        bool IsTran { get; }
    }
}