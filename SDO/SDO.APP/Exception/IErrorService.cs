using SDO.Models;
namespace SDO.Services
{
    public interface IErrorService
    {
        (int statusCode, IRtnResult returnObject) ErrorHandler();
    }
}