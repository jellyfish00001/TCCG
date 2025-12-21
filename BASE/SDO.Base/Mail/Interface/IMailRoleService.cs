using System.Collections.Generic;
using System.Threading.Tasks;
using SDO.Models;

namespace SDO.Services
{
    public interface IMailRoleService
    {
        Task<RtnResultModel> Create(MailRoleMdfModel role);
        Task<RtnResultModel> Delete(string roleId);
        Task<IList<UserCountModel>> Read();
        Task<MailRoleModel> ReadById(string roleId);
        Task<IList<MailRoleuUserModel>> ReadUsers(string roleId);
        Task<RtnResultModel> Update(MailRoleMdfModel role);
    }
}