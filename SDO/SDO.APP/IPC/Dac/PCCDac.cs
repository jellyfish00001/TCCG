using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.APP.IPC.Enum;
using SDO.Base.Utils.Models;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class PCCDac : Dac, IPCCDac
    {
        public PCCDac(IConnectionControlCenter connectionControlCenter,
           IHttpContextAccessor httpContextAccessor,
           ISqlTrace trace,
           IUserProfile profile,
           IParameterAdaptor ParameterAdaptor,
           IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 取得 Pcc Table 欄位
        /// </summary>
        /// <param name="tableName">Pcc 資料表名稱</param>
        /// <returns></returns>
        public async Task<List<string>> GetPccmColumns(string tableName)
        {
            string sql = @"select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = @tableName";
            return (await ExecuteQueryAsync<string>(sql, new { tableName })).ToList();
        }

        /// <summary>
        /// 取得 Pcc 資料
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<T>> GetPccmData<T>(PccModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@$"
                select 
                    {string.Join(", ", model.Columns)} 
                from {model.TableName}
                where 1 = 1");

            if (model.PccFilter != null)
            {
                sql = GetPccFilter(model.PccFilter, model.TableName, sql);
            }

            return (await ExecuteQueryAsync<T>(sql.ToString(), model.PccFilter)).ToList();
        }

        /// <summary>
        /// 取得 Pcc 資料 to Dict
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<Dictionary<string, object>>> GetPccmDataToDict(PccModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@$"
                select 
                    {string.Join(", ", model.Columns)} 
                from {model.TableName}
                where 1 = 1");

            if (model.PccFilter != null)
            {
                sql = GetPccFilter(model.PccFilter, model.TableName, sql);
            }

            if (model.Orderby != null && model.Orderby.Any())
            {
                sql.AppendLine($"order by {string.Join(", ", model.Orderby.Select(x => $"{x.Key} {x.Value}"))}");
            }

            return (await ExecuteQueryDictAsync(sql.ToString(), model.PccFilter)).Select(x => x.ToDictionary(y => y.Key, z => z.Value)).ToList();
        }

        /// <summary>
        /// 取得 Pcc 查詢
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tableName"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        private StringBuilder GetPccFilter(PccFilterModel model, PccmNameEnum tableName, StringBuilder sql)
        {
            if (!string.IsNullOrEmpty(model.PCC_PROJECT_UID))
            {
                sql.AppendLine("and plnprj_uid = @PCC_PROJECT_UID");
            }

            if (!string.IsNullOrEmpty(model.PCC_PROJECT_NO))
            {
                sql.AppendLine("and plnprj_id like '%' +  @PCC_PROJECT_NO + '%'");
            }

            if (!string.IsNullOrEmpty(model.PCC_PROJECT_NAME))
            {
                sql.AppendLine("and plnprj_name like '%' + @PCC_PROJECT_NAME + '%'");
            }

            if (!string.IsNullOrEmpty(model.PCC_EXEC_ORG_NAME))
            {
                sql.AppendLine("and execorg_name like '%' + @PCC_EXEC_ORG_NAME + '%'");
            }

            if (model.PCC_PROJECT_YEAR.HasValue)
            {
                switch (tableName)
                {
                    case PccmNameEnum.PCCM_DS15:
                        sql.AppendLine("and yr = @PCC_PROJECT_YEAR");
                        break;
                    default:
                        sql.AppendLine("and plnprj_year = @PCC_PROJECT_YEAR");
                        break;
                }
            }

            if (!string.IsNullOrEmpty(model.PCC_PROJECT_MONTH))
            {
                switch (tableName)
                {
                    case PccmNameEnum.PCCM_DS15:
                        sql.AppendLine("and mnth = @PCC_PROJECT_MONTH");
                        break;
                    default:
                        sql.AppendLine("and plnprj_month = @PCC_PROJECT_MONTH");
                        break;
                }
            }

            return sql;
        }

        /// <summary>
        /// 取得 Pcc 基本檔
        /// </summary>
        /// <param name="DSNAME"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetPccmSchemaInfoDict(string DSNAME)
        {
            string sql = @"select 
                                PCCM_EN_NAME, 
                                PCCM_CN_NAME 
                            from PCCM_SCHEMA_INFO 
                            where DSNAME = @DSNAME
                            order by PCCM_INDEX";
            return (await ExecuteQueryAsync<(string, string)>(sql, new { DSNAME })).ToDictionary(x => x.Item1, y => y.Item2);
        }

        /// <summary>
        /// 關聯工程會標案資料 - 更新計畫基本資料
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectBasicByPCC(ProjectMapPCCModel model)
        {
            string sql = $@"update PROJECT_BASIC
                           set TENDER_AWARDING_AMT = @TENDER_AWARDING_AMT,
                               PROCUREMENT_AMT = @PROCUREMENT_AMT
                           where PROJECT_NO = @PROJECT_NO";

            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 關聯工程會標案資料 - 調整計畫預定實際期程資料
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjCtrlExeByPCC(ProjectMapPCCModel model)
        {
            string sql = $@"update PROJECT_CONTROL_EXECUTE
                            set 
                                PCC_PROJECT_UID = @PCC_PROJECT_UID,
                                PCC_PROJECT_NO = @PCC_PROJECT_NO,
                                PCC_PROJECT_NAME = @PCC_PROJECT_NAME,
                                FACTORY_CONTACT = @FACTORY_CONTACT,
                                FACTORY_TEL = @FACTORY_TEL
                            where PROJECT_NO = @PROJECT_NO";

            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 調整特定控制點的工程會預定/實際 完成日期
        /// </summary>
        /// <param name="models"></param>
        public void UpdateProjChkItemByPCC(List<PCCProjChkItemModel> models)
        {
            string sql = $@"update PROJECT_CHECKITEM 
                            set PCC_ESTIMATED_ENDDATE = @PCC_ESTIMATED_ENDDATE,
                                PCC_ACTUAL_ENDDATE = @PCC_ACTUAL_ENDDATE,
                                ACTUAL_ENDDATE = (case when @ACTUAL_ENDDATE is null then ACTUAL_ENDDATE else @ACTUAL_ENDDATE end)
                            -- 調整特定控制點得工程會預定/實際 完成日期
                            where SEQ = (select projChk.SEQ from PROJECT_CHECKITEM projChk
                                         left join CODE_CHECKPOINT_ITEM codeChk
	                                        on projChk.CHECKITEM_SEQ = codeChk.SEQ 
                                         where PROJECT_NO = @PROJECT_NO and codeChk.CTRL_POINT = @CTRL_POINT)";
            ExecuteCommand(sql, models);
        }

        /// <summary>
        /// 修改使用國發會介接資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="IS_USER_FTY_DATA"></param>
        /// <returns></returns>
        public bool SaveProjectUsePCC(string PROJECT_NO, bool IS_USER_FTY_DATA)
        {
            string sql = $@"update PROJECT_BASIC 
                            set IS_USER_FTY_DATA = @IS_USER_FTY_DATA,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            where PROJECT_NO = @PROJECT_NO";
            return ExecuteCommand(sql, new
            {
                PROJECT_NO,
                IS_USER_FTY_DATA,
                MDF_USER = UserId
            });
        }

        /// <summary>
        /// 檢查計畫是否關聯工程會標案
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> CheckProjIsAssociatePCC(string PROJECT_NO)
        {
            string sql = @"select PCC_PROJECT_UID
                           from PROJECT_CONTROL_EXECUTE (nolock) 
                           where PROJECT_NO = @PROJECT_NO";

            return  !string.IsNullOrEmpty(await ExecuteQueryFirstOrDefaultAsync<string>(sql, new { PROJECT_NO })); 
        }

        /// <summary>
        /// 新增重新執行的計畫編號
        /// </summary>
        /// <param name="projNos"></param>
        /// <returns></returns>
        public async Task InsertProjBasicReloads(List<string> projNos)
        {
            string valueSql = string.Join(", ", projNos.Select(x => $"('{x}')"));
            string sql = $"insert into PROJECT_BASIC_RELOAD (PROJECT_NO) values {valueSql}";
            await ExecuteCommandAsync(sql, null);
        }

        /// <summary>
        /// 取得重新執行的計畫編號
        /// </summary>
        /// <param name="cycleModel"></param>
        /// <returns></returns>
        public async Task<List<AssociatePccProjectModel>> GetReloadProjectNos(ProjectFillCycleModel cycleModel)
        {
            string sql = @"select 
                            m1.PROJECT_NO,
                            m2.PROJECT_NAME,
                            m2.PROJECT_YEAR,
                            isnull(m2.IS_USER_FTY_DATA, 0) as IS_USER_FTY_DATA,
                            m4.PCC_PROJECT_UID,
                            m3.YEAR,
                            m3.MONTH,
                            m3.IS_SEND,
                            m3.TEN_RES_PRG,
                            m3.TEN_ACT_PRG 
                        from PROJECT_BASIC_RELOAD m1 (nolock)
                        inner join PROJECT_BASIC m2 (nolock)
	                        on m1.PROJECT_NO = m2.PROJECT_NO
                        inner join PROJECT_ENGINEERING_PROGRESS m3 (nolock)
                            on m1.PROJECT_NO = m3.PROJECT_NO and m3.YEAR = @PROJECT_YEAR and m3.MONTH = @PROJECT_MONTH
                        inner join PROJECT_CONTROL_EXECUTE m4 (nolock)
                            on m1.PROJECT_NO = m4.PROJECT_NO and isnull(m4.PCC_PROJECT_UID, '') != ''";
            return (await ExecuteQueryAsync<AssociatePccProjectModel>(sql, cycleModel)).ToList();
        }

        /// <summary>
        /// 刪除重新執行的計畫編號
        /// </summary>
        /// <returns></returns>
        public void DeleteReloadProjectNo()
        {
            string sql = @"truncate table PROJECT_BASIC_RELOAD";
            ExecuteCommand(sql, null);
        }

        /// <summary>
        /// 修改使用國發會介接資料
        /// </summary>
        /// <param name="PROJECT_NOs"></param>
        /// <param name="isUserFtyData"></param>
        /// <returns></returns>
        public void UpdateProjIsUserFtyData(List<string> PROJECT_NOs, bool isUserFtyData)
        {
            string sql = $@"update PROJECT_BASIC 
                            set 
                                IS_USER_FTY_DATA = { (isUserFtyData ? 1 : 0) },
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            where PROJECT_NO in @PROJECT_NOs";
            ExecuteCommand(sql, new { PROJECT_NOs, MDF_USER = UserId });
        }

        /// <summary>
        /// 更新有關聯的工程會異動檢核點
        /// 且 CTRL_POINT 如果是 C，會同步更新 CHECKITEM_SEQ 大於 CTRL_POINT=C 的資料
        /// </summary>
        /// <param name="models"></param>
        public void UpdatePccProjChkItemsByPcc(List<PCCProjChkItemModel> models)
        {
            string sql = @"
                update M1
                set 
                    ACTUAL_ENDDATE = (case when ACTUAL_ENDDATE is null then @PCC_ACTUAL_ENDDATE else ACTUAL_ENDDATE end),
                    PCC_ESTIMATED_ENDDATE = (case when @PCC_ESTIMATED_ENDDATE is null then PCC_ESTIMATED_ENDDATE else @PCC_ESTIMATED_ENDDATE end),
                    PCC_ACTUAL_ENDDATE = @PCC_ACTUAL_ENDDATE,
                    MDF_DATE = @MDF_DATE
                from PROJECT_CHECKITEM M1
                inner join CODE_CHECKPOINT_ITEM M2 on M2.SEQ = M1.CHECKITEM_SEQ and M2.CTRL_POINT = @CTRL_POINT
                where M1.PROJECT_NO = @PROJECT_NO

                if (@CTRL_POINT = 'C')
                begin
	                update PROJECT_CHECKITEM
	                set
		                ACTUAL_ENDDATE = (case when ACTUAL_ENDDATE is null then @PCC_ACTUAL_ENDDATE else ACTUAL_ENDDATE end),
                        PCC_ESTIMATED_ENDDATE = (case when @PCC_ESTIMATED_ENDDATE is null then PCC_ESTIMATED_ENDDATE else @PCC_ESTIMATED_ENDDATE end),
		                PCC_ACTUAL_ENDDATE = @PCC_ACTUAL_ENDDATE,
		                MDF_DATE = @MDF_DATE
	                where PROJECT_NO = @PROJECT_NO
		                and CHECKITEM_SEQ > (
			                select CHECKITEM_SEQ from PROJECT_CHECKITEM M1
			                inner join CODE_CHECKPOINT_ITEM M2 on M2.SEQ = M1.CHECKITEM_SEQ
			                where M1.PROJECT_NO = @PROJECT_NO and M2.CTRL_POINT = 'C'
		                )
                end";

            ExecuteCommand(sql, models);
        }

        /// <summary>
        /// 更新工程會異動檢核點
        /// 且 CTRL_POINT 如果是 C，會同步更新 CHECKITEM_SEQ 大於 CTRL_POINT=C 的資料
        /// </summary>
        /// <param name="models"></param>
        public void UpdatePccProjChkItems(List<PCCProjChkItemModel> models)
        {
            string sql = @"
                update M1
                set 
                    PCC_ESTIMATED_ENDDATE = (case when @PCC_ESTIMATED_ENDDATE is null then PCC_ESTIMATED_ENDDATE else @PCC_ESTIMATED_ENDDATE end),
                    PCC_ACTUAL_ENDDATE = @PCC_ACTUAL_ENDDATE
                from PROJECT_CHECKITEM M1
                inner join CODE_CHECKPOINT_ITEM M2 on M2.SEQ = M1.CHECKITEM_SEQ and M2.CTRL_POINT = @CTRL_POINT
                where M1.PROJECT_NO = @PROJECT_NO

                if (@CTRL_POINT = 'C')
                begin
	                update PROJECT_CHECKITEM
	                set
                        PCC_ESTIMATED_ENDDATE = (case when @PCC_ESTIMATED_ENDDATE is null then PCC_ESTIMATED_ENDDATE else @PCC_ESTIMATED_ENDDATE end),
		                PCC_ACTUAL_ENDDATE = @PCC_ACTUAL_ENDDATE
	                where PROJECT_NO = @PROJECT_NO
		                and CHECKITEM_SEQ > (
			                select CHECKITEM_SEQ from PROJECT_CHECKITEM M1
			                inner join CODE_CHECKPOINT_ITEM M2 on M2.SEQ = M1.CHECKITEM_SEQ
			                where M1.PROJECT_NO = @PROJECT_NO and M2.CTRL_POINT = 'C'
		                )
                end";

            ExecuteCommand(sql, models);
        }

        /// <summary>
        /// 透過工程會API資料更新使用標案系統資料的計畫工程進度
        /// </summary>
        /// <param name="models"></param>
        public void UpdateEngProgressUseFtyDataByPcc(List<ProjectEngProgressInsertModel> models)
        {
            string sql = $@"update PROJECT_ENGINEERING_PROGRESS 
                            set TEN_RES_PRG = @TEN_RES_PRG,
	                            TEN_ACT_PRG = @TEN_ACT_PRG,
                                IPC_RES_PRG = @IPC_RES_PRG, 
                                IPC_ACT_PRG = @IPC_ACT_PRG,
                                EXECUTE_CONDITION = @EXECUTE_CONDITION,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow},
                                IS_SYNC_PCC = 1
                            where PROJECT_NO = @PROJECT_NO and YEAR = @YEAR and MONTH = @MONTH
                            -- 若沒資料則新增
                            if @@rowcount = 0
                            begin      
                                insert into PROJECT_ENGINEERING_PROGRESS (
                                    PROJECT_NO, 
                                    YEAR, 
                                    MONTH,
                                    EXECUTE_CONDITION,
                                    IS_SEND, 
                                    SEND_DATE,
                                    IPC_RES_PRG, 
                                    IPC_ACT_PRG,
                                    TEN_RES_PRG, 
                                    TEN_ACT_PRG,
                                    CRT_USER, 
                                    CRT_DATE, 
                                    MDF_USER, 
                                    MDF_DATE,
                                    IS_SYNC_PCC)
                                VALUES (
                                    @PROJECT_NO, 
                                    @YEAR, 
                                    @MONTH,
                                    @EXECUTE_CONDITION,
                                    1, 
                                    {DTNow},
                                    @IPC_RES_PRG, 
                                    @IPC_ACT_PRG,
                                    @TEN_RES_PRG, 
                                    @TEN_ACT_PRG,
                                    @CRT_USER,
                                    {DTNow},
                                    @MDF_USER,
                                    {DTNow},
                                    1)
                            end";
            ExecuteCommand(sql, models);
        }

        /// <summary>
        /// 更新使用標案系統資料的計畫工程進度
        /// </summary>
        /// <param name="models"></param>
        public void UpdateEngProgressUseFtyData(List<ProjectEngProgressInsertModel> models)
        {
            string sql = $@"update PROJECT_ENGINEERING_PROGRESS 
                            set TEN_RES_PRG = @TEN_RES_PRG,
	                            TEN_ACT_PRG = @TEN_ACT_PRG,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            where PROJECT_NO = @PROJECT_NO and YEAR = @YEAR and MONTH = @MONTH
                            -- 若沒資料則新增
                            if @@rowcount = 0
                            begin      
                                insert into PROJECT_ENGINEERING_PROGRESS (
                                    PROJECT_NO, 
                                    YEAR, 
                                    MONTH,
                                    TEN_RES_PRG, 
                                    TEN_ACT_PRG,
                                    CRT_USER, 
                                    CRT_DATE, 
                                    MDF_USER, 
                                    MDF_DATE)
                                VALUES (
                                    @PROJECT_NO, 
                                    @YEAR, 
                                    @MONTH,
                                    @TEN_RES_PRG, 
                                    @TEN_ACT_PRG,
                                    @CRT_USER,
                                    {DTNow},
                                    @MDF_USER,
                                    {DTNow})
                            end";
            ExecuteCommand(sql, models);
        }

        /// <summary>
        /// 取得落後原因類別
        /// </summary>
        /// <returns></returns>
        public async Task<List<CodeDelayClassModel>> GetCodeDelayClass()
        {
            string sql = @"select 
                                DELAY_CLASS_ID,
                                DELAY_CLASS_ITEM,
                                DELAY_CLASS_SUB_ID,
                                DELAY_CLASS_SUB_ITEM
                            from CODE_DELAY_CLASS (nolock)";
            return (await ExecuteQueryAsync<CodeDelayClassModel>(sql, null)).ToList();
        }

        /// <summary>
        /// 增修 計畫落後原因
        /// </summary>
        /// <param name="models"></param>
        public void AddMdfDelayCausal(List<ProjectDelayCausalModel> models)
        {
            string sql = $@"
                update PROJECT_DELAY_CAUSAL 
                set
                    DELAY_KIND = @DELAY_KIND,
                    DELAY_CLASS_C = @DELAY_CLASS_C,
                    DELAY_SUBCLASS_C = @DELAY_SUBCLASS_C,
                    DELAY_CAUSAL = @DELAY_CAUSAL,
                    DELAY_RESPON = @DELAY_RESPON,
                    SOLUTION = @SOLUTION,
                    COORDINATION = @COORDINATION,
                    DEADLINES = @DEADLINES,
                    MDF_USER = @MDF_USER,
                    MDF_DATE = {DTNow}
                where PROJECT_NO = @PROJECT_NO 
                    and DATA_YEAR = @YEAR 
                    and DATA_MONTH = @MONTH
                -- 若沒異動則新增
                if @@rowcount = 0
                begin
                    insert into PROJECT_DELAY_CAUSAL (
                        PROJECT_NO,
                        DATA_YEAR,
                        DATA_MONTH,
                        DELAY_KIND,
                        DELAY_CLASS_C,
                        DELAY_SUBCLASS_C,
                        DELAY_CAUSAL,
	                    DELAY_RESPON,
	                    SOLUTION,
	                    COORDINATION,
	                    DEADLINES,
	                    CRT_USER,
                        CRT_DATE,
                        MDF_USER,
                        MDF_DATE)
                    values(
                        @PROJECT_NO,
                        @YEAR,
                        @MONTH,
                        @DELAY_KIND,
                        @DELAY_CLASS_C,
                        @DELAY_SUBCLASS_C,
                        @DELAY_CAUSAL,
                        @DELAY_RESPON,
                        @SOLUTION,
                        @COORDINATION,
                        @DEADLINES,
                        @CRT_USER,
                        {DTNow},
                        @MDF_USER,
                        {DTNow})
                END";

            ExecuteCommand(sql, models);
        }

        /// <summary>
        /// 取得工程進度資料清單
        /// </summary>
        /// <param name="projectNos"></param>
        /// <param name="YEAR"></param>
        /// <param name="MONTH"></param>
        /// <returns></returns>
        public async Task<List<ProjectEngineeringProgressModel>> GetProjEngProgresses(List<string> projectNos, int YEAR, string MONTH)
        {
            string sql = @"select
                                PROJECT_NO,
                                IPC_RES_PRG,
                                IPC_ACT_PRG
                            from PROJECT_ENGINEERING_PROGRESS (nolock)
                            where YEAR = @YEAR 
                                and MONTH = @MONTH
                                and PROJECT_NO in @projectNos";

            return (await ExecuteQueryAsync<ProjectEngineeringProgressModel>(sql, new { projectNos, YEAR, MONTH })).ToList();
        }

        /// <summary>
        /// 取得落後原因類型
        /// </summary>
        /// <param name="PROJECT_NO">計畫編號</param>
        /// <param name="FILL_END_DATE">填報週期迄</param>
        /// <returns></returns>
        public string GetDelayKind(string PROJECT_NO, DateTime FILL_END_DATE)
        {
            string sql = "select dbo.FN_GET_DELAY_TYPE(@PROJECT_NO, null, @FILL_END_DATE)";
            return ExecuteQuery<string>(sql, new { PROJECT_NO, FILL_END_DATE }).FirstOrDefault();
        }

        /// <summary>
        /// 更新IS_SEND SEND_DATE 
        /// </summary>
        /// <param name="projectNos"></param>
        /// <param name="model"></param>
        public void UpdateSyncFlag(List<string> projectNos, ProjectFillCycleModel model)
        {
            string sql = $@"update PROJECT_ENGINEERING_PROGRESS 
                            set IS_SEND = 1,
                                SEND_DATE = (case when SEND_DATE is null then {DTNow} else SEND_DATE end)
                            where PROJECT_NO in @projectNos and YEAR = @YEAR and MONTH = @MONTH";
            ExecuteCommand(sql, new { projectNos, YEAR = model.PROJECT_YEAR, MONTH = model.PROJECT_MONTH });
        }

        /// <summary>
        /// 取得工程標案資料集
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetPccSrcTables()
        {
            string sql = @"select DSNAME as value,
                                  DSNAME_CHI as text
                           from PCC_SRC_TABLE (nolock)";
            return (await ExecuteQueryAsync<DropDownListModel>(sql)).ToList();
        }
    }
}
