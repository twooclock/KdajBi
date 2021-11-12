using KdajBi.Core;
using KdajBi.Core.dtoModels;
using KdajBi.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace KdajBi.API.Controllers
{
    [Authorize(Roles = "Super, Admin")]
    [ApiController]
    public class WorkplacesSchedulesController : _BaseController
    {
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public WorkplacesSchedulesController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<SchedulesController> logger, IEmailSender emailSender)
            : base(context, userManager, signInManager, logger, emailSender)
        {

        }



        // POST: api/WorplaceSchedule
        [HttpPost("/api/WorplaceSchedule/")]
        public async Task<ActionResult<Schedule>> PostWorplaceSchedule(dtoWorkplaceSchedule wpSchedule)
        {
            if (wpSchedule.ScheduleId == 0)
            {
                if (ModelState.IsValid)
                {
                    if (WorkspaceIsMine(wpSchedule.WorkplaceId) == false) { return NotFound(); }

                    Schedule myNewschedule = new Schedule();
                    myNewschedule.CreatedDate = DateTime.Now;
                    myNewschedule.CreatedUserID = _CurrentUserID();
                    myNewschedule.EventsJson = wpSchedule.calEvents;
                    myNewschedule.Type = (long)wpSchedule.Type;
                    WorkplaceSchedule wps = new WorkplaceSchedule();
                    wps.WorkplaceId = wpSchedule.WorkplaceId;
                    wps.Schedule = myNewschedule;
                    _context.WorkplaceSchedules.Add(wps);
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
            else
            {
                if (ScheduleIsMine(wpSchedule.WorkplaceId, wpSchedule.ScheduleId) == false) { return NotFound(); }
                var Scheduleindb = _context.Schedules.Single(c => c.Id == wpSchedule.ScheduleId);

                Scheduleindb.UpdatedUserID = _CurrentUserID();
                Scheduleindb.UpdatedDate = DateTime.Now;
                Scheduleindb.EventsJson = wpSchedule.calEvents;
                //Scheduleindb.Active = Schedule.Active;
                //Scheduleindb.MondayStart = Schedule.MondayStart;
                //Scheduleindb.MondayEnd = Schedule.MondayEnd;
                //Scheduleindb.TuesdayStart = Schedule.TuesdayStart;
                //Scheduleindb.TuesdayEnd = Schedule.TuesdayEnd;
                //Scheduleindb.WednesdayStart = Schedule.WednesdayStart;
                //Scheduleindb.WednesdayEnd = Schedule.WednesdayEnd;
                //Scheduleindb.ThursdayStart = Schedule.ThursdayStart;
                //Scheduleindb.ThursdayEnd = Schedule.ThursdayEnd;
                //Scheduleindb.FridayStart = Schedule.FridayStart;
                //Scheduleindb.FridayEnd = Schedule.FridayEnd;
                //Scheduleindb.SaturdayStart = Schedule.SaturdayStart;
                //Scheduleindb.SaturdayEnd = Schedule.SaturdayEnd;
                //Scheduleindb.SundayStart = Schedule.SundayStart;
                //Scheduleindb.SundayEnd = Schedule.SundayEnd;
                //var utcDate = Schedule.SundayStart.ToUniversalTime();
                _context.Entry(Scheduleindb).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!ScheduleExists(wpSchedule.ScheduleId))
                    { return NotFound(); }
                    else
                    { throw; }
                }
                catch (Exception ex)
                {
                    throw;
                }

            }
            return Json("OK");

        }


        // POST: api/WorplaceSchedule
        [HttpPost("/api/SaveWorplaceScheduleExceptions/{date}")]
        public async Task<ActionResult<Schedule>> SaveWorplaceScheduleExceptions(string date, dtoWorkplaceSchedule wpSchedule)
        {
            //save schedule Exceptions for a given day
            //delete them first
            if (WorkspaceIsMine(wpSchedule.WorkplaceId))
            {
                _context.Database.ExecuteSqlRaw("DELETE FROM [WorkplaceScheduleExceptions] where WorkplaceId=@WorkplaceId and [Date]=@date", new SqlParameter("@WorkplaceId",wpSchedule.WorkplaceId.ToString()), new SqlParameter("@date",date));
                WorkplaceScheduleException myWSE = new WorkplaceScheduleException();
                myWSE.CreatedDate = DateTime.Now;
                myWSE.CreatedUserID = _CurrentUserID();
                myWSE.WorkplaceId = wpSchedule.WorkplaceId;
                myWSE.Date = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);
                myWSE.EventsJson = wpSchedule.calEvents;
                _context.WorkplaceScheduleExceptions.Add(myWSE);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw;
                }

            }

            return Json("OK");

        }

        // POST: api/WorplaceSchedule
        [HttpPost("/api/ResetWorplaceScheduleExceptions/{workplaceid}/{date}")]
        public async Task<ActionResult<Schedule>> ResetWorplaceScheduleExceptions(long workplaceid, string date)
        {
            //delete schedule Exceptions
            if (WorkspaceIsMine(workplaceid))
            {
                _context.Database.ExecuteSqlRaw("DELETE FROM [WorkplaceScheduleExceptions] where WorkplaceId=@WorkplaceId and [Date]=@date", new SqlParameter("@WorkplaceId", workplaceid.ToString()), new SqlParameter("@date", date));
            }

            return Json("OK");

        }

        // POST: api/WorplaceSchedule
        [HttpPost("/api/GetWorplaceScheduleExceptions/{workplaceid}/{date}")]
        public async Task<ActionResult<Schedule>> GetWorplaceScheduleExceptions(long workplaceid, string date)
        {
            //get schedule Exceptions
            if (WorkspaceIsMine(workplaceid))
            {
                DateTime d = DateTime.ParseExact(date,"yyyyMMdd", CultureInfo.InvariantCulture);
                DateTime firstDayOfMonth = new DateTime(d.Year, d.Month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddTicks(-1);

                List<dynamic> retval = new List<dynamic>();
                var neki = _context.WorkplaceScheduleExceptions.Where(wse => wse.WorkplaceId == workplaceid && (wse.Date>= firstDayOfMonth && wse.Date<= lastDayOfMonth));
                foreach (var item in neki)
                {
                    retval.Add(new { date = item.Date, eventsJson = item.EventsJson });
                }
                return Json(retval);
            }

            return Json("[]");

        }
        private bool ScheduleExists(long id)
        {
            return _context.Schedules.Any(e => e.Id == id);
        }

        private bool ScheduleIsMine(long p_WorkplaceId, long p_ScheduleId)
        {
            //check if workplace belongs to company and schedule belongs to workplace
            long myLocation = _context.Workplaces.Find(p_WorkplaceId).LocationId;
            bool locCompany = (_context.Locations.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == myLocation).Count() == 1);
            bool wpsSchedule = (_context.WorkplaceSchedules.Where(c => c.WorkplaceId == p_WorkplaceId && c.ScheduleId == p_ScheduleId).Count() == 1);
            return (locCompany && wpsSchedule);
        }

        private bool WorkspaceIsMine(long p_WorkplaceId)
        {
            //check if workplace belongs to company and schedule belongs to workplace
            long myLocation = _context.Workplaces.Find(p_WorkplaceId).LocationId;
            bool locCompany = (_context.Locations.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == myLocation).Count() == 1);
            return (locCompany);
        }
    }
}
