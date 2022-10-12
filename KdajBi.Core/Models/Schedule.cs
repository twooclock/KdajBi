using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KdajBi.Core.Models
{
    public enum ScheduleType
    {
        AllWeeks = 0,
        OddWeeks = 1,
        EvenWeeks = 2
    }
    public class Schedule : BaseModel
    {
        //default schedule start-end
        private DateTime DefaultWorkdayStart = new DateTime(2020, 1, 1, 8, 0, 0);
        private DateTime DefaultWorkdayEnd = new DateTime(2020, 1, 1, 20, 0, 0);
        private DateTime DefaultSaturdayStart = new DateTime(2020, 1, 1, 8, 0, 0);
        private DateTime DefaultSaturdayEnd = new DateTime(2020, 1, 1, 8, 0, 0);
        private DateTime DefaultSundayStart = new DateTime(2020, 1, 1, 0, 0, 0);
        private DateTime DefaultSundayEnd = new DateTime(2020, 1, 1, 0, 0, 0);

        public long Id { get; set; }

        //ScheduleType:
        //0-All (default)
        //1-Odd weeks (reserved)
        //2-Even Weeks (reserved)
        public long Type { get; set; }
        public DateTime MondayStart { get; set; }
        public DateTime MondayEnd { get; set; }
        public DateTime TuesdayStart { get; set; }
        public DateTime TuesdayEnd { get; set; }
        public DateTime WednesdayStart { get; set; }
        public DateTime WednesdayEnd { get; set; }
        public DateTime ThursdayStart { get; set; }
        public DateTime ThursdayEnd { get; set; }
        public DateTime FridayStart { get; set; }
        public DateTime FridayEnd { get; set; }
        public DateTime SaturdayStart { get; set; }
        public DateTime SaturdayEnd { get; set; }
        public DateTime SundayStart { get; set; }
        public DateTime SundayEnd { get; set; }
        public string EventsJson { get; set; }

        public Schedule()
        {
            MondayStart = DefaultWorkdayStart;
            MondayEnd = DefaultWorkdayEnd;
            TuesdayStart = DefaultWorkdayStart;
            TuesdayEnd = DefaultWorkdayEnd;
            WednesdayStart = DefaultWorkdayStart;
            WednesdayEnd = DefaultWorkdayEnd;
            ThursdayStart = DefaultWorkdayStart;
            ThursdayEnd = DefaultWorkdayEnd;
            FridayStart = DefaultWorkdayStart;
            FridayEnd = DefaultWorkdayEnd;
            SaturdayStart = DefaultSaturdayStart;
            SaturdayEnd = DefaultSaturdayEnd;
            SundayStart = DefaultSundayStart;
            SundayEnd = DefaultSundayEnd;
        }

        public string minTime()
        {
            DateTime currMin = DateTime.MaxValue;
            if (MondayStart != MondayEnd && MondayStart.TimeOfDay < currMin.TimeOfDay) { currMin = MondayStart; }
            if (TuesdayStart != TuesdayEnd && TuesdayStart.TimeOfDay < currMin.TimeOfDay) { currMin = TuesdayStart; }
            if (WednesdayStart != WednesdayEnd && WednesdayStart.TimeOfDay < currMin.TimeOfDay) { currMin = WednesdayStart; }
            if (ThursdayStart != ThursdayEnd && ThursdayStart.TimeOfDay < currMin.TimeOfDay) { currMin = ThursdayStart; }
            if (FridayStart != FridayEnd && FridayStart.TimeOfDay < currMin.TimeOfDay) { currMin = FridayStart; }
            if (SaturdayStart != SaturdayEnd && SaturdayStart.TimeOfDay < currMin.TimeOfDay) { currMin = SaturdayStart; }
            if (SundayStart != SundayEnd && SundayStart.TimeOfDay < currMin.TimeOfDay) { currMin = SundayStart; }
            return currMin.TimeOfDay.ToString("c");
        }

        public string maxTime()
        {
            TimeSpan currMax = TimeSpan.Zero;
            if (MondayEnd.TimeOfDay.CompareTo(currMax) == 1) { currMax = MondayEnd.TimeOfDay; }
            if (TuesdayEnd.TimeOfDay.CompareTo(currMax) == 1) { currMax = TuesdayEnd.TimeOfDay; }
            if (WednesdayEnd.TimeOfDay.CompareTo(currMax) == 1) { currMax = WednesdayEnd.TimeOfDay; }
            if (ThursdayEnd.TimeOfDay.CompareTo(currMax) == 1) { currMax = ThursdayEnd.TimeOfDay; }
            if (FridayEnd.TimeOfDay.CompareTo(currMax) == 1) { currMax = FridayEnd.TimeOfDay; }
            if (SaturdayEnd.TimeOfDay.CompareTo(currMax) == 1) { currMax = SaturdayEnd.TimeOfDay; }
            if (SundayEnd.TimeOfDay.CompareTo(currMax) == 1) { currMax = SundayEnd.TimeOfDay; }
            return currMax.ToString("c");
        }
    }
}
