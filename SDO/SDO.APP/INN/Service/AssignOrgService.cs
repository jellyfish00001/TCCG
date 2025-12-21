using Microsoft.CodeAnalysis;
using SDO.Base.Utils;
using SDO.Base.Utils.Models;
using SDO.Dac;
using SDO.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class AssignOrgService : Service, IAssignOrgService
    {
        private readonly IAssignOrgDac dac;

        public AssignOrgService(IAssignOrgDac dac)
        {
            this.dac = dac;

        }

        /// <summary>
        /// 抓取截止時間
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        public async Task<AssignOrgModel> GetInnAssignOrg(string INN_YEAR)
        {
            AssignOrgModel model = await dac.GetInnAssignOrg(INN_YEAR);

            return model ?? new AssignOrgModel();
        }


        /// <summary>
        /// 存取截止時間
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> SaveInnAssignOrg(AssignOrgModel model)
        {
            model.CLOSE_DATE = model.CLOSE_DATE?.AddHours(8);

            // 新增/修改截止時間
            await dac.AddMdfInnAssignOrgCloseDate(model);

            return true;
        }

    }
}
