using SDO.Models;

namespace SDO.Services
{
    public interface IHiPKIService
    {
        HiPKIModel CheckSignature(string sigResultJson);
    }
}