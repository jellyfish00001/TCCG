using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class InnProjectPrintService : Service, IInnProjectPrintService
    {
        private readonly IInnProjectService projectService;
        private readonly IProjectExecuteService projectExecuteService;
        private readonly IProjectClosedService projectClosedService;
        public InnProjectPrintService(IInnProjectService projectService)
        {
            this.projectService = projectService;
        }

        /// <summary>
        /// 取得計畫預覽資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<InnProjectBasicFillModel> GetProjectPrint(string PROJECT_NO, string type)
        {

            InnProjectBasicFillModel result = await projectService.GetInnBasic(PROJECT_NO);
            
            return result;
        }

        
    }
}
