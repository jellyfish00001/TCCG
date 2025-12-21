using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IProjectExtensionService
    {
        /// <summary>
        /// 取得展延紀錄清單
        /// <param name="PLAN_NO">計畫編號</param>
        /// <returns></returns>
        Task<List<ProjectExtensionModel>> GetExtensionList(string PLAN_NO);

        /// <summary>
        /// 取得展延紀錄明細
        /// <param name="EXTENSION_NO">展延編號</param>
        /// <returns></returns>
        Task<ProjectExtensionModel> GetRDExtension(string EXTENSION_NO);

        /// <summary>
        /// 儲存展延紀錄
        /// </summary>
        /// <param name="model">展延紀錄 Model</param>
        Task<string> SaveRDExtension(ProjectExtensionModel model);
    }
}
