using Aspose.Words;
using Aspose.Words.Tables;
using System.Collections.Generic;
using System.IO;
using SDO.Models;

namespace SDO.Services
{
    public interface IWordSetService
    {
        /// <summary>
        /// 已建立之aspose word 物件
        /// </summary>
        Document doc { get; set; }
        /// <summary>
        /// 格式化欄位
        /// </summary>
        List<string> FormatColumns { get; set; }
        /// <summary>
        /// 輸出MimeType
        /// </summary>
        string mimeType { get; }
        Table FindTable(CompositeNode node, string tableName, bool isDeep = true);
        void GenLoopTableByList(CompositeNode node, ITableData tableData);
        void GenLoopTableByList(Table table, IEnumerable<object> datas, int cloneRowIndex = 1);
        MemoryStream GenWord(object basicData, IEnumerable<object> listData, string tempFilePath, int startRow, SaveFormat format, int tableindex = 0);
        void GenWord(Stream outStream, object basicData, IEnumerable<object> listData, string tempFilePath, int startRow, SaveFormat format, int tableindex = 0);
        void GenWord(Stream outStream, object basicData, IEnumerable<ITableData> tableDatas, string tempFilePath, SaveFormat format);
        void GenWord(object basicData, IEnumerable<ITableData> tableDatas);
        void GenWord(object basicData, IEnumerable<object> listData,string tableName);
        void ReplaceByDictionary(Node node, IEnumerable<KeyValuePair<string, object>> keyValuePairs);
        void ReplaceByObject(Node node, object data);
        void CleanTag(Node node);
        void ReplaceText(Node node, string key, string value);
        MemoryStream Save(string format);
        /// <summary>
        /// 匯入Doc
        /// </summary>
        /// <param name="From">來源</param>
        /// <param name="To">匯入至</param>
        /// <param name="ResetPageNum">是否重設頁碼</param>
        void ImportDoc(Document From, Document To, bool ResetPageNum = true);
    }
}