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
    public class IPCCodeDac : Dac, IIPCCodeDac
    {
        public IPCCodeDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 取得執行方式
        /// </summary>
        /// <param name="CP_KIND"></param>
        /// <param name="isShowDel"></param>
        /// <returns></returns>
        public async Task<List<IPCCodeCheckpointModel>> GetCodeCheckpoint(string CP_KIND, bool isShowDel)
        {
            string sql = $@"SELECT 
                                CHECKPOINT_CLASS_ID,
                                CHECKPOINT_CLASS,
                                CP_KIND,
                                dbo.FN_GetSetParam('CP_KIND', CP_KIND) AS CP_KIND_DESC,
                                IS_BASIC,
                                DEL_FLG
                            FROM CODE_CHECKPOINT (NOLOCK)
                            WHERE CP_KIND = @CP_KIND";
            if (!isShowDel)
                sql += " AND DEL_FLG = 0";
            return (await ExecuteQueryAsync<IPCCodeCheckpointModel>(sql, new { CP_KIND })).ToList();
        }

        /// <summary>
        /// 新增執行方式
        /// </summary>
        /// <param name="model"></param>
        public void InsertCodeCheckpoint(List<IPCCodeCheckpointModel> model)
        {
            string sql = $@"INSERT INTO CODE_CHECKPOINT
                                (CHECKPOINT_CLASS,
                                CP_KIND,
                                IS_BASIC,
                                DEL_FLG,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES
                                (@CHECKPOINT_CLASS,
                                @CP_KIND,
                                @IS_BASIC,
                                @DEL_FLG,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 修改執行方式
        /// </summary>
        /// <param name="model"></param>
        public void UpdateCodeCheckpoint(List<IPCCodeCheckpointModel> model)
        {
            string sql = $@"UPDATE CODE_CHECKPOINT
                            SET CHECKPOINT_CLASS = @CHECKPOINT_CLASS,
                                CP_KIND = @CP_KIND,
                                IS_BASIC = @IS_BASIC,
                                DEL_FLG = @DEL_FLG,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE CHECKPOINT_CLASS_ID = @CHECKPOINT_CLASS_ID";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 取得自訂檢核點
        /// </summary>
        /// <param name="CHK_POINT_CLASS_ID"></param>
        /// <param name="forSettings"></param>
        /// <returns></returns>
        public async Task<List<IPCCusChkItemModel>> GetCusChkItem(int CHK_POINT_CLASS_ID, bool forSettings)
        {
            string sql = $@"SELECT
                                SEQ,
                                CHECKPOINT_CLASS_ID AS CHK_POINT_CLASS_ID,
                                CHK_NAME AS NAME,
                                PROGRESS,
                                IS_ENABLE,
                                CTRL_POINT,
                                CITY_GOV_ID,
                                DEL_FLG
                            FROM CODE_CHECKPOINT_ITEM (NOLOCK)
                            WHERE CHECKPOINT_CLASS_ID = @CHK_POINT_CLASS_ID";
            // 若非用於設定，須排除已停用資料
            if (!forSettings)
                sql += " AND DEL_FLG = 0";

            sql += " ORDER BY PROGRESS";

            return (await ExecuteQueryAsync<IPCCusChkItemModel>(sql, new { CHK_POINT_CLASS_ID })).ToList();
        }

        /// <summary>
        /// 新增自訂檢核點
        /// </summary>
        /// <param name="model"></param>
        public void InsertCusChkItem(List<IPCCusChkItemModel> model)
        {
            string sql = $@"INSERT INTO CODE_CHECKPOINT_ITEM
                                (CHECKPOINT_CLASS_ID,
                                CHK_NAME,
                                PROGRESS,
                                IS_ENABLE,
                                CTRL_POINT,
                                CITY_GOV_ID,
                                DEL_FLG,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES
                                (@CHK_POINT_CLASS_ID,
                                @NAME,
                                @PROGRESS,
                                @IS_ENABLE,
                                @CTRL_POINT,
                                @CITY_GOV_ID,
                                @DEL_FLG,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 修改自訂檢核點
        /// </summary>
        /// <param name="model"></param>
        public void UpdateCusChkItem(List<IPCCusChkItemModel> model)
        {
            string sql = $@"UPDATE CODE_CHECKPOINT_ITEM
                            SET CHK_NAME = @NAME,
                                PROGRESS = @PROGRESS,
                                IS_ENABLE = @IS_ENABLE,
                                CTRL_POINT = @CTRL_POINT,
                                CITY_GOV_ID = @CITY_GOV_ID,
                                DEL_FLG = @DEL_FLG,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 取得落後原因類別
        /// </summary>
        /// <param name="DELAY_CLASS_ID"></param>
        /// <param name="DEL_FLG"></param>
        /// <returns></returns>
        public async Task<List<IPCCodeDelayClassModel>> GetCodeDelayClass(string DELAY_CLASS_ID, bool? DEL_FLG)
        {
            string sql = $@"SELECT 
                                DELAY_CLASS_ID,
                                DELAY_CLASS_ITEM,
                                DELAY_CLASS_SUB_ID,
                                DELAY_CLASS_SUB_ID AS OLD_DELAY_CLASS_SUB_ID,
                                DELAY_CLASS_SUB_ITEM,
                                CITY_GOV_ID,
                                IS_SYSTEM,
                                IS_ENABLED,
                                DEL_FLG
                            FROM CODE_DELAY_CLASS (NOLOCK)
                            WHERE DELAY_CLASS_ID = @DELAY_CLASS_ID";
            if (DEL_FLG != null)
            {
                sql += @" AND DEL_FLG = DEL_FLG";
            }
            sql += @" ORDER BY DELAY_CLASS_SUB_ID";
            return (await ExecuteQueryAsync<IPCCodeDelayClassModel>(sql, new { DELAY_CLASS_ID = DELAY_CLASS_ID, DEL_FLG = DEL_FLG })).ToList();
        }

        /// <summary>
        /// 取得已存在的落後項目代碼
        /// </summary>
        /// <param name="delayClassSubIds"></param>
        /// <returns></returns>
        public async Task<List<string>> GetExistDelayClassSubId(string[] delayClassSubIds)
        {
            string sql = $@"SELECT DELAY_CLASS_SUB_ID 
                            FROM CODE_DELAY_CLASS (NOLOCK) 
                            WHERE DELAY_CLASS_SUB_ID IN @DELAY_CLASS_SUB_ID";
            return (await ExecuteQueryAsync<string>(sql, new { DELAY_CLASS_SUB_ID = delayClassSubIds })).ToList();
        }

        /// <summary>
        /// 取得已存在的落後項目
        /// </summary>
        /// <param name="delayClassSubItems"></param>
        /// <returns></returns>
        public async Task<List<string>> GetExistDelayClassSubItem(string[] delayClassSubItems)
        {
            string sql = $@"SELECT DELAY_CLASS_SUB_ITEM 
                            FROM CODE_DELAY_CLASS (NOLOCK) 
                            WHERE DELAY_CLASS_SUB_ITEM IN @DELAY_CLASS_SUB_ITEM";
            return (await ExecuteQueryAsync<string>(sql, new { DELAY_CLASS_SUB_ITEM = delayClassSubItems })).ToList();
        }

        /// <summary>
        /// 新增落後原因類別
        /// </summary>
        /// <param name="model"></param>
        public void InsertCodeDelayClass(List<IPCCodeDelayClassModel> model)
        {
            string sql = $@"INSERT INTO CODE_DELAY_CLASS
                                (DELAY_CLASS_ID,
                                DELAY_CLASS_ITEM,
                                DELAY_CLASS_SUB_ID,
                                DELAY_CLASS_SUB_ITEM,
                                CITY_GOV_ID,
                                IS_SYSTEM,
                                IS_ENABLED,
                                DEL_FLG,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES
                                (@DELAY_CLASS_ID,
                                @DELAY_CLASS_ITEM,
                                @DELAY_CLASS_SUB_ID,
                                @DELAY_CLASS_SUB_ITEM,
                                @CITY_GOV_ID,
                                @IS_SYSTEM,
                                @IS_ENABLED,
                                @DEL_FLG,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 修改落後原因類別
        /// </summary>
        /// <param name="model"></param>
        public void UpdateCodeDelayClass(List<IPCCodeDelayClassModel> model)
        {
            string sql = $@"UPDATE CODE_DELAY_CLASS
                            SET DELAY_CLASS_ID = @DELAY_CLASS_ID,
                                DELAY_CLASS_ITEM = @DELAY_CLASS_ITEM,
                                DELAY_CLASS_SUB_ID = @DELAY_CLASS_SUB_ID,
                                DELAY_CLASS_SUB_ITEM = @DELAY_CLASS_SUB_ITEM,
                                CITY_GOV_ID = @CITY_GOV_ID,
                                IS_SYSTEM = @IS_SYSTEM,
                                IS_ENABLED = @IS_ENABLED,
                                DEL_FLG = @DEL_FLG,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE DELAY_CLASS_SUB_ID = @OLD_DELAY_CLASS_SUB_ID";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 修改落後原因類別
        /// </summary>
        /// <param name="model"></param>
        public void UpdateCodeDelayClass(List<IPCSetParamModel> model)
        {
            string sql = $@"update CODE_DELAY_CLASS
                            set 
                                DELAY_CLASS_ID = @SET_TYPE,
                                DELAY_CLASS_ITEM = @SET_VALUE,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            where DELAY_CLASS_ID = @OLD_SET_TYPE";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 取得預算來源
        /// </summary>
        /// <param name="LEVEL_MARK">1:本府預算來源 2:中央預算來源</param>
        /// <param name="DEL_FLG"></param>
        /// <returns></returns>
        public async Task<List<IPCCodePlanItemModel>> GetCodePlanItem(string LEVEL_MARK, bool? DEL_FLG)
        {
            string sql = $@"SELECT 
                                PLAN_YEAR,
                                PLAN_ITEM_ID,
                                PLAN_ITEM_ID AS OLD_PLAN_ITEM_ID,
                                PLAN_ITEM_NAME,
                                PARENT_ID,
                                ORGAN_BUDGET_SEQ,
                                PLAN_ITEM_BUDGET,
                                LEVEL_MARK,
                                DEL_FLG
                            FROM CODE_PLAN_ITEM (NOLOCK)
                            WHERE LEVEL_MARK = @LEVEL_MARK";
            if (DEL_FLG != null)
            {
                sql += @" AND DEL_FLG = @DEL_FLG ";
            }
            sql += @" ORDER BY PLAN_ITEM_ID";
            return (await ExecuteQueryAsync<IPCCodePlanItemModel>(sql, new { LEVEL_MARK, DEL_FLG })).ToList();
        }

        /// <summary>
        /// 新增預算來源
        /// </summary>
        /// <param name="model"></param>
        public void InsertCodePlanItem(List<IPCCodePlanItemModel> model)
        {
            string sql = $@"INSERT INTO CODE_PLAN_ITEM
                                (PLAN_YEAR,
                                PLAN_ITEM_ID,
                                PLAN_ITEM_NAME,
                                PARENT_ID,
                                ORGAN_BUDGET_SEQ,
                                PLAN_ITEM_BUDGET,
                                LEVEL_MARK,
                                DEL_FLG,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES
                                (@PLAN_YEAR,
                                @PLAN_ITEM_ID,
                                @PLAN_ITEM_NAME,
                                @PARENT_ID,
                                @ORGAN_BUDGET_SEQ,
                                @PLAN_ITEM_BUDGET,
                                @LEVEL_MARK,
                                @DEL_FLG,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 修改預算來源
        /// </summary>
        /// <param name="model"></param>
        public void UpdateCodePlanItem(List<IPCCodePlanItemModel> model)
        {
            string sql = $@"UPDATE CODE_PLAN_ITEM
                            SET PLAN_YEAR = @PLAN_YEAR,
                                PLAN_ITEM_ID = @PLAN_ITEM_ID,
                                PLAN_ITEM_NAME = @PLAN_ITEM_NAME,
                                PARENT_ID = @PARENT_ID,
                                ORGAN_BUDGET_SEQ = @ORGAN_BUDGET_SEQ,
                                PLAN_ITEM_BUDGET = @PLAN_ITEM_BUDGET,
                                LEVEL_MARK = @LEVEL_MARK,
                                DEL_FLG = @DEL_FLG,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE LEVEL_MARK = @LEVEL_MARK AND PLAN_ITEM_ID = @OLD_PLAN_ITEM_ID";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 判斷有無該年度工作日
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<int> GetWorkingDayCountByYear(int year)
        {
            string sql = $@"SELECT COUNT(DATE)
                            FROM WORKING_DAY (NOLOCK)
                            WHERE YEAR(DATE) = @year";
            return (await ExecuteQueryAsync<int>(sql, new { year = year })).FirstOrDefault();
        }

        /// <summary>
        /// 取得工作日
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<List<IPCWorkingDayModel>> GetWorkingDay(string startDate, string endDate)
        {
            string sql = $@"SELECT DATE, IS_WORKING
                            FROM WORKING_DAY (NOLOCK)
                            WHERE DATE BETWEEN @startDate AND @endDate";
            return (await ExecuteQueryAsync<IPCWorkingDayModel>(sql, new { startDate = startDate, endDate = endDate })).ToList();
        }

        /// <summary>
        /// 產生年度工作日
        /// </summary>
        /// <param name="model"></param>
        public void InsertWorkingDay(List<IPCWorkingDayModel> model)
        {
            string sql = $@"insert into WORKING_DAY (
                                DATE, 
                                IS_WORKING,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            values (
                                @DATE, 
                                @IS_WORKING,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 修改工作日
        /// </summary>
        /// <param name="model"></param>
        public void UpdateWorkingDay(List<IPCWorkingDayModel> model)
        {
            string sql = $@"UPDATE WORKING_DAY
                            SET 
                                IS_WORKING = @IS_WORKING,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE DATE = @DATE";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 取得機關窗口維護
        /// </summary>
        /// <returns></returns>
        public async Task<List<IPCDeptContactModel>> GetDeptContact()
        {
            string sql = $@"SELECT DC_ID,
                                ORGAN,
                                SOURCE,
                                CONTACT,
                                s.USR_NAME as CONTACT_NAME,
                                TEL,
                                EMAIL
                            FROM DEPT_CONTACT (NOLOCK) p
                            LEFT JOIN {SC30_M}.SCUSERM s on s.USR_ID = p.CONTACT";
            return (await ExecuteQueryAsync<IPCDeptContactModel>(sql, null, MainDBKey)).ToList();
        }

        /// <summary>
        /// 取得機關聯絡窗口
        /// </summary>
        /// <param name="ORGAN"></param>
        /// <returns></returns>
        public async Task<List<IPCDeptContactModel>> GetDeptContactByOrgan(string ORGAN)
        {
            string sql = $@"SELECT DC_ID,
                                ORGAN,
                                SOURCE,
                                CONTACT,
                                TEL,
                                EMAIL
                            FROM DEPT_CONTACT (NOLOCK)
                            WHERE ORGAN = @ORGAN";
            return (await ExecuteQueryAsync<IPCDeptContactModel>(sql, new { ORGAN })).ToList();
        }

        /// <summary>
        /// 新增機關窗口維護
        /// </summary>
        /// <param name="model"></param>
        public void InsertDeptContact(List<IPCDeptContactModel> model)
        {
            string sql = $@"INSERT INTO DEPT_CONTACT
                                (ORGAN, 
                                SOURCE,
                                CONTACT,
                                TEL,
                                EMAIL,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES
                                (@ORGAN, 
                                @SOURCE,
                                @CONTACT,
                                @TEL,
                                @EMAIL,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除機關窗口維護
        /// </summary>
        /// <param name="ORGAN"></param>
        public void DeleteDeptContact(string ORGAN)
        {
            string sql = $@"DELETE DEPT_CONTACT WHERE ORGAN = @ORGAN";
            ExecuteCommand(sql, new { ORGAN });
        }
    }
}
