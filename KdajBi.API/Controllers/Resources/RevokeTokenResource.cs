using System.ComponentModel.DataAnnotations;

namespace KdajBi.API.Controllers.Resources
{
    public class RevokeTokenResource
    {
        [Required]
        public string Token { get; set; }
    }
}