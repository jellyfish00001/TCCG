using Aspose.Cells;
using SDO.XlsReader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.XlsReader.Interface
{
    public interface IXlsService
    {
        /// <summary>
        /// 取得資料
        /// </summary>
        /// <param name="excel">檔案</param>
        /// <param name="parameter">參數檔</param>
        /// <returns></returns>
        public RtnXlsResultModel GetExcelData(Workbook excel, XlsParameter parameter);
    }
}
