using System.Threading.Tasks;
using KdajBi.API.SecurityCore.Security.Hashing;
using KdajBi.API.SecurityCore.Security.Tokens;
using KdajBi.API.SecurityCore.Services;
using KdajBi.API.SecurityCore.Services.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using KdajBi.Core.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace KdajBi.API.Services
{
    public class AuthenticationService : API.SecurityCore.Services.IAuthenticationService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser>  _signInManager;
        private readonly ITokenHandler _tokenHandler;
        
        public AuthenticationService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenHandler tokenHandler)
        {
            _tokenHandler = tokenHandler;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<TokenResponse> CreateAccessTokenAsync(string email, string password)
        {
            //var user = await _userManager.FindByEmailAsync(email);
            var user = await _userManager.Users
                    .Include(x => x.Company)
                    .SingleAsync(x => x.NormalizedEmail == email);

            if (user == null)
            {
                return new TokenResponse(false, "Invalid credentials.", null);
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Count > 0)
            {
                user.UserRoles = new List<AppRole>();
                foreach (var item in roles)
                {
                    user.UserRoles.Add(new AppRole(item));
                }
            }
            var result = await _signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: false);
            if (result.Succeeded == false) { return new TokenResponse(false, "Invalid credentials.", null); }

            var token = _tokenHandler.CreateAccessToken(user);

            return new TokenResponse(true, null, token);
        }

        public async Task<TokenResponse> CreateAccessTokenForAsync(string email)
        {
            //var user = await _userManager.FindByEmailAsync(email);
            var user = await _userManager.Users
                    .Include(x => x.Company)
                    .SingleAsync(x => x.NormalizedEmail == email);

            if (user == null)
            {
                return new TokenResponse(false, "Invalid credentials.", null);
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Count > 0)
            {
                user.UserRoles = new List<AppRole>();
                foreach (var item in roles)
                {
                    user.UserRoles.Add(new AppRole(item));
                }
            }

            var token = _tokenHandler.CreateAccessToken(user);

            return new TokenResponse(true, null, token);
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, string userEmail)
        {
            var token = _tokenHandler.TakeRefreshToken(refreshToken);

            if (token == null)
            {
                return new TokenResponse(false, "Invalid refresh token.", null);
            }

            if (token.IsExpired())
            {
                return new TokenResponse(false, "Expired refresh token.", null);
            }

            //var user = await _userManager.FindByEmailAsync(userEmail);
            var user = await _userManager.Users
                    .Include(x => x.Company)
                    .SingleAsync(x => x.NormalizedEmail == userEmail);
            if (user == null)
            {
                return new TokenResponse(false, "Invalid refresh token user.", null);
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Count > 0)
            {
                user.UserRoles = new List<AppRole>();
                foreach (var item in roles)
                {
                    user.UserRoles.Add(new AppRole(item));
                }
            }
            var accessToken = _tokenHandler.CreateAccessToken(user);
            return new TokenResponse(true, null, accessToken);
        }

        public void RevokeRefreshToken(string refreshToken)
        {
            _tokenHandler.RevokeRefreshToken(refreshToken);
        }
    }
}