using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDO.Base.Utils.Models;
using SDO.Models;
using SDO.ReportBuilder.Models;
using SDO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class PCCController : ControllerBase
    {
        private readonly IPCCService service;

        public PCCController(IPCCService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 取得關聯工程會標案資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ProjectMapPCCGridModel>> GetProjectMapPCC(PccFilterModel model)
            => await service.GetProjectMapPCC(model);

        /// <summary>
        /// 取得工程會基本資料
        /// </summary>
        /// <param name="PCC_PROJECT_UID"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<Dictionary<string, string>> GetPccmDs01([FromBody] string PCC_PROJECT_UID)
            => await service.GetPccmDs01(PCC_PROJECT_UID);

        /// <summary>
        /// 取得標案系統執行進度資料
        /// </summary>
        /// <param name="PCC_PROJECT_UID"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<PCCExeProgressGridModel> GetPCCExeProgress([FromBody] string PCC_PROJECT_UID)
            => await service.GetPCCExeProgress(PCC_PROJECT_UID);

        /// <summary>
        /// 工程標案工程概要資料
        /// </summary>
        /// <param name="PCC_PROJECT_UID"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<Dictionary<string, string>> GetPCCDs07([FromBody] string PCC_PROJECT_UID)
            => await service.GetPCCDs07(PCC_PROJECT_UID);

        /// <summary>
        /// 工程標案決標資料
        /// </summary>
        /// <param name="PCC_PROJECT_UID"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<Dictionary<string, string>> GetPCCDs09([FromBody] string PCC_PROJECT_UID)
            => await service.GetPCCDs09(PCC_PROJECT_UID);

        /// <summary>
        /// 關聯工程會標案資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="PCC_PROJECT_UID"></param>
        /// <param name="START_WORK"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectMapPCC([FromForm] string PROJECT_NO, [FromForm] string PCC_PROJECT_UID, [FromForm] DateTime? START_WORK)
            => service.SaveProjectMapPCC(PROJECT_NO, PCC_PROJECT_UID, START_WORK);

        /// <summary>
        /// 修改使用國發會介接資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="IS_USER_FTY_DATA"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectUsePCC([FromForm] string PROJECT_NO, [FromForm] bool IS_USER_FTY_DATA)
            => service.SaveProjectUsePCC(PROJECT_NO, IS_USER_FTY_DATA);

        /// <summary>
        /// 工程標案同步
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SyncPCCData([FromBody] string PROJECT_NO)
            => await service.SyncPCCData(PROJECT_NO);

        /// <summary>
        /// 工程標案 excel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ObjectResultModel<PccmXlsGridModel>> GetPccmXls(PccXlsFilterModel model)
            => await service.GetPccmXls(model);

        /// <summary>
        /// 下載工程標案 excel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> DownloadPccmXls(PccXlsFilterModel model)
        {
            var result = await service.DownloadPccmXls(model);
            return File(result.bytes, result.contentType, result.fileName);
        }

        /// <summary>
        /// 取得工程標案資料集
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<List<DropDownListModel>> GetPccSrcTables()
            => await service.GetPccSrcTables();
     }
}
