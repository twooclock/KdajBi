using KdajBi.Core.Models;
using KdajBi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KdajBi.Web.FullCalendar;
using System.Globalization;

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

        private string _cboScheduleTypeHTML="";
        public List<resourceWD> resourcesWD = new List<resourceWD>();
        public string cboScheduleTypeHTML(long p_scheduleType, bool p_altWeeks) {
            if (_cboScheduleTypeHTML == "")
            {
                //synchronized with FullCalendar.cs function getRRule
                int week = (int)Math.Ceiling(((DateTime.Now - new DateTime(2018, 01, 01)).TotalDays / 7));
                _cboScheduleTypeHTML = "<div><select id=\"cboScheduleType\" name=\"cboScheduleType\" class=\"selectpicker custom-select custom-select-sm form-control\">";
                _cboScheduleTypeHTML += "<option value=\"0\" " + ((p_scheduleType == 0) ? " selected " : "") + ((p_altWeeks == true) ? "disabled = \"disabled\"":"") + ">Splošni urnik</option>";
                 
                _cboScheduleTypeHTML += "<option value=\"1\"" + ((p_scheduleType == 1) ? " selected " : "") + ((p_altWeeks == true) ?"":"disabled = \"disabled\"")+">" +((week%2==0)?"Naslednji teden":"Ta teden")+"</option>";
                _cboScheduleTypeHTML += "<option value=\"2\"" + ((p_scheduleType == 2) ? " selected " : "") + ((p_altWeeks == true) ? "" : "disabled = \"disabled\"") + " >" + ((week % 2 == 0) ? "Ta teden" : "Naslednji teden") + "</option>";
                
                _cboScheduleTypeHTML += "</select></div>";
            }

            return _cboScheduleTypeHTML; } 

        public WeekDay[] WeekDays = new WeekDay[7];
        public vmWorkplace()
        {
            for (int i = 0; i < 7; i++) { WeekDays[i] = new WeekDay(); }
        }

    }
}
