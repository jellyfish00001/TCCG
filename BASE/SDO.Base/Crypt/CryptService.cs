using SDO.CryptSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class CryptService : Service, ICryptService
    {
        private readonly IEncryptService encryptService;
        private readonly IDecryptService decryptService;

        public CryptService(IEncryptService encryptService, IDecryptService decryptService)
        {
            this.encryptService = encryptService;
            this.decryptService = decryptService;
        }

        public string EncryptAES256(string encrypt, string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
                encryptService.SetKey(key);
            return encryptService.AES256(encrypt).encryptedString;
        }

        public string DecryptAES256(string decrypt, string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
                decryptService.SetKey(key);
            return decryptService.AES256(decrypt).decryptedString;
        }
    }
}
