using SDO.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SDO.Dac;
using SDO.Utils;
namespace Microsoft.Extensions.DependencyInjection
{
    public static class LoginExtension
    {
        public static IServiceCollection AddLogin(this IServiceCollection services)
        {
            //加入必要Dac
            services.AddDac<IEmpOrgDac, EmpOrgDac>();

            //Transient
            //每次注入時，都重新 new 一個新的實例。
            //Scoped 
            //每個 Request 都重新 new 一個新的實例，同一個 Request 不管經過多少個 Pipeline 都是用同一個實例。上例所使用的就是 Scoped。
            //Singleton
            //被實例化後就不會消失，程式運行期間只會有一個實例。
            services.TryAddTransient<ISCUserDac, SCUserDac>();
            services.TryAddTransient<ISecureRandomNum, SecureRandomNum>();
            services.TryAddTransient<ICallAPI, CallAPI>();
            services.TryAddTransient<ICaptcha, CaptchaService>();
            services.TryAddTransient<ILoginService, SCLoginService>();
            return services;
        }
    }
}
