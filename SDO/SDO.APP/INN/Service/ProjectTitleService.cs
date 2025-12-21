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
    public class ProjectTitleService : Service, IProjectTitleService
    {
        private readonly IProjectTitleDac dac;

        public ProjectTitleService(IProjectTitleDac dac
           )
        {
            this.dac = dac;

        }
        /// <summary>
        /// 取得維護專題
        /// </summary>
        /// <param name="INN_YEAR"></param>
        public async Task<MaintainInnProjectTitleModel> GetMaintainInnProjectTitle(string INN_YEAR)
        {
            MaintainInnProjectTitleModel model = new MaintainInnProjectTitleModel();

            var isPluralResult = await dac.GetInnProjectIsPlural(INN_YEAR);
            var isEditResult = await dac.GetInnProjectIsEdit(INN_YEAR);

            if (isPluralResult != null)
            {
                model.IS_PLURAL = isPluralResult.IS_PLURAL;
                model.IS_EDIT = isEditResult.IS_EDIT;

                model.InnProjectTitles = await GetInnProjectTitle(INN_YEAR);
            }

            return model ?? new MaintainInnProjectTitleModel();
        }

        /// <summary>
        /// 取得維護專題Grid
        /// </summary>
        /// <param name="INN_YEAR"></param>
        private async Task<List<ProjectTitleModel>> GetInnProjectTitle(string INN_YEAR)
        {
            List<ProjectTitleModel> model = await dac.GetInnProjectTitle(INN_YEAR);

            return model ?? new List<ProjectTitleModel>();
        }

        /// <summary>
        /// 儲存維護專題
        /// </summary>
        /// <param name="model"></param>
        public async Task<bool> SaveInnProjectTitle(MaintainInnProjectTitleModel model)
        {
            
            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                await dac.UpdateInnProjectIsPlural(model);
                await dac.DeleteInnProjectTitle(model.YEAR);

                foreach (ProjectTitleModel item in model.InnProjectTitles)
                {
                    item.YEAR = model.YEAR;
                    await dac.InsertInnProjectTitle(item);
                }
                scope.Complete();
            }
            return true;
        }

    }
}
