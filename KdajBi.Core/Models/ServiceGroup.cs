using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace KdajBi.Core.Models
{
    //service offered
    public class ServiceGroup : BaseModel
    {
        public long Id { get; set; }

        [ForeignKey("Company"), Required]
        public long CompanyId { get; set; }

        [ForeignKey("Location"), Required]
        public long LocationId { get; set; }

        [MinLength(3), MaxLength(150), Required]
        public string Name { get; set; }
        
        public int SortPosition { get; set; }

        [MinLength(6), MaxLength(6)]
        public string Color { get; set; }

    }
}
