using System;
using System.Collections.Generic;
using System.Linq;
namespace KdajBi.Core.dtoModels.Requests
{
    public class BookingRequest
    {
        public BookingRequest(DateTime date, string timeSlot)
        {
            DateTime start = DateTime.Parse(date.ToString("yyyy-MM-dd") + " " + timeSlot.Split('-')[0]);
            DateTime end = DateTime.Parse(date.ToString("yyyy-MM-dd") + " " + timeSlot.Split('-')[1]);
            this.TimeSlot = new TimeSlot(start, end);
        }

        public TimeSlot TimeSlot { get; set; }
    }

    public class BookingTimeslots
    {
        public List<TimeSlot> TimeSlots { get; set; }
        public long WorkplaceId { get; set; }

        public BookingTimeslots(long p_WorkplaceId, List<TimeSlot> p_TimeSlots)
        {
            TimeSlots = new List<TimeSlot>();
            TimeSlots.AddRange(p_TimeSlots);
            WorkplaceId = p_WorkplaceId;
        }
    }
}