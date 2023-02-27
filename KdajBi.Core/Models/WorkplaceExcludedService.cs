using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KdajBi.Core.Models
{
    public class WorkplaceExcludedService
    {
        public long Id { get; set; }

        [ForeignKey("Workplace"), Required]
        public long WorkplaceId { get; set; }
        public Workplace Workplace { get; set; }

        [ForeignKey("Service"), Required]
        public long ServiceId { get; set; }
        public Service Service { get; set; }
        public WorkplaceExcludedService() { }
        public WorkplaceExcludedService(long workplaceId, long serviceId) { WorkplaceId = workplaceId; ServiceId = serviceId; }

    }

    

}
