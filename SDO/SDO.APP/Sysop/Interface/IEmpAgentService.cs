using System.Threading.Tasks;
using SDO.Models;
using System.Collections.Generic;

namespace SDO.Services
{
    public interface IEmpAgentService
    {
        Task<RtnResultModel> Create(EmpAgentMdfModel agent);
        Task<RtnResultModel> Delete(string sid);
        Task<IList<EmpAgentModel>> Read();
    }
}