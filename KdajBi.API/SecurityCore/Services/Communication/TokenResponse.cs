using KdajBi.API.SecurityCore.Security.Tokens;

namespace KdajBi.API.SecurityCore.Services.Communication
{
    public class TokenResponse : BaseResponse
    {
        public AccessToken Token { get; set; }

        public TokenResponse(bool success, string message, AccessToken token) : base(success, message)
        {
            Token = token;
        }
    }
}