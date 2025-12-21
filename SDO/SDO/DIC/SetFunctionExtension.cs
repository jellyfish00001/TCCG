using SDO.Services;
using SDO.Dac;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SetFunctionExtension
    {
        public static IServiceCollection AddSetFunction(this IServiceCollection services)
        {
            //Transient
            //每次注入時，都重新 new 一個新的實例。
            //Scoped 
            //每個 Request 都重新 new 一個新的實例，同一個 Request 不管經過多少個 Pipeline 都是用同一個實例。上例所使用的就是 Scoped。
            //Singleton
            //被實例化後就不會消失，程式運行期間只會有一個實例。
            services.TryAddTransient<ISetFunctionDac, SCFunctionDac>();
            services.TryAddTransient<ISetFunctionService, SCFunctionService>();
            return services;
        }
    }
}
