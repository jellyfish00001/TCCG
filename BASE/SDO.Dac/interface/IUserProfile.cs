using SDO.Models;

namespace SDO.Utils
{
    public interface IUserProfile
    {
        IUserData GetLoginUser();
        
        void SetLoginUser(IUserData user);

        void Logout();

        bool hasLogged { get; }
    }
}