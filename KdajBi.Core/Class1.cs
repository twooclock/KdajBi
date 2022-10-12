using KdajBi.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KdajBi.Core
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }

    public static class SettingsHelper
    {
        public static void getSettings(ApplicationDbContext _context, long companyid,  long? locationid, Dictionary<string, string> p_settings)
        {
            string[] keys = p_settings.Keys.ToArray();

            List<Setting> mySettings;
            if (locationid.HasValue == true)
            {
                mySettings = _context.Settings.Where(a => a.CompanyId == companyid && a.LocationId == locationid && keys.Contains(a.Key)).ToList();
            }
            else
            { mySettings = _context.Settings.Where(a => a.CompanyId == companyid && a.LocationId == null && keys.Contains(a.Key)).ToList(); }

            foreach (var item in mySettings)
            {
                p_settings[item.Key] = item.Value;
            }
        }

        public static string getSetting(ApplicationDbContext p_context, long companyid, long? locationid, string settingName, string defaultValue)
        {
            string retval = defaultValue;
            Setting mySetting;
            if (locationid.HasValue == true)
            {
                mySetting = p_context.Settings.Where(a => a.CompanyId == companyid && a.LocationId == locationid && a.Key==settingName).FirstOrDefault();
            }
            else
            { mySetting = p_context.Settings.Where(a => a.CompanyId == companyid && a.LocationId == null && a.Key == settingName).FirstOrDefault(); }
            if (mySetting != null)
            { retval = mySetting.Value; }
            return retval;
        }
    }


    public class TimeSlot
    {
        public DateTime start;
        public DateTime end;
        public TimeSlot() { }
        public TimeSlot(DateTime p_start, DateTime p_end) { start = p_start; end = p_end; }


    }

    public static class TimeSlotManager
    {
        public static List<TimeSlot> generateTimeSlots(List<TimeSlot> workhours, long minutes)
        {
            List<TimeSlot> timeSlots = new List<TimeSlot>();

            foreach (TimeSlot slot in workhours)
            {
                DateTime start = slot.start;
                DateTime end = slot.end;

                while (start.AddMinutes(minutes) <= end)
                {
                    if (start >= DateTime.Now) // we only add slots in future
                    {
                        timeSlots.Add(new TimeSlot(start, start.AddMinutes(minutes)));
                    }
                    start = start.AddMinutes(30); // We can change this value to get interval from settings
                }
            }

            return timeSlots;
        }

        public static bool isTimeBetween(DateTime p_Time, DateTime p_StartTime, DateTime p_EndTime)
        {
            return (p_Time >= p_StartTime && p_Time <= p_EndTime);
        }

        public static List<TimeSlot> removeOccupiedAppointments(List<TimeSlot> availableAppointments, TimeSlot occupiedAppointment)
        {
            List<TimeSlot> retval = new List<TimeSlot>(availableAppointments);
            foreach (TimeSlot appointment in availableAppointments)
            {
                if (occupiedAppointment.start <= appointment.start && occupiedAppointment.end > appointment.start)
                {
                    retval.Remove(appointment);
                    continue;
                }

                if (occupiedAppointment.start > appointment.start && occupiedAppointment.start < appointment.end)
                {
                    retval.Remove(appointment);
                }
            }
            return retval;
        }
    }

}
