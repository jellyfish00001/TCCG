namespace SDO.Models
{
    public class TokenSetting
    {
        public bool TokenRefresh { get; set; }

        public int TokenExpireTime { get; set; }

        public string PrivateKeyPath { get; set; }
    }
}
