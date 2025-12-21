using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;

namespace SDO.Utils
{
    public class ConnectionControlCenter : IConnectionControlCenter, IDisposable
    {
        //連線字串
        protected string strConnMain;
        //Connection
        protected DbConnection _conn { get; set; }
        //Transaction參數
        protected bool _useTranscation { get; set; }
        //DbTransaction
        protected DbTransaction _tran { get; set; }
        protected  IConfiguration configuration { get; set; }
        /// <summary>
        /// 是否開啟交易
        /// </summary>
        public bool IsTran { get { return _useTranscation; } }

        public ConnectionControlCenter(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.strConnMain = configuration.GetConnectionString("MainDBConnection");
            //預設不使用Transcation
            this._useTranscation = false;
        }

        public void SetConnection(string Key)
        {
            this.strConnMain = configuration.GetConnectionString(Key);
        }

        public virtual void Dispose()
        {
            if (_useTranscation && _tran != null)
            {
                Rollback();
            }
            _conn.Close();
            _conn.Dispose();
        }

        /// <summary>
        /// 取得連線
        /// </summary>
        /// <returns></returns>
        public virtual DbConnection GetConnection()
        {
            //若無連線則建立
            //Transaction時單一Scope才使用同一Connection
            if (_useTranscation)
            {
                _conn ??= new OdbcConnection(strConnMain);
                return _conn;
            }
            else
                return new OdbcConnection(strConnMain);
        
        }

        /// <summary>
        /// 取得DbTransaction
        /// </summary>
        /// <returns></returns>
        public DbTransaction GetTransaction()
        {
            //若不使用Transcation機制則回傳null
            if (_useTranscation)
            {
                if(_conn.State== ConnectionState.Closed)
                    _conn.Open();
                _tran ??= _conn.BeginTransaction();
            }
            return _tran;
        }

        /// <summary>
        /// 使用交易機制
        /// </summary>
        public void BeginTransaction()
        {
            _useTranscation = true;
        }

        /// <summary>
        /// Transaction Commit
        /// </summary>
        public virtual void Commit()
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
        public virtual void Rollback()
        {
            _useTranscation = false;
            GetTransaction()?.Rollback();
            GetTransaction()?.Dispose();
            GetConnection()?.Close();
            _tran = null;
        }
    }
}
