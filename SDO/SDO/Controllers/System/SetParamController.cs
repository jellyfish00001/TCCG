using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;

namespace SDO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "handUser")]
    public class SetParamController : ControllerBase
    {
        private readonly IIPCSetParamService setParamService;
        private readonly IIPCCodeService codeService;

        public SetParamController(IIPCSetParamService setParamService, IIPCCodeService codeService)
        {
            this.setParamService = setParamService;
            this.codeService = codeService;
        }

        /// <summary>
        /// 取得system param
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IList<SetParamModel>> GetParams()
        {
            return await setParamService.GetSysParams();
        }

        /// <summary>
        /// 取得system param
        /// </summary>
        /// <param name="setItem"></param>
        /// <returns></returns>
        [HttpGet("{setItem}")]
        public async Task<IList<SetParamModel>> GetParamByItem(string setItem)
        {
            return await setParamService.GetSysParams(HttpUtility.HtmlEncode(setItem));
        }

        /// <summary>
        /// 取得system param
        /// </summary>
        /// <param name="setItem"></param>
        /// <returns></returns>
        [HttpGet("{setItem}/{setType}")]
        public async Task<SetParamModel> GetParamByItemAndType(string setItem, string setType)
        {
            return await setParamService.GetSysParam(HttpUtility.HtmlEncode(setItem), HttpUtility.HtmlEncode(setType));
        }

        /// <summary>
        /// 新增system param
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<RtnResultModel> InsertParam(SetParamModel model)
        {
            return await setParamService.InsertSysParam(model);
        }

        /// <summary>
        /// 更新 system param
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<RtnResultModel> UpdateParam(SetParamModel model)
        {
            return await setParamService.UpdateSysParam(model);
        }

        /// <summary>
        /// 刪除 system param
        /// </summary>
        /// <param name="setItem"></param>
        /// <param name="setType"></param>
        /// <returns></returns>
        [HttpDelete("{setItem}/{setType}")]
        public async Task<RtnResultModel> DeleteParam(string setItem, string setType)
        {
            return await setParamService.DeleteSysParam(setItem, setType);
        }

        /// <summary>
        /// 取得system param
        /// </summary>
        /// <param name="setItem"></param>
        /// <param name="delFlg"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<IList<IPCSetParamModel>> GetParamByItem([FromForm] string setItem, [FromForm] bool? delFlg, [FromForm] int fromWhere)
        {
            return await setParamService.GetSysParams(HttpUtility.HtmlEncode(setItem), delFlg, fromWhere);
        }

        /// <summary>
        /// 取得多組system param
        /// </summary>
        /// <param name="setItems">key:setItem；value:是否有預設值</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<Dictionary<string, object>> GetParamByItems(Dictionary<string, bool> setItems)
        {
            return await setParamService.GetParamByItems(setItems);
        }

        /// <summary>
        /// 儲存 system param
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SaveParamItem(List<IPCSetParamModel> models)
        {
            return await setParamService.SaveParamItem(models);
        }

        /// <summary>
        /// 取得執行方式
        /// </summary>
        /// <param name="CP_KIND"></param>
        /// <param name="isShowDel">是否顯示已停用的資料</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<IPCCodeCheckpointModel>> GetCodeCheckpoint([FromForm] string CP_KIND, [FromForm] bool isShowDel = false)
        {
            return await codeService.GetCodeCheckpoint(CP_KIND, isShowDel);
        }

        /// <summary>
        /// 儲存執行方式
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveCodeCheckpoint(List<IPCCodeCheckpointModel> models)
        {
            return codeService.SaveCodeCheckpoint(models);
        }

        /// <summary>
        /// 取得自訂檢核點
        /// </summary>
        /// <param name="CHK_POINT_CLASS_ID"></param>
        /// <param name="forSettings">是否用於設定</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<IPCCusChkItemModel>> GetCusChkItem([FromForm] int CHK_POINT_CLASS_ID, [FromForm] bool forSettings = false)
        {
            return await codeService.GetCusChkItem(CHK_POINT_CLASS_ID, forSettings);
        }

        /// <summary>
        /// 儲存自訂檢核點
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveCusChkItem(List<IPCCusChkItemModel> models)
        {
            return codeService.SaveCusChkItem(models);
        }

        /// <summary>
        /// 取得落後原因類別
        /// </summary>
        /// <param name="DELAY_CLASS_ID"></param>
        /// <param name="DEL_FLG"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<IPCCodeDelayClassModel>> GetCodeDelayClass([FromForm] string DELAY_CLASS_ID, [FromForm] bool? DEL_FLG)
        {
            return await codeService.GetCodeDelayClass(DELAY_CLASS_ID, DEL_FLG);
        }

        /// <summary>
        /// 儲存落後原因類別
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SaveCodeDelayClass(List<IPCCodeDelayClassModel> models)
        {
            return await codeService.SaveCodeDelayClass(models);
        }

        /// <summary>
        /// 取得預算來源
        /// </summary>
        /// <param name="LEVEL_MARK">1:本府預算來源 2:中央預算來源</param>
        /// <param name="DEL_FLG"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<IPCCodePlanItemModel>> GetCodePlanItem([FromForm] string LEVEL_MARK, [FromForm] bool? DEL_FLG)
        {
            return await codeService.GetCodePlanItem(LEVEL_MARK, DEL_FLG);
        }

        /// <summary>
        /// 儲存預算來源
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SaveCodePlanItem(List<IPCCodePlanItemModel> models)
        {
            return await codeService.SaveCodePlanItem(models);
        }

        /// <summary>
        /// 判斷有無該年度工作日
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<int> GetWorkingDayCountByYear([FromBody] int year)
        {
            return await codeService.GetWorkingDayCountByYear(year);
        }

        /// <summary>
        /// 取得工作日
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<IPCWorkingDayModel>> GetWorkingDay([FromForm] string startDate, [FromForm] string endDate)
        {
            return await codeService.GetWorkingDay(startDate, endDate);
        }

        /// <summary>
        /// 產生年度工作日
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> GenerateWorkingDay([FromBody] int year)
        {
            return await codeService.GenerateWorkingDay(year);
        }

        /// <summary>
        /// 儲存工作日
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveWorkingDay(List<IPCWorkingDayModel> models)
        {
            return codeService.SaveWorkingDay(models);
        }

        /// <summary>
        /// 取得機關窗口維護
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<IPCSetContactModel>> GetSetContact()
        {
            return await codeService.GetSetContact();
        }

        /// <summary>
        /// 取得機關聯絡窗口
        /// </summary>
        /// <param name="ORGAN"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<IPCDeptContactModel>> GetSetContactByOrgan([FromBody] string ORGAN)
        {
            return await codeService.GetSetContactByOrgan(ORGAN);
        }

        /// <summary>
        /// 儲存機關窗口維護
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveSetContact(IPCSetContactModel model)
        {
            return codeService.SaveSetContact(model);
        }
    }
}