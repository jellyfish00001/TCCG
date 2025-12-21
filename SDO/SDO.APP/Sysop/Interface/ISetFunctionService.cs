using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface ISetFunctionService
    {
        Task<RtnResultModel> Create(SetFunctionModel function)=>null;
        Task<RtnResultModel> Delete(string id) => null;
        Task<IList<SetFunctionModel>> Read() => null;
        Task<IList<SetFunctionModel>> ReadByGroup(SetFunctionQryModel model) => null;
        Task<SetFunctionModel> ReadById(string functionId) => null;
        Task<IList<SetFunctionModel>> ReadByUser(string userId, string apid);
        Task<RtnResultModel> Update(SetFunctionModel function) => null;
        Task<RtnResultModel> SetSortorder(string[] functionIds) => null;
        Task<IList<SetFunctionListModel>> ReadFunctionList(string apid) => null;
        Task<IList<SetFunctionModel>> ReadByRight(string rightId) => null;
        Task<IList<SetFunctionModel>> ReadByFunctionRoot() => null;
    }
}