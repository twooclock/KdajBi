using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KdajBi.Core.Models
{
    public class Tag : BaseModel
    {
        public long Id { get; set; }

        [ForeignKey("Company"), Required]
        public long CompanyId { get; set; }

        [MaxLength(20),Required]
        public string Title { get; set; }

        [JsonIgnore]
        public ICollection<ClientTag> ClientTags { get; set; }

        [NotMapped]
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public IEnumerable<Client> Clients
        {
            get => ClientTags.Select(r => r.Client);
            set => ClientTags = value.Select(v => new ClientTag() { TagId = v.Id }).ToList();
        }

        public Tag()
        {
            ClientTags = new List<ClientTag>();
        }

        public Tag(long companyId, string title)
        {
            CompanyId = companyId;
            Title = title;
            ClientTags = new List<ClientTag>();
        }
    }
}
