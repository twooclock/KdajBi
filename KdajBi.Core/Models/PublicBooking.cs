using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KdajBi.Core.Models
{
    public class PublicBooking: BaseModel
    {
        public long Id { get; set; }

        [Required]
        [MinLength(3), MaxLength(25)]
        public string Token { get; set; }

        [ForeignKey("Location"), Required]
        public long LocationId { get; set; }
        public virtual Location Location { get; set; }

        [MaxLength(20)]
        public string Mobile { get; set; }

        public int PIN { get; set; }

        public DateTime? Authorized { get; set; }

        //actual booking
        [ForeignKey("Workplace")]
        public long? WorkplaceId { get; set; }
        public virtual Workplace Workplace { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

        [ForeignKey("Client")]
        public long? ClientId { get; set; }
        public virtual Client Client { get; set; }

        [ForeignKey("Service")]
        public long? ServiceId { get; set; }
        public virtual Service Service { get; set; }

        [MaxLength(1024)]
        public string GCalId { get; set; }

        public long Status { get; set; } //0-needs confirmation 1-confirmed 2-rejected
    }
}
