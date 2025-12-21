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
    public class EmpAgentDac : Dac, IEmpAgentDac
    {
        public EmpAgentDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public async Task<IList<EmpAgentModel>> Read()
        {
            string sql = $@"
                SELECT 
                    SID,
                    EMP_AGENT.USER_ID,
                    AGENT_ID,
                    AGENT_FROM,
                    AGENT_TO,
                    EMP_AGENT.DEL_FLG,
                    USER_NAME AS [AGENT_NAME] 
                FROM  dbo.EMP_AGENT (NOLOCK)
                INNER JOIN dbo.EMP_USER (NOLOCK) ON EMP_USER.USER_ID=EMP_AGENT.AGENT_ID
                WHERE {DTNow} BETWEEN AGENT_FROM AND DATEADD(DAY,1,AGENT_TO) AND EMP_AGENT.DEL_FLG=0";
            return await ExecuteQueryAsync<EmpAgentModel>(sql);
        }

        public async Task<IList<EmpAgentModel>> ReadUserIsAgent(EmpAgentQryModel search)
        {
            string sql = @"
                DECLARE @AGENT_FROM DateTime = ?AGENT_FROM?;
                DECLARE @AGENT_TO DateTime = ?AGENT_TO?;
                DECLARE @USER_ID nvarchar(20) = ?USER_ID?;
                SELECT  SID,
                        USER_ID,
                        AGENT_ID,
                        AGENT_FROM,
                        AGENT_TO,
                        DEL_FLG
                FROM dbo.EMP_AGENT (NOLOCK)
                WHERE DEL_FLG=0 and USER_ID=@USER_ID
                AND ((AGENT_FROM BETWEEN @AGENT_FROM AND DATEADD(day,1,@AGENT_TO)) or (AGENT_TO BETWEEN @AGENT_FROM AND DATEADD(day,1,@AGENT_TO)))";
            return await ExecuteQueryAsync<EmpAgentModel>(sql, search);
        }

        public async Task Insert(EmpAgentMdfModel agent)
        {
            string sql = $@"
                INSERT INTO dbo.EMP_AGENT( 
                        USER_ID,
                        AGENT_ID,
                        AGENT_FROM,
                        AGENT_TO,
                        CRT_DATE,
                        CRT_USER,
                        MDF_DATE,
                        MDF_USER )
                VALUES( ?USER_ID?,
                        ?AGENT_ID?,
                        ?AGENT_FROM?,
                        ?AGENT_TO?,
                        {DTNow},
                        ?CRT_USER?,
                        {DTNow},
                        ?MDF_USER? )";
            await ExecuteCommandAsync(sql, agent);
        }

        public async Task Delete(long sid, string userId)
        {
            string sql = $@"
                UPDATE dbo.EMP_AGENT 
                SET DEL_FLG=1,
                    MDF_DATE={DTNow},
                    MDF_USER=?MDF_USER?
                WHERE SID=?SID?";
            await ExecuteCommandAsync(sql, new EmpAgentMdfModel() { SID = sid, MDF_USER = userId });
        }
    }
}
