using SDO.Dac;
using SDO.Models;
using System.Threading.Tasks;

namespace SDO.Utils.Dac
{
    public interface ICaptchaDac : IDac
    {
        /// <summary>
        /// 新增驗證碼
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="randomNum">隨機碼</param>
        /// <returns></returns>
        void InsertCaptcha(string guid, string randomNum);
        /// <summary>
        /// 檢查是否存在此組GUID+驗證碼，若存在即刪除此組資料
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="randomNum">隨機碼</param>
        /// <returns>是否存在</returns>
        Task<bool> CheckAndDeleteCaptcha(string guid, string randomNum);
        /// <summary>
        /// 清除驗證碼
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task DeleteCaptcha(string guid);
        /// <summary>
        /// 檢查PMO雙因子驗證碼
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> CheckPMO2AuthCaptchaValid(CaptchaVerifyModel model);

        /// <summary>
        /// 新增PMO雙因子驗證碼
        /// </summary>
        /// <param name="model"></param>
        void InsertPMO2AuthCaptcha(CaptchaVerifyModel model);

        /// <summary>
        /// 刪除PMO雙因子驗證碼
        /// </summary>
        /// <param name="CaptchaId"></param>
        void DeletePMO2AuthCaptcha(string CaptchaId);
    }
}
