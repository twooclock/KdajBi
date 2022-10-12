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
        private string _cboScheduleTypeHTML="";
        public string cboScheduleTypeHTML(long p_scheduleType, bool p_altWeeks) {
            if (_cboScheduleTypeHTML == "")
            {
                Calendar cal = new CultureInfo("en-US").Calendar; //TODO!
                int week = cal.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                _cboScheduleTypeHTML = "<div><select id=\"cboScheduleType\" name=\"cboScheduleType\" class=\"selectpicker custom-select custom-select-sm form-control\">";
                _cboScheduleTypeHTML += "<option value=\"0\" " + ((p_scheduleType == 0) ? " selected " : "") + ((p_altWeeks == true) ? "disabled = \"disabled\"":"") + ">Splošni urnik</option>";
                 
                _cboScheduleTypeHTML += "<option value=\"1\"" + ((p_scheduleType == 1) ? " selected " : "") + ((p_altWeeks == true) ?"":"disabled = \"disabled\"")+">Lihi urnik (" +((week%2==0)?"Naslednji teden":"Ta teden")+")</option>";
                _cboScheduleTypeHTML += "<option value=\"2\"" + ((p_scheduleType == 2) ? " selected " : "") + ((p_altWeeks == true) ? "" : "disabled = \"disabled\"") + " >Sodi urnik (" + ((week % 2 == 0) ? "Ta teden" : "Naslednji teden") + ")</option>";
                
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
