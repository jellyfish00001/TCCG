using Aspose.Cells;
using Microsoft.AspNetCore.Hosting;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SDO.AsposeSet.Models;
using SDO.AsposeSet.Service;
using SDO.Utils;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Interface;

namespace SDO.Services
{
    public class AsposeExcelSetService : Service, IAsposeExcelSetService
    {
        private readonly ISysParam sysParam;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IUserProfile userProfile;
        private readonly IReportFactory rpt;

        public AsposeExcelSetService(ISysParam sysParam, IWebHostEnvironment webHostEnvironment, IUserProfile userProfile, IReportFactory rpt)
        {
            this.sysParam = sysParam;
            this.webHostEnvironment = webHostEnvironment;
            this.userProfile = userProfile;
            this.rpt = rpt;
        }

        /// <summary>
        /// 取得範本檔案
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> GetSampleFile()
        {
            string path = Path.Combine((await sysParam.GetSysParam("SystemConfig", "ReportFolder")).SET_VALUE.Replace("~", webHostEnvironment.ContentRootPath), "SampleReport.xls");
            return await File.ReadAllBytesAsync(path);
        }

        #region Excel套版 單一欄位套版

        public async Task<byte[]> ReportSingle(string saveFormat)
        {
            #region 取得範本檔
            string path = Path.Combine((await sysParam.GetSysParam("SystemConfig", "ReportFolder")).SET_VALUE.Replace("~", webHostEnvironment.ContentRootPath), "SampleReport.xls");
            Workbook workbook = GetReportTmpWorkbook(path);
            Worksheet baseSheet = workbook.Worksheets["單一欄位替換"];

            #endregion 取得範本檔

            #region 取得資料

            //(此為範例程式，正式開發會近DB讀取，各隻客制報表請放在Report資料夾下依程式代號命名(EX: RPT001，RPT001Dac)
            AsposeExcelSetModel result = new AsposeExcelSetModel()
            {
                NO = 1,
                ProjectName = "競賽專案",
                F_COUNT = 10,
                M_COUNT = 31
            };

            #endregion 取得資料

            //複製和定義工作表名稱
            Worksheet sheet = workbook.Worksheets.Add("測試範本");
            sheet.Copy(baseSheet);

            #region 表頭標籤替換

            SetFieldDataByModel(sheet, userProfile.GetLoginUser());

            #endregion 表頭標籤替換

            #region 動態長資料

            SetFieldDataByModel(sheet, result);

            #endregion 動態長資料

            workbook.Worksheets.RemoveAt(baseSheet.Index);
            //刪除其他範本檔
            workbook.Worksheets.RemoveAt("縱向動態長資料範例(自動CreateRow)");
            workbook.Worksheets.RemoveAt("縱向動態長資料範例(預設Row範圍)");

            byte[] bytes = ConvertTypeExcelToByte(workbook, SetFormatType(saveFormat));

            return bytes;
        }

        #endregion

        #region Excel套版 縱向動態生成
        /// <summary>
        /// 縱向動態長資料範例(自動CreateRow)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<byte[]> ReportAuto(string saveFormat)
        {
            //(此為範例程式，建議客制報表請放在Report資料夾下依程式代號命名(EX: RPT001，RPT001Model)

            #region 取得範本檔

            // 取得範本檔
            string path = Path.Combine((await sysParam.GetSysParam("SystemConfig", "ReportFolder")).SET_VALUE.Replace("~", webHostEnvironment.ContentRootPath), "SampleReport.xls");
            Workbook workbook = GetReportTmpWorkbook(path);

            Worksheet baseSheet = workbook.Worksheets["縱向動態長資料範例(自動CreateRow)"];

            #endregion 取得範本檔

            #region 取得資料

            //(此為範例程式，各隻客制報表請放在Report資料夾下依程式代號命名(EX: RPT001，RPT001Dac)
            IList<SetParamItemModel> readItem = await sysParam.GetSysParamItems();

            List<AsposeExcelSetModel> result2 = new List<AsposeExcelSetModel>();
            result2.Add(new AsposeExcelSetModel()
            {
                NO = 1,
                ProjectName = "競賽專案",
                F_COUNT = 10,
                M_COUNT = 31
            });
            result2.Add(new AsposeExcelSetModel()
            {
                NO = 2,
                ProjectName = "We are ...",
                F_COUNT = 99,
                M_COUNT = 49
            });
            #endregion 取得資料

            //複製和定義工作表名稱
            Worksheet sheet = workbook.Worksheets.Add("測試範本");
            sheet.Copy(baseSheet);

            #region 表頭標籤替換

            SetFieldDataByModel(sheet, userProfile.GetLoginUser());

            #endregion 表頭標籤替換

            #region 動態長資料

            if (readItem.Count > 0)
            {
                //新增資料
                InsertLoopDataForExcel(sheet, readItem, "1");
                InsertLoopDataForExcel(sheet, result2, "2");
            }

            #endregion 動態長資料

            //刪除範本sheet
            workbook.Worksheets.RemoveAt(baseSheet.Index);
            //刪除其他範本檔
            workbook.Worksheets.RemoveAt("單一欄位替換");
            workbook.Worksheets.RemoveAt("縱向動態長資料範例(預設Row範圍)");

            byte[] bytes = ConvertTypeExcelToByte(workbook, SetFormatType(saveFormat));
            return bytes;
        }

