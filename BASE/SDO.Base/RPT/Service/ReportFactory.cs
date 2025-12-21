using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using SDO.Utils;
using SDO.Services;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Interface;
using Microsoft.AspNetCore.Hosting;
using Aspose.Pdf;
using Aspose.Pdf.Facades;
using Aspose.Pdf.Text;
using Autofac;



namespace SDO.ReportBuilder.Services
{
    public class ReportFactory : IReportFactory
    {
        private readonly IFTPService ftp;
        private readonly IComponentContext coms;
        private string bookUploadPath;

        public ReportFactory(IComponentContext coms)
        {
            this.coms = coms;
            Aspose.Cells.License licenseCells = new Aspose.Cells.License();
            Aspose.Words.License licenseWords = new Aspose.Words.License();
            License licensePdf = new License();
            licenseCells.SetLicense("License\\Aspose.Total.655.lic");
            licenseWords.SetLicense("License\\Aspose.Total.655.lic");
            licensePdf.SetLicense("License\\Aspose.Total.655.lic");
            this.ftp = coms.Resolve<IFTPService>();
        }

        /// <summary>
        /// 建立報表
        /// </summary>
        /// <param name="parameter">參數檔</param>
        /// <returns></returns>
        public async Task<RtnRptModel> CreateRPT<T>(RptParameter parameter)
        {
            IRPTBuilder obj = GetObj<T>(parameter);

            //取得類別
            var reportobj = await obj.Create(parameter);
            using (reportobj.ms)
            {
                return new RtnRptModel
                {
                    Bytes = reportobj.ms.ToArray(),
                    OutputName = reportobj.outputName,
                    Mime = reportobj.mime
                };
            }
        }

        /// <summary>
        /// 建立報表並存檔
        /// </summary>
        /// <param name="parameter">參數檔</param>
        /// <returns></returns>
        public async Task CreateRPTBySave<T>(RptParameter parameter)
        {
            IRPTBuilder obj = GetObj<T>(parameter);

            // 取得類別
            var reportobj = await obj.Create(parameter);
            if (!string.IsNullOrEmpty(parameter.SavePath))
            {
                using (reportobj.ms)
                {
                    if (!Directory.Exists(parameter.SavePath))
                    {
                        Directory.CreateDirectory(parameter.SavePath);
                    }

                    string path = Path.Combine(parameter.SavePath, reportobj.outputName);
                    using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write))
                    {
                        reportobj.ms.WriteTo(file);
                    }
                }
            }
        }

        private IRPTBuilder GetObj<T>(RptParameter parameter)
        {
            var theAssembly = Assembly.GetAssembly(typeof(T));
            Type theType = theAssembly.GetType($"SDO.APP.RPT.{parameter.ReportType}.{parameter.ReportId}", false, true);
            IRPTBuilder obj = (IRPTBuilder)Activator.CreateInstance(theType,
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new object[] { coms },
                System.Globalization.CultureInfo.InvariantCulture,
                null);
            return obj;
        }

        /// <summary>
        /// 製作合併檔
        /// </summary>
        /// <param name="paramter"></param>
        /// <returns></returns>
        public virtual async Task<RtnRptModel> MergePDF(RptParameter paramter)
        {
            ftp.AutoDisconnect = false;
            var msList = await GetReports(paramter);
            if (!msList.Any())
            {
                return new RtnRptModel();
            }

            using (var ms = new MemoryStream())
            {
                //合併pdf
                Merge(msList, ms);
                //清除memory
                foreach (var stream in msList)
                {
                    stream.Dispose();
                }
                ftp.Upload(bookUploadPath, ms.ToArray());
                ftp.Disconnect();
                ftp.AutoDisconnect = true;
                return new RtnRptModel
                {
                    Bytes = ms.ToArray(),
                    OutputName = paramter.FileName,
                    Mime = UCTableExport.MIME_PDF
                };
            }
        }

        /// <summary>
        /// 準備需合併的pdf
        /// </summary>
        /// <param name="paramter"></param>
        /// <returns></returns>
        protected virtual Task<IList<MemoryStream>> GetReports(RptParameter paramter)
        {
            return null;
        }

        /// <summary>
        /// 重設頁碼並上傳至ftp
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="pageNobegin"></param>
        /// <returns></returns>
        protected virtual (int count, int startNo, MemoryStream ms) ResetPDFPage(MemoryStream ms, int pageNobegin)
        {
            int startNumber = pageNobegin + 1;
            Document pdfDocument = new Document(ms);
            // ms.Dispose();
            // totalpage = pageNobegin + pdfDocument.Pages.Count;
            // Create page number stamp
            PageNumberStamp pageNumberStamp = new PageNumberStamp();

            // Whether the stamp is background
            pageNumberStamp.Background = false;
            pageNumberStamp.Format = "#";
            pageNumberStamp.BottomMargin = 10;
            pageNumberStamp.HorizontalAlignment = HorizontalAlignment.Center;
            pageNumberStamp.StartingNumber = startNumber;
            // Set text properties
            pageNumberStamp.TextState.Font = FontRepository.FindFont("Calibri");
            pageNumberStamp.TextState.FontSize = 12;
            pageNumberStamp.TextState.FontStyle = FontStyles.Regular;
            pageNumberStamp.TextState.FontStyle = FontStyles.Regular;
            //pageNumberStamp.TextState. = "Calibri";
            // pageNumberStamp.TextState.ForegroundColor = Color.Aqua;
            MemoryStream Pms = new MemoryStream();
            foreach (var page in pdfDocument.Pages)
            {
                if (page != null)
                {
                    page.AddStamp(pageNumberStamp);
                    pageNobegin++;
                }
            }
            pdfDocument.Save(Pms);
            ms.Dispose();
            return (pageNobegin, startNumber, Pms);
        }

        /// <summary>
        /// 合併PDF
        /// </summary>
        /// <param name="pdfsIn">需合併之pdf </param>
        /// <param name="ms"></param>
        protected void Merge(IList<MemoryStream> pdfsIn, MemoryStream ms)
        {
            PdfFileEditor pdfEditor = new PdfFileEditor();
            pdfEditor.Concatenate(pdfsIn.ToArray(), ms);
        }
    }
}
