using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SDO.Dac;
using SDO.Models;

namespace SDO.Utils
{
    public class UserProfile : IUserProfile
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private IUserData user;
        private bool _hasLogged;

        public UserProfile(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            user = new UserDataModel();
            _hasLogged = false;
        }

        public void SetLoginUser(IUserData user)
        {
            this.user = user;
            this.user.USER_IP =httpContextAccessor?.HttpContext.Connection.RemoteIpAddress.ToString(); 
            //this.user.USER_MACHINE= System.Net.Dns.GetHostEntry(this.user.USER_IP).HostName;
            SetHasLogged(true);
        }

        public IUserData GetLoginUser()
        {
            return user;
        }

        public void Logout()
        {
            SetHasLogged(false);
        }

        public bool hasLogged => this._hasLogged;

        private void SetHasLogged(bool hasLogged)
        {
            this._hasLogged = hasLogged;
        }
    }
}
