using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace KdajBi.Core.Models
{
    public class UserCompany
    {
        public long Id { get; set; }

        [ForeignKey("User"),Required]
        public long UserId { get; set; }

        [ForeignKey("Company"), Required]
        public long CompanyId { get; set; }

        [MinLength(10), MaxLength(10)]
        public string Permissions { get; set; }

    }
}
