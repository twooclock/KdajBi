using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System;

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

        [ForeignKey("Workplace"), Required]
        public long WorkplaceId { get; set; }
        public virtual Workplace Workplace { get; set; }

        [ForeignKey("AppUser"), Required]
        public long AppUserId { get; set; }

        //actual booking
        public DateTime? BookingCreated { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        [MaxLength(1024)]
        public string GCalId { get; set; }

        public long Status { get; set; } //0-needs confirmation 1-confirmed 2-rejected

    }
}
