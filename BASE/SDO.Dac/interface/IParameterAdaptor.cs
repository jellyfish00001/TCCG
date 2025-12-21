using System.Collections.Generic;

namespace SDO.Utils
{
    public interface IParameterAdaptor
    {
        /// <summary>
        /// 替換sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        string ConvertSql(string sql, object param = null);
        /// <summary>
        /// List參數替換sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        string ConvertSql(string sql, IEnumerable<object> param = null);
    }
}