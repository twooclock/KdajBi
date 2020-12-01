using KdajBi.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KdajBi.Web.Services
{
    public class MyUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser,AppRole>
    {
        public MyUserClaimsPrincipalFactory(
            UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
                : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
//            identity.AddClaim(new Claim("Nadzornik", bool.TrueString));
            return identity;
        }
    }
}
