using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KdajBi.Core.Models
{
    public class ClientTag
    {
        public long Id { get; set; }

        [ForeignKey("Client"), Required]
        public long ClientId { get; set; }

        public Client Client { get; set; }

        [ForeignKey("Tag"), Required]
        public long TagId { get; set; }
        public Tag Tag { get; set; }
        public ClientTag() { }
        public ClientTag(long clientId, long tagId) { ClientId = clientId; TagId = tagId; }
    }
}
