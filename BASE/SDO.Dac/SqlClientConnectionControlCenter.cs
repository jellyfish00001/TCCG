using Microsoft.Extensions.Configuration;
using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace SDO.Utils
{
    /// <summary>
    /// sqlclinet連結物件
    /// </summary>
    public class SqlClientConnectionControlCenter : ConnectionControlCenter, IConnectionControlCenter, IDisposable
    {

        public SqlClientConnectionControlCenter(IConfiguration configuration):base(configuration)
        {
        }


        /// <summary>
        /// 取得連線
        /// </summary>
        /// <returns></returns>
        public override DbConnection GetConnection()
        {
            //若無連線則建立
            //單一Scope皆使用同一Connection
            if (_useTranscation)
            {
                _conn ??= new SqlConnection(strConnMain);
                return _conn;
            }
            else
                return new SqlConnection(strConnMain);
        }

        public override void Dispose()
        {
            if (_useTranscation && _tran != null)
            {
                Rollback();
                _conn.Close();
                _conn.Dispose();
            }
        }

        /// <summary>
        /// Transaction Commit
        /// </summary>
        public override void Commit()
        {
            _useTranscation = false;
            GetTransaction()?.Commit();
            GetTransaction()?.Dispose();
            GetConnection()?.Close();
            _tran = null;
        }

        /// <summary>
        /// Transaction Rollback
        /// </summary>
        public override void Rollback()
        {
            _useTranscation = false;
            GetTransaction()?.Rollback();
            GetTransaction()?.Dispose();
            GetConnection()?.Close();
            _tran = null;
        }
    }
}
