using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Attributes;
using SDO.Dac.Models;
using SDO.Models;
using SDO.Services;

namespace SDO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "handUser")]
    public class SetParamItemController : ControllerBase
    {
        private readonly ISetParamService setParamService;

        public SetParamItemController(ISetParamService setParamService)
        {
            this.setParamService = setParamService;
        }

        /// <summary>
        /// 取得system param item
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IList<SetParamItemModel>> GetParamItems()
        {
            return await setParamService.GetSysParamItems();
        }

        /// <summary>
        /// 取得system param item
        /// </summary>
        /// <returns></returns>
        [HttpGet("{skip}/{take}/{orderByField}/{dir}")]
        [HttpGet("{skip}/{take}")]
        public async Task<GridModel<SetParamItemModel>> GetParamItems(int skip, int take, string orderByField= "set_item", string dir="asc")
        {
            return await setParamService.GetSysParamItems(skip, take,  orderByField,  dir);
        }


        /// <summary>
        /// 取得system param item
        /// </summary>
        /// <returns></returns>
        [HttpGet("{setItem}")]
        public async Task<SetParamItemModel> GetParamItem(string setItem)
        {
            return await setParamService.GetSysParamItem(setItem);
        }

        /// <summary>
        /// 新增system param item
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<RtnResultModel> InsertParamItem(SetParamItemModel model)
        {
            return await setParamService.InsertSysParamItem(model);
        }

        /// <summary>
        /// 更新system param item
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<RtnResultModel> UpdateParamItem(SetParamItemModel model)
        {
            return await setParamService.UpdateSysParamItem(model);
        }

        /// <summary>
        /// 刪除system param item
        /// </summary>
        /// <param name="setItem"></param>
        /// <returns></returns>
        [HttpDelete("{setItem}")]
        public async Task<RtnResultModel> DeleteParamItem(string setItem)
        {
            return await setParamService.DeleteSysParamItem(setItem);
        }
    }
}