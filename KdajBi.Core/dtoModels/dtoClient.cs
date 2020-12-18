using KdajBi.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KdajBi.Core.dtoModels
{
    public class dtoClient
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }

        public long LocationId { get; set; }

        public string CompanyName { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string FullName { get { return $"{FirstName} {LastName}"; } }

        public string Address { get; set; }
        public string ZipCode { get; set; }

        public string Mobile { get; set; }

        public string Email { get; set; }

        public DateTime? Birthday { get; set; }

        public bool AllowsSMS { get; set; }
        public bool AllowsEmail { get; set; }

        public string TaxId { get; set; }
        public bool IsCompany { get; set; }

        public string Notes { get; set; }

        public ICollection<ClientTag> ClientTags { get; set; }


        public dtoClient()
        {
            AllowsEmail = true;
            AllowsSMS = true;
            TaxId = "";
            ClientTags = new List<ClientTag>();
        }
    }
}
