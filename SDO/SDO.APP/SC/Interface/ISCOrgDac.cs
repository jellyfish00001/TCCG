using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace SDO.Dac
{
    public interface ISCOrgDac : IDac
    {
        /// <summary>
        /// 透過OU類別取得OU資料 
        /// </summary>
        /// <param name="OU_KIND"></param>
        /// <returns></returns>
        List<SCOrgModel> GetOUByOUKind(string OU_KIND);
    }
}
