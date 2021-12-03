
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;


namespace KdajBi.Core.Models
{
    public class ContactMail : BaseModel
    {
        public long Id { get; set; }
        [MaxLength(250)]
        public string FromEmail { get; set; }
        public long FromCompanyId { get; set; }
        public long FromLocationId { get; set; }
        [MaxLength(250)]
        public string ToEmail { get; set; }
        public long ToCompanyId { get; set; }
        public long ToLocationId { get; set; }
        [MaxLength(150)]
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool EmailSent { get; set; }

    }
}