        /// <summary>
        /// 縱向動態長資料範例(預設Row範圍)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<byte[]> ReportDefault(string saveFormat)
        {
            #region 取得範本檔
            string path = Path.Combine((await sysParam.GetSysParam("SystemConfig", "ReportFolder")).SET_VALUE.Replace("~", webHostEnvironment.ContentRootPath), "SampleReport.xls");
            Workbook workbook = GetReportTmpWorkbook(path);
            Worksheet baseSheet = workbook.Worksheets["縱向動態長資料範例(預設Row範圍)"];

            #endregion 取得範本檔

            #region 取得資料
            //(此為範例程式，正式開發會近DB讀取，各隻客制報表請放在Report資料夾下依程式代號命名(EX: RPT001，RPT001Dac)

            IList<AsposeExcelSetModel> result = new List<AsposeExcelSetModel>();
            result.Add(new AsposeExcelSetModel()
            {
                NO = 1,
                ProjectName = "競賽專案",
                F_COUNT = 10,
                M_COUNT = 31
            });
            result.Add(new AsposeExcelSetModel()
            {
                NO = 2,
                ProjectName = "We are ...",
                F_COUNT = 99,
                M_COUNT = 49
            });
            result.Add(new AsposeExcelSetModel()
            {
                NO = 3,
                ProjectName = "Hello",
                F_COUNT = 1,
                M_COUNT = 43
            });

            List<AsposeExcelSetModel> resultL2 = new List<AsposeExcelSetModel>();
            resultL2.Add(new AsposeExcelSetModel()
            {
                NO = 1,
                ProjectName = "玉華阿姨托嬰申請",
                M_COUNT = 14
            });
            resultL2.Add(new AsposeExcelSetModel()
            {
                NO = 2,
                ProjectName = "喬姐姐托嬰中心申請",
                M_COUNT = 9
            });

            #endregion 取得資料

            //複製和定義工作表名稱
            Worksheet sheet = workbook.Worksheets.Add("測試範本");
            sheet.Copy(baseSheet);

            #region 表頭標籤替換
            SetFieldDataByModel(sheet, userProfile.GetLoginUser());

            #endregion 表頭標籤替換

            #region 動態長資料

            if (result.Count > 0)
            {
                //新增資料
                SetLoopDataForExcel(sheet, result, 2, "1");
                SetLoopDataForExcel(sheet, resultL2, 2, "2");
            }

            #endregion 動態長資料

            //刪除範本sheet
            workbook.Worksheets.RemoveAt(baseSheet.Index);
            //刪除其他範本檔
            workbook.Worksheets.RemoveAt("單一欄位替換");
            workbook.Worksheets.RemoveAt("縱向動態長資料範例(自動CreateRow)");

            byte[] bytes = ConvertTypeExcelToByte(workbook, SetFormatType(saveFormat));
            return bytes;
        }

        /// <summary>
        /// 匯出文件類型
        /// </summary>
        /// <param name="Format">匯出型態</param>
        /// <returns>MIME類型</returns>
        public string SetContentType(string Format)
        {
            switch (Format.ToLower())
            {
                case "pdf":
                    return "application/pdf";
                case "doc":
                    return "application/msword";
                case "docx":
                    return "application/vnd.openxmlformats - officedocument.wordprocessingml.document";
                case "odt":
                    return "application/vnd.oasis.opendocument.text";
                case "xls":
                    return "application/vnd.ms-excel";
                case "xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case "ods":
                    return "application/vnd.oasis.opendocument.spreadsheet";
                case "html":
                    return "text/html";
                default:
                    return "application/pdf";
            }
        }

