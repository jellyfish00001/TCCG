using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Services;
using SDO.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class IPCSetParamDac : Dac, IIPCSetParamDac
    {
        public IPCSetParamDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 取得setItem類別的系統參數
        /// </summary>
        /// <param name="setItem"></param>
        /// <param name="delFlg"></param>
        /// 
        /// <returns></returns>
        public async Task<IList<IPCSetParamModel>> GetSysParams(string setItem, bool? delFlg, int fromWhere = 0)
        {
            string sql = @" SELECT 
                                A.SET_ITEM,
                                A.SET_TYPE,
                                A.SET_TYPE AS OLD_SET_TYPE,
                                A.SET_VALUE,
                                A.MEMO,
						        A.DEL_FLG,
                                A.SORT_ORDER,
                                B.EDITABLE
                            FROM SET_PARAM A (NOLOCK)
                            INNER JOIN SET_PARAMITEM B (NOLOCK) ON B.SET_ITEM = A.SET_ITEM 
                            WHERE A.SET_ITEM = @SET_ITEM";
            if (delFlg != null)
            {
                sql += @" AND A.DEL_FLG = @DEL_FLG AND B.DEL_FLG = @DEL_FLG";
            }

            string DB = MainDBKey;
            // 前端呼叫指定DB
            switch ((DBConnection)fromWhere)
            {
                case DBConnection.SCDBKey:
                    DB = SCDBKey;
                    break;
                case DBConnection.RISDBKey:
                    DB = RISDBKey;
                    break;
                case DBConnection.IPCDBKey:
                    DB = IPCDBKey;
                    break;
                case DBConnection.INNDBKey:
                    DB = INNDBKey;
                    break;
                case DBConnection.PWSDBKey:
                    DB = PWSDBKey;
                    break;
                case DBConnection.RDDBKey:
                    DB = RDDBKey;
                    break;
            }

            sql += @" ORDER BY A.SORT_ORDER";

            return await ExecuteQueryAsync<IPCSetParamModel>(sql, new { SET_ITEM = setItem, DEL_FLG = delFlg }, DB);
        }

        /// <summary>
        /// 新增 SetParam資料
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public bool Insert(List<IPCSetParamModel> models)
        {
            string sql = $@"INSERT INTO SET_PARAM 
                               (SET_ITEM,
                                SET_TYPE,
                                SET_VALUE,
                                MEMO,
                                SORT_ORDER,
                                DEL_FLG,
                                CRT_DATE,
                                CRT_USER,
                                MDF_DATE,
                                MDF_USER
                                ) 
                            VALUES(
                                @SET_ITEM,
                                @SET_TYPE,
                                @SET_VALUE,
                                @MEMO,
                                @SORT_ORDER,
                                @DEL_FLG,
                                {DTNow},
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER)";
            return ExecuteCommand(sql, models);
        }

        /// <summary>
        /// 修改 SetParam資料
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public bool Update(List<IPCSetParamModel> models)
        {
            string sql = $@"UPDATE SET_PARAM 
                            SET 
                                SET_TYPE = @SET_TYPE,
                                SET_VALUE = @SET_VALUE,
                                MEMO = @MEMO,
                                SORT_ORDER = @SORT_ORDER,
                                DEL_FLG = @DEL_FLG,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE SET_ITEM = @SET_ITEM 
                            AND   SET_TYPE = @OLD_SET_TYPE";
            return ExecuteCommand(sql, models);
        }

    }
}
