using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Web;

namespace SDO.Services
{
    public class HiPKIService : Service, IHiPKIService
    {

        public HiPKIModel CheckSignature(string sigResultJson)
        {
            string signature = JsonDeserialize<string>(sigResultJson);

            SignedCms signedCms = new SignedCms();
            signedCms.Decode(Convert.FromBase64String(signature));

            signedCms.CheckSignature(true);
            if (signedCms.SignerInfos.Count > 0)
            {
                SignerInfo signerInfo = signedCms.SignerInfos[0];
                (DateTime signTime, string cardNumber) = ParseAttribute(signerInfo);
                X509Certificate2 x509 = signerInfo.Certificate;
                return new HiPKIModel(true, "")
                {
                    tbs = BytesToString(signedCms.ContentInfo.Content),
                    Subject = x509.Subject,
                    Issuer = x509.Issuer,
                    SerialNumber = x509.SerialNumber,
                    signTime = signTime,
                    cardNumber = cardNumber,
                    NotBefore = x509.NotBefore,
                    NotAfter = x509.NotAfter
                };
            }
            return new HiPKIModel(false, HtmlEncode(i18N.Message.R16));
        }

        private (DateTime signTime, string cardNumber) ParseAttribute(SignerInfo signerInfo)
        {
            DateTime signTime = default;
            string cardNumber = default;
            foreach (CryptographicAttributeObject attr in signerInfo.SignedAttributes)
            {
                AsnEncodedData[] data = new AsnEncodedData[1];
                attr.Values.CopyTo(data, 0);
                switch (attr.Oid.Value)
                {
                    case "1.2.840.113549.1.9.5":
                        signTime = DateTime.ParseExact(Encoding.UTF8.GetString(data[0].RawData, 2, data[0].RawData.Length - 2), "yyMMddHHmmssZ", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal);
                        break;
                    case "2.16.886.1.100.2.204":
                        cardNumber = Encoding.UTF8.GetString(data[0].RawData, 2, data[0].RawData.Length - 2);
                        break;
                }
            }

            return (signTime: signTime, cardNumber: cardNumber);
        }
    }
}
