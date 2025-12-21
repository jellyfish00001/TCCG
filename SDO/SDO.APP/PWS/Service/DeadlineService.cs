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
    public class DeadlineService : Service, IDeadlineService
    {
        private readonly IDeadlineDac dac;
        private readonly IDropDownDac dropDownDac;

        public DeadlineService(IDeadlineDac dac, IDropDownDac dropDownDac)
        {
            this.dac = dac;
            this.dropDownDac = dropDownDac;
        }

        /// <summary>
        /// 取先期年度
        /// </summary>
        /// <returns></returns>
        public async Task<List<DeadlineModel>> GetPlanYear()
        {
            return await dac.GetPlanYear();
        }

        /// <summary>
        /// 取先期年度截止日
        /// </summary>
        /// <returns></returns>
        public async Task<DeadlineModel> GetPlanDeadline(string year)
        {
            DeadlineModel model = new DeadlineModel();
            // 取得機關截止日
            var orgDeadline = await dac.GetOrgDeadline(year);
            if (orgDeadline != null)
            {
                model.OrgEndTime = orgDeadline.OrgEndTime;
            }
            // 取得區公所截止日
            var disDeadline = await dac.GetDisDeadline(year);
            if (disDeadline != null)
            {
                model.DistrictHallEndTime = disDeadline.DistrictHallEndTime;
            }

            return model;
        }

        /// <summary>
        /// 更新截止日期
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SetDeadLine(DeadlineModel model)
        {
            // 調整時差
            if (model.OrgEndTime != null)
            {
                model.OrgEndTime = model.OrgEndTime.Value.AddHours(8);
            }
            if (model.DistrictHallEndTime != null)
            {
                model.DistrictHallEndTime = model.DistrictHallEndTime.Value.AddHours(8);
            }
            // 是否有當年度指派作業
            bool IsAddAssign = await dac.IsDeadlineYear(model.PLANYEAR);
            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                // 新增指派作業
                if (!IsAddAssign)
                {
                    // 新增年度機關
                    await dac.AddOrgData(model);
                }
                // 機關截止日
                await dac.SetOrgDeadLine(model);
                // 區公所截止日
                await dac.SetDisDeadLine(model);
                
                scope.Complete();
            }
            return true;
        }

    }
}
