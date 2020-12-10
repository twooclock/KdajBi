using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KdajBi.Core.Models
{
    public class Client : BaseModel
    {
        public long Id { get; set; }
        [ForeignKey("Company"), Required] //not really necessary but should be easier to navigate
        public long CompanyId { get; set; }

        [ForeignKey("Location"), Required]
        public long LocationId { get; set; }

        [MaxLength(150)]
        public string CompanyName { get; set; }
        [MaxLength(150)]
        public string FirstName { get; set; }

        [MaxLength(150)]
        public string LastName { get; set; }
        public string FullName { get { return $"{FirstName} {LastName}"; } }

        [MaxLength(150)]
        public string Address { get; set; }
        [MaxLength(150)]
        public string ZipCode { get; set; }

        [MaxLength(20)]
        public string Mobile { get; set; }

        [MaxLength(150)]
        public string Email { get; set; }

        public DateTime? Birthday { get; set; }

        public bool AllowsSMS { get; set; }
        public bool AllowsEmail { get; set; }

        [MaxLength(15)]
        public string TaxId { get; set; }
        public bool IsCompany { get; set; }

        public string Notes { get; set; }
    
        public Client() {
            AllowsEmail = true;
            AllowsSMS = true;
            TaxId = "";
        }
    }
}
