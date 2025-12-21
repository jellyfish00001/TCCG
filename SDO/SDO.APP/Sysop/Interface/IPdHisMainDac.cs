using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IPdHisMainDac : IDac
    {
        Task<bool> CheckPdSamePassTime(string userId, string userPd, int sameTime,string Table);
        Task Create(string userId, string userPd);
        Task<IList<ScPswdHismModel>> ReadById(string userId);
    }
}