using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KdajBi.Core.Models
{
    public class Workplace : BaseModel
    {
        public long Id { get; set; }

        [ForeignKey("Location"), Required]
        public long LocationId { get; set; }

        [ForeignKey("AppUser")]
        public long? UserId { get; set; }

        [MinLength(3), MaxLength(150), Required]
        public string Name { get; set; }

        public int SortPosition { get; set; }

        [MaxLength(150)]
        public string GoogleCalendarID { get; set; }
        public Workplace() { }

    }
}
