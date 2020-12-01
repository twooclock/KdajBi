
using KdajBi.Core.Models;

namespace KdajBi.API.SecurityCore.Services.Communication
{
    public class CreateUserResponse : BaseResponse
    {
        public AppUser User { get; private set; }

        public CreateUserResponse(bool success, string message, AppUser user) : base(success, message)
        {
            User = user;
        }
    }
}