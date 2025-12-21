using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IOfficialDocService
    {
        byte[] ExportHTMLToPDF(string offDoc);

        byte[] ExportHTMLToWord(string offDoc);
    }
}
