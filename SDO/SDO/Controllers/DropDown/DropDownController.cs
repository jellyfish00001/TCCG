using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Base.Utils.Models;
using SDO.Models;
using SDO.Services;
using SDO.Utils;

namespace SDO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "handUser")]
    public class DropDownController : ControllerBase
    {
        private readonly IDropDownService service;
        private readonly IUserProfile userProfile;
        public DropDownController(IDropDownService service, IUserProfile userProfile)
        {
            this.service = service;
            this.userProfile = userProfile;
        }

        /// <summary>
        /// 取得指定角色清單
        /// </summary>
        /// <param name="ROLE_ID"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<DropDownListModel>> GetRoleUser([FromBody] string ROLE_ID)
        {
            return await service.GetRoleUser(ROLE_ID);
        }

        /// <summary>
        /// 取得計畫年度下拉選單資料
        /// </summary>
        /// <param name="year"></param>
        /// <param name="type">預設:系統年度往前推;A:系統年度前後推</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<DropDownListModel>> GetProjectYearList([FromForm] int year, [FromForm] string? type)
            => await service.GetProjectYearList(year, type);

        /// <summary>
        /// 取得機關下拉選單資料
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<List<DropDownListModel>> GetOrganList()
            => await service.GetOrganList();

        /// <summary>
        /// 取得登入者的機關下拉選單資料
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<List<DropDownListModel>> GetOrgByUsr()
            => await service.GetOrgByUsr();

        /// <summary>
        /// 取得審核狀態下拉選單資料
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<List<DropDownListModel>> GetSendStatusList()
            => await service.GetSendStatusList();

        /// <summary>
        /// 取得基金下拉選單資料
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<List<DropDownListModel>> GetFundList()
            => await service.GetFundList();

        /// <summary>
        /// 取得基金機關下拉選單資料
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<List<DropDownListModel>> GetFundOrgList()
            => await service.GetFundOrgList();


        /// <summary>
        /// 取得機關下的使用者帳號下拉選單資料
        /// </summary>
        /// <param name="OU_ID"></param>
        /// <param name="UNDERTAKER_TYPE">承辦人類型(主管、執行、協辦、代辦)</param>
        /// <param name="IS_ENABLE">是否啟用</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<DropDownListModel>> GetUserByOrg([FromForm] string OU_ID, [FromForm] int UNDERTAKER_TYPE, [FromForm] bool IS_ENABLE)
            => await service.GetUserByOrg(OU_ID, UNDERTAKER_TYPE, IS_ENABLE);

        /// <summary>
        /// 取得機關下的使用者帳號下拉選單資料
        /// </summary>
        /// <param name="OU_ID"></param>
        /// <param name="IS_ENABLE">是否啟用</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<SponsorDropDownListModel>> GetUserInfoByOrg([FromForm] string OU_ID, [FromForm] bool IS_ENABLE)
            => await service.GetUserInfoByOrg(OU_ID, IS_ENABLE);

        /// <summary>
        /// 取得辦理地點 (區)
        /// </summary>
        /// <param name="CITY_ID"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<DropDownListModel>> GetCodeTownByCityId([FromBody] string CITY_ID)
            => await service.GetCodeTownByCityId(CITY_ID);

        /// <summary>
        /// 取得作業階段
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<DropDownListModel>> GetWorkStage([FromBody] string PROJECT_NO)
            => await service.GetWorkStage(PROJECT_NO);

        /// 取得主要提案類別
        /// </summary>
        /// <param name="INN_YEAE"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<DropDownListModel>> GetInnPropsalType([FromBody] string INN_YEAE)
            => await service.GetInnPropsalType(INN_YEAE);

        /// 取得已設創新提案截止日期年度
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<DropDownListModel>> GetInnYear()
            => await service.GetInnYear();

        /// 取機關的單位
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<DropDownListModel>> GetUnitList([FromBody] string OU_ID)
            => await service.GetUnitList(OU_ID);
    }
}