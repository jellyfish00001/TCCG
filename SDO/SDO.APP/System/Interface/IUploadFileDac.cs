using SDO.Models;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IUploadFileDac : IDac
    {
        Task<UploadFileModel> GetByFileSeqNo(int fileSeqNo);
        Task<int> Insert(UploadFileModel model);
        void Delete(string fileSeqNo);
    }
}