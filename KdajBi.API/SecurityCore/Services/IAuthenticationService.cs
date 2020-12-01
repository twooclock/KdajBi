using System.Threading.Tasks;
using KdajBi.API.SecurityCore.Services.Communication;

namespace KdajBi.API.SecurityCore.Services
{
    public interface IAuthenticationService
    {
         Task<TokenResponse> CreateAccessTokenAsync(string email, string password);
        Task<TokenResponse> CreateAccessTokenForAsync(string email);
        Task<TokenResponse> RefreshTokenAsync(string refreshToken, string userEmail);
         void RevokeRefreshToken(string refreshToken);
    }
}