using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KdajBi.Core.Models
{
    public class WorkplaceSchedule 
    {
        public long Id { get; set; }

        [ForeignKey("Workplace"), Required]
        public long WorkplaceId { get; set; }
        public Workplace Workplace { get; set; }

        [ForeignKey("Schedule"), Required]
        public long ScheduleId { get; set; }
        public Schedule Schedule { get; set; }
        public WorkplaceSchedule() { }
        public WorkplaceSchedule(long workplaceId, long scheduleId) { WorkplaceId = workplaceId; ScheduleId = scheduleId; }

    }

    public class WorkplaceScheduleException : BaseModel
    {
        public long Id { get; set; }

        [ForeignKey("Workplace"), Required]
        public long WorkplaceId { get; set; }
        public Workplace Workplace { get; set; }
        public DateTime Date { get; set; }
        public string EventsJson { get; set; }
    }

}
