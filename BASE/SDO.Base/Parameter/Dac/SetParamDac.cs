using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class SetParamDac : Dac, ISetParamDac
    {
        public SetParamDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public SetParamDac(IConnectionControlCenter connectionControlCenter,
                 ISqlTrace trace,
                 IParameterAdaptor parameterAdaptor, 
                 IUserData userData,
                 IConfiguration configuration) : base(connectionControlCenter, trace, parameterAdaptor, userData, configuration)
        {
        }

        public async Task<SetParamModel> CheckExist(string setItem, string setType)
        {
            string sql = @"
                SELECT 
                    SET_ITEM,
                    SET_TYPE,
                    SET_VALUE,
                    MEMO,
                    DEL_FLG
                FROM   dbo.SET_PARAM  (NOLOCK)
                WHERE  SET_ITEM = ?SET_ITEM? 
                AND SET_TYPE = ?SET_TYPE?";
            return (await ExecuteQueryAsync<SetParamModel>(sql, new { SET_ITEM = setItem, SET_TYPE = setType })).FirstOrDefault();
        }

        public async Task<SetParamItemModel> CheckItemExist(string setItem)
        {
            string sql = @"
                SELECT 
                    SET_ITEM,
                    SET_ITEM_NAME,
                    MEMO,
                    EDITABLE,
                    DEL_FLG 
                FROM   dbo.SET_PARAMITEM (NOLOCK) 
                WHERE  SET_ITEM = ?SET_ITEM?";
            return (await ExecuteQueryAsync<SetParamItemModel>(sql, new { SET_ITEM = setItem })).FirstOrDefault();
        }

        public async Task Insert(SetParamModel model)
        {
            string sql = @"
                INSERT INTO dbo.SET_PARAM( 
                        SET_ITEM,
                        SET_TYPE,
                        SET_VALUE,
                        MEMO,
                        CRT_DATE,
                        CRT_USER,
                        MDF_DATE,
                        MDF_USER ) 
                VALUES( ?SET_ITEM?,
                        ?SET_TYPE?,
                        ?SET_VALUE?,
                        ?MEMO?,
                        DATEADD(HH,8,GETUTCDATE()) ,
                        ?CRT_USER?,
                        DATEADD(HH,8,GETUTCDATE()) ,
                        ?MDF_USER? )";
            await ExecuteCommandAsync(sql, model);
        }

        public async Task InsertItem(SetParamItemModel model)
        {
            string sql = @"
                INSERT INTO dbo.SET_PARAMITEM(
                        SET_ITEM,
                        SET_ITEM_NAME,
                        MEMO,
                        CRT_DATE,
                        CRT_USER,
                        MDF_DATE,
                        MDF_USER  ) 
                VALUES( ?SET_ITEM?,
                        ?SET_ITEM_NAME?,
                        ?MEMO?,
                        DATEADD(HH,8,GETUTCDATE()) ,
                        ?CRT_USER?,
                        DATEADD(HH,8,GETUTCDATE()) ,
                        ?MDF_USER? )";
            await ExecuteCommandAsync(sql, model);
        }

        public async Task Update(SetParamModel model)
        {
            string sql = @"
                UPDATE dbo.SET_PARAM 
                SET DEL_FLG = 0,
                    SET_VALUE = ?SET_VALUE?,
                    MEMO = ?MEMO?,
                    MDF_DATE = DATEADD(HH,8,GETUTCDATE()) ,
                    MDF_USER = ?MDF_USER?
                WHERE  MDF_USER = ?SET_ITEM? 
                AND SET_TYPE = ?SET_TYPE?";
            await ExecuteCommandAsync(sql, model);
        }

        public async Task UpdateItem(SetParamItemModel model)
        {
            string sql = @"
                UPDATE dbo.SET_PARAMITEM 
                SET DEL_FLG=0,
                    SET_ITEM_NAME = ?SET_ITEM_NAME?,
                    MEMO = ?MEMO?,
                    MDF_DATE = DATEADD(HH,8,GETUTCDATE()) ,
                    MDF_USER = ?MDF_USER?
                WHERE  SET_ITEM = ?SET_ITEM?";
            await ExecuteCommandAsync(sql, model);
        }

        public async Task Delete(string setItem, string setType)
        {
            string sql = @"
                UPDATE dbo.SET_PARAM 
                SET DEL_FLG = 1,
                    MDF_DATE = DATEADD(HH,8,GETUTCDATE()) ,
                    MDF_USER = ?MDF_USER?
                WHERE  SET_ITEM = ?SET_ITEM? 
                AND SET_TYPE = ?SET_TYPE?";
            await ExecuteCommandAsync(sql, new SetParamModel() { SET_ITEM = setItem, SET_TYPE = setType });
        }

        public async Task DeleteItem(string setItem)
        {
            string sql = @"
                UPDATE dbo.SET_PARAMITEM 
                SET DEL_FLG = 1,
                    MDF_DATE = DATEADD(HH,8,GETUTCDATE()) ,
                    MDF_USER = ?MDF_USER_PARAMITEM?
                    WHERE  SET_ITEM = ?SET_ITEM_PARAMITEM?;
                UPDATE dbo.SET_PARAM 
                SET DEL_FLG = 1,
                    MDF_DATE = DATEADD(HH,8,GETUTCDATE()) ,
                    MDF_USER = ?MDF_USER_PARAM?
                WHERE  SET_ITEM = ?SET_ITEM_PARAM?";
            await ExecuteCommandAsync(sql,
                new
                {
                    SET_ITEM_PARAMITEM = setItem,
                    MDF_USER_PARAMITEM = UserId,
                    SET_ITEM_PARAM = setItem,
                    MDF_USER_PARAM = UserId
                });
        }
    }
}
