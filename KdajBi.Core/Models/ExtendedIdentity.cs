using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Security.Principal;

namespace KdajBi.Core.Models
{
    public static class UserExtended
    {
        public static string GetFullName(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.Name);
            return claim == null ? null : claim.Value;
        }
        public static string GetAddress(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.StreetAddress);
            return claim == null ? null : claim.Value;
        }
        public static bool HasClaim(this IPrincipal user, string ClaimType, string ClaimValue)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimType);
            return claim == null ? false : (claim.Value == ClaimValue);
        }
    }

    public class AppRole : IdentityRole<int>
    {
        public AppRole() { }

        public AppRole(string name)
        {
            Name = name;
        }
    }



    public class AppUser : IdentityUser<int>
    {
        [PersonalData, StringLength(20)]
        public string FirstName { get; set; }

        [PersonalData, StringLength(20)]
        public string LastName { get; set; }

        public string FullName { get { return $"{FirstName} {LastName}"; } }

        [ForeignKey("Company"), Required]
        public long CompanyId { get; set; }
        public virtual Company Company { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? CreatedDate { get; set; }
        public int? CreatedUserID { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? UpdatedDate { get; set; }

        public int? UpdatedUserID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? LastLoginDate { get; set; }

        public virtual ICollection<AppRole> UserRoles { get; set; }
    }


    public static class RolesConfig
    {

        public static async Task InitialiseAsync(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
            string[] roleNames = { "Admin" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                    await roleManager.CreateAsync(new AppRole(roleName));
            }
        }
    }

}
