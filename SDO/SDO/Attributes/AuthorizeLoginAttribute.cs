using Microsoft.AspNetCore.Authorization;

namespace SDO.Attributes
{
    /// <summary>
    /// 僅驗證是否登入，不驗證使用者權限
    /// </summary>
    public class AuthorizeLoginAttribute : AuthorizeAttribute
    {
    }
}
