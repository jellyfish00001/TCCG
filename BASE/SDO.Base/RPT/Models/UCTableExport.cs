using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Models
{
    public class UCTableExport
    {
        public const string MIME_ODT = "application/vnd.oasis.opendocument.text";
        public const string MIME_DOC = "application/msword";
        public const string MIME_DOCX = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public const string MIME_PDF = "application/pdf";

        public const string MIME_ODS = "application/vnd.oasis.opendocument.spreadsheet";
        public const string MIME_XLS = "application/vnd.ms-excel";
        public const string MIME_XLSX = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public const string MIME_HTML = "text/html";
        private const int INDEX_COLUMN_FIELD = 0;
        private const int INDEX_COLUMN_TITLE = 1;
    }
}
