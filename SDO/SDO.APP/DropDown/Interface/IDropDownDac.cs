using System.Collections.Generic;
using System.Threading.Tasks;
using SDO.Base.Utils.Models;
using SDO.Models;

namespace SDO.Dac
{
    public interface IDropDownDac : IDac
    {
        /// <summary>
        /// 取得指定角色清單
        /// </summary>
        /// <param name="delFlg"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        Task<List<DropDownListModel>> GetRoleUser(string roleId);

        /// <summary>
        /// 取得機關下拉清單
        /// </summary>
        /// <returns></returns>
        Task<List<DropDownListModel>> GetOrganList();

        /// <summary>
        /// 取得登入者的機關下拉選單資料
        /// </summary>
        /// <returns></returns>
        Task<List<DropDownListModel>> GetOrgByUsr();

        /// <summary>
        /// 取得審核狀態下拉選單資料
        /// </summary>
        /// <returns></returns>
        Task<List<DropDownListModel>> GetSendStatusList();

        /// <summary>
        /// 取基金下拉選單資料
        /// </summary>
        /// <returns></returns>
        Task<List<DropDownListModel>> GetFundList();

        /// <summary>
        /// 取基金下拉選單資料
        /// </summary>
        /// <returns></returns>
        Task<List<DropDownListModel>> GetFundOrgList();

        /// <summary>
        /// 取得機關下的使用者帳號下拉選單資料
        /// </summary>
        /// <param name="ouId"></param>
        /// <param name="undertakerType">承辦人類型(主管、執行、協辦、代辦)</param>
        /// <param name="isEnable">是否啟用</param>
        /// <returns></returns>
        Task<List<DropDownListModel>> GetUserByOrg(string ouId, int undertakerType, bool isEnable);

        /// <summary>
        /// 取得 辦理地點 (區)
        /// </summary>
        /// <param name="cityId"></param>
        /// <returns></returns>
        Task<List<DropDownListModel>> GetCodeTownByCityId(string cityId);

        /// <summary>
        /// 取得作業階段
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<DropDownListModel>> GetWorkStage(string PROJECT_NO);

        /// <summary>
        /// 取得建設類別
        /// </summary>
        /// <returns></returns>
        Task<List<DropDownListModel>> GetComPlanKind();

       /// <summary>
       /// 取得主要提案類別
       /// </summary>
       /// <param name="INN_YEAR"></param>
       /// <returns></returns>
        Task<List<DropDownListModel>> GetInnPropsalType(string INN_YEAR);
        /// <summary>
        /// 取得作業階段
        /// </summary>
        /// <returns></returns>
        Task<List<DropDownListModel>> GetInnYear();
        /// <summary>
        /// 取得機關下的使用者帳號下拉選單資料
        /// </summary>
        /// <param name="ouId"></param>
        /// /// <param name="undertakerType">承辦人類型(主管、執行、協辦、代辦)</param>
        /// <param name="isEnable"></param>
        /// <returns></returns>
        Task<List<SponsorDropDownListModel>> GetUserInfoByOrg(string ouId, bool isEnable);
        /// <summary>
        /// 取機關的單位
        /// </summary>
        /// <param name="OU_ID"></param>
        /// <returns></returns>
        Task<List<DropDownListModel>> GetUnitList(string OU_ID);
    }
}
