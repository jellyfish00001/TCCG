using SDO.Models;

namespace SDO.DAC
{
    public interface IUser
    {
        EmpUserModel.UserData GetLoginUser();
        
        void SetLoginUser(EmpUserModel.UserData user);

        void Logout();

        bool hasLogged { get; }
    }
}