using Dapper;
using SDO.Models;
using SDO.SqlMaker;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace SDO.Dac
{
    public abstract class DacNoTrace : Dac, IDac, IDisposable
    {
        private readonly IConnectionControlCenter connectionControlCenter;

        //若是代理人，需調整UserId格式
        public DacNoTrace(IConnectionControlCenter connectionControlCenter):base(connectionControlCenter)
        {
            this.connectionControlCenter = connectionControlCenter;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        protected override void ExecTrace()
        {

        }
    }

    public abstract class DacNoTrace<TSqlMaker> : Dac
        where TSqlMaker : ISqlMaker
    {
        protected readonly TSqlMaker sqlMaker;

        public Dac(IConnectionControlCenter connectionControlCenter,
                   TSqlMaker sqlMaker) : base(connectionControlCenter)
        {
            this.sqlMaker = sqlMaker;
        }
    }
}
