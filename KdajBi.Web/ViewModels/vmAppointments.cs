using KdajBi.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KdajBi.Web.ViewModels
{

    public class vmAppointments : _BaseViewModel
    {
        public Location Location;
        public List<Tuple<string, string, long>> GoogleCalendars = new List<Tuple<string, string, long>>(); //new Dictionary<string, string>();
        public Dictionary<string, string> Settings = new Dictionary<string, string>();
        public string calEvents = "";
        public string ClientsJson;

        public string calBGEvents;
        public List<resourceWD> resourcesWD=new List<resourceWD>();

        public void AddcalEvents(string p_events) {
            if (string.IsNullOrEmpty( calEvents) == true)
            { calEvents = p_events; }
            else
            {
                //remove []
                calEvents = calEvents.Substring(1, calEvents.Length - 2);
                if (calEvents != "") { calEvents += ","; }
                    calEvents += p_events.Substring(1, p_events.Length - 2);
                calEvents = "[" + calEvents + "]";
            }
            calEvents = calEvents.Replace(",\"color\":null", "");
        }
        public void AddcalBGEvents(string p_events)
        {
            if (string.IsNullOrEmpty(calBGEvents) == true)
            { calBGEvents = p_events; }
            else
            {
                if (p_events.Substring(1, p_events.Length - 2).Trim().Length > 0)
                { 
                    calBGEvents = calBGEvents.Substring(1, calBGEvents.Length - 2); //remove []
                    if (calBGEvents != "") { calBGEvents += ","; }
                    calBGEvents += p_events.Substring(1, p_events.Length - 2);
                    calBGEvents = "[" + calBGEvents + "]"; //add [] back
                }
            }
            calBGEvents = calBGEvents.Replace(",\"color\":null", "");
            
        }

    }
    public class resourceWD
    {
        public long resourceId { get; set; }
        public WeekDay[] WeekDays = new WeekDay[7];
        public resourceWD()
        {
            for (int i = 0; i < 7; i++) { WeekDays[i] = new WeekDay(); }
        }
    }
}
