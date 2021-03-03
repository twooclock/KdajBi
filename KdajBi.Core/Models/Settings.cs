using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KdajBi.Core.Models
{
    public class Setting : BaseModel
    {
        public long Id { get; set; }
        
        [ForeignKey("Company"), Required] //not really necessary but should be easier to navigate
        public long CompanyId { get; set; }

        [ForeignKey("AppUser"), Required]
        public long UserId { get; set; }

        [MaxLength(100), Required]
        public string Key { get; set; }

        [Required]
        public string Value { get; set; }

        public Setting() { }

    }
}
