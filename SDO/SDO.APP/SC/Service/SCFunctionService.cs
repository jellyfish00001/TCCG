using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SDO.Services
{
    public class SCFunctionService : SetFunctionService, ISetFunctionService
    {
        private readonly ISetFunctionDac dac;
        private readonly IUserProfile userProfile;

        public SCFunctionService(ISetFunctionDac dac,
            IUserProfile userProfile):base(dac,userProfile)
        {
            this.dac = dac;
            this.userProfile = userProfile;
        }

        /// <summary>
        /// 以使用者ID抓function清單
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public override async Task<IList<SetFunctionModel>> ReadByUser(string userId, string apid)
        {
            return await dac.ReadByUser(apid, userId);
        }
    }
}
