//using SDO.CryptSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class CryptService : Service, ICryptService
    {
        //private readonly IEncryptService encryptService;
        //private readonly IDecryptService decryptService;

        public CryptService(/*IEncryptService encryptService, IDecryptService decryptService*/)
        {
            //this.encryptService = encryptService;
            //this.decryptService = decryptService;
        }

        public string EncryptAES256(string encrypt, string key)
        {
            //if (!string.IsNullOrWhiteSpace(key))
            //    encryptService.SetKey(key);
            //return encryptService.AES256(encrypt).encryptedString;
            return encrypt; // 暫時不加密，直接返回
        }

        public string DecryptAES256(string decrypt, string key)
        {
            //if (!string.IsNullOrWhiteSpace(key))
            //    decryptService.SetKey(key);
            //return decryptService.AES256(decrypt).decryptedString;
            return decrypt; // 暫時不解密，直接返回
        }
    }
}
