using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace KdajBi.Core.Models
{
    //service offered
    public class Service : BaseModel
    {
        public long Id { get; set; }

        [ForeignKey("Company"), Required]
        public long CompanyId { get; set; }


        [MinLength(3), MaxLength(150), Required]
        public string Name { get; set; }

        //approximate service duration in minutes
        public int Minutes { get; set; } 

    }
}
