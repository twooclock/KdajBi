using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace KdajBi.Core.Models
{
    //service offered
    public class Service : BaseModel
    {
        public long Id { get; set; }

        [ForeignKey("Company"), Required]
        public long CompanyId { get; set; }

        [ForeignKey("Location"), Required]
        public long LocationId { get; set; }

        [MinLength(3), MaxLength(150), Required]
        public string Name { get; set; }
        public int Minutes { get; set; } //service duration in minutes
        public int Offset { get; set; } //service offset when crating free slots

        [MinLength(6), MaxLength(6)]
        public string Color { get; set; }
        public int SortPosition { get; set; }
        public bool UsedInClientBooking { get; set; }
		
        [ForeignKey("ServiceGroup")]
		public long? ServiceGroupId { get; set; }
		public virtual ServiceGroup ServiceGroup { get; set; }

        [MaxLength(50)]
        [DefaultValue("")]
        public string PriceDescription { get; set; }

        [MaxLength(250)]
        [DefaultValue("")]
        public string Notes { get; set; }
    }
}
