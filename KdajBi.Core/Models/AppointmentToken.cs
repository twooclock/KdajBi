using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace KdajBi.Core.Models
{
    public class AppointmentToken:BaseModel
    {
        public long Id { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string Service { get; set; }

        [Required]
        public long Minutes { get; set; }

        [ForeignKey("Company"), Required]
        public long CompanyId { get; set; }
        
        public virtual Company Company { get; set; }

        [ForeignKey("Location"), Required]
        public long LocationId { get; set; }
        public virtual Location Location { get; set; }

        [ForeignKey("Client"), Required]
        public long ClientId { get; set; }
        public virtual Client Client { get; set; }

        [ForeignKey("AppUser"), Required]
        public long AppUserId { get; set; }

    }
}
