using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SDO.Services
{
    public class SetFunctionService : Service, ISetFunctionService
    {
        private readonly ISetFunctionDac dac;
        private readonly IUserProfile userProfile;

        public SetFunctionService(ISetFunctionDac dac,
            IUserProfile userProfile)
        {
            this.dac = dac;
            this.userProfile = userProfile;
        }

        public async Task<IList<SetFunctionModel>> ReadByGroup(SetFunctionQryModel model)
        {
            IList<SetFunctionModel> result = await dac.ReadByGroup(model.PARENT_ID);
            if (result != null && !string.IsNullOrWhiteSpace(model.FUNCTION_ID))
            {
                result = result.Where(p => p.FUNCTION_ID.ToLower().Contains(HtmlEncode(model.FUNCTION_ID.Trim().ToLower()))).ToList();
            }
            if (result != null && !string.IsNullOrWhiteSpace(model.FUNCTION_NAME))
            {
                result = result.Where(p => p.FUNCTION_NAME.Contains(HtmlEncode(model.FUNCTION_NAME.Trim()))).ToList();
            }
            return result;
        }

        public async Task<IList<SetFunctionModel>> ReadByFunctionRoot()
        {
            return await dac.ReadByFunctionRoot();
        }

        public async Task<SetFunctionModel> ReadById(string functionId)
        {
            return await dac.ReadById(functionId, true);
        }

        public virtual async Task<IList<SetFunctionModel>> ReadByUser(string userId, string apid)
        {
            return await dac.ReadByUser(userId);
        }

        public async Task<IList<SetFunctionModel>> ReadByRight(string rightId)
        {
            return await dac.ReadByRight(rightId);
        }

        public async Task<IList<SetFunctionModel>> Read()
        {
            IList<SetFunctionModel> result = await dac.Read();
            result = result.Where(p => !string.IsNullOrWhiteSpace(p.PARENT_ID)).OrderBy(p => p.PARENT_ID + p.SORT_ID).ToList();
            return result;
        }

        public async Task<RtnResultModel> Create(SetFunctionModel function)
        {
            (bool isExist, bool delFlg) = await IsExistAndDelFlg(function);
            if (isExist && !delFlg)
                return ChangeResult(ResultType.Fail | ResultType.Insert, HtmlEncode(function.FUNCTION_ID));

            function.FUNCTION_URL ??= string.Empty;
            function.PARENT_ID ??= string.Empty;
            function.DEL_FLG = false;

            await (isExist && delFlg)
                .IsTrue(async () => await dac.Update(function))
                .IsFalse(async () => await dac.Insert(function));

            return ChangeResult(ResultType.Success | ResultType.Insert);
        }

        public async Task<RtnResultModel> Update(SetFunctionModel function)
        {
            function.FUNCTION_URL ??= string.Empty;
            function.PARENT_ID ??= string.Empty;
            function.DEL_FLG = false;

            await dac.Update(function);
            return ChangeResult(ResultType.Success | ResultType.Update);
        }

        public async Task<RtnResultModel> Delete(string id)
        {
            await dac.Delete(id);
            return ChangeResult(ResultType.Success | ResultType.Delete);
        }

        public async Task<RtnResultModel> SetSortorder(string[] functionIds)
        {
            IList<SetFunctionModel> paramList = new List<SetFunctionModel>();
            int sort = 0;
            foreach (string functionId in functionIds)
            {
                paramList.Add(new SetFunctionModel()
                {
                    SORT_ID = sort++,
                    FUNCTION_ID = functionId
                });
            }

            await dac.UpdateSortorder(paramList);
            return ChangeResult(ResultType.Success | ResultType.Update);
        }

        /// <summary>
        /// 撈取功能列清單
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IList<SetFunctionListModel>> ReadFunctionList(string apid)
        {
            var userData = userProfile.GetLoginUser();

            IList<SetFunctionModel> functions = null;
            functions = await ReadByUser(userProfile.GetLoginUser().USER_ID, apid);

            IList<SetFunctionListModel> result = GetChildrenFunctions(functions, "").ToList(); //取出第一層
            Parallel.ForEach(result, (functionList) =>
            { //加入子功能
                    functionList.children = GetChildrenFunctions(functions, functionList.functionId).ToList();
                if (functionList.parentid == "" && functionList.functionUrl != "")
                    functionList.children = null;

            });
            return result;
        }

        /// <summary>
        /// 搜尋子功能
        /// </summary>
        /// <param name="functions"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        protected IEnumerable<SetFunctionListModel> GetChildrenFunctions(IEnumerable<SetFunctionModel> functions, string parentId)
        {
            return functions
                .Where(f => f.PARENT_ID == parentId)
                .Select(f => new SetFunctionListModel()
                {
                    title = f.FUNCTION_NAME,
                    functionId = f.FUNCTION_ID,
                    sortId = f.SORT_ID,
                    functionUrl= f.FUNCTION_URL
                })
                .OrderBy(f => f.sortId);
        }

        /// <summary>
        /// 確認Function是否存在及取得其DelFlg
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        protected async Task<(bool isExist, bool delFlg)> IsExistAndDelFlg(SetFunctionModel function)
        {
            SetFunctionModel result = await dac.ReadById(function.FUNCTION_ID, false);
            if (result == default(SetFunctionModel))
                return (isExist: false, delFlg: false);
            return (isExist: true, delFlg: result.DEL_FLG);
        }
    }
}
