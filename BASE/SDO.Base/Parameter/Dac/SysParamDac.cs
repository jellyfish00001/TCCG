using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Dac;
using SDO.Dac.Models;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class SysParamDac : Dac,  ISysParamDac
    {
        public SysParamDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public SysParamDac(IConnectionControlCenter connectionControlCenter,
                ISqlTrace trace,
                IParameterAdaptor parameterAdaptor, 
                IUserData userData,
                IConfiguration configuration) : base(connectionControlCenter, trace, parameterAdaptor, userData, configuration)
        {
        }

        public async Task<SetParamModel> GetSysParam(string setItem, string setType)
        {
            string sql = @"
                SELECT 
                    SET_ITEM,
                    SET_TYPE,
                    SET_VALUE,
                    MEMO
                FROM SET_PARAM (NOLOCK) 
                WHERE DEL_FLG = 0 
                AND SET_ITEM = ?SET_ITEM? 
                AND SET_TYPE = ?SET_TYPE?";
            return (await ExecuteQueryAsync<SetParamModel>(sql, new { SET_ITEM = setItem, SET_TYPE = setType }, MainDBKey)).FirstOrDefault();
        }

        public async Task<IList<SetParamModel>> GetSysParams(string setItem)
        {
            string sql = @"
                SELECT 
                    A.SET_ITEM,
                    A.SET_TYPE,
                    A.SET_VALUE,
                    A.MEMO,
                    B.EDITABLE 
                FROM SET_PARAM A (NOLOCK)
                INNER JOIN SET_PARAMITEM B (NOLOCK) ON B.SET_ITEM = A.SET_ITEM 
                WHERE  A.DEL_FLG = 0 AND A.SET_ITEM = ?SET_ITEM?";
            return await ExecuteQueryAsync<SetParamModel>(sql, new { SET_ITEM = setItem }, MainDBKey);
        }

        public async Task<IList<SetParamModel>> GetSysParams()
        {
            string sql = @"
                SELECT 
                    SET_ITEM,
                    SET_TYPE,
                    SET_VALUE,
                    MEMO
                FROM SET_PARAM (NOLOCK) 
                WHERE DEL_FLG = 0";
            return await ExecuteQueryAsync<SetParamModel>(sql);
        }

        public async Task<IList<SetParamModel>> GetSysParams(int fromWhere = 0)
        {
            string sql = @"
                SELECT 
                    SET_ITEM,
                    SET_TYPE,
                    SET_VALUE,
                    MEMO
                FROM SET_PARAM (NOLOCK) 
                WHERE DEL_FLG = 0";
            string DB = MainDBKey;
            // 前端呼叫指定DB
            switch ((DBConnection)fromWhere)
            {
                case DBConnection.SCDBKey:
                    DB = SCDBKey;
                    break;
                case DBConnection.RISDBKey:
                    DB = RISDBKey;
                    break;
                case DBConnection.IPCDBKey:
                    DB = IPCDBKey;
                    break;
                case DBConnection.INNDBKey:
                    DB = INNDBKey;
                    break;
                case DBConnection.PWSDBKey:
                    DB = PWSDBKey;
                    break;
                case DBConnection.RDDBKey:
                    DB = RDDBKey;
                    break;
            }
            return await ExecuteQueryAsync<SetParamModel>(sql,null, DB);
        }

        public async Task<GridModel<SetParamItemModel>> GetSysParamItems(int skip, int take, string orderByField, string dir)
        {
            const string sqlTemplate1 = @"SELECT /**select**/ FROM dbo.SET_PARAMITEM (nolock)  /**where**/ /**orderby**/
                                            OFFSET ?SKIP? ROWS
					                         FETCH NEXT ?TAKE? ROWS ONLY;
                                          SELECT  count(SET_ITEM)
                                            FROM  dbo.SET_PARAMITEM 
                                             /**where**/";
            var sqlBuilder = new SqlBuilder();
            sqlBuilder.Select("SET_ITEM");
            sqlBuilder.Select("SET_ITEM_NAME");
            sqlBuilder.Select("MEMO");
            sqlBuilder.Select("EDITABLE");
            sqlBuilder.Where("DEL_FLG=0");
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("DIR", dir, DbType.String);
            parameters.Add("SKIP", skip, DbType.Int32);
            parameters.Add("TAKE", take, DbType.Int32);
            //檢查orderByField欄位是否存在於model裡，以及驗證dir字串
            if (typeof(SetParamItemModel).GetProperty(orderByField.ToUpper()) != null && (dir == "asc" || dir == "desc"))
            {
                sqlBuilder.OrderBy($"{orderByField} {dir}");
            }
            var template = sqlBuilder.AddTemplate(sqlTemplate1);
            var sql = template.RawSql;



            return await ExecuteQueryMultipleAsync<SetParamItemModel>(
                sql,
                parameters
                );
        }

       

        public async Task<IList<SetParamItemModel>> GetSysParamItems()
        {
            string sql = @"
                SELECT 
                    SET_ITEM,
                    SET_ITEM_NAME,
                    MEMO,
                    EDITABLE
                FROM SET_PARAMITEM (NOLOCK) 
                WHERE DEL_FLG = 0";
            return await ExecuteQueryAsync<SetParamItemModel>(sql);
        }

        public async Task<SetParamItemModel> GetSysParamItem(string setItem)
        {
            string sql = @"
                SELECT 
                    SET_ITEM,
                    SET_ITEM_NAME,
                    MEMO,
                    EDITABLE
                FROM SET_PARAMITEM (NOLOCK)
                WHERE DEL_FLG = 0 
                AND SET_ITEM = @SET_ITEM";
            return (await ExecuteQueryAsync<SetParamItemModel>(sql, new { SET_ITEM = setItem })).FirstOrDefault();
        }
    }
}
