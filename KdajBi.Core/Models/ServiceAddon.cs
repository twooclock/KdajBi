using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KdajBi.Core.Models
{
    public class ServiceAddon : BaseModel
    {
        public long Id { get; set; }

        [ForeignKey("Service"), Required]
        public long ServiceId { get; set; }

        public string Name { get; set; }
      
        public int Minutes { get; set; } //service addon duration in minutes

        [MaxLength(50)]
        [DefaultValue("")]
        public string PriceDescription { get; set; }

        [MaxLength(250)]
        [DefaultValue("")]
        public string Notes { get; set; }

    }
}
