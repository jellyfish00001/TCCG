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
    public class ProjectTitleDac : Dac, IProjectTitleDac
    {
        public ProjectTitleDac(IConnectionControlCenter connectionControlCenter,
          IHttpContextAccessor httpContextAccessor,
          ISqlTrace trace,
          IUserProfile profile,
          IParameterAdaptor ParameterAdaptor,
          IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {

        }

        /// <summary>
        /// 取得是否開放涉及其他提案類別
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        public async Task<MaintainInnProjectTitleModel> GetInnProjectIsPlural(string INN_YEAR)
        {
            string sql = @"SELECT IS_PLURAL
						   FROM INN_ASSIGN_ORG (NOLOCK)
						   WHERE INN_YEAR = @INN_YEAR ";

            return await ExecuteQueryFirstOrDefaultAsync<MaintainInnProjectTitleModel>(sql, new { INN_YEAR }, INNDBKey);
        }

        /// <summary>
        /// 是否可編輯
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        public async Task<MaintainInnProjectTitleModel> GetInnProjectIsEdit(string INN_YEAR)
        {
            string sql = @"SELECT COUNT(*) AS IS_EDIT 
                           FROM INN_PROJECT_PROPOSALTYPE M1(NOLOCK)
                           WHERE M1.PROPOSAL_TYPE_ID 
                           IN (SELECT M2.ID 
                               FROM INN_PROJECT_TITLE M2 (NOLOCK)
                               WHERE M2.YEAR = @INN_YEAR)";

            return await ExecuteQueryFirstOrDefaultAsync<MaintainInnProjectTitleModel>(sql, new { INN_YEAR }, INNDBKey);
        }

        /// <summary>
        /// 取得維護專題
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        public async Task<List<ProjectTitleModel>> GetInnProjectTitle(string INN_YEAR)
        {
            string sql = @"SELECT ID, CODE, CODE_VALUE
						   FROM INN_PROJECT_TITLE (NOLOCK)
						   WHERE YEAR = @INN_YEAR ";

            return (await ExecuteQueryAsync<ProjectTitleModel>(sql, new { INN_YEAR } ,INNDBKey)).ToList();
        }

        /// <summary>
        /// 新增維護專題
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertInnProjectTitle(ProjectTitleModel model)
        {
            string sql = $@"
                INSERT INTO INN_PROJECT_TITLE
                    (YEAR
                    ,CODE
                    ,CODE_VALUE
                    ,CRT_USER
                    ,CRT_DATE)
                VALUES
                    (@YEAR
                    ,@CODE
                    ,@CODE_VALUE
                    ,@CRT_USER
                    ,{DTNow})";
            await ExecuteCommandAsync(sql, model, INNDBKey);
        }

        /// <summary>
        /// 修改維護專題
        /// </summary>
        /// <param name="model"></param>
        public async Task UpdateInnProjectTitle(ProjectTitleModel model)
        {
            string sql = $@"
                UPDATE  INN_PROJECT_TITLE
                SET CODE = @CODE,
                    CODE_VALUE = @CODE_VALUE,
                    MDF_USER = @MDF_USER,
                    MDF_DATE = {DTNow}
                WHERE ID = @ID";
            await ExecuteCommandAsync(sql, model, INNDBKey);
        }

        /// <summary>
        /// 刪除維護專題
        /// </summary>
        /// <param name="id"></param>
        public async Task DeleteInnProjectTitle(object YEAR)
        {
            string sql = @"
                DELETE FROM INN_PROJECT_TITLE
                WHERE YEAR = @YEAR";
            await ExecuteCommandAsync(sql, new { YEAR = YEAR }, INNDBKey);
        }

        /// <summary>
        /// 編輯是否開放涉及其他提案類別
        /// </summary>
        /// <param name="model"></param>
        public async Task UpdateInnProjectIsPlural(MaintainInnProjectTitleModel model)
        {
            string sql = $@"
                UPDATE  INN_ASSIGN_ORG
                SET IS_PLURAL = @IS_PLURAL,
                    MDF_USER = @MDF_USER,
                    MDF_DATE = {DTNow}
                WHERE INN_YEAR = @YEAR";
            await ExecuteCommandAsync(sql, model, INNDBKey);
        }


    }
}
