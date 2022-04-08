using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;


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

    public class TimeSlot
    {
        public DateTime start;
        public DateTime end;
        public TimeSlot() { }
        public TimeSlot(DateTime p_start, DateTime p_end) { start = p_start; end = p_end; }


    }

    public static class TimeSlotManager
    {

        public static bool isTimeBetween(DateTime p_Time, DateTime p_StartTime, DateTime p_EndTime)
        {
            return (p_Time >= p_StartTime && p_Time <= p_EndTime);
        }
        /// <summary>
        /// add TimeSlot (event) to List of (available) TimeSlots - occupy TimeSlot
        /// </summary>
        /// <param name="p_List">exsisting List of available TimeSlots</param>
        /// <param name="p_Event"></param>
        /// <returns></returns>
        public static List<TimeSlot> AddEvent(List<TimeSlot> p_List, TimeSlot p_Event)
        {
            List<TimeSlot> retval = new List<TimeSlot>(p_List);
            for (int i = p_List.Count - 1; i >= 0; i--)
            {
                TimeSlot urnik = p_List[i];
                if (urnik.start <= p_Event.start && urnik.end >= p_Event.end)
                {
                    retval.RemoveAt(i);
                    if (urnik.start != p_Event.start) { retval.Add(new TimeSlot(urnik.start, p_Event.start)); }
                    if (urnik.end != p_Event.end) { retval.Add(new TimeSlot(p_Event.end, urnik.end)); }
                }
                else
                {
                    if (p_Event.start < urnik.start && p_Event.end > urnik.end)
                    { retval.RemoveAt(i); }
                    else
                    {
                        if (p_Event.start < urnik.start && isTimeBetween(p_Event.end, urnik.start, urnik.end))
                        {
                            retval[i].start = p_Event.end;
                        }
                        else
                        {
                            if (isTimeBetween(p_Event.start, urnik.start, urnik.end) && p_Event.end > urnik.end)
                            { retval[i].end = p_Event.start; }
                        }
                    }
                }
            }
            return retval;
        }
    }

}
