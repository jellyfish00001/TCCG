using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class DAMTBDac : Dac, IDAMTBDac
    {
        public DAMTBDac(
            IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 刪除經費需求細項
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task DeleteDAMTB(string PLANNO)
        {
            string sql = @"
                            DELETE FROM PWSSDPLANFUND 
                            WHERE PLANNO = @PLANNO;
                          ";

            await ExecuteCommandAsync(sql, new { PLANNO }, PWSDBKey);
        }


        /// <summary>
        /// 存經費需求細項
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SaveDAMTB(List<DAMTBModel> models)
        {
            string sql = $@"
                            INSERT INTO PWSSDPLANFUND 
                            (
                                PLANNO, 
                                FUNDID, 
                                FUNDDESC, 
                                CALCULATIONDESC, 
                                PRICE, 
                                AMOUNT, 
                                FUNDTOT, 
                                CRT_USER,
                                CRT_DATE
                            ) 
                            VALUES 
                            (
                                @PLANNO,
                                @FUNDID,
                                @FUNDDESC, 
                                @CALCULATIONDESC, 
                                @PRICE, 
                                @AMOUNT, 
                                @FUNDTOT, 
                                @CRT_USER,
                                {DTNow}
                            )
                        ";

            await ExecuteCommandAsync(sql, models, PWSDBKey);
        }

        /// <summary>
        /// 取經費需求細項
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<DAMTBModel>> GetDAMTB(string PLANNO)
        {
            string sql = @"
                          SELECT 
                                PLANNO, 
                                FUNDID, 
                                FUNDDESC, 
                                CALCULATIONDESC, 
                                PRICE, 
                                AMOUNT, 
                                FUNDTOT
                            FROM 
                                PWSSDPLANFUND (NOLOCK)
                           WHERE 
                                PLANNO = @PLANNO;
                          ";
            var result = await ExecuteQueryAsync<DAMTBModel>(sql, new { PLANNO }, PWSDBKey);
            return result.ToList();
        }

        /// <summary>
        /// 取基本計畫資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<int> GetDAMTBTOTAL(string PLANNO)
        {
            string sql = @"
                           SELECT 
                                (ISNULL([PUBLICMONEY], 0) + ISNULL([FUNDMONEY], 0) + ISNULL([OTHERMONEY], 0) + ISNULL([CENTERMONEY], 0)) AS TOTAL
                            FROM 
                                 PWSSDPLANMAIN
                           WHERE PLANNO = @PLANNO
                         ;
                          ";
            var result = await ExecuteQueryFirstOrDefaultAsync<int>(sql, new { PLANNO }, PWSDBKey);
            return result;
        }
    }
}
