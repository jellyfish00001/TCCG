using Microsoft.AspNetCore.Http;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;

namespace SDO.Dac
{
    public class AdjustNumberDac : Dac, IAdjustNumberDac
    {
        public AdjustNumberDac(
            IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 取計畫資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<AdjustNumberModel>> GetAdjustNumber( AdjustNumberQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"
                             SELECT
                                    PM.PLANNO,
                                    PM.OU_ID,
                                    dbo.FN_GetOuName(PM.OU_ID, 3) AS OU_NAME,
                                    PM.PLANYEAR,
                                    PM.PLANNAME,
                                    PM.FUNDNO,
                                    PM.PLANKIND,
                                    PM.BUDGETTYPE,
                                    PM.CREATEUNITOUID,
                                    dbo.FN_GetOuName(PM.CREATEUNITOUID, 3) AS UNITOUNAME,
                                    PM.CREATEORGOUID,
                                    dbo.FN_GetOuName(PM.CREATEORGOUID, 3) AS ORGOUNAME,
                                    PM.PLANORDERNUMBER,
                                    PM.IS_SEND,
                                    PM.SEND_STATUS, 
                                    PM.PLANORDERNUMBER,
                                    PK.SET_VALUE AS PLANKINDNAME,
                                    PS.SET_VALUE AS SENDTYPE,
									fo.FUNDNAME
                               FROM PWSSDPLANMAIN PM (NOLOCK)
                               JOIN SET_PARAM PK ON PM.PLANKIND = PK.SET_TYPE AND PK.SET_ITEM = 'PLAN_KIND'
                               JOIN SET_PARAM PS ON PM.IS_SEND = PS.SET_TYPE AND PS.SET_ITEM = 'PROJECT_STATUS'
							   LEFT JOIN FUNDORG fo ON PM.FUNDNO = fo.FUNDNO
                              WHERE 1 = 1
                         ");
            // 年度
            if (model.PLANYEAR > 0)
            {
                sql.AppendLine(" AND PM.PLANYEAR = @PLANYEAR");
            }
            // 計畫類別
            if (!string.IsNullOrEmpty(model.PLANKIND))
            {
                sql.AppendLine(" AND PM.PLANKIND = @PLANKIND ");
            }
            // 提報機關
            if (!string.IsNullOrEmpty(model.CREATEORGOUID))
            {
                sql.AppendLine(" AND PM.CREATEORGOUID = @CREATEORGOUID ");
            }
            // 提報機關
            if (!string.IsNullOrEmpty(model.OU_ID))
            {
                sql.AppendLine(" AND PM.OU_ID = @OU_ID ");
            }
            // 計畫狀態
            if (!string.IsNullOrEmpty(model.IS_SEND))
            {
                sql.AppendLine(" AND PM.IS_SEND = @IS_SEND ");
            }
            // 經費來源
            if (!string.IsNullOrEmpty(model.BUDGETTYPE))
            {
                sql.AppendLine(" AND PM.BUDGETTYPE = @BUDGETTYPE ");
            }
            // 基金名稱
            if (!string.IsNullOrEmpty(model.BUDGETTYPE) && model.FUNDNO.HasValue)
            {
                sql.AppendLine(" AND PM.FUNDNO = @FUNDNO ");
            }

            var result = (await ExecuteQueryAsync<AdjustNumberModel>(sql.ToString(), model, PWSDBKey)).ToList();
            return result;
        }

        /// <summary>
        /// 更新計畫優先順序
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SetAdjustNumber(List<AdjustNumberModel> models)
        {
            string sql = $@"
                            UPDATE PWSSDPLANMAIN
                               SET PLANORDERNUMBER = @PLANORDERNUMBER,
                                   MDF_USER = @MDF_USER,
                                   MDF_DATE = {DTNow}
                             WHERE PLANNO = @PLANNO
                          ";
            await ExecuteCommandAsync(sql, models, PWSDBKey);
        }

        /// <summary>
        /// 確認是否有重複順序
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> CheckAdjustNumber(AdjustNumberQueryModel model)
        {
            string sql = @"
                            SELECT PLANYEAR,
                                   OU_ID,
                                   PLANKIND,
                                   BUDGETTYPE,
                                   CASE
                                        WHEN BUDGETTYPE = '1' THEN NULL
                                        ELSE FUNDNO
                                   END AS FUNDNO,
                                   PLANORDERNUMBER,
                                   COUNT(*) AS DuplicateCount
                             FROM PWSSDPLANMAIN
                            WHERE PLANKIND = @PLANKIND
                              AND PLANYEAR = @PLANYEAR
                              AND (BUDGETTYPE = '1' OR (BUDGETTYPE = '2' AND FUNDNO = @FUNDNO))
                              AND OU_ID = @OU_ID
                              AND IS_SEND = 1
                         GROUP BY PLANYEAR, 
                                  PLANKIND, 
                                  BUDGETTYPE, 
                                  CASE WHEN BUDGETTYPE = '1' THEN NULL ELSE FUNDNO END, 
                                  PLANORDERNUMBER, 
                                  OU_ID
                           HAVING COUNT(*) > 1;
                         ";
            var result = await ExecuteQueryAsync<AdjustNumberModel>(sql, model, PWSDBKey);
            return result.Count > 0 ? false : true;
        }

        /// <summary>
        /// 確認是否有空值
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> CheckAdjustNumberIsNull(AdjustNumberQueryModel model)
        {
            string sql = @"
                           SELECT 
                                  PLANORDERNUMBER
                             FROM PWSSDPLANMAIN
                            WHERE PLANKIND = @PLANKIND
                              AND PLANYEAR = @PLANYEAR
                              AND (BUDGETTYPE = '1' OR (BUDGETTYPE = '2' AND FUNDNO = @FUNDNO))
                              AND OU_ID = @OU_ID          
                              AND IS_SEND = 1
                              AND PLANORDERNUMBER IS NULL
                         ";
            var result = await ExecuteQueryAsync<AdjustNumberModel>(sql, model, PWSDBKey);
            return result.Count > 0 ? false : true;
        }

        /// <summary>
        /// 改為已審查狀態
        /// </summary>
        /// <returns></returns>
        public async Task PlanAdjustState(AdjustNumberQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine($@"
                            UPDATE PWSSDPLANMAIN
                                SET SEND_STATUS = 1,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                                WHERE PLANYEAR = @PLANYEAR
                                AND IS_SEND = 1    
                          ");
            if (!string.IsNullOrEmpty(model.PLANKIND))
            {
                sql.AppendLine(" AND PLANKIND = @PLANKIND ");
            }
            if (!string.IsNullOrEmpty(model.OU_ID))
            {
                sql.AppendLine(" AND OU_ID = @OU_ID ");
            }
            await ExecuteCommandAsync(sql.ToString(), model, PWSDBKey);
        }
    }
}
