using KdajBi.Core.Models;

namespace KdajBi.API.SecurityCore.Security.Tokens
{
    public interface ITokenHandler
    {
         AccessToken CreateAccessToken(AppUser user);
         RefreshToken TakeRefreshToken(string token);
         void RevokeRefreshToken(string token);
    }
}