using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IMailRoleDac : IDac
    {
        Task Delete(string roleId);
        Task DeleteMap(string roleId);
        Task Insert(MailRoleMdfModel role);
        Task InsertMap(IEnumerable<MapUserMailRoleMdfModel> maps);
        Task<IList<UserCountModel>> Read();
        Task<MailRoleModel> ReadById(string roleId);
        Task<IList<MailRoleuUserModel>> ReadUsers(string roleId);
        Task Update(MailRoleMdfModel role);
    }
}