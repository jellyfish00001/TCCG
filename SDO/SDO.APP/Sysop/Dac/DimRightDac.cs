using Microsoft.AspNetCore.Http;
using SDO.Models;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SDO.Dac
{
    public class DimRightDac : Dac, IDimRightDac
    {
        public DimRightDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public async Task<IList<DimRightModel>> Read()
        {
            string sql = @"
                SELECT 
                    RIGHT_ID,
                    RIGHT_NAME,
                    DEL_FLG
                FROM dbo.DIM_RIGHT (NOLOCK) 
                WHERE DEL_FLG=0";
            return await ExecuteQueryAsync<DimRightModel>(sql);
        }

        public async Task<DimRightModel> ReadById(string rightId, bool delFlg)
        {
            string sql = @"
                SELECT 
                    RIGHT_ID,
                    RIGHT_NAME,
                    DEL_FLG
                FROM dbo.DIM_RIGHT (NOLOCK) 
                WHERE {0} RIGHT_ID=?RIGHT_ID?";
            sql = string.Format(sql, delFlg ? "DEL_FLG = ?DEL_FLG? AND" : "");
            return (await ExecuteQueryAsync<DimRightModel>(sql, new { RIGHT_ID = rightId, DEL_FLG = delFlg })).SingleOrDefault();
        }

        public async Task<IList<DimRightModel>> ReadByRole(string roleId)
        {
            string sql = @"
                SELECT 
                    DIM_RIGHT.RIGHT_ID,
                    RIGHT_NAME,
                    RIGHT_ID
                FROM dbo.DIM_RIGHT (NOLOCK)
                INNER JOIN dbo.MAP_ROLE_RIGHT (NOLOCK) ON MAP_ROLE_RIGHT.RIGHT_ID=DIM_RIGHT.RIGHT_ID
                WHERE DIM_RIGHT.RIGHT_ID=0 
                AND MAP_ROLE_RIGHT.ROLE_ID=?ROLE_ID?";
            return await ExecuteQueryAsync<DimRightModel>(sql, new { ROLE_ID = roleId });
        }

        public async Task Insert(DimRightModel right)
        {
            string sql = $@"
                INSERT INTO dbo.DIM_RIGHT(
                        RIGHT_ID,
                        RIGHT_NAME,
                        CRT_DATE,
                        CRT_USER,
                        MDF_DATE,
                        MDF_USER ) 
                VALUES( ?RIGHT_ID?,
                        ?RIGHT_NAME?,
                        {DTNow},
                        ?CRT_USER?,
                        {DTNow},
                        ?MDF_USER?)";
            await ExecuteCommandAsync(sql, right);
        }

        public async Task Update(DimRightModel right)
        {
            string sql = $@"
                UPDATE dbo.DIM_RIGHT 
                SET DEL_FLG=0,
                    RIGHT_NAME=?RIGHT_NAME?,
                    MDF_DATE={DTNow},
                    MDF_USER=?MDF_USER?
                WHERE RIGHT_ID=?RIGHT_ID?";
            await ExecuteCommandAsync(sql, right);
        }

        public async Task Delete(string rightId)
        {
            string sql = $@"
                UPDATE dbo.DIM_RIGHT 
                SET DEL_FLG = 1,
                    MDF_DATE = {DTNow},
                    MDF_USER = ?MDF_USER?
                WHERE RIGHT_ID = ?RIGHT_ID?";
            await ExecuteCommandAsync(sql, new DimRightModel() { RIGHT_ID = rightId });
        }

        public async Task InsertMap(IEnumerable<MapRightFunctionModel> maps)
        {
            string sql = $@"
                INSERT INTO dbo.MAP_RIGHT_FUNCTION( 
                        RIGHT_ID,
                        FUNCTION_ID,
                        CRT_DATE,
                        CRT_USER ) 
                VALUES( ?RIGHT_ID?,
                        ?FUNCTION_ID?,
                        {DTNow},
                        ?CRT_USER?)";
            await ExecuteCommandAsync(sql, maps);
        }

        public async Task DeleteMap(string rightId)
        {
            string sql = @"
                DELETE FROM dbo.MAP_RIGHT_FUNCTION 
                WHERE RIGHT_ID=?RIGHT_ID? ";
            await ExecuteCommandAsync(sql, new { RIGHT_ID = rightId });
        }
    }
}
