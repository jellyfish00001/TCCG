using Microsoft.AspNetCore.Mvc;
using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SDO.Services
{
    public class DAMTBService : Service, IDAMTBService
    {
        private readonly IDAMTBDac dac;

        public DAMTBService(IDAMTBDac dac)
        {
            this.dac = dac;
        }

        /// <summary>
        /// 取經費需求細項
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task<DAMTBIDModel> GetDAMTB(string PLANNO)
        {
            DAMTBIDModel model = new();
            model.DAMTBListModel = await dac.GetDAMTB(PLANNO);
            model.TOTAL = await dac.GetDAMTBTOTAL(PLANNO);
            return model;
        }

        /// <summary>
        /// 存經費需求細項
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SaveDAMTB(DAMTBIDModel model)
        {
            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                await dac.DeleteDAMTB(model.PLANNO);
                await dac.SaveDAMTB(model.DAMTBListModel);
                
                scope.Complete();
            }
            return true;
        }

    }
}
