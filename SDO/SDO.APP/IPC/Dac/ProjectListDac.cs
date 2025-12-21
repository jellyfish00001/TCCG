using Microsoft.AspNetCore.Http;
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
    public class ProjectListDac : Dac, IProjectListDac
    {
        public ProjectListDac(IConnectionControlCenter connectionControlCenter,
           IHttpContextAccessor httpContextAccessor,
           ISqlTrace trace,
           IUserProfile profile,
           IParameterAdaptor ParameterAdaptor,
           IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 取得計畫列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectListModel>> GetProjectList(ProjectListQueryModel model)
        {
            // 若需篩選特殊加註資料，則另外Join特殊加註Table資料
            string specNoteSql = !string.IsNullOrEmpty(model.SPEC_NOTE) 
                ? "left join PROJECT_MAPPING_DATA map on main.PROJECT_NO = map.PROJECT_NO and map.SET_ITEM = 'SPEC_NOTE'"
                : string.Empty;

            string sql = $@"
                select 
                    main.PROJECT_NO,
                    main.PROJECT_STATUS as PROJECT_STATUS_C,
	                setParam.SET_VALUE as PROJECT_STATUS,
					main.PROJECT_AW_STATUS as PROJECT_AW_STATUS_C,
	                setParam2.SET_VALUE as PROJECT_AW_STATUS,
	                main.PROJECT_YEAR,
	                main.PROJECT_NAME,
	                (case when fav.PROJECT_NO is null then 0 else 1 end) as PIS_SELECT,
	                budget.BUDGET_TOTAL,
	                main.MASTER_ORGAN_C,
	                main.EXEC_ORGAN_C,
                    SCUser.USR_NAME as EXEC_UNDERTAKER,
	                SCForMasterOrg.OU_NAME as MASTER_ORGAN_NAME,
                    SCForExecOrg.OU_SORT_ORDER as EXEC_ORGAN_ORDER,
	                SCForExecOrg.OU_NAME as EXEC_ORGAN_NAME,
                    projAdj.PROJ_ADJ_ID,
                    projAdj.AW_KIND,
                    main.CP_KIND
                from PROJECT_BASIC main
                left join {SC30_M}.SCORG_UNITM as SCForMasterOrg
                    on main.MASTER_ORGAN_C = SCForMasterOrg.OU_ID 
                left join {SC30_M}.SCORG_UNITM as SCForExecOrg
                    on main.EXEC_ORGAN_C = SCForExecOrg.OU_ID 
                left join {SC30_M}.SCUSERM as SCUser
					on main.EXEC_UNDERTAKER_C = SCUser.USR_ID
                left join (
                    select PROJECT_NO, SUM(BUDGET_CENTRAL + BUDGET_LOCAL) as BUDGET_TOTAL
		            from PROJECT_BUDGET_SOURCE_G (nolock)
		            GROUP by PROJECT_NO
                ) budget
                    on main.PROJECT_NO = budget.PROJECT_NO
                left join SET_PARAM setParam
					on main.PROJECT_STATUS = setParam.SET_TYPE and setParam.SET_ITEM = 'PROJECT_STATUS' 
                left join SET_PARAM setParam2
					on main.PROJECT_AW_STATUS = setParam2.SET_TYPE and setParam2.SET_ITEM = 'PROJECT_AW_STATUS'
                left join SUPERIOR_FAVORITEPROJECT fav
					on main.PROJECT_NO = fav.PROJECT_NO and fav.CRT_USER = @CRT_USER
                left join PROJECT_BASIC_ADJ projAdj
                    on main.PROJECT_NO = projAdj.PROJECT_NO and main.PROJECT_AW_STATUS = projAdj.PROJECT_AW_STATUS
                {specNoteSql}
                where main.EXEC_ORGAN_C is not null 
                    and main.EXEC_ORGAN_C != ''
                    and main.IS_CANCELED = 0 
                    {GetProjectListQueryConditions(model)}
                order by PIS_SELECT desc,
                    main.PROJECT_STATUS,
                    main.PROJECT_YEAR desc, 
                    main.PROJECT_NO desc";
            model.CRT_USER = UserId;
            return (await ExecuteQueryAsync<ProjectListModel>(sql, model)).ToList();
        }

        /// <summary>
        /// 取得計畫列表查詢條件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static string GetProjectListQueryConditions(ProjectListQueryModel model)
        {
            StringBuilder sb = new();
            // 年度
            switch (model.PROJECT_YEAR_STATUS)
            {
                case "":
                    sb.AppendLine("and main.PROJECT_YEAR = @PROJECT_YEAR");
                    break;
                case "A": // 含之前所有案件
                    sb.AppendLine("and main.PROJECT_YEAR <= @PROJECT_YEAR");
                    break;
                case "B": // 含之前未結案件
                    sb.AppendLine(@"and (main.PROJECT_YEAR = @PROJECT_YEAR 
	                    or (main.PROJECT_YEAR < @PROJECT_YEAR and main.PROJECT_STATUS in ('1', '2', '3', '4', '5', '6'))
	                    or (year(main.FINISH_DATE) - 1911 >= @PROJECT_YEAR and main.PROJECT_STATUS in ('7', '8'))
                    )");
                    break;
            }

            // 計畫編號
            if (!string.IsNullOrEmpty(model.PROJECT_NO))
            {
                List<string> projNoSqls = model.PROJECT_NO
                    .Split(new char[] { ',', ' ', '　' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x=> $"main.PROJECT_NO like '%{x}%'")
                    .ToList();

                if (projNoSqls.Any())
                {
                    sb.AppendLine($"and ({string.Join(" or ", projNoSqls)})");
                }
            }
            // 計畫名稱
            if (!string.IsNullOrEmpty(model.PROJECT_NAME))
            {
                List<string> projNameSqls = model.PROJECT_NAME
                    .Split(new char[] { ',', ' ', '　' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => $"main.PROJECT_NAME like '%{x}%'")
                    .ToList();

                if (projNameSqls.Any())
                {
                    sb.AppendLine($"and ({string.Join(" or ", projNameSqls)})");
                }
            }
            // 作業階段
            if (model.PROJECT_STATUS != null && model.PROJECT_STATUS.Any())
            {
                sb.AppendLine(" and main.PROJECT_STATUS in @PROJECT_STATUS");
            }
            // 執行方式類別
            if (!string.IsNullOrEmpty(model.CP_KIND))
            {
                sb.AppendLine(" and main.CP_KIND = @CP_KIND");
            }
            // 執行方式名稱
            if (!string.IsNullOrEmpty(model.RUNWAY_C))
            {
                sb.AppendLine(" and main.RUNWAY_C = @RUNWAY_C");
            }
            // 特殊加註
            if (!string.IsNullOrEmpty(model.SPEC_NOTE))
            {
                sb.AppendLine(" and map.SET_TYPE = @SPEC_NOTE");
            }
            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sb.AppendLine(" and main.MASTER_ORGAN_C = @MASTER_DEPT");
            }
            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sb.AppendLine(" and main.EXEC_ORGAN_C = @EXEC_DEPT");
            }
            // 協辦機關
            if (!string.IsNullOrEmpty(model.ASS_DEPT))
            {
                sb.AppendLine(" and main.PROJECT_NO in (select PROJECT_NO from PROJECT_ASST_ORG where ASSISTANT_ORGAN_C = @ASS_DEPT)");
            }
            // 代辦機關
            if (!string.IsNullOrEmpty(model.AGCY_DEPT))
            {
                sb.AppendLine(" and main.BUDGET_HOLD_ORGAN_C = @AGCY_DEPT");
            }
            // 計畫調整狀態
            if (model.isAdjustList == 1)
            {
                sb.AppendLine(" and main.PROJECT_AW_STATUS is null");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 更新計畫釘選狀態
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="PIS_SELECT"></param>
        /// <returns></returns>
        public bool UpdateProjectPisSelect(string PROJECT_NO, bool PIS_SELECT)
        {
            string sql = $@"update PROJECT_BASIC 
                            set PIS_SELECT = @PIS_SELECT,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            where PROJECT_NO = @PROJECT_NO";
            return ExecuteCommand(sql, new
            {
                PROJECT_NO,
                PIS_SELECT,
                MDF_USER = UserId
            });
        }

        /// <summary>
        /// 釘選計畫
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        public void InsertFavoriteProject(string PROJECT_NO)
        {
            string sql = @"insert into SUPERIOR_FAVORITEPROJECT 
                                (PROJECT_NO, CRT_USER) 
                           values(@PROJECT_NO,@CRT_USER)";

            ExecuteCommand(sql, new { PROJECT_NO, CRT_USER = UserId });
        }

        /// <summary>
        /// 取消釘選計畫
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        public void DeleteFavoriteProject(string PROJECT_NO)
        {
            string sql = @"delete SUPERIOR_FAVORITEPROJECT where PROJECT_NO = @PROJECT_NO and CRT_USER = @CRT_USER";
            ExecuteCommand(sql, new { PROJECT_NO, CRT_USER = UserId });
        }

        /// <summary>
        /// 刪除計畫
        /// </summary>
        /// <param name="projectNos"></param>
        public void SaveProjectCanceled(List<string> projectNos)
        {
            string sql = $@"update PROJECT_BASIC 
                            set IS_CANCELED = 1,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            where PROJECT_NO = @PROJECT_NO";
            ExecuteCommand(sql,
                projectNos.Select(x => new { PROJECT_NO = x, MDF_USER = UserId }));
        }

        /// <summary>
        /// 取得計劃異動紀錄清單
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectLogListModel>> GetProjectLogList(string PROJECT_NO)
        {
            string sql = $@"select
                                logData.LOG_ID,
                                main.PROJECT_NO,
                                scUser.USR_NAME as LOG_USER,
                                logData.CRT_DATE as LOG_DATE,
                                logData.LOG_STATUS as LOG_STATUS_C,
                                paramForProjStage.SET_VALUE  as PROJECT_STAGE,
                                paramForLogStatus.SET_VALUE as LOG_STATUS,
                                dbo.FN_GetOuName(main.MASTER_ORGAN_C,3) as MASTER_ORGAN_NAME,
								dbo.FN_GetOuName(main.EXEC_ORGAN_C,3) as EXEC_ORGAN_NAME,
                                logData.MEMO,
                                logData.MDF_ORG_NAME
                            from PROJECT_BASIC_LOG logData
                            left join PROJECT_BASIC main
	                            on main.PROJECT_NO = logData.PROJECT_NO
                            left join SET_PARAM paramForProjStage
	                            on logData.PROJECT_STAGE = paramForProjStage.SET_TYPE and paramForProjStage.SET_ITEM = 'PROJECT_STAGE'
                            left join SET_PARAM paramForLogStatus
	                            on logData.LOG_STATUS = paramForLogStatus.SET_TYPE and paramForLogStatus.SET_ITEM = 'LOG_STATUS'
                            left join {SC30_M}.SCUSERM as scUser
								on logData.CRT_USER = scUser.USR_ID
                            where main.PROJECT_NO = @PROJECT_NO
                            order by LOG_DATE desc";

            return (await ExecuteQueryAsync<ProjectLogListModel>(sql, new { PROJECT_NO })).ToList();
        }

    }
}
