using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using SDO.Base.Utils;
using SDO.Base.Utils.Models;
using SDO.Dac;
using SDO.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SDO.Services
{
    public class ProjectCusFieldService : Service, IProjectCusFieldService
    {
        private readonly IProjectCusFieldDac dac;

        public ProjectCusFieldService(IProjectCusFieldDac dac
           )
        {
            this.dac = dac;

        }

        /// <summary>
        /// 取得自訂欄位
        /// </summary>
        /// <param name="INN_YEAR"></param>
        public async Task<MaintainInnProjectCusFieldModel> GetInnProjectCusField(string INN_YEAR)
        {
            MaintainInnProjectCusFieldModel model = new MaintainInnProjectCusFieldModel();

            var isEditResult = await dac.GetInnProjectIsEdit(INN_YEAR);

            model.IS_EDIT = isEditResult.IS_EDIT;
            model.InnProjectCusFields = await dac.GetInnProjectCusField(INN_YEAR);

            return model ?? new MaintainInnProjectCusFieldModel();
        }

        /// <summary>
        /// 儲存維護專題
        /// </summary>
        /// <param name="model"></param>
        public async Task<bool> SaveInnProjectCusField(MaintainInnProjectCusFieldModel model)
        {

            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                await dac.DeleteInnProjectCusField(model.YEAR);
                foreach (ProjectCusFieldModel item in model.InnProjectCusFields)
                {
                    await dac.InsertInnProjectCusField(item);
                }
                scope.Complete();
            }
            return true;
        }

    }
}
