using Autofac;
using AutoMapper;
using SDO.APP.IPC.Models.ProjectAdjust;
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
    /// 檢核點調整內容
    /// </summary>
    public class ProjectCheckPointAdjDiff : ProjectCheckPoint
    {
        private readonly IProjectService projectService;
        private readonly IIPCSetParamDac ipcSetparamDac;
        private readonly IIPCCodeDac ipcCodeDac;
        private readonly IProjectAdjustService projectAdjustService;

        public ProjectCheckPointAdjDiff(IComponentContext coms) : base(coms)
        {
            this.projectService = coms.Resolve<IProjectService>();
            this.ipcSetparamDac = coms.Resolve<IIPCSetParamDac>();
            this.ipcCodeDac = coms.Resolve<IIPCCodeDac>();
            this.projectAdjustService = coms.Resolve<IProjectAdjustService>();
        }

        protected override async Task GetData()
        {
            projectCheckpointData = await GetProjectCheckpointData();
            cpKindData = await ipcSetparamDac.GetSysParams("CP_KIND", false);
            codeCheckpointData = await ipcCodeDac.GetCodeCheckpoint(projectCheckpointData.CP_KIND, false);
        }

        /// <summary>
        /// 轉換model (AdjustCheckPointModel => ProjectCheckpointModel)
        /// </summary>
        /// <returns>計畫檢核點設定</returns>
        private async Task<ProjectCheckpointModel> GetProjectCheckpointData()
        {
            // 若當下的 LOG_ID 為調整流水號，則取調整檔的資料，否則取歷程檔中的資料
            if (Parameter.DiffId == Parameter.PROJ_ADJ_ID)
            {
                AdjustCheckPointModel data = await projectAdjustService.GetAdjustCheckPoint(Parameter.DiffId);

                var config = new MapperConfiguration(cfg =>
                    cfg.CreateMap<AdjustCheckPointModel, ProjectCheckpointModel>()); // 註冊Model間的對映
                var mapper = config.CreateMapper(); // 建立 Mapper
                ProjectCheckpointModel result = mapper.Map<ProjectCheckpointModel>(data); // 轉換型別
                return result;
            }
            return await projectService.GetProjectCheckpoint(Parameter.PROJECT_NO, Parameter.DiffId);
        }
    }
}