        /// <summary>
        /// workbook轉byte[]
        /// </summary>
        /// <param name="workbook">匯出的excel檔案(固定xls格式)</param>
        /// <param name="toFormat">匯出型態</param>
        /// <returns>回傳轉換好的byte[]</returns>
        private byte[] ConvertTypeExcelToByte(Workbook workbook, ExcelModel.FileFormat toFormat)
        {
            using (MemoryStream msOutput = new MemoryStream())
            {
                workbook.Save(msOutput, (SaveFormat)toFormat);
                return msOutput.ToArray();
            }
        }

        /// <summary>
        /// 匯出文件類型
        /// </summary>
        /// <param name="Format">匯出型態</param>
        /// <returns>ExcelModel.FileFormat型態</returns>
        private ExcelModel.FileFormat SetFormatType(string Format)
        {
            switch (Format.ToLower())
            {
                case "pdf":
                    return ExcelModel.FileFormat.pdf;
                case "xls":
                    return ExcelModel.FileFormat.xls;
                case "xlsx":
                    return ExcelModel.FileFormat.xlsx;
                case "ods":
                    return ExcelModel.FileFormat.ods;
                case "html":
                    return ExcelModel.FileFormat.html;
                default:
                    return ExcelModel.FileFormat.pdf;
            }
        }

        #endregion

        #region 取得範本檔
        /// <summary>
        /// 取得範本檔
        /// </summary>
        /// <param name="tmpFilePath">範本檔案路徑</param>
        /// <returns></returns>
        private Workbook GetReportTmpWorkbook(string tmpFilePath)
        {
            if (File.Exists(tmpFilePath))
            {
                Workbook workbook = new Workbook(tmpFilePath);
                return workbook;
            }
            return null;
        }
        #endregion

        #region SetFieldData
        private void SetFieldDataByModel<T>(Worksheet worksheet, T model)
        {
            //將字串轉換為data的property，取得資料
            Type dataType = model.GetType();
            IList<PropertyInfo> property = dataType.GetProperties().ToList();
            foreach (PropertyInfo prop in property)
            {
                SetFieldData(worksheet, prop.Name, (prop.GetValue(model) == null) ? "" : prop.GetValue(model).ToString());
            }
        }

        /// <summary>
        /// 單一sheet取代固定欄位標籤(Excel)
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="ColumnName"></param>
        /// <param name="ColumnValue"></param>
        /// <history>
        /// 2015/08/24  Fiona Lin    Create
        /// </history>
        private void SetFieldData(Worksheet worksheet, string ColumnName, string ColumnValue)
        {
            SetFieldData(worksheet, ColumnName, ColumnValue, worksheet.Cells.MinDataRow, worksheet.Cells.MaxDataRow);
        }

