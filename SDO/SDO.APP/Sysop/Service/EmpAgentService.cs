using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Transactions;
using SDO.Utils;

namespace SDO.Services
{
    public class EmpAgentService : Service, IEmpAgentService
    {
        private readonly IEmpAgentDac dac;
        private readonly IUserProfile userProfile;

        public EmpAgentService(IEmpAgentDac dac, IUserProfile userProfile)
        {
            this.dac = dac;
            this.userProfile = userProfile;
        }

        public async Task<IList<EmpAgentModel>> Read()
        {
            return await dac.Read();
        }

        public async Task<RtnResultModel> Create(EmpAgentMdfModel agent)
        {
            //搜尋是否已有代理
            ParseUtil.Parse(agent, out EmpAgentQryModel search);
            IList<EmpAgentModel> userAgent = await dac.ReadUserIsAgent(search);
            if (userAgent.Any())
                return ChangeResult(false, string.Format(i18N.Message.R10, userAgent.Min(p => p.AGENT_FROM), userAgent.Max(p => p.AGENT_TO)));

            agent.CRT_USER = userProfile.GetLoginUser().USER_ID;
            agent.MDF_USER = userProfile.GetLoginUser().USER_ID;

            await dac.Insert(agent);

            return ChangeResult(ResultType.Success | ResultType.Insert);
        }

        public async Task<RtnResultModel> Delete(string sid)
        {
            await dac.Delete(Convert.ToInt64(sid), userProfile.GetLoginUser().USER_ID);
            return ChangeResult(ResultType.Success | ResultType.Delete);
        }
    }
}

