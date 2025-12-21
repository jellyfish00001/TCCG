using SDO.Utils;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CacheExtension
    {
        public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
        {
            //考量Load Balance狀況，使用分散式快取
            //建立CacheTable cmd: dotnet sql-cache create "Server={Server};UID={UID};PWD={PWD};Database=MACSD_MVC_M;" dbo CACHE
            //限SqlServer
            //若使用SqlServer以外DB，請自行管理Cache並實作ICache 或是 使用本機快取
            services.AddDistributedSqlServerCache(option => { 
                option.ConnectionString = configuration.GetValue<string>("CacheSetting:CacheConnection"); //不得含有Driver欄位
                option.TableName = configuration.GetValue<string>("CacheSetting:CacheTableName");
                option.SchemaName = configuration.GetValue<string>("CacheSetting:CacheSchemaName");
                //快取有效期限
                option.DefaultSlidingExpiration = TimeSpan.FromSeconds(configuration.GetValue<int>("CacheSetting:CacheExpireTime"));
            });

            //本機快取(不支援Load Balance)
            //services.AddMemoryCache();

            //Transient
            //每次注入時，都重新 new 一個新的實例。
            //Scoped 
            //每個 Request 都重新 new 一個新的實例，同一個 Request 不管經過多少個 Pipeline 都是用同一個實例。上例所使用的就是 Scoped。
            //Singleton
            //被實例化後就不會消失，程式運行期間只會有一個實例。
            services.TryAddTransient<ICache, Cache>();

            return services;
        }
    }
}
