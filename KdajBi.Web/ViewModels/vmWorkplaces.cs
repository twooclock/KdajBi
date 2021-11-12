using KdajBi.Core.Models;
using KdajBi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KdajBi.Web.FullCalendar;

namespace KdajBi.Web.ViewModels
{
    public class WeekDay
    {
        public List<rEvent> calEvents;
        public List<string> exDates;

        public WeekDay() { calEvents = new List<rEvent>(); exDates = new List<string>(); }
    }
    public class vmWorkplace : _BaseViewModel
    {
        public Workplace Workplace;
        public Location Location;
        public long ScheduleId; //id of the schedule that is shown/edited
        public string calWEvents;
        public string calBGEvents;

        public WeekDay[] WeekDays = new WeekDay[7];
        public vmWorkplace()
        {
            for (int i = 0; i < 7; i++) { WeekDays[i] = new WeekDay(); }
        }

    }
}
