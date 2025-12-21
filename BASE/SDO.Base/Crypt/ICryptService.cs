namespace SDO.Services
{
    public interface ICryptService
    {
        string DecryptAES256(string decrypt, string key);
        string EncryptAES256(string encrypt, string key);
    }
}