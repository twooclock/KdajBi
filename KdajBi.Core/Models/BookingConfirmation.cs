using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace KdajBi.Core.Models
{
    public class BookingConfirmation:BaseModel
    {
        public long Id { get; set; }

        [ForeignKey("AppointmentToken"), Required]
        public long AppointmentTokenId { get; set; }
        
        public virtual AppointmentToken AppointmentToken { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        [MaxLength(1024)]
        public string GCalId { get; set; }

    }
}
