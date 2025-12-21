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
    public class MailRoleDac : Dac, IMailRoleDac
    {
        public MailRoleDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public MailRoleDac(IConnectionControlCenter connectionControlCenter,
              ISqlTrace trace,
              IParameterAdaptor parameterAdaptor, 
              IUserData userData,
              IConfiguration configuration) : base(connectionControlCenter, trace, parameterAdaptor, userData, configuration)
        {
        }

        public async Task<IList<UserCountModel>> Read()
        {
            string sql = @"
                SELECT	
                    ROLE_ID, 
                    ROLE_NAME, 
                    DEL_FLG,  
                    (
	                    SELECT	COUNT([USER_ID])
	                    FROM	MAP_USER_MAILROLE
	                    WHERE	ROLE_ID = MR.ROLE_ID
                    ) AS USER_COUNT
                FROM	MAIL_ROLE AS MR
                WHERE	DEL_FLG=0";
            return await ExecuteQueryAsync<UserCountModel>(sql);
        }

        public async Task<MailRoleModel> ReadById(string roleId)
        {
            string sql = @"
                SELECT	ROLE_ID, ROLE_NAME, DEL_FLG  
                FROM	dbo.MAIL_ROLE 
                WHERE	ROLE_ID = ?ROLE_ID?";
            return (await ExecuteQueryAsync<MailRoleModel>(sql, new { ROLE_ID = roleId })).SingleOrDefault();
        }

        public async Task<IList<MailRoleuUserModel>> ReadUsers(string roleId)
        {
            string sql = @"
                SELECT	    
                    MUM.[USER_ID], 
                    EU.[USER_NAME],
                    MUM.[ROLE_ID]
                FROM dbo.MAP_USER_MAILROLE AS MUM
                LEFT JOIN  EMP_USER AS EU 
                    ON  MUM.[USER_ID] = EU.[USER_ID]
                WHERE   MUM.ROLE_ID = ?ROLE_ID?";
            return await ExecuteQueryAsync<MailRoleuUserModel>(sql, new { ROLE_ID = roleId });
        }

        public async Task Insert(MailRoleMdfModel role)
        {
            string sql = @"
                INSERT INTO	dbo.MAIL_ROLE
                (
                    ROLE_ID, 
                    ROLE_NAME, 
                    CRT_DATE, 
                    CRT_USER, 
                    MDF_DATE, 
                    MDF_USER
                ) VALUES (
                    ?ROLE_ID?, 
                    ?ROLE_NAME?, 
                    DATEADD(HH,8,GETUTCDATE()) , 
                    ?CRT_USER?, 
                    DATEADD(HH,8,GETUTCDATE()) , 
                    ?MDF_USER?
                )";
            await ExecuteCommandAsync(sql, role);
        }

        public async Task Update(MailRoleMdfModel role)
        {
            string sql = @"
                UPDATE	dbo.MAIL_ROLE 
                SET	DEL_FLG = 0,
                    ROLE_NAME = ?ROLE_NAME?,
                    MDF_DATE = DATEADD(HH,8,GETUTCDATE()) ,
                    MDF_USER = ?MDF_USER?
                WHERE	ROLE_ID = ?ROLE_ID?";
            await ExecuteCommandAsync(sql, role);
        }

        public async Task Delete(string roleId)
        {
            string sql = @"
                UPDATE  dbo.MAIL_ROLE 
                SET   DEL_FLG = 1,
                    MDF_DATE = DATEADD(HH,8,GETUTCDATE()) ,
                    MDF_USER = ?MDF_USER? 
                WHERE ROLE_ID = ?ROLE_ID?";
            await ExecuteCommandAsync(sql, new MailRoleMdfModel() { ROLE_ID = roleId });
        }

        public async Task DeleteMap(string roleId)
        {
            string sql = @"
                DELETE FROM dbo.MAP_USER_MAILROLE 
                WHERE ROLE_ID = ?ROLE_ID?";
            await ExecuteCommandAsync(sql, new { ROLE_ID = roleId });
        }

        public async Task InsertMap(IEnumerable<MapUserMailRoleMdfModel> maps)
        {
            string sql = @"
                INSERT INTO	dbo.MAP_USER_MAILROLE
                (	
	                ROLE_ID,
                    USER_ID, 
                    CRT_DATE, 
                    CRT_USER
                ) VALUES (
	                ?ROLE_ID?, 
                    ?USER_ID?, 
                    DATEADD(HH,8,GETUTCDATE()) , 
                    ?CRT_USER?
                )";
            await ExecuteCommandAsync(sql, maps);
        }
    }
}
