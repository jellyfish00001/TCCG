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
    public class DimRoleDac : Dac, IDimRoleDac
    {
        public DimRoleDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public async Task<IList<DimRoleModel>> Read()
        {
            string sql = @"
                SELECT 
                    ROLE_ID,
                    ROLE_NAME,
                    DEL_FLG
                FROM DIM_ROLE (NOLOCK)
                WHERE DEL_FLG=0";
            return await ExecuteQueryAsync<DimRoleModel>(sql);
        }

        public async Task<DimRoleModel> ReadById(string roleId, bool filteDelFlg, bool delFlg = false)
        {
            string sql = @"
                SELECT 
                    ROLE_ID,
                    ROLE_NAME,
                    DEL_FLG
                FROM dbo.DIM_ROLE (NOLOCK)
                WHERE {0} ROLE_ID=?ROLE_ID?";

            sql = string.Format(sql, filteDelFlg ? "DEL_FLG = ?DEL_FLG? AND" : "");

            return (await ExecuteQueryAsync<DimRoleModel>(sql, new { ROLE_ID = roleId, DEL_FLG = delFlg })).SingleOrDefault();
        }

        public async Task<IList<DimRoleModel>> ReadByUser(string userId)
        {
            string sql = @"
                SELECT 
                    DIM_ROLE.ROLE_ID,
                    ROLE_NAME,
                    DEL_FLG
                FROM dbo.DIM_ROLE (NOLOCK)
                INNER JOIN dbo.MAP_USER_ROLE (NOLOCK) ON MAP_USER_ROLE.ROLE_ID=DIM_ROLE.ROLE_ID
                WHERE DIM_ROLE.DEL_FLG=0 AND MAP_USER_ROLE.USER_ID=?USER_ID?";
            return await ExecuteQueryAsync<DimRoleModel>(sql, new { USER_ID = userId });
        }

        public async Task Insert(DimRoleMdfModel role)
        {
            string sql = $@"
                INSERT INTO dbo.DIM_ROLE(
                    ROLE_ID,
                    ROLE_NAME,
                    CRT_DATE,
                    CRT_USER,
                    MDF_DATE,
                    MDF_USER ) 
                VALUES( ?ROLE_ID?,
                    ?ROLE_NAME?,
                    {DTNow},
                    ?CRT_USER?,
                    {DTNow},
                    ?MDF_USER?)";
            await ExecuteCommandAsync(sql, role);
        }

        public async Task Update(DimRoleMdfModel role)
        {
            string sql = $@"
                UPDATE dbo.DIM_ROLE 
                SET DEL_FLG=0,
                    ROLE_NAME=?ROLE_NAME?,
                    MDF_DATE={DTNow},
                    MDF_USER=?MDF_USER?
                WHERE ROLE_ID=?ROLE_ID?";
            await ExecuteCommandAsync(sql, role);
        }

        public async Task Delete(string roleId)
        {
            string sql = $@"
                UPDATE dbo.DIM_ROLE 
                SET DEL_FLG=1,
                    MDF_DATE={DTNow},
                    MDF_USER=?MDF_USER?
                WHERE ROLE_ID=?ROLE_ID?";
            await ExecuteCommandAsync(sql, new DimRoleMdfModel() { ROLE_ID = roleId });
        }

        public async Task DeleteMap(string roleId)
        {
            string sql = @"
                DELETE FROM dbo.MAP_ROLE_RIGHT 
                WHERE USER_ID=?ROLE_ID?";
            await ExecuteCommandAsync(sql, new { ROLE_ID = roleId });
        }

        public async Task InsertMap(IEnumerable<MapRoleRightModel> maps)
        {
            string sql = $@"
                INSERT INTO dbo.MAP_ROLE_RIGHT(
                    ROLE_ID,
                    RIGHT_ID,
                    CRT_DATE,
                    CRT_USER )
                VALUES(?ROLE_ID?,
                    ?RIGHT_ID?,
                    {DTNow},
                    ?CRT_USER?)";
            await ExecuteCommandAsync(sql, maps);
        }
    }
}
