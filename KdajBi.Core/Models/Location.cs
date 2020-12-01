using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KdajBi.Core.Models
{
    //could be salon, dental office, etc.
    public class Location : BaseModel
    {
        public long Id { get; set; }

        [ForeignKey("Company"), Required]
        public long CompanyId { get; set; }
        

        [MinLength(3), MaxLength(150), Required]
        public string Name { get; set; }

        [MinLength(9), MaxLength(15)]
        public string Tel { get; set; }
        public Location() { }

    }
}
