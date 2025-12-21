using System.Collections.Generic;
using System.Threading.Tasks;
using SDO.Dac;
using SDO.Base.Utils.Models;
using System;
using SDO.Utils;
using SDO.Models;

namespace SDO.Services
{
    public class DropDownService : Service, IDropDownService
    {
        // 共用Dac
        private readonly IDropDownDac dac;
        public DropDownService(IDropDownDac dac)
        {
            this.dac = dac;
        }

        /// <summary>
        /// 取得指定角色清單
        /// </summary>
        /// <param name="delFlg"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetRoleUser(string roleId)
        {
            return await dac.GetRoleUser(roleId);
        }

        /// <summary>
        /// 取得計畫年度下拉清單
        /// </summary>
        /// <param name="year"></param>
        /// <param name="type">預設:系統年度往前推;A:系統年度前後推</param>
        /// <returns></returns>
        public Task<List<DropDownListModel>> GetProjectYearList(int year, string type)
        {
            List<DropDownListModel> result = new();
            int currentYear = Convert.ToInt32(DateTime.Now.ToTwDateString("yyy"));

            int start = 0, end = 0;

            switch (type)
            {
                // 系統年度前後推
                case "A":
                    start = currentYear + year;
                    end = currentYear - year;
                    break;
                //民國100年到系統年度
                case "B":
                    start = currentYear;
                    end = 100;
                    break;
                // 系統年度前推
                default:
                    start = currentYear;
                    end = currentYear - year;
                    break;
            }
            for (int x = start; x >= end; x--)
            {
                result.Add(new DropDownListModel
                {
                    text = x.ToString(),
                    value = x.ToString()
                });
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// 取得機關下拉清單
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetOrganList()
        {
            return await dac.GetOrganList();
        }

        /// <summary>
        /// 取得登入者的機關下拉選單資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetOrgByUsr()
        {
            return await dac.GetOrgByUsr();
        }

        /// <summary>
        /// 取得審核狀態下拉選單資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetSendStatusList()
        {
            return await dac.GetSendStatusList();
        }

        /// <summary>
        /// 取得基金下拉選單資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetFundList()
        {
            return await dac.GetFundList();
        }

        /// <summary>
        /// 取得基金下拉選單資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetFundOrgList()
        {
            return await dac.GetFundOrgList();
        }

        /// <summary>
        /// 取得機關下的使用者帳號下拉選單資料
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="undertakerType">承辦人類型(主管、執行、協辦、代辦)</param>
        /// <param name="isEnable">是否啟用</param>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetUserByOrg(string orgId, int undertakerType, bool isEnable)
        {
            return await dac.GetUserByOrg(orgId, undertakerType, isEnable);
        }

        /// <summary>
        /// 取得機關下的使用者帳號下拉選單資料
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="undertakerType">承辦人類型(主管、執行、協辦、代辦)</param>
        /// <param name="isEnable">是否啟用</param>
        /// <returns></returns>
        public async Task<List<SponsorDropDownListModel>> GetUserInfoByOrg(string orgId, bool isEnable)
        {
            return await dac.GetUserInfoByOrg(orgId, isEnable);
        }

        /// <summary>
        /// 取得 辦理地點 (區)
        /// </summary>
        /// <param name="cityId"></param>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetCodeTownByCityId(string cityId)
        {
            return await dac.GetCodeTownByCityId(cityId);
        }

        /// <summary>
        /// 取得作業階段
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetWorkStage(string PROJECT_NO)
        {
            return await dac.GetWorkStage(PROJECT_NO);
        }

        /// 取得作業階段
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetInnPropsalType(string INN_YEAE)
        {
            return await dac.GetInnPropsalType(INN_YEAE);
        }

        /// 取得作業階段
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetInnYear()
        {
            return await dac.GetInnYear();
        }
        /// <summary>
        /// 取機關的單位
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetUnitList(string OU_ID)
        {
            return await dac.GetUnitList(OU_ID);
        }
    }
}
