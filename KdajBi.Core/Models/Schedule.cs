using System;
using System.Collections.Generic;
using System.Text;

namespace KdajBi.Core.Models
{
    public enum ScheduleType
    {
        AllWeeks=0,
        OddWeeks=1,
        EvenWeeks=2
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
    }
}
