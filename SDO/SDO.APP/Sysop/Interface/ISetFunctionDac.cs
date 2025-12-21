using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface ISetFunctionDac : IDac
    {
        Task Delete(string functionId)=>null;
        Task Insert(SetFunctionModel function) => null;
        Task<IList<SetFunctionModel>> ReadByFunctionRoot() => null;
        Task<IList<SetFunctionModel>> ReadByGroup(string parentId) => null;
        Task<SetFunctionModel> ReadById(string functionId, bool filtDelFlg = false, bool delFlg = false) => null;
        Task<IList<SetFunctionModel>> ReadByRight(string rightId) => null;
        Task<IList<SetFunctionModel>> ReadByUser(string userId) => null;
        Task<IList<SetFunctionModel>> ReadByUser(string ApId, string userId, string CompId="GSS") => null;
        Task<IList<SetFunctionModel>> Read() => null;
        Task Update(SetFunctionModel function) => null;
        Task UpdateSortorder(IEnumerable<SetFunctionModel> functions) => null;
    }
}