using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Base.Utils.Enum;
using SDO.Base.Utils.Models;
using SDO.Models;
using SDO.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;


namespace SDO.Dac
{
    public class DropDownDac : Dac, IDropDownDac
    {
        private readonly IUserData _userData;
        public DropDownDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
            this._userData = profile.GetLoginUser();
            this.UserId = _userData?.USER_ID ?? "";
        }

        /// <summary>
        /// 取得指定角色清單
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetRoleUser(string roleId)
        {
            string sql = @" SELECT USER_NAME as text , E.USER_ID as value
                            FROM EMP_USER E (NOLOCK)
                            LEFT JOIN MAP_USER_ROLE (NOLOCK) M
	                            ON E.USER_ID = M.USER_ID 
                            WHERE M.ROLE_ID = ?ROLE_ID?";

            return (await ExecuteQueryAsync<DropDownListModel>(sql, new { ROLE_ID = roleId })).ToList();
        }

        /// <summary>
        /// 取得機關下拉選單資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetOrganList()
        {
            string sql = @"SELECT
                            M1.OU_ID AS value,
                            M1.OU_NAME AS text
                        FROM SCORG_UNITM (NOLOCK) M1
                        INNER JOIN GPREL_GRP_GRPM (NOLOCK) M2 
                            on M1.OU_ID=M2.T_GROUP_ID AND M2.T_GROUP_KIND='SC_OU'
                        WHERE 
                            M1.OU_KIND=1
                            AND M1.OU_NAME <> '-'
                            AND M1.OU_NAME <>'桃園市政府'
                            AND LEN(M1.OU_ID) > 7
                            AND M1.IS_ENABLE = 'Y'
                        GROUP BY M1.OU_ID, M1.OU_NAME, M1.OU_SORT_ORDER
                        ORDER BY M1.OU_SORT_ORDER";
            return (await ExecuteQueryAsync<DropDownListModel>(sql, null, SCDBKey)).ToList();
        }

        /// <summary>
        /// 取得登入者的機關下拉選單資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetOrgByUsr()
        {
            string sql = @"select 
	                            OU_ID AS value,
                                OU_NAME AS text
                            from SCREL_ORG_USRMV (nolock)
                            where USR_ID = @UserId
	                            and OU_KIND = '1'
	                            and REL_KIND = '1'";
            return (await ExecuteQueryAsync<DropDownListModel>(sql, new { UserId }, SCDBKey)).ToList();
        }

        /// <summary>
        /// 取得審核狀態下拉選單資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetSendStatusList()
        {
            string sql = @" SELECT 
                                SET_TYPE AS value,
                                SET_VALUE AS text
                            FROM SET_PARAM  (NOLOCK)
                            WHERE SET_ITEM = 'SEND_STATUS'";
            return (await ExecuteQueryAsync<DropDownListModel>(sql)).ToList();
        }

        /// <summary>
        /// 取得審核狀態下拉選單資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetFundList()
        {
            string sql = @" SELECT 
                                FUNDNO AS value,
                                FUNDNAME AS text,
                                OU_ID AS param
                            FROM FUNDORG  (NOLOCK)
                          ";
            return (await ExecuteQueryAsync<DropDownListModel>(sql, null, PWSDBKey)).ToList();
        }

        /// <summary>
        /// 取得審核狀態下拉選單資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetFundOrgList()
        {
            string sql = @" SELECT 
                                 OU_ID AS value,
                                 dbo.FN_GetOuName(OU_ID, 3) AS text
                            FROM FUNDORG  (NOLOCK)
                          ";
            return (await ExecuteQueryAsync<DropDownListModel>(sql, null, PWSDBKey)).ToList();
        }

        /// <summary>
        /// 取得機關下的使用者帳號下拉選單資料
        /// </summary>
        /// <param name="ouId"></param>
        /// <param name="isEnable">是否啟用</param>
        /// <returns></returns>
        public async Task<List<SponsorDropDownListModel>> GetUserInfoByOrg(string ouId,  bool isEnable)
        {
            string sql = GetUserInfoByOrgSql(isEnable);
            return (await ExecuteQueryAsync<SponsorDropDownListModel>(sql, new { OU_ID = ouId }, SCDBKey)).ToList();
        }

        private string GetUserInfoByOrgSql(bool isEnable)
        {
            string isEnableSql = isEnable ? "AND fn1.ID_ENABLE = 'Y'" : string.Empty;
            string sql = string.Empty;

            sql = @$"
                    SELECT 
                           fn1.USR_ID AS value, fn1.USR_NAME AS text, fn1.ID_ENABLE AS param,
                           fn1.USR_TITLE AS SPONSOR_TITLE,OuName AS SPONSOR_UNIT,UnitOuName AS SPONSOR_ORG
                    FROM [IPC_SCREL_OU_USRV] fn 
                    INNER JOIN SCUSERM fn1 ON fn.USR_ID = fn1.USR_ID
                    WHERE fn.OU_ID IN (            
                        SELECT A.OU_ID
                        FROM [IPC_SCORG_UNITV] A
                        WHERE A.PARENT_OU_ID = @OU_ID AND A.OU_KIND = '3'
                        UNION
                        SELECT A.OU_ID
                        FROM [IPC_SCORG_UNITV] A
                        WHERE A.PARENT_OU_ID = @OU_ID AND A.OU_KIND = '2'
                        UNION
                        SELECT OU_ID
                        FROM [IPC_SCORG_UNITV] B
                        WHERE B.PARENT_OU_ID IN (
                            SELECT C.OU_ID
                            FROM [IPC_SCORG_UNITV] C
                            WHERE C.PARENT_OU_ID = @OU_ID AND C.OU_KIND = '2')
                    )
                    {isEnableSql}
                    ORDER BY fn1.USR_NAME";
            return sql;
        }

        /// <summary>
        /// 取得機關下的使用者帳號下拉選單資料
        /// </summary>
        /// <param name="ouId"></param>
        /// <param name="undertakerType">承辦人類型(主管、執行、協辦、代辦)</param>
        /// <param name="isEnable">是否啟用</param>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetUserByOrg(string ouId, int undertakerType, bool isEnable)
        {
            string sql = GetUserByOrgSql(undertakerType, isEnable);
            return (await ExecuteQueryAsync<DropDownListModel>(sql, new { OU_ID = ouId }, SCDBKey)).ToList();
        }

        private string GetUserByOrgSql(int undertakerType, bool isEnable)
        {
            string isEnableSql = isEnable ? "AND fn1.ID_ENABLE = 'Y'" : string.Empty;

            string sql = string.Empty;
            switch ((UndertakerTypeEnum)undertakerType)
            {
                //主管機關
                case UndertakerTypeEnum.MASTER:
                    sql = @$"
                        SELECT fn1.USR_ID AS value, fn1.USR_NAME AS text, fn1.ID_ENABLE AS param
                        FROM [IPC_SCREL_OU_USRV] fn 
                        INNER JOIN SCUSERM fn1 ON fn.USR_ID = fn1.USR_ID
                        WHERE fn.OU_ID IN (            
                            SELECT A.OU_ID
                            FROM [IPC_SCORG_UNITV] A
                            WHERE A.PARENT_OU_ID = @OU_ID AND A.OU_KIND = '3'
                            UNION
                            SELECT A.OU_ID
                            FROM [IPC_SCORG_UNITV] A
                            WHERE A.PARENT_OU_ID = @OU_ID AND A.OU_KIND = '2'
                            UNION
                            SELECT OU_ID
                            FROM [IPC_SCORG_UNITV] B
                            WHERE B.PARENT_OU_ID IN (
                                SELECT C.OU_ID
                                FROM [IPC_SCORG_UNITV] C
                                WHERE C.PARENT_OU_ID = @OU_ID AND C.OU_KIND = '2')
                        )
                        {isEnableSql}
                        ORDER BY fn1.USR_NAME";
                    break;
                //執行機關
                case UndertakerTypeEnum.EXEC:
                    sql = @$"
                        SELECT fn1.USR_ID AS value, fn1.USR_NAME AS text, fn1.ID_ENABLE AS param
                        FROM [IPC_SCREL_OU_USRV] fn 
                        INNER JOIN SCUSERM fn1 ON fn.USR_ID = fn1.USR_ID
                        WHERE fn.OU_ID IN (            
                            SELECT A.OU_ID
                            FROM [IPC_SCORG_UNITV] A
                            WHERE A.PARENT_OU_ID = @OU_ID AND A.OU_KIND = '3'
                            UNION
                            SELECT A.OU_ID
                            FROM [IPC_SCORG_UNITV] A
                            WHERE A.PARENT_OU_ID = @OU_ID AND A.OU_KIND = '2'
                            UNION
                            SELECT OU_ID
                            FROM [IPC_SCORG_UNITV] B
                            WHERE B.PARENT_OU_ID IN (
                                SELECT C.OU_ID
                                FROM [IPC_SCORG_UNITV] C
                                WHERE C.PARENT_OU_ID = @OU_ID AND C.OU_KIND = '2')
                        )
                        {isEnableSql}
                        ORDER BY fn1.USR_NAME";
                    break;
                //協辦機關
                case UndertakerTypeEnum.ASSISTANT:
                    sql = @$"
                        SELECT fn1.USR_ID AS value, fn1.USR_NAME AS text, fn1.ID_ENABLE AS param
                        FROM [IPC_SCREL_OU_USRV] fn 
                        INNER JOIN SCUSERM fn1 ON fn.USR_ID = fn1.USR_ID
                        WHERE fn.OU_ID IN (        
	                        SELECT A.OU_ID
	                        FROM [IPC_SCORG_UNITV] A
	                        WHERE A.PARENT_OU_ID = @OU_ID AND A.OU_KIND = '3'
	                        UNION
	                        SELECT OU_ID
	                        FROM [IPC_SCORG_UNITV] B
	                        WHERE B.PARENT_OU_ID IN (
                                SELECT C.OU_ID
                                FROM [IPC_SCORG_UNITV] C
                                WHERE C.PARENT_OU_ID = @OU_ID AND C.OU_KIND = '2')
                        )
                        {isEnableSql}
                        ORDER BY fn1.USR_NAME";
                    break;
                //代辦機關
                case UndertakerTypeEnum.BUDGET_HOLD:
                    sql = @$"
                        SELECT fn1.USR_ID AS value, fn1.USR_NAME AS text, fn1.ID_ENABLE AS param
                        FROM [IPC_SCREL_OU_USRV] fn 
                        INNER JOIN SCUSERM fn1 ON fn.USR_ID = fn1.USR_ID
                        WHERE fn.OU_ID IN (        
                            SELECT A.OU_ID
                            FROM [IPC_SCORG_UNITV] A
                            WHERE A.PARENT_OU_ID = @OU_ID AND A.OU_KIND = '3'
                            UNION
                            SELECT OU_ID
                            FROM [IPC_SCORG_UNITV] B
                            WHERE B.PARENT_OU_ID IN (
                                SELECT C.OU_ID
                                FROM [IPC_SCORG_UNITV] C
                                WHERE C.PARENT_OU_ID = @OU_ID AND C.OU_KIND = '2')
                        )
                        {isEnableSql}
                        ORDER BY fn1.USR_NAME";
                    break;
            }
            return sql;
        }



        /// <summary>
        /// 取得 辦理地點 (區)
        /// </summary>
        /// <param name="cityId"></param>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetCodeTownByCityId(string cityId)
        {
            string sql = @" SELECT 
                                TOWN_ID AS value,
                                TOWNNAME AS text
                            FROM CODE_TOWN (NOLOCK)
                            WHERE CITY_ID = @CITY_ID";
            return (await ExecuteQueryAsync<DropDownListModel>(sql, new { CITY_ID = cityId })).ToList();
        }

        /// <summary>
        /// 取得作業階段
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetWorkStage(string PROJECT_NO)
        {
            string sql = $@"
                DECLARE @COUNT INT
                SET @COUNT = (SELECT COUNT(PROJECT_NO)
                              FROM PROJECT_BASIC_LOG (NOLOCK)
                              WHERE PROJECT_NO = @PROJECT_NO 
                              -- 立案審核通過
                              AND LOG_STATUS = '4')
                SELECT 
                    A.SET_TYPE AS value,
                    A.SET_VALUE AS text
                FROM SET_PARAM A (NOLOCK)
                INNER JOIN SET_PARAMITEM B (NOLOCK) ON B.SET_ITEM = A.SET_ITEM 
                WHERE A.SET_ITEM = 'PROJECT_STAGE'
                -- 若有'立案審核通過'才撈出'執行情形'
                AND (@COUNT != 0 OR SET_TYPE != 'S2')";
            return (await ExecuteQueryAsync<DropDownListModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 取得建設類別
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetComPlanKind()
        {
            string sql = @"select
	                            SET_VALUE as text,
	                            SET_TYPE as value
                            from SET_PARAM (nolock)
                            where DEL_FLG = 0 and SET_ITEM = 'COM_PLANKIND'";
            return (await ExecuteQueryAsync<DropDownListModel>(sql)).ToList();
        }

        /// 取得主要提案類別
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetInnPropsalType(string INN_YEAR)
        {
            string sql = $@"
                SELECT 
                    ID AS value,
                    CODE_VALUE AS text
                FROM INN_PROJECT_TITLE
                WHERE YEAR = @INN_YEAR";
            return (await ExecuteQueryAsync<DropDownListModel>(sql, new { INN_YEAR }, INNDBKey)).ToList();

        }

        /// 取得主要提案類別
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetInnYear()
        {
            string sql = $@"
                SELECT 
                    INN_YEAR AS value,
                    INN_YEAR AS text
                FROM INN_ASSIGN_ORG
                ORDER BY INN_YEAR DESC";
            return (await ExecuteQueryAsync<DropDownListModel>(sql, null, INNDBKey)).ToList();

        }

        /// <summary>
        /// 取機關的單位
        /// </summary>
        /// <param name="OU_ID"></param>
        /// <returns></returns>
        public async Task<List<DropDownListModel>> GetUnitList(string OU_ID)
        {
            string sql = $@"
                           SELECT 
                                  OU_NAME AS text,
                                  OU_ID AS value
                             FROM IPC_SCORG_UNITV
                            WHERE PARENT_OU_ID = @OU_ID
                          ";
            return (await ExecuteQueryAsync<DropDownListModel>(sql, new { OU_ID }, SCDBKey)).ToList();
        }
    }
}
