using System;
using System.Drawing;
using System.Xml;

namespace SDO.Models
{
    /// <summary>
    /// SE轉Word的設定參數
    /// </summary>
    public class SEAttribute
    {
        /// <summary>
        /// BarCode輸出設定檔(文字)
        /// </summary>
        public string BarCodeSettingXml { get; set; }
        /// <summary>
        /// BarCode物件(XmlDocument) add by simon 20170110
        /// </summary>
        public XmlDocument BarCodeXml { get; set; }
        /// <summary>
        /// SE檔內容
        /// </summary>
        public string SEContent { get; set; }
        /// <summary>
        /// 騎縫章圖示
        /// </summary>
        public Image SealImage { get; set; }
        /// <summary>
        /// 裝訂線圖示
        /// </summary>
        public SkiaSharp.SKBitmap GutterImage { get; set; }
        /// <summary>
        /// 浮水印圖示 (20170509新增)
        /// </summary>
        public SkiaSharp.SKBitmap WatermarkImage { get; set; }
        /// <summary>
        /// 輸出檔案的存放路徑
        /// </summary>
        public string SavePath { get; set; }
        /// <summary>
        /// 輸出檔案的檔名 (不含副檔名)
        /// </summary>
        public string SaveFileName { get; set; }
        /// <summary>
        /// [20161206 測試版本]
        /// 簽稿會核單內容陣列 (每頁獨立)
        /// </summary>
        [Obsolete("希望改用WholeApprovalContent，不用再自己切成陣列，改傳單一string")]
        public string[] ApprovalContent { get; set; }
        /// <summary>
        /// 簽稿會核單內容，createApproval的結果直接傳入即可
        /// " <div>page1</div> <div>page2</div> "
        /// </summary>
        public string WholeApprovalContent { get; set; }
        /// <summary>
        /// 職章的CSS樣式，如果不傳則用預設
        /// <div style="margin-top: 10px;" data-speed-id="sealContainer0" data-speed="">
        ///   <div style = "margin-top: 10px;" data-speed-id="seal-box">
        ///     <table data-speed-id="seal-table">
        ///       <tbody>
        ///         <tr>
        ///           <td style = "font-size: 10px;" data-speed-sealouid="10120">資訊處</td>
        ///           <td style = "font-size: 16px;" rowspan="2">系統維護人員</tr>
        ///         <tr>
        ///         <td style = "font-size: 10px;" > 立法委員 </ td >
        ///         </ tr >
        ///       </ tbody >
        ///     </ table >
        ///     < div style="font-size: 12px;">106/05/12 09:56</div>
        ///   </div>
        /// </div>
        /// </summary>
        public string StampCSS { get; set; }

        /// <summary>
        /// 正副本章的CSS (不輸入則不印正副本章)
        /// </summary>
        public string OCSealCSS { get; set; }
        /// <summary>
        /// OutKind，用來判斷要產生正本還是副本章
        /// 1,3,4 正本
        /// 2,5 副本
        /// 7 抄本
        /// </summary>
        public int OutKind { get; set; }

        /// <summary>
        /// 文化部職章+決行意見的SE
        /// </summary>
        public string StampSE { get; set; }
        /// <summary>
        /// ExportPdfWithStamp / ExportPdfWithApproval 如果是紙本就需要傳入TIFF本文檔的內容
        /// </summary>
        public byte[] TIFFContent { get; set; }
        /// <summary>
        /// ExportPdfWithApproval 如果已有PDF本文檔的話，可直接傳入內容
        /// </summary>
        public byte[] PdfContent { get; set; }
        /// <summary>
        /// 機關地址是否需要縮成一行
        /// </summary>
        public bool ShrinkAddress { get; set; }

    }
}