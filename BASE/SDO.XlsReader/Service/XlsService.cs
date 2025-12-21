using Aspose.Cells;
using SDO.XlsReader.Interface;
using SDO.XlsReader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.XlsReader.Service
{
    public class XlsService : IXlsService
    {
        /// <summary>
        /// 取得資料
        /// </summary>
        /// <param name="excel">檔案</param>
        /// <param name="parameter">參數檔</param>
        /// <returns></returns>
        public RtnXlsResultModel GetExcelData(Workbook excel, XlsParameter parameter)
        {
            Worksheet sheet = GetSheet(excel, parameter);
            if (sheet == null)
            {
                return new RtnXlsResultModel { ErrMsg = "Excel格式不正確(無工作表)" };
            }

            Cells cells = sheet.Cells;

            // 取得最大表頭列
            int maxHeaderColumn = GetMaxHeaderColumn(cells, parameter);
            if (maxHeaderColumn == -1)
            {
                return new RtnXlsResultModel { ErrMsg = "Excel無法讀取" };
            }

            var result = new RtnXlsResultModel();

            // 讀取表頭列值
            List<string> headers = GetHeaders(cells, maxHeaderColumn, parameter);
            result.Headers = headers;

            // 讀取內容
            result.Contents = GetContents(cells, headers, parameter);

            if (!result.Contents.Any())
            {
                result.ErrMsg = "無資料";
            }

            return result;
        }

        /// <summary>
        /// 取得活頁
        /// </summary>
        /// <param name="excel">檔案</param>
        /// <param name="parameter">參數檔</param>
        /// <returns></returns>
        private Worksheet GetSheet(Workbook excel, XlsParameter parameter)
        {
            Worksheet sheet = null;
            if (!string.IsNullOrEmpty(parameter.WorksheetName))
            {
                sheet = excel.Worksheets[parameter.WorksheetName];
            }
            else
            {
                sheet = excel.Worksheets[parameter.WorksheetIndex];
            }
            return sheet;
        }

        /// <summary>
        /// 取得最大表頭列
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="parameter">參數檔</param>
        /// <returns></returns>
        private int GetMaxHeaderColumn(Cells cells, XlsParameter parameter)
        {
            int maxHeaderColumn = parameter.HeaderStrColumn;
            for (int i = parameter.HeaderStrColumn; i < cells.MaxDataColumn; i++)
            {
                if (cells[parameter.HeaderStrRow, i].Type == CellValueType.IsNull)
                {
                    maxHeaderColumn = i - 1;
                    break;
                }
                else
                {
                    maxHeaderColumn++;
                }
            }
            return maxHeaderColumn;
        }

        /// <summary>
        /// 讀取表頭列值
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="maxHeaderColumn">最大表頭列</param>
        /// <param name="parameter">參數檔</param>
        /// <returns></returns>
        private List<string> GetHeaders(Cells cells, int maxHeaderColumn, XlsParameter parameter)
        {
            List<string> headers = new List<string>();
            for (int i = parameter.HeaderStrColumn; i <= maxHeaderColumn; i++)
            {
                headers.Add(cells[parameter.HeaderStrRow, i].Value.ToString());
            }
            return headers;
        }

        /// <summary>
        /// 讀取內容
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="headers">表頭列值</param>
        /// <param name="parameter">參數檔</param>
        /// <returns></returns>
        private List<Dictionary<string, object>> GetContents(Cells cells, List<string> headers, XlsParameter parameter)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            for (int row = parameter.ContentStrRow; row <= cells.MaxDataRow; row++)
            {
                var cellTypes = new List<CellValueType>();
                var detailDic = new Dictionary<string, object>();
                for (int i = 0; i < headers.Count; i++)
                {
                    var cell = cells[row, parameter.HeaderStrColumn + i];
                    detailDic.Add(headers[i], cell.Value);
                    cellTypes.Add(cell.Type);
                }

                if (cellTypes.Where(x => x != CellValueType.IsNull).Any())
                {
                    result.Add(detailDic);
                }
            }
            return result;
        }
    }
}
