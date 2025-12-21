using Autofac;
using AutoMapper;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Base.Utils.Models;
using SDO.Dac;
using SDO.Models;
using SDO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    /// <summary>
    /// 基本資料調整內容
    /// </summary>
    public class ProjectBasicAdjDiff : ProjectFillBasic
    {
        private readonly IProjectService projectService;
        private readonly ISysParamDac sysParamDac;
        private readonly IIPCCodeDac ipcCodeDac;
        private readonly IDropDownDac dropDownDac;
        private readonly IProjectAdjustService projectAdjustService;

        public ProjectBasicAdjDiff(IComponentContext coms) : base(coms)
        {
            this.projectService = coms.Resolve<IProjectService>();
            this.sysParamDac = coms.Resolve<ISysParamDac>();
            this.ipcCodeDac = coms.Resolve<IIPCCodeDac>();
            this.dropDownDac = coms.Resolve<IDropDownDac>();
            this.projectAdjustService = coms.Resolve<IProjectAdjustService>();
        }

        protected override async Task GetData()
        {
            projectFillBasicData = await GetProjectFillBasicData();
            sysParamData = await sysParamDac.GetSysParams();
            planItemCData = await ipcCodeDac.GetCodePlanItem("2", false);
            planItemLData = await ipcCodeDac.GetCodePlanItem("1", false);
            organData = await dropDownDac.GetOrganList();
            townData = await dropDownDac.GetCodeTownByCityId("H");
            masterUserData = await dropDownDac.GetUserByOrg(projectFillBasicData.ProjectBasic.MASTER_ORGAN_C, 1, true);
            execUserData = await dropDownDac.GetUserByOrg(projectFillBasicData.ProjectBasic.EXEC_ORGAN_C, 2, true);
            budgetUserData = await dropDownDac.GetUserByOrg(projectFillBasicData.ProjectBasic.BUDGET_HOLD_ORGAN_C, 4, true);
        }

        /// <summary>
        /// 轉換model (ProjectBasicFillAdjustModel => ProjectBasicFillModel)
        /// </summary>
        /// <returns>計畫基本資料</returns>
        private async Task<ProjectBasicFillModel> GetProjectFillBasicData()
        {
            // 若當下的 LOG_ID 為調整流水號，則取調整檔的資料，否則取歷程檔中的資料
            if (Parameter.DiffId == Parameter.PROJ_ADJ_ID)
            {
                ProjectBasicFillAdjustModel data = await projectAdjustService.GetProjectBasicAdj(Parameter.PROJECT_NO, Parameter.DiffId);

                var config = new MapperConfiguration(cfg =>
                    cfg.CreateMap<ProjectBasicFillAdjustModel, ProjectBasicFillModel>()); // 註冊Model間的對映
                var mapper = config.CreateMapper(); // 建立 Mapper
                ProjectBasicFillModel result = mapper.Map<ProjectBasicFillModel>(data); // 轉換型別
                return result;
            }
            return await projectService.GetProjectBasicFill(Parameter.PROJECT_NO, Parameter.DiffId);
        }
    }
}
