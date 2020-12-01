using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace KdajBi.Core.Models
{
    public class Company:BaseModel
    {
        public long Id { get; set; }

        [MinLength(3), MaxLength(150), Required]
        public string Name { get; set; }

        [MinLength(3), MaxLength(15), Required]
        public string Davcna { get; set; }
        public virtual ICollection<Location> CompanyLocation { get; set; }

        public  Company() { }

    }
}
