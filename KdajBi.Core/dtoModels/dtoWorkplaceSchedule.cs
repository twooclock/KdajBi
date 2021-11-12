using KdajBi.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KdajBi.Core.dtoModels
{
    public class dtoWorkplaceSchedule
    {
        public long WorkplaceId { get; set; }
        public long ScheduleId { get; set; }
        public ScheduleType Type { get; set; }
        public string calEvents { get; set; }
    }
}
