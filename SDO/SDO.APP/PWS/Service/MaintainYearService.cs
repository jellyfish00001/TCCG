using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class MaintainYearService : Service, IMaintainYearService
    {
        private readonly IMaintainYearDac dac;

        public MaintainYearService(IMaintainYearDac dac)
        {
            this.dac = dac;
        }

        /// <summary>
        /// 取先期年度
        /// </summary>
        /// <returns></returns>
        public async Task<List<MaintainYearModel>> GetMaintainYear()
        {
            return await dac.GetMaintainYear();
        }

        /// <summary>
        /// 更新先期年度
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SetMaintainYear(List<MaintainYearModel> model)
        {
            await dac.SetMaintainYear(model);
            return true;
        }

    }
}
