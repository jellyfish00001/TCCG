using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security;
namespace SDO.Dac
{
    public interface IEmpUserDac : IDac
    {
        Task<UserDataModel> CheckExists(string userId, bool delFlg);
        Task Delete(string userId, string del_reason);
        Task DeleteMap(string userId);
        Task<UserDataModel> GetUserById(string id, bool delFlg = false);
        Task<IList<UserDataModel>> GetUserByIds(IEnumerable<string> Ids);
        Task<IList<UserDataModel>> GetUserByOrg(string orgId = "");
        Task Insert(EmpUserModel createModel);
        Task InsertMap(IEnumerable<MapUserRoleModel> maps);
        Task<UserDataModel> Login(LoginModel Login);
        Task<IList<UserDataModel>> Read(EmpUserReadModel model);
        Task<IList<SCUserModel>> ReadUserAgent(string userId);
        Task Update(EmpUserModel createModel);
        Task UpdateLoginInfo(string userId, bool login, int conFault = 0, int nonconFault = 0, bool flag = false,string Table= "EMP_USER");
        Task UpdatePassword(string userId, string userPd);
        Task DeleteMapOrgUser(string userId);
        Task InsertMapOrgUser(string orgId, string userId, string crtUser);
        Task InsertLoginLog(LoginLogModel model);
        Task<List<LoginLogModel>> GetLoginLog(string USER_ID);
    }
}