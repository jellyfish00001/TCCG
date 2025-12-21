using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IEmpAgentDac : IDac
    {
        Task Delete(long sid, string userId);
        Task Insert(EmpAgentMdfModel agent);
        Task<IList<EmpAgentModel>> Read();
        Task<IList<EmpAgentModel>> ReadUserIsAgent(EmpAgentQryModel search);
    }
}