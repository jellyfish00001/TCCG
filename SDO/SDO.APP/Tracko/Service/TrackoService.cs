using System.Threading.Tasks;
using System;
using SDO.Models;
using Microsoft.AspNetCore.Http;
using Aspose.Cells;
using System.Linq;
using System.Globalization;
using SDO.Utils;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net;

namespace SDO.Services
{
    public class TrackoService : Service, ITrackoService
    {
        private readonly ICallAPI apiService;
        private readonly IUserProfile userProfile;
        private readonly ISysParam sysParam;
        private readonly ILogger<TrackoService> logger;

        public TrackoService(ICallAPI apiService, IUserProfile userProfile, ISysParam sysParam, ILogger<TrackoService> logger)
        {
            this.apiService = apiService;
            this.userProfile = userProfile;
            this.sysParam = sysParam;
            this.logger = logger;
        }

        /// <summary>
        /// 取得本日到期或待辦案件資訊
        /// </summary>
        /// <returns></returns>
        public async Task<RtnTrackoModel> GetTrackoData()
        {
            // 本日到期及已逾期案件
            List<TrackoModel> overTimeData = await SendTrackoApi("0");
            // 所有案件
            List<TrackoModel> agencyData = await SendTrackoApi("1");
            // 是否使用新版本
            bool isNewVersion = (await sysParam.GetSysParam("SystemConfig", "TrackoIsNewVersion"))?.SET_VALUE == "Y";

            string parliament = "PTMS"; // 議會案件
            string track = "PTMS2"; // 追蹤案件

            return new RtnTrackoModel()
            {
                OverParliamentProjs = isNewVersion ? overTimeData.Where(x => x.AP_ID == parliament).ToList() : new(),
                OverTrackProjs = overTimeData.Where(x => x.AP_ID == track).ToList(),
                ParliamentProjs = isNewVersion ? agencyData.Where(x => x.AP_ID == parliament).ToList() : new(),
                TrackProjs = agencyData.Where(x => x.AP_ID == track).ToList(),
                IsNewVersion = isNewVersion
            };
        }

        /// <summary>
        /// 呼叫 Tracko API
        /// </summary>
        /// <param name="workPeriodType">查詢清單種類：0：本日；1：待辦</param>
        /// <returns></returns>
        private async Task<List<TrackoModel>> SendTrackoApi(string workPeriodType)
        {
            string userId = userProfile.GetLoginUser().USER_ID;

            logger.LogInformation($"SendTrackoApi_{userId} Start");

            SetParamModel trackoApiKey = await sysParam.GetSysParam("SystemConfig", "TrackoApiKey");
            if (trackoApiKey == null)
            {
                logger.LogInformation($"SendTrackoApi_{userId} SET_PARAM SET_ITEM=SystemConfig and SET_TYPE=TrackoApiKey 無資料");
                logger.LogInformation($"SendTrackoApi_{userId} End");
                return new List<TrackoModel>();
            }

            SetParamModel trackoApiUrl = await sysParam.GetSysParam("SystemConfig", "TrackoApiUrl");
            if (trackoApiUrl == null)
            {
                logger.LogInformation($"SendTrackoApi_{userId} SET_PARAM SET_ITEM=SystemConfig and SET_TYPE=TrackoApiUrl 無資料");
                logger.LogInformation($"SendTrackoApi_{userId} End");
                return new List<TrackoModel>();
            }

            string url = $"{trackoApiUrl.SET_VALUE}/api/RISToDoList";
            object param = new
            {
                format = "JSON",
                userId = userId,
                workPeriodType = workPeriodType,
                apiKey = trackoApiKey.SET_VALUE
            };

            logger.LogInformation($"SendTrackoApi_{userId} Url:{url}");
            logger.LogInformation($"SendTrackoApi_{userId} Param:{JsonConvert.SerializeObject(param)}");

            List<TrackoModel> result = new();
            try
            {
                HttpResponseMessage response = await apiService.SendRequest<HttpResponseMessage>(MethodEnum.POST, url, param);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    TrackoApiRtnModel rtnModel = await apiService.ProcessResult<TrackoApiRtnModel>(response);
                    if (rtnModel.StatusCode != "200")
                    {
                        logger.LogInformation($"SendTrackoApi_{userId} RtnModel: {JsonConvert.SerializeObject(rtnModel)}");
                    }
                    else
                    {
                        return JsonConvert.DeserializeObject<List<TrackoModel>>(rtnModel.StatusMsg);
                    }
                }
                else
                {
                    logger.LogInformation($"SendTrackoApi_{userId} StatusCode:{(int)response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation($"SendTrackoApi_{userId} ErrMsg:{ex.Message}");
            }
            logger.LogInformation($"SendTrackoApi_{userId} End");
            return result;
        }
    }
}
