using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IAccessService
    {
        Task<bool> CheckControllerAccess(string controllerName);
        Task<bool> CheckControllerAccess(string userId, string controllerName);
    }
}