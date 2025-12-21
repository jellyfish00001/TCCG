using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SDO.Dac;
using SDO.Utils;

namespace SDO.Services
{
    public class AccessService : IAccessService
    {
        private readonly IVUserFunctionDac vUserFunctionDac;
        private readonly IUserProfile userProfile;

        public AccessService(IUserProfile userProfile, IVUserFunctionDac vUserFunctionDac)
        {
            this.vUserFunctionDac = vUserFunctionDac;
            this.userProfile = userProfile;
        }

        public async Task<bool> CheckControllerAccess(string controllerName)
        {
            if (string.IsNullOrWhiteSpace(controllerName) || !userProfile.hasLogged)
                return false;
            return await CheckControllerAccess(userProfile.GetLoginUser().USER_ID, controllerName);
        }

        public async Task<bool> CheckControllerAccess(string userId, string controllerName)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
                return false;
            return await vUserFunctionDac.CheckControllerAccess(userId, controllerName);
        }
    }
}
