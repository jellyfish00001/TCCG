using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IVUserFunctionDac : IDac
    {
        Task<bool> CheckControllerAccess(string userId, string functionController);
    }
}