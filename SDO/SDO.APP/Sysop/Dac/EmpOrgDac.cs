using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class EmpOrgDac : Dac, IEmpOrgDac
    {
        public EmpOrgDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public virtual async Task<IList<EmpOrgModel>> ReadAllOrgs()
        {
            string sql = @"
                WITH cteOrg( ORG_ID, ORG_NAME, PARENT_ID, DEL_FLG  ) AS ( 
                    SELECT  ORG_ID, ORG_NAME, PARENT_ID, DEL_FLG
                    FROM    dbo.EMP_ORG (NOLOCK)
                    WHERE   DEL_FLG = 0 AND PARENT_ID = ''

                    UNION ALL

                    SELECT  sub.ORG_ID, sub.ORG_NAME, sub.PARENT_ID, sub.DEL_FLG
                    FROM    dbo.EMP_ORG sub (NOLOCK)
                    INNER JOIN cteOrg ON sub.PARENT_ID = cteOrg.ORG_ID 
                    WHERE   sub.DEL_FLG = 0 )

                SELECT  ORG_ID, ORG_NAME, PARENT_ID, DEL_FLG
                FROM    cteOrg";
            return await ExecuteQueryAsync<EmpOrgModel>(sql);
        }

        public virtual async Task<EmpOrgModel> ReadChildOrgs(string orgId)
        {
            string sql = @"
                WITH cteOrg( ORG_ID, ORG_NAME, PARENT_ID, DEL_FLG ) AS ( 
                    SELECT ORG_ID, ORG_NAME, PARENT_ID, DEL_FLG
                    FROM EMP_ORG (NOLOCK)
                    WHERE DEL_FLG = 0 AND ORG_ID = ?ORG_ID?
                     
                    UNION ALL
                     
                    SELECT sub.ORG_ID, sub.ORG_NAME, sub.PARENT_ID, sub.DEL_FLG
                    FROM EMP_ORG sub (NOLOCK)
                    INNER JOIN cteOrg ON sub.PARENT_ID = cteOrg.ORG_ID
                    WHERE sub.DEL_FLG = 0
                )
                     
                SELECT ORG_ID, ORG_NAME, PARENT_ID, DEL_FLG
                FROM cteOrg";
            return (await ExecuteQueryAsync<EmpOrgModel>(sql, new { ORG_ID = orgId })).FirstOrDefault();
        }

        public virtual async Task<IList<EmpOrgModel>> ReadByParentOrg(string orgId)
        {
            string sql = @"
                SELECT 
                    ORG_ID,
                    ORG_NAME,
                    PARENT_ID,
                    DEL_FLG,
                    CAST((  SELECT CASE COUNT(0) WHEN 0 THEN 0 ELSE 1 END
                        FROM dbo.EMP_ORG (NOLOCK)
                        WHERE DEL_FLG = 0 AND PARENT_ID = a.ORG_ID) AS bit ) AS HASCHILDREN 
                FROM   dbo.EMP_ORG a (NOLOCK)
                WHERE  DEL_FLG = 0 AND DEL_FLG = ?PARENT_ID?";
            return await ExecuteQueryAsync<EmpOrgModel>(sql, new { PARENT_ID = orgId });
        }

        public virtual async Task<EmpOrgModel> ReadByOrgId(string orgId)
        {
            string sql = @"
                SELECT ORG_ID, ORG_NAME, PARENT_ID, DEL_FLG
                FROM EMP_ORG (NOLOCK)
                WHERE DEL_FLG = 0 AND ORG_ID = ?ORG_ID?";
            return (await ExecuteQueryAsync<EmpOrgModel>(sql, new { ORG_ID = orgId })).FirstOrDefault();
        }

        public virtual async Task<IList<EmpOrgModel>> ReadByOrgIds(string[] orgIds)
        {
            string sql = @"
                SELECT ORG_ID, ORG_NAME, PARENT_ID, DEL_FLG
                FROM EMP_ORG (NOLOCK)
                WHERE DEL_FLG = 0 AND ORG_ID IN ?ORG_ID?";
            return await ExecuteQueryAsync<EmpOrgModel>(sql, new { ORG_ID = orgIds });
        }

        public virtual async Task<EmpOrgModel> ReadByUserId(string userId)
        {
            string sql = @"
                SELECT ORG_ID, ORG_NAME, PARENT_ID, DEL_FLG
                FROM dbo.VW_USER_ORG (NOLOCK)
                WHERE DEL_FLG = 0 AND USER_ID = ?USER_ID?";
            return (await ExecuteQueryAsync<EmpOrgModel>(sql, new { USER_ID = userId })).FirstOrDefault();
        }

        public virtual async Task<EmpOrgModel> ReadForCheck(string ORG_ID)
        {
            string sql = @"
                SELECT ORG_ID, ORG_NAME, PARENT_ID, DEL_FLG
                FROM EMP_ORG (NOLOCK)
                WHERE ORG_ID = ?ORG_ID?";
            return (await ExecuteQueryAsync<EmpOrgModel>(sql, new { ORG_ID = ORG_ID })).FirstOrDefault();
        }

        public async Task Insert(EmpOrgModel empOrg)
        {
            string sql = $@"
                INSERT INTO dbo.EMP_ORG( 
                    ORG_ID,
                    ORG_NAME,
                    PARENT_ID,
                    CRT_DATE,
                    CRT_USER,
                    MDF_DATE,
                    MDF_USER ) 
                VALUES( ?ORG_ID?,
                    ?ORG_NAME?,
                    ?PARENT_ID?,
                    {DTNow},
                    ?CRT_USER?,
                    {DTNow},
                    ?MDF_USER? )";
            await ExecuteCommandAsync(sql, empOrg);
        }

        public async Task Update(EmpOrgModel empOrg)
        {
            string sql = $@"
                UPDATE dbo.EMP_ORG 
                SET DEL_FLG = 0,
                    ORG_NAME = ?ORG_NAME?,
                    PARENT_ID = ?PARENT_ID?,
                    MDF_DATE = {DTNow},
                    MDF_USER = ?MDF_USER?
                WHERE  ORG_ID = ?ORG_ID?";
            await ExecuteCommandAsync(sql, empOrg);
        }

        public async Task Delete(IEnumerable orgs)
        {
            string sql = $@"
                UPDATE dbo.EMP_ORG
                SET DEL_FLG = 1,
                    MDF_DATE = {DTNow},
                    MDF_USER = ?MDF_USER?
                WHERE  ORG_ID = ?ORG_ID?";
            await ExecuteCommandAsync(sql, orgs);
        }

        public async Task InsertMap(IEnumerable<MapOrgUserModel> mapOrgUserDbEditors)
        {
            string sql = $@"
                INSERT INTO dbo.MAP_ORG_USER( 
                    ORG_ID,
                    USER_ID,
                    CRT_DATE,
                    CRT_USER )
                VALUES( 
                    ?ORG_ID?,
                    ?USER_ID?,
                    {DTNow},
                    ?CRT_USER? )";
            await ExecuteCommandAsync(sql, mapOrgUserDbEditors);
        }

        public async Task DeleteMap(IEnumerable<string> orgIds)
        {
            string sql = @"
                DELETE FROM dbo.MAP_ORG_USER 
                WHERE ORG_ID IN ?ORG_ID?";
            await ExecuteCommandAsync(sql, new { ORG_ID = orgIds });
        }
    }
}
