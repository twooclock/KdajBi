using KdajBi.Core;
using KdajBi.Core.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace KdajBi.Web
{
    public static class FullCalendar
    {
        public class Resource
        {
            public string id;
            public int order;
            public string title;
            public string eventColor;
            public string eventTextColor;
            public List<businessHours> businessHours;
            public Dictionary<string,string> extendedProps=new Dictionary<string, string>();
            public Resource() { businessHours = new List<businessHours>();  }
            public Resource(string p_id, int p_order, string p_title, string p_eventColor)
            { id = p_id; order = p_order; title = p_title; eventColor = p_eventColor; eventTextColor = Utils.PickTextColorBasedOnBgColorSimple(p_eventColor); businessHours = new List<businessHours>(); }

            public string ToJson() { return Newtonsoft.Json.JsonConvert.SerializeObject(this); }
        }
        public class businessHours
        {
            public string startTime;
            public string endTime;
            public int[] daysOfWeek;

            public businessHours(string p_startTime, string p_endTime, int[] p_daysOfWeek)
            { startTime = p_startTime; endTime = p_endTime; daysOfWeek = p_daysOfWeek; }
        }

        public class Event
        {
            private string _color;
            public String id { get; set; }
            public String resourceId { get; set; }
            public String title { get; set; }
            public String start { get; set; }
            public String end { get; set; }
            public bool allDay { get; set; }
            public String display { get; set; }
            public String color {
                get { return _color; }
                set { _color = value; this.textColor = Utils.PickTextColorBasedOnBgColorSimple(_color); }
            }
            public String textColor { get; set; }

            public Dictionary<string, string> extendedProps = new Dictionary<string, string>();
        }

        public class rEvent
        {
            public String id { get; set; }
            public String title { get; set; }
            public String duration { get; set; }
            public bool allDay { get; set; }
            public String display { get; set; }
            public String backgroundColor { get; set; }
            public String rrule { get; set; }

            public rEvent() { }

            public string resourceId { get; set; }

        }

        public class rEventShow
        {
            public String resourceId { get; set; }
            public String title { get; set; }
            public String startTime { get; set; }
            public String endTime { get; set; }
            public bool allDay { get; set; }
            public String display { get; set; }

            public rEventShow() { }


        }




        public static List<FullCalendar.rEvent> CreateREventsFromREventShow(long p_scheduletype, List<FullCalendar.rEventShow> p_events, string p_color)
        {
            FullCalendar.rEvent myREvent;
            var devents = new List<FullCalendar.rEvent>();
            foreach (var item in p_events)
            {
                myREvent = CreateREvent(p_scheduletype, item.resourceId, item.startTime, item.endTime);
                if (myREvent != null)
                {
                    myREvent.display = "background";
                    myREvent.backgroundColor = p_color;
                    
                    devents.Add(myREvent);
                }
            }
            return devents;
        }

        public static List<FullCalendar.rEventShow> getWeekrEventShowFromSchedule(Schedule p_schedule)
        {
            var events = new List<FullCalendar.rEventShow>();
            FullCalendar.rEventShow myEvent;
            myEvent = CreateEvent("0", p_schedule.SundayStart, p_schedule.SundayEnd);
            if (myEvent != null) { events.Add(myEvent); }
            myEvent = CreateEvent("1", p_schedule.MondayStart, p_schedule.MondayEnd);
            if (myEvent != null) { events.Add(myEvent); }
            myEvent = CreateEvent("2", p_schedule.TuesdayStart, p_schedule.TuesdayEnd);
            if (myEvent != null) { events.Add(myEvent); }
            myEvent = CreateEvent("3", p_schedule.WednesdayStart, p_schedule.WednesdayEnd);
            if (myEvent != null) { events.Add(myEvent); }
            myEvent = CreateEvent("4", p_schedule.ThursdayStart, p_schedule.ThursdayEnd);
            if (myEvent != null) { events.Add(myEvent); }
            myEvent = CreateEvent("5", p_schedule.FridayStart, p_schedule.FridayEnd);
            if (myEvent != null) { events.Add(myEvent); }
            myEvent = CreateEvent("6", p_schedule.SaturdayStart, p_schedule.SaturdayEnd);
            if (myEvent != null) { events.Add(myEvent); }
            return events;
        }
        /// <summary>
        /// Changes schedule start end times according to events
        /// </summary>
        /// <param name="p_Schedule"></param>
        /// <param name="p_Events"></param>
        public static void setScheduleFromWeekrEventShowEvents(ref Schedule p_Schedule, List<FullCalendar.rEventShow> p_Events)
        {
            //"remove" all schedules
            p_Schedule.SundayStart = p_Schedule.SundayStart.Date;
            p_Schedule.SundayEnd = p_Schedule.SundayEnd.Date;
            p_Schedule.MondayStart = p_Schedule.MondayStart.Date;
            p_Schedule.MondayEnd = p_Schedule.MondayEnd.Date;
            p_Schedule.TuesdayStart = p_Schedule.TuesdayStart.Date;
            p_Schedule.TuesdayEnd = p_Schedule.TuesdayEnd.Date;
            p_Schedule.WednesdayStart = p_Schedule.WednesdayStart.Date;
            p_Schedule.WednesdayEnd = p_Schedule.WednesdayEnd.Date;
            p_Schedule.ThursdayStart = p_Schedule.ThursdayStart.Date;
            p_Schedule.ThursdayEnd = p_Schedule.ThursdayEnd.Date;
            p_Schedule.FridayStart = p_Schedule.FridayStart.Date;
            p_Schedule.FridayEnd = p_Schedule.FridayEnd;
            p_Schedule.SaturdayStart = p_Schedule.SaturdayStart.Date;
            p_Schedule.SaturdayEnd = p_Schedule.SaturdayEnd.Date;
            //you will change only days supplied in events
            foreach (var item in p_Events)
            {
                switch (item.resourceId)
                {
                    case "0":
                        p_Schedule.SundayStart = p_Schedule.SundayStart.Date.AddDays(1).AddSeconds(-1);
                        break;
                    case "1":
                        p_Schedule.MondayStart = p_Schedule.MondayStart.Date.AddDays(1).AddSeconds(-1);
                        break;
                    case "2":
                        p_Schedule.TuesdayStart = p_Schedule.TuesdayStart.Date.AddDays(1).AddSeconds(-1);
                        break;
                    case "3":
                        p_Schedule.WednesdayStart = p_Schedule.WednesdayStart.Date.AddDays(1).AddSeconds(-1);
                        break;
                    case "4":
                        p_Schedule.ThursdayStart = p_Schedule.ThursdayStart.Date.AddDays(1).AddSeconds(-1);
                        break;
                    case "5":
                        p_Schedule.FridayStart = p_Schedule.FridayStart.Date.AddDays(1).AddSeconds(-1);
                        break;
                    case "6":
                        p_Schedule.SaturdayStart = p_Schedule.SaturdayStart.Date.AddDays(1).AddSeconds(-1);
                        break;
                    default:
                        break;
                }


            }


            foreach (var item in p_Events)
            {
                TimeSpan start = TimeSpan.Parse(item.startTime);
                TimeSpan end = TimeSpan.Parse(item.endTime);
                switch (item.resourceId)
                {
                    case "0":
                        if (p_Schedule.SundayStart.TimeOfDay > start) { p_Schedule.SundayStart = DateTime.Now.Date.Add(start); }
                        if (p_Schedule.SundayEnd.TimeOfDay < end) { p_Schedule.SundayEnd = DateTime.Now.Date.Add(end); }
                        break;
                    case "1":
                        if (p_Schedule.MondayStart.TimeOfDay > start) { p_Schedule.MondayStart = DateTime.Now.Date.Add(start); }
                        if (p_Schedule.MondayEnd.TimeOfDay < end) { p_Schedule.MondayEnd = DateTime.Now.Date.Add(end); }
                        break;
                    case "2":
                        if (p_Schedule.TuesdayStart.TimeOfDay > start) { p_Schedule.TuesdayStart = DateTime.Now.Date.Add(start); }
                        if (p_Schedule.TuesdayEnd.TimeOfDay < end) { p_Schedule.TuesdayEnd = DateTime.Now.Date.Add(end); }
                        break;
                    case "3":
                        if (p_Schedule.WednesdayStart.TimeOfDay > start) { p_Schedule.WednesdayStart = DateTime.Now.Date.Add(start); }
                        if (p_Schedule.WednesdayEnd.TimeOfDay < end) { p_Schedule.WednesdayEnd = DateTime.Now.Date.Add(end); }
                        break;
                    case "4":
                        if (p_Schedule.ThursdayStart.TimeOfDay > start) { p_Schedule.ThursdayStart = DateTime.Now.Date.Add(start); }
                        if (p_Schedule.ThursdayEnd.TimeOfDay < end) { p_Schedule.ThursdayEnd = DateTime.Now.Date.Add(end); }
                        break;
                    case "5":
                        if (p_Schedule.FridayStart.TimeOfDay > start) { p_Schedule.FridayStart = DateTime.Now.Date.Add(start); }
                        if (p_Schedule.FridayEnd.TimeOfDay < end) { p_Schedule.FridayEnd = DateTime.Now.Date.Add(end); }
                        break;
                    case "6":
                        if (p_Schedule.SaturdayStart.TimeOfDay > start) { p_Schedule.SaturdayStart = DateTime.Now.Date.Add(start); }
                        if (p_Schedule.SaturdayEnd.TimeOfDay < end) { p_Schedule.SaturdayEnd = DateTime.Now.Date.Add(end); }
                        break;
                    default:
                        break;
                }
                
            }
        }
        public static FullCalendar.rEventShow CreateEvent(string p_intDayOfTheWeek, DateTime p_dateStart, DateTime p_dateEnd)
        {
            //ta se uporablja na prikazu tedenskega urnika
            if (p_dateStart.TimeOfDay != p_dateEnd.TimeOfDay)
            {
                return (new FullCalendar.rEventShow()
                {
                    resourceId = p_intDayOfTheWeek,
                    title = "",
                    startTime = DateTime.Now.Date.Add(p_dateStart.TimeOfDay).ToString("HH:mm"),
                    endTime = DateTime.Now.Date.Add(p_dateEnd.TimeOfDay).ToString("HH:mm"),
                    display = "block",
                    allDay = false
                });
            }
            return null;
        }
        public static FullCalendar.rEvent CreateREvent(long p_scheduleType, string p_intDayOfTheWeek, string p_StartTime, string p_EndTime)
        {

            if (p_StartTime != p_EndTime)
            {
                TimeSpan duration = DateTime.Parse(p_EndTime).Subtract(DateTime.Parse(p_StartTime));

                return (new FullCalendar.rEvent()
                {
                    id = p_scheduleType.ToString()+ p_intDayOfTheWeek,
                    title = "",
                    duration = duration.ToString("c"),
                    rrule = getRRule(p_scheduleType, p_StartTime, p_intDayOfTheWeek),
                    display = "block",
                    allDay = false
                });
            }
            return null;
        }

        public static string getRRule(long p_scheduleType, string p_StartTime, string p_dayoftheweek)
        {
            string interval;
            DateTime dt;
            switch (p_scheduleType)
            {
                case 1:
                    dt = new DateTime(2018, 01, 01); //monday
                    interval = "2";
                    break;
                case 2:
                    dt = new DateTime(2018, 01, 01).AddDays(7);
                    interval = "2";
                    break;
                default:
                    dt = new DateTime(2018, 01, 01);
                    interval = "1";
                    break;
            }
            string rrule = "DTSTART:" + dt.ToString("yyyyMMdd") + "T" + p_StartTime.Replace(":", "") + "00";
            rrule += "\nRRULE:FREQ=WEEKLY;INTERVAL=" + interval + ";";
            //CultureInfo ci = new CultureInfo("en-US");
            //DateTimeFormatInfo dtfi = ci.DateTimeFormat;
            rrule += "BYDAY=" + ((DayOfWeek)int.Parse(p_dayoftheweek)).ToString().Substring(0, 2).ToUpper();
            return rrule;
        }


    }
}
