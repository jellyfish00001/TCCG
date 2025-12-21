using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class ProjectCusFieldDac : Dac, IProjectCusFieldDac
    {
        public ProjectCusFieldDac(IConnectionControlCenter connectionControlCenter,
          IHttpContextAccessor httpContextAccessor,
          ISqlTrace trace,
          IUserProfile profile,
          IParameterAdaptor ParameterAdaptor,
          IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {

        }

        /// <summary>
        /// 取得自訂欄位
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        public async Task<List<ProjectCusFieldModel>> GetInnProjectCusField(string INN_YEAR)
        {
            string sql = @"SELECT CUS_FIELD_ID
                                  ,IS_USE
                                  ,CUS_ITEM
                                  ,CUS_FIELD_NANE
                                  ,YEAR
                           FROM INN_PROJECT_CUS_FIELD(NOLOCK)
						   WHERE YEAR = @INN_YEAR ";

            return (await ExecuteQueryAsync<ProjectCusFieldModel>(sql, new { INN_YEAR } ,INNDBKey)).ToList();
        }

        /// <summary>
        /// 取得是否可編輯
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        public async Task<MaintainInnProjectCusFieldModel> GetInnProjectIsEdit(string INN_YEAR)
        {
            string sql = @"SELECT COUNT(*) AS IS_EDIT 
                           FROM INN_PROJECT_CUS_FIELD_VALUE M1(NOLOCK)
                           WHERE M1.CUS_FIELD_ID 
                           IN (SELECT M2.CUS_FIELD_ID 
                               FROM INN_PROJECT_CUS_FIELD M2 (NOLOCK)
                               WHERE M2.YEAR = @INN_YEAR)";

            return await ExecuteQueryFirstOrDefaultAsync<MaintainInnProjectCusFieldModel>(sql, new { INN_YEAR }, INNDBKey);
        }

        /// <summary>
        /// 新增自訂欄位
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertInnProjectCusField(ProjectCusFieldModel model)
        {
            string sql = $@"
                INSERT INTO INN_PROJECT_CUS_FIELD
                    (YEAR
                    ,IS_USE
                    ,CUS_ITEM
                    ,CUS_FIELD_NANE
                    ,CRT_USER
                    ,CRT_DATE)
                VALUES
                    (@YEAR
                    ,@IS_USE
                    ,@CUS_ITEM
                    ,@CUS_FIELD_NANE
                    ,@CRT_USER
                    ,{DTNow})";
            await ExecuteCommandAsync(sql, model, INNDBKey);
        }

        /// <summary>
        /// 刪除自訂欄位
        /// </summary>
        /// <param name="id"></param>
        public async Task DeleteInnProjectCusField(object YEAR)
        {
            string sql = @"
                DELETE FROM INN_PROJECT_CUS_FIELD
                WHERE YEAR = @YEAR";

            await ExecuteCommandAsync(sql, new { YEAR }, INNDBKey);
        }
        
    }
}
