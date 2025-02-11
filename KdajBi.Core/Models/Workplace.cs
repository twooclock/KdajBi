using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KdajBi.Core.Models
{
    public class Workplace : BaseModel
    {
        public long Id { get; set; }

        [ForeignKey("Location"), Required]
        public long LocationId { get; set; }

        [ForeignKey("AppUser")]
        public long? UserId { get; set; }

        [MinLength(3), MaxLength(150), Required]
        public string Name { get; set; }

        public int SortPosition { get; set; }

        [MaxLength(150)]
        public string GoogleCalendarID { get; set; }

        public bool SequentialBooking { get; set; } //when offering available timeslots for public booking, it offers only timeslots that are adjacent to exsisting bookings

        [NotMapped]
        public string GoogleCalendarSummary { get; set; }
        [NotMapped]
        public string GoogleCalendarColor { get; set; }

        [JsonIgnore]
        public ICollection<WorkplaceSchedule> WorkplaceSchedules { get; set; }

        [NotMapped]
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public ICollection<Schedule> Schedules
        {
            get => WorkplaceSchedules.Select(r => r.Schedule).ToList();
            set => WorkplaceSchedules = value.Select(v => new WorkplaceSchedule() { ScheduleId = v.Id }).ToList();
        }

        
        public ICollection<WorkplaceScheduleException> WorkplaceScheduleExceptions { get; set; }

        public Schedule ScheduleByType(long p_ScheduleType)
        {
            Schedule retval;
            try
            {
                retval = Schedules.First(s => s.Type == p_ScheduleType);
            }
            catch (Exception)
            {

                return null; ;
            }
            
            return retval;
        }
        public Workplace() { WorkplaceSchedules = new List<WorkplaceSchedule>(); }

    }
}
