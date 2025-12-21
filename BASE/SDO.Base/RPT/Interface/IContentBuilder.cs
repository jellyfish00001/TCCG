using Aspose.Words;
using SDO.Models;
using SDO.ReportBuilder.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Interface
{
    public interface IContentBuilder 
    {
        /// <summary>
        /// 注入所需參數
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="parameter"></param>
        void InjectParameter(DocumentBuilder builder, RptParameter parameter);
        Task<bool> MakeContent();
        Task<bool> MakeContenByTemplate();
    }
}
