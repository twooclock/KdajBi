using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
