using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KdajBi.Core.Models
{
    public class BaseModel
    {
        [JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? CreatedDate { get; set; }
        [JsonIgnore]
        public int? CreatedUserID { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public DateTime? UpdatedDate { get; set; }
        [JsonIgnore]
        public int? UpdatedUserID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public bool? Active { get; set; }

    }
}
