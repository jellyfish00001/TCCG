using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class SCSessionService : Service, ISCSessionService
    {
        private ISCApplicationDac scSessionDac;
        private readonly IUserProfile userProfile;

        public SCSessionService(ISCApplicationDac scSessionDac, IUserProfile userProfile)
        {
            this.scSessionDac = scSessionDac;
            this.userProfile = userProfile;
        }

        /// <summary>
        /// 取得 Session
        /// </summary>
        /// <param name="apId"></param>
        /// <returns></returns>
        public SCSessionModel InsertSession(string apId)
        {
            return new SCSessionModel
            {
                AP_ID = apId,
                USR_COMP_ID = "GSS",
                USR_ID = userProfile.GetLoginUser().USER_ID,
                SESSION_ID = Guid.NewGuid().ToString()
            };
        }

        /// <summary>
        /// 新增 Session
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void InsertSession(SCSessionModel model)
        {
            scSessionDac.InsertScSession(model);
        }

    }
}