        /// <summary>
        /// 單一sheet取代固定欄位標籤(Excel)
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        /// <param name="startIndex">報表取代區塊起始行</param>
        /// <param name="endIndex">報表取代區塊結束行</param>
        /// <param name="style">style</param>
        /// <history>
        /// 2015/08/24  Fiona Lin    Create
        /// </history>
        private void SetFieldData(Worksheet worksheet, string columnName, string columnValue, int startIndex, int endIndex, Style style = null)
        {
            //檢查頁首頁尾
            //尋找的字串
            string SearchTag = String.Format("#FLD_{0}#", columnName);
            PageSetup pageSetup = worksheet.PageSetup;
            //section 0: left,
            //        1: center,
            //        2: right
            for (int i = 0; i < 3; i++)
            {
                if (pageSetup.GetHeader(i) != null)
                    pageSetup.SetHeader(i, pageSetup.GetHeader(i).Replace(SearchTag, columnValue));
                if (pageSetup.GetFooter(i) != null)
                    pageSetup.SetFooter(i, pageSetup.GetFooter(i).Replace(SearchTag, columnValue));
            }
            //頁籤中所有的列都要檢查
            for (int rowNumber = startIndex; rowNumber <= endIndex; rowNumber++)
            {
                Row row = worksheet.Cells.GetRow(rowNumber);
                if (row != null)
                {
                    //所有的行都要檢查
                    for (int cellNumber = row.FirstCell.Column; cellNumber <= row.LastCell.Column; cellNumber++)
                    {
                        Cell cell = row.GetCellOrNull(cellNumber);

                        if (cell != null && cell.Type == CellValueType.IsString)
                        {
                            //包含[searchTag]:N 
                            if (cell.StringValue.Contains(SearchTag + ":N"))
                            {
                                if (IsNumeric(columnValue) && (!String.IsNullOrEmpty(columnValue)))
                                {
                                    //如果是數值且不是空值
                                    cell.PutValue(Convert.ToDouble(cell.StringValue.Replace(SearchTag + ":N", GetReplaceValue(columnValue, "\n"))));
                                    //將數值轉回Numeric
                                    cell.GetStyle().Number = 0;
                                }
                                else
                                {
                                    cell.PutValue(cell.StringValue.Replace(SearchTag + ":N", GetReplaceValue(columnValue, "\n")));
                                }
                            }
                            //包含[searchTag] 欄位替換且type=String
                            else if (cell.StringValue.Contains(SearchTag))
                            {
                                cell.PutValue(cell.StringValue.Replace(SearchTag, GetReplaceValue(columnValue, "\n")));
                                if (style != null)
                                {
                                    cell.SetStyle(style);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 判斷是否為數值字串
        /// </summary>
        /// <param name="strNumber">要判斷的字串</param>
        /// <returns></returns>
        /// <history>
        /// 2015/08/21  Fiona Lin    Modify strNumber=null時防呆
        /// </history>
        private bool IsNumeric(string strNumber)
        {
            Regex NumberPattern = new Regex("[^0-9.-]");
            string testValue = string.Empty;
            if (!string.IsNullOrEmpty(strNumber))
            {
                testValue = strNumber;
            }
            return !NumberPattern.IsMatch(testValue);
        }

        /// <summary>
        /// 取得代換字串
        /// </summary>
        /// <param name="colomnValue"></param>
        /// <param name="replaceTag"></param>
        /// <returns></returns>
        private string GetReplaceValue(string colomnValue, string replaceTag)
        {
            string replaceValue = null;

            if (!string.IsNullOrEmpty(colomnValue))
            {
                replaceValue = colomnValue.Replace(replaceTag, Environment.NewLine);
            }

            return replaceValue;
        }
        #endregion

        #region 縱向動態長資料
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="worksheet"></param>
        /// <param name="reportData"></param>
        private void InsertLoopDataForExcel<T>(Worksheet worksheet, IList<T> reportData)
        {
            InsertLoopDataForExcel(worksheet, reportData, null);
        }

        /// <summary>
        /// 新增每筆資料(範本檔只需設定一列TAG)
        /// </summary>
        /// <param name="worksheet">Sheet</param>
        /// <param name="reportData">資料來源</param>
        private void InsertLoopDataForExcel<T>(Worksheet worksheet, IList<T> reportData, string listName)
        {
            string listStartName = string.Format("#L{0}_", listName ?? "0");
            //記錄開始取代迴圈位置，若無，則回傳-1
            int startLoopRow = GetStartLoopRow(worksheet);
            //設定取參數值變數
            int templateValueIndexOf = 0;
            int templateValueLastIndexOf = 0;
            string parameterTag = "";
            //存放儲存格資料參數
            string reportValue = "";
            //將字串轉換為data的property，藉此抓取資料
            PropertyInfo propertyInfo;
            //有需取代文字，才作取代動作
            if (startLoopRow != -1)
            {
                //記錄迴圈的列
                Row loopRow = worksheet.Cells.GetRow(startLoopRow);
                int currentLoopRow = startLoopRow;
                //紀錄範本值
                string[] templateValues = new string[loopRow.LastCell.Column + 1];
                int firstCellNum = loopRow.FirstCell.Column;
                //取得範本檔參數行資料
                for (int replaceCell = firstCellNum; replaceCell <= loopRow.LastCell.Column; replaceCell++)
                {
                    //取得原有參數行的樣式和值
                    Cell cell = loopRow.GetCellOrNull(replaceCell);
                    if (cell != null)
                    {
                        templateValues[replaceCell - firstCellNum] = cell.ToString();
                    }
                }

                //開始新增資料
                for (int replaceRow = 0; replaceRow < reportData.Count() || !reportData.Any(); replaceRow++)
                {
                    //複製Row，原本使用createRow會覆蓋到範本檔其他Row資料
                    if (replaceRow != 0)
                    {
                        worksheet.Cells.InsertRow(currentLoopRow);
                        worksheet.Cells.CopyRow(worksheet.Cells, startLoopRow, currentLoopRow);
                    }
                    Row loopRowNew = worksheet.Cells.GetRow(currentLoopRow);

                    currentLoopRow++;
                    for (int index = firstCellNum; index <= loopRowNew.LastCell.Column; index++)
                    {
                        int templateIndex = index - firstCellNum;

                        //取得儲存格
                        Cell createCell = loopRowNew.GetCellOrNull(index);

                        //將參數值取得兩個#符號中間的欄位名稱 templateValueIndexOf：從前面數來第一個#L{0}_的位置  templateValueLastIndexOf：從後面數來第一個#的位置
                        templateValueIndexOf = templateValues[templateIndex].IndexOf(listStartName);
                        templateValueLastIndexOf = templateValues[templateIndex].LastIndexOf("#");
                        if (templateValueIndexOf > -1 && templateValueLastIndexOf > -1)
                        {
                            parameterTag = templateValues[templateIndex].Substring(templateValueIndexOf + listStartName.Length, templateValueLastIndexOf - templateValueIndexOf - listStartName.Length);
                        }
                        else
                        {
                            parameterTag = "";
                        }

                        if (!reportData.Any())
                        {
                            createCell.PutValue("");
                            continue;
                        }
                        //將字串轉換為data的property，取得資料
                        propertyInfo = reportData[replaceRow].GetType().GetProperty(parameterTag);
                        if (propertyInfo != null && propertyInfo.GetValue(reportData[replaceRow]) != null)
                        {
                            //將字串轉換為data的property，藉此抓取資料
                            reportValue = propertyInfo.GetValue(reportData[replaceRow]).ToString().Replace('"', '＂');

                            #region 範本有特殊標籤另外處理  :N 將資料轉成Double 都沒有的以字串直接顯示

                            //判斷是否有數字標籤
                            if (templateValues[templateIndex].Contains(":N"))
                            {
                                decimal number;
                                if (decimal.TryParse(reportValue, out number))
                                {
                                    createCell.GetStyle().Number = 0;
                                    createCell.PutValue(Convert.ToDouble(reportValue));
                                }
                                else
                                {
                                    createCell.PutValue(reportValue);
                                }
                            }
                            else if (templateValues[templateIndex].Contains(":S"))
                            {
                                decimal number;
                                if (!reportValue.Contains(',') && decimal.TryParse(reportValue, out number))
                                {
                                    createCell.PutValue(Convert.ToDouble(reportValue).ToString("0.##"));
                                }
                                else
                                {
                                    createCell.PutValue(reportValue);
                                }
                            }
                            else
                            {
                                createCell.PutValue(reportValue);
                                createCell.GetStyle().IsTextWrapped = true;
                            }

                            #endregion 範本有特殊標籤另外處理  :N 將資料轉成Double 都沒有的以字串直接顯示
                        }
                    }

                    if (!reportData.Any())
                        break;
                }
            }
        }

        private void SetLoopDataForExcel<T>(Worksheet worksheet, IList<T> reportData, int limitRowCount)
        {
            SetLoopDataForExcel(worksheet, reportData, limitRowCount, null);
        }

        /// <summary>
        /// 縱向塞每筆資料(適用於原本範本檔就有規劃好的區塊)
        /// </summary>
        /// <param name="worksheet">Sheet</param>
        /// <param name="reportData">資料來源</param>
        /// <param name="limitRowCount">最大筆數</param>
        /// <param name="listName">動態資料列名稱</param>
        private void SetLoopDataForExcel<T>(Worksheet worksheet, IList<T> reportData, int limitRowCount, string listName)
        {
            string listStartName = string.Format("#L{0}_", listName ?? "0");
            //記錄開始取代迴圈位置，若無，則回傳-1
            int startLoopRow = GetStartLoopRow(worksheet);
            //設定取參數值變數
            int templateValueIndexOf = 0;
            int templateValueLastIndexOf = 0;
            string parameterTag = "";
            //存放儲存格資料參數
            string reportValue = "";
            //將字串轉換為data的property，藉此抓取資料
            PropertyInfo propertyInfo;
            //有需取代文字，才作取代動作
            if (startLoopRow != -1)
            {
                //記錄迴圈的列
                Row loopRow = worksheet.Cells.GetRow(startLoopRow);
                int firstCellNum = loopRow.FirstCell.Column;
                //紀錄範本值
                string[] templateValues = new string[loopRow.LastCell.Column + 1];
                //取得範本檔參數行資料
                for (int replaceCell = firstCellNum; replaceCell <= loopRow.LastCell.Column; replaceCell++)
                {
                    Cell cell = loopRow.GetCellOrNull(replaceCell);
                    if (cell != null)
                    {
                        templateValues[replaceCell - firstCellNum] = cell.ToString();
                    }
                }

                //取得最大筆數，避免範本檔設定3列，結果資料有4筆，造成破版
                int maxRow = (limitRowCount > reportData.Count()) ? reportData.Count() : limitRowCount;
                //開始新增資料
                for (int replaceRow = 0; replaceRow < maxRow; replaceRow++)
                {
                    //取得行
                    Row loopRowNew = worksheet.Cells.GetRow(startLoopRow);
                    startLoopRow++;
                    for (int index = firstCellNum; index <= loopRowNew.LastCell.Column; index++)
                    {
                        int templateIndex = index - firstCellNum;

                        //取得儲存格值
                        Cell createCell = loopRowNew.GetCellOrNull(index);

                        //將參數值取得兩個#符號中間的欄位名稱 templateValueIndexOf：從前面數來第一個#L{0}_的位置  templateValueLastIndexOf：從後面數來第一個#的位置
                        templateValueIndexOf = templateValues[templateIndex].IndexOf(listStartName);
                        templateValueLastIndexOf = templateValues[templateIndex].LastIndexOf("#");
                        if (templateValueIndexOf > -1 && templateValueLastIndexOf > -1)
                        {
                            parameterTag = templateValues[templateIndex].Substring(templateValueIndexOf + listStartName.Length, templateValueLastIndexOf - templateValueIndexOf - listStartName.Length);
                        }
                        else
                        {
                            parameterTag = "";
                        }

                        //將字串轉換為data的property，取得資料
                        propertyInfo = reportData[replaceRow].GetType().GetProperty(parameterTag);
                        if (propertyInfo != null && propertyInfo.GetValue(reportData[replaceRow]) != null)
                        {
                            //將字串轉換為data的property，藉此抓取資料
                            reportValue = propertyInfo.GetValue(reportData[replaceRow]).ToString().Replace('"', '＂');

                            #region 範本有特殊標籤另外處理  :N 將資料轉成Double 都沒有的以字串直接顯示

                            //判斷是否有數字標籤
                            if (templateValues[templateIndex].Contains(":N"))
                            {
                                decimal number;
                                if (decimal.TryParse(reportValue, out number))
                                {
                                    createCell.PutValue(Convert.ToDouble(reportValue));
                                    createCell.GetStyle().Number = 0;
                                }
                                else
                                {
                                    createCell.PutValue(reportValue);
                                }
                            }
                            else if (templateValues[templateIndex].Contains(":S"))
                            {
                                decimal number;
                                if (!reportValue.Contains(',') && decimal.TryParse(reportValue, out number))
                                {
                                    createCell.PutValue(Convert.ToDouble(reportValue).ToString("0.##"));
                                }
                                else
                                {
                                    createCell.PutValue(reportValue);
                                }
                            }
                            else
                            {
                                createCell.PutValue(reportValue);
                                createCell.GetStyle().IsTextWrapped = true;
                            }

                            #endregion 範本有特殊標籤另外處理  :N 將資料轉成Double 都沒有的以字串直接顯示
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 取得參數列
        /// </summary>
        /// <param name="worksheet">範本工作表</param>
        /// <returns>範本參數起始列</returns>
        private int GetStartLoopRow(Worksheet worksheet)
        {
            int loopRow = -1;
            //跑所有的列
            for (int rowNumber = worksheet.Cells.MinDataRow; rowNumber <= worksheet.Cells.MaxDataRow; rowNumber++)
            {
                Row row = worksheet.Cells.GetRow(rowNumber);
                if (row != null && loopRow == -1)
                {
                    //跑所有的行
                    for (int cellNumber = row.FirstDataCell.Column; cellNumber <= row.LastDataCell.Column; cellNumber++)
                    {
                        //找到任一格有#符號的 即跳出
                        Cell cell = row.GetCellOrNull(cellNumber);
                        if (cell != null && cell.Type == CellValueType.IsString && cell.StringValue.Contains('#'))
                        {
                            loopRow = rowNumber;
                            break;
                        }
                    }
                    if (loopRow != -1)
                    {
                        break;
                    }
                }
            }
            return loopRow;
        }
        #endregion

        public async Task<RtnRptModel> ReportAutoRPT(string saveFormat)
        {
            DemoParameter parameter = new DemoParameter
            {
                FileName = "demo",
                ReportId = "DemoXLSReport",
                Extension = "saveFormat",
                USER_ID = userProfile.GetLoginUser().USER_ID,
                USER_NAME = userProfile.GetLoginUser().USER_NAME
            };
            return await rpt.CreateRPT<AsposeExcelSetService>(parameter);
        }
    }
}
