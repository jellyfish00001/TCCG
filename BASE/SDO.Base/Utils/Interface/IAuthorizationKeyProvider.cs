namespace SDO.Utils
{
    public interface IAuthorizationKeyProvider
    {
        byte[] AuthorizationKey { get; }
    }
}