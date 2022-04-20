using System;
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
}