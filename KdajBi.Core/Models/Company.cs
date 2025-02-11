using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KdajBi.Core.Models
{
    public class Company:BaseModel
    {
        public long Id { get; set; }

        [MinLength(3), MaxLength(150), Required]
        public string Name { get; set; }

        [MaxLength(150), Required]
        public string Address { get; set; }
        
       
        public int Zip { get; set; }

        [MaxLength(150), Required]
        public string ZipTown { get; set; }
        
        [MinLength(3), MaxLength(15), Required]
        public string TaxVATNumber { get; set; }

        [MinLength(10), MaxLength(15), Required]
        public string TaxIDNumber { get; set; }

        public bool IsTaxpayer { get; set; }

        [MaxLength(20), Required]
        public string Mobile { get; set; }
        public virtual ICollection<Location> CompanyLocation { get; set; }

        public  Company() { }

    }
}
