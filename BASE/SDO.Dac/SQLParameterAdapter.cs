using System.Data;
using System.Collections.Generic;
using System.Linq;
using Dapper;
namespace SDO.Utils
{
    /// <summary>
    /// ODBC PARAMETER 2 SQL PARAMETER
    /// </summary>
    public class SQLParameterAdapter:IParameterAdaptor
    {
        /// <summary>
        /// 替換sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string ConvertSql(string sql, object param = null)
        {
            if (param is IEnumerable<object>)
                return ConvertSql(sql, ((IEnumerable<object>)param).FirstOrDefault());
            if (param == null)
                return sql;
            if(param is DynamicParameters)
                return ReplaceSQL(sql, ((DynamicParameters)param).ParameterNames.ToArray());
            return ReplaceSQL(sql, param);
        }
        /// <summary>
        /// List參數替換sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string ConvertSql(string sql, IEnumerable<object> param = null)
        {
            if (param == null|| !param.Any())
                return sql;
            else if (param.FirstOrDefault() is DynamicParameters)
                return ReplaceSQL(sql, ((DynamicParameters)param).ParameterNames.ToArray());
            else
                return ReplaceSQL(sql, param.FirstOrDefault());
        }
        /// <summary>
        /// 替換sql參數
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        protected virtual string ReplaceSQL(string sql, object param)
        {
            string[] keys;
            if (param is string[])
            {
                keys = (string[])param;
            }
            else
                keys = param.GetType().GetProperties().Select(x => x.Name).ToArray();
            foreach (var k in keys)
            {
              sql=  sql.Replace($"?{k}?", $"@{k}");
            }
            return sql;
        }

    }
}
