using Autofac;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SDO.Authentication;
using SDO.Filters;
using SDO.Middleware;
using SDO.Models;
using SDO.Services;
using SDO.Dac;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SDO.LOG.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using System.Collections.Generic;
using SDO.XlsReader.Interface;
using SDO.XlsReader.Service;

namespace SDO
{
    public class Startup
    {
        private List<Type> RegistedTypes=new List<Type>();
        //如果要替換原來底層已經註冊的service需加入排除清單
        private static List<string> ExcludeService = new List<string>();
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
            configuration.GetSection("ExcludeService").Bind(ExcludeService);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        //需要注入的Service都在這個function加入
        public void ConfigureServices(IServiceCollection services)
        {
            //讀取PrivateKeyPath
            string privateKeyPath = Configuration.GetValue<string>("TokenSetting:PrivateKeyPath");
            //取得PrivateKey JWT驗證使用 ， 若無PrivateKey則自動產生。
            byte[] privateKey = ReadRSAPrivateKey(privateKeyPath);

            //設定CORS RULE
            services.AddCors(options =>
            {
                //預設CORS RULE (正式環境)
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins(Configuration.GetValue<string>("AllowedOrigins").Split(";")) //允許網域
                            .WithHeaders("Authorization", //允許Headers
                                         "content-type",
                                         "CacheToken", "ClientIP")
                            .WithMethods("GET", "POST", "PUT", "DELETE") //允許的Method
                            .WithExposedHeaders("Authorization", "CacheToken", "content-disposition", "ClientIP"); //允許被讀取Headers
                    });
                //開發環境用CORS RULE
                options.AddPolicy("Development",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .WithHeaders("Authorization", //允許Headers
                                         "content-type",
                                         "CacheToken")
                            .WithMethods("GET", "POST", "PUT", "DELETE", "ClientIP") //允許的Method
                            .WithExposedHeaders("Authorization", "CacheToken", "content-disposition", "ClientIP"); //允許被讀取Headers
                    });
            });

            //將TokenSetting加入Configure中，方便後續注入
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
            //add httpclient物件
            services.AddHttpClient();
            //services.AddHttpsRedirection(opt => opt.HttpsPort = 443);
            //加入資料庫連線控制中心
            services.AddConnectionControlCenter();

            //加入讀取參數工具
            services.AddSysParam();

            //加入AddHttpContextAccessor提供後續讀取HttpContext
            services.AddHttpContextAccessor();

            //加入Cache
            services.AddCache(Configuration);

            //加入SqlTrace功能
            services.AddSqlTrace();

            //加入ApiTrac功能
            services.AddApiTrace();

            //加入ErrorHandler
            services.TryAddTransient<IErrorService, ErrorService>();

            //加入UserProfile功能 存取User資料
            services.AddUserProfile();

            //加入XlsReader
            services.TryAddTransient<IXlsService, XlsService>();

            //加入SqlLog功能
            services.AddSqlLog();

            //加入ApiLog功能
            services.AddApiLog();

            //加入SetParam功能 存取SetParam資料
            services.AddSetParam();

            //加入SystemInfo功能
            services.AddSystemInfo();

            //加入權限控管功能 存取ScPolicy資料
            services.AddScPolicy();

            //加入權限驗證功能
            services.AddAccess();

            //加入驗證密鑰控制中心
            services.AddAuthorizationKeyProvider(privateKey);

            //加入Token及驗證相關功能
            services.AddToken();

            //加入Login功能
            services.AddLogin();
            services.AddUploadFile();
            //加入公告填寫功能
            services.AddAnnouncement();

            //加入Excel套版功能
            services.AddAsposeExcelSet();

            //加入功能管理功能
            services.AddSetFunction();

            //加入權利功能
            services.AddDimRight();

            //加入角色功能
            services.AddDimRoleExtension();

            //加入組織功能
            services.AddEmpOrg();

            //加入EmpUser相關功能 存取EmpUser資料
            services.AddEmpUser();

            //加入代理人管理功能
            services.AddEmpAgent();

            //加入效能監控功能(生命週期需為Singleton)  注意: 此Service限Windows，且須特殊權限
            //services.TryAddSingleton<IPerformanceService>(new PerformanceService());
            //加入加解密API

            //加入加解密功能
            services.AddCrypt();

            //加入行事曆功能
            services.AddCalendar();

            //加入下拉清單
            services.AddDropDown();

            //加入Word報表套版範例
            services.AddExportWordReport();

            //加入文字編輯器匯出功能
            services.AddOfficialDoc();

            //加入憑證解析功能

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(privateKey),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                x.Events = new JwtBearerEvents()
                {
                    //自訂Token取得方法
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Headers["Authorization"];
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();
            //注入自訂AuthorizationHandler，調整授權失敗之回傳訊息
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationHandler>();
            RegistedTypes.AddRange(services.Select(x => x.ServiceType));
            //RegisterCustomTypes(services);
            services.AddControllers()
            //Request.Header["Accept"] = application/json  回傳Json格式
            .AddJsonOptions(options =>
            {
                // Use the default property (Pascal) casing.
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            })
            //Request.Header["Accept"] = text/xml 回傳XML格式
            .AddXmlSerializerFormatters();

            //產生Swagger json
            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds((type) => type.FullName);
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SDO_FW", Version = "v1" });
            });
            

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //設定中間層
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto
            });
            app.UseRequestBody();
            app.UseSDOExceptionHandler();

            //依Request 改變Cultures
            app.UseRequestLocalization(options =>
                        options
                        .AddSupportedCultures("zh-TW") //增加支援中文
                        .AddSupportedUICultures("zh-TW") //增加支援中文
                        .SetDefaultCulture("en-US") //預設為英文
                );

            if (!env.IsDevelopment())
            {
                app.UseHsts();
            }
            else
            {
                //測試環境啟用SwaggerUI
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SDO_FW v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            if (!env.IsDevelopment())
                app.UseCors();
            else
                app.UseCors("Development");

            //紀錄 Request Response OnException SqlTrace ApiTrace
            app.UseLog();

            //更新Authentication Token
            app.UseLoginTokenRefresh();

            app.UseAuthentication();

            app.UseAuthorization();

            //設定UserProfile
            app.UseLoadUserProfile();



            //判斷使用者角色權限
            app.UseMiddleware<AccessMiddleware>();

            app.UseEndpoints(option =>
            {
                option.MapControllers();
            });
        }


        /// <summary>
        /// 設定Autofac的容器
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            builder = RegisterCustomTypes(builder);

            //var b = builder.Build();
        }

        /// <summary>
        /// 動態注入assembly的service
        /// myAssemblies放入欲搜尋的assembly名稱
        /// nameContains放入欲模糊搜尋的service名稱
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        private static ContainerBuilder RegisterCustomTypes(ContainerBuilder builder)
        {
            string[] myAssemblies = { "SDO.Dac","SDO.APP", "SDO.Base" };

            string[][] nameContains = {
                new string[] { "Dac" },
                new string[] { "Service" },
                new string[] { "SqlMaker" }};

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in myAssemblies)
            {
                int idx = 0;
                var cAssembly = assemblies
                    .Where(a => a.FullName.StartsWith(assembly, StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault();
                if (cAssembly == null)
                    cAssembly = AppDomain.CurrentDomain.Load(assembly);
                while (idx < nameContains.Length)
                {
                    // 開啟SqlTrace，需排除SqlTrace、SCLog
                    var types = cAssembly.GetTypes().Where(t => nameContains[idx].Where(n => t.FullName.Contains(n) && !t.FullName.Contains("SqlTrace") && !t.FullName.Contains("SCLog")).Any());
                    types = types.Where(t => t.IsPublic && !t.IsAbstract);
                    var typesExclude = types.Where(t => ExcludeService.Contains(t.Name)); 
                    types = types.Except(typesExclude);
                    var vt = types.ToArray();
                    
                    builder.RegisterTypes(vt).AsImplementedInterfaces();
                    idx++;
                }
            }
            return builder;
        }
       
        private  void RegisterCustomTypes(IServiceCollection service)
        {
            try
            {
                string[] myAssemblies = { "SDO.Dac", "SDO.APP", "SDO.Base" };

                string[][] nameContains = {
                new string[] { "Dac" },
                new string[] { "Service" },
                new string[] { "SqlMaker" }};


                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in myAssemblies)
                {
                    int idx = 0;
                    var cAssembly = assemblies
                        .Where(a => a.FullName.StartsWith(assembly, StringComparison.InvariantCultureIgnoreCase))
                        .FirstOrDefault();
                    if (cAssembly == null)
                        cAssembly = AppDomain.CurrentDomain.Load(assembly);

                    while (idx < nameContains.Length)
                    {
                        var types = cAssembly.GetTypes().Where(t => nameContains[idx].Where(n => t.FullName.Contains(n)).Any());
                        types = types.Where(t => t.IsPublic && !t.IsAbstract && ! t.IsInterface && !t.IsEnum );
                        types = types.Where(x => !service.Any(s => x.GetInterfaces().Contains(s.ServiceType))).ToArray();
                       
                        foreach (var type in types)
                        {
                            if (type.GetInterfaces().Any() && !type.Name.Contains("PerformanceService"))
                                service.TryAddTransient(type.GetInterfaces().LastOrDefault(), type);
                        }
                        idx++;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        //產生RSAPrivateKey
        private void BuildRSAPrivateKey(string privateKeyPath)
        {
            if (!File.Exists(privateKeyPath))
            {
                RSA rsa = RSA.Create();
                byte[] key = rsa.ExportRSAPublicKey();
                File.WriteAllBytes(privateKeyPath, key);
            }
        }

        //讀取RSAPrivateKey
        private byte[] ReadRSAPrivateKey(string privateKeyPath)
        {
            BuildRSAPrivateKey(privateKeyPath);
            return File.ReadAllBytes(privateKeyPath);
        }
    }
}
