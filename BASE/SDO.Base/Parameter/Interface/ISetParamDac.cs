using SDO.Models;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface ISetParamDac : IDac
    {
        Task<SetParamModel> CheckExist(string setItem, string setType);
        Task<SetParamItemModel> CheckItemExist(string setItem);
        Task Delete(string setItem, string setType);
        Task DeleteItem(string setItem);
        Task Insert(SetParamModel model);
        Task InsertItem(SetParamItemModel model);
        Task Update(SetParamModel model);
        Task UpdateItem(SetParamItemModel model);
    }
}