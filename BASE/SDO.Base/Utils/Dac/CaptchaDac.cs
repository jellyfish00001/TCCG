using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Dac;
using SDO.Models;
using System.Threading.Tasks;

namespace SDO.Utils.Dac
{
    public class CaptchaDac : SDO.Dac.Dac, ICaptchaDac
    {
        private readonly IUserData userData;
        public CaptchaDac(IConnectionControlCenter connectionControlCenter,
                           IHttpContextAccessor httpContextAccessor,
                           ISqlTrace trace,
                           IUserProfile userProfile,
                           IParameterAdaptor parameterAdaptor, IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, userProfile,  parameterAdaptor, configuration)
        {
            userData = userProfile.GetLoginUser();
        }

        /// <summary>
        /// 新增驗證碼
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="randomNum">隨機碼</param>
        /// <returns></returns>
        public void InsertCaptcha(string guid, string randomNum)
        {
            string sql = @$"INSERT INTO CAPTCHA
                                ([ID]
                                ,[RANDOM_NUM]
                                ,[CRT_DATE])
                             VALUES
                                (@ID
                                ,@RANDOM_NUM
                                ,{DTNow});
                            --清除過期驗證碼
                            DELETE FROM CAPTCHA
                            WHERE DATEDIFF(MI,CRT_DATE,{DTNow})>5
                            ";

            ExecuteCommand(sql, new { ID = guid, RANDOM_NUM = randomNum });
        }

        /// <summary>
        /// 檢查是否存在此組GUID+驗證碼，若存在即刪除此組資料
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="randomNum">隨機碼</param>
        /// <returns>是否存在</returns>
        public async Task<bool> CheckAndDeleteCaptcha(string guid, string randomNum)
        {
            string sql = @$"DELETE FROM CAPTCHA
                            OUTPUT 1
                            WHERE [ID] = @ID
                                AND RANDOM_NUM = @RANDOM_NUM";

            return await ExecuteQueryFirstOrDefaultAsync<int>(sql, new { ID = guid, RANDOM_NUM = randomNum }) > 0;
        }
        /// <summary>
        /// 檢查是否存在此組GUID+驗證碼，若存在即刪除此組資料
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="randomNum">隨機碼</param>
        /// <returns>是否存在</returns>
        public async Task DeleteCaptcha(string guid)
        {
            string sql = @$"DELETE FROM CAPTCHA
                            OUTPUT 1
                            WHERE [ID] = @ID ";
            await ExecuteCommandAsync(sql, new { ID = guid });
        }
        /// <summary>
        /// 檢查PMO雙因子驗證碼
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> CheckPMO2AuthCaptchaValid(CaptchaVerifyModel model)
        {
            string sql = $@"select count(*) 
                            from CACHE (nolock)
                            where Id = @CaptchaId 
                                and Value = (SELECT CONVERT(VARBINARY(MAX), @CaptchaCodeEncode, 1)) 
                                and ExpiresAtTime >= {DTNow}";

            return await ExecuteQueryFirstOrDefaultAsync<int>(sql, model) > 0;
        }
        /// <summary>
        /// 新增PMO雙因子驗證碼
        /// </summary>
        /// <param name="model"></param>
        public void InsertPMO2AuthCaptcha(CaptchaVerifyModel model)
        {
            string sql = $@"INSERT INTO CACHE 
                                (Id,
                                Value,
                                ExpiresAtTime)
                            VALUES 
                                (@CaptchaId,
                                (SELECT CONVERT(VARBINARY(MAX), @CaptchaCodeEncode, 1)),                        
                                @ExpiresAtTime)";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除PMO雙因子驗證碼
        /// </summary>
        /// <param name="CaptchaId"></param>
        public void DeletePMO2AuthCaptcha(string CaptchaId)
        {
            string sql = @"delete CACHE where Id = @CaptchaId";
            ExecuteCommand(sql, new { CaptchaId });
        }


    }
}
