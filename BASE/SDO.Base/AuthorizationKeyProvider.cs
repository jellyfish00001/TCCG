using Microsoft.Extensions.Options;
using SDO.Models;
using System.IO;

namespace SDO.Utils
{
    public class AuthorizationKeyProvider : IAuthorizationKeyProvider
    {
        private readonly byte[] key;

        public AuthorizationKeyProvider(byte[] key)
        {
            this.key = key;
        }

        public byte[] AuthorizationKey => this.key;
    }
}
