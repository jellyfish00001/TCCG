using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class SetFunctionDac : Dac, ISetFunctionDac
    {
        public SetFunctionDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public async Task<IList<SetFunctionModel>> ReadByGroup(string parentId)
        {
            string sql = @"
                SELECT 
                    FUNCTION_ID,
                    FUNCTION_NAME,
                    FUNCTION_URL,
                    FUNCTION_CONTROLLER,
                    PARENT_ID,
                    SORT_ID,
                    DEL_FLG
                FROM SET_FUNCTION ((NOLOCK)
                WHERE DEL_FLG=0 AND PARENT_ID=?PARENT_ID?
                ORDER BY SORT_ID";
            return await ExecuteQueryAsync<SetFunctionModel>(sql, new { PARENT_ID = parentId ?? string.Empty });
        }

        public async Task<IList<SetFunctionModel>> ReadByFunctionRoot()
        {
            string sql = @"
                SELECT 
                    FUNCTION_ID+'-'+ FUNCTION_NAME AS FUNCTION_NAME,
                    FUNCTION_ID,
                    SORT_ID
                FROM SET_FUNCTION (NOLOCK)
                WHERE DEL_FLG=0 AND PARENT_ID=''
                ORDER BY SORT_ID";
            return await ExecuteQueryAsync<SetFunctionModel>(sql);
        }

        public async Task<SetFunctionModel> ReadById(string functionId, bool filtDelFlg = false, bool delFlg = false)
        {
            string sql = @"
                SELECT
                    FUNCTION_ID,
                    FUNCTION_NAME,
                    FUNCTION_URL,
                    FUNCTION_CONTROLLER,
                    PARENT_ID,
                    SORT_ID,
                    DEL_FLG
                FROM SET_FUNCTION (NOLOCK)
                WHERE {0} FUNCTION_ID=?FUNCTION_ID? 
                ORDER BY SORT_ID";
            sql = string.Format(sql, delFlg ? "DEL_FLG = ?DEL_FLG? AND" : "");
            return (await ExecuteQueryAsync<SetFunctionModel>(sql, new { DEL_FLG = delFlg ? 1 : 0, FUNCTION_ID = functionId })).SingleOrDefault();
        }

        public async Task<IList<SetFunctionModel>> ReadByUser(string userId)
        {
            string sql = @"
                SELECT 
                    FUNCTION_ID,
                    FUNCTION_NAME,
                    FUNCTION_URL,
                    FUNCTION_CONTROLLER,
                    PARENT_ID,
                    SORT_ID,
                    DEL_FLG
                FROM SET_FUNCTION (NOLOCK)
                WHERE ISNULL(FUNCTION_ID,'')='' AND DEL_FLG=0 AND FUNCTION_ID IN (SELECT PARENT_ID FROM dbo.VW_USER_FUNCTION WHERE USER_ID=?USER_ID?)
                UNION 
                SELECT 
                    FUNCTION_ID,
                    FUNCTION_NAME,
                    FUNCTION_URL,
                    FUNCTION_CONTROLLER,
                    PARENT_ID,
                    SORT_ID,
                    DEL_FLG
                FROM dbo.VW_USER_FUNCTION (NOLOCK) WHERE USER_ID=?USER_ID?";
            return await ExecuteQueryAsync<SetFunctionModel>(sql, new { USER_ID = userId });
        }

        public async Task<IList<SetFunctionModel>> ReadByRight(string rightId)
        {
            string sql = @"
                SELECT 
                    SET_FUNCTION.FUNCTION_ID,
                    FUNCTION_NAME,
                    FUNCTION_URL,
                    FUNCTION_CONTROLLER,
                    PARENT_ID,
                    SORT_ID,
                    DEL_FLG
                FROM SET_FUNCTION (NOLOCK)
                INNER JOIN MAP_RIGHT_FUNCTION (NOLOCK) ON MAP_RIGHT_FUNCTION.FUNCTION_ID = SET_FUNCTION.FUNCTION_ID
                WHERE dbo.SET_FUNCTION.DEL_FLG = 0 AND MAP_RIGHT_FUNCTION.RIGHT_ID=?RIGHT_ID? 
                ORDER BY SORT_ID";
            return await ExecuteQueryAsync<SetFunctionModel>(sql, new { RIGHT_ID = rightId });
        }

        public async Task<IList<SetFunctionModel>> Read()
        {
            string sql = @"
                SELECT 
                    FUNCTION_ID,
                    FUNCTION_NAME,
                    FUNCTION_URL,
                    FUNCTION_CONTROLLER,
                    PARENT_ID,
                    SORT_ID,  
                    DEL_FLG
                FROM   SET_FUNCTION (NOLOCK)
                WHERE  DEL_FLG=0 
                ORDER BY SORT_ID";
            return await ExecuteQueryAsync<SetFunctionModel>(sql);
        }

        public async Task Insert(SetFunctionModel function)
        {
            string sql = $@"
                INSERT INTO dbo.SET_FUNCTION( 
                        FUNCTION_ID,
                        FUNCTION_NAME,
                        FUNCTION_URL,
                        FUNCTION_CONTROLLER,
                        PARENT_ID,
                        CRT_DATE,
                        CRT_USER,
                        MDF_DATE,
                        MDF_USER)
                VALUES( ?FUNCTION_ID?,
                        ?FUNCTION_NAME?,
                        ?FUNCTION_URL?,
                        ?FUNCTION_CONTROLLER?,
                        ?PARENT_ID?,
                        {DTNow},
                        ?CRT_USER?,
                        {DTNow},
                        ?MDF_USER? )";
            await ExecuteCommandAsync(sql, function);
        }

        public async Task Update(SetFunctionModel function)
        {
            string sql = $@"
                UPDATE dbo.SET_FUNCTION 
                SET DEL_FLG=?DEL_FLG?,
                    UNCTION_NAME=?FUNCTION_NAME?,
                    FUNCTION_URL=?FUNCTION_URL?,
                    FUNCTION_CONTROLLER=?FUNCTION_CONTROLLER?,
                    PARENT_ID=?PARENT_ID?,
                    MDF_DATE={DTNow},
                    MDF_USER=?MDF_USER?
                WHERE FUNCTION_ID=?FUNCTION_ID?";
            await ExecuteCommandAsync(sql, function);
        }

        public async Task Delete(string functionId)
        {
            string sql = $@"
                UPDATE dbo.SET_FUNCTION 
                SET DEL_FLG=1,
                    MDF_DATE={DTNow},
                    MDF_USER=?MDF_USER?
                WHERE FUNCTION_ID=?FUNCTION_ID?";
            await ExecuteCommandAsync(sql, new SetFunctionModel() { FUNCTION_ID = functionId });
        }

        public async Task UpdateSortorder(IEnumerable<SetFunctionModel> functions)
        {
            string sql = $@"
                UPDATE dbo.SET_FUNCTION 
                SET SORT_ID=?SORT_ID?,
                    MDF_DATE={DTNow},
                    MDF_USER=?MDF_USER?
                WHERE FUNCTION_ID=?FUNCTION_ID?";
            await ExecuteCommandAsync(sql, functions);
        }
    }
}
