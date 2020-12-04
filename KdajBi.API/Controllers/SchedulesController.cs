using KdajBi.Core;
using KdajBi.Core.dtoModels;
using KdajBi.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace KdajBi.API.Controllers
{
    [Authorize(Roles = "Super, Admin")]
    [ApiController]
    public class SchedulesController : _BaseController
    {


        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public SchedulesController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<SchedulesController> logger, IEmailSender emailSender)
            : base(context, userManager, signInManager, logger, emailSender)
        {
            
        }


        //[HttpPost]
        //[Route("/api/Schedulestable/{id}")]
        //public JsonResult SchedulesTable(long id, [FromBody] DataTableAjaxPostModel param)
        //{
        //    int recordsTotal = 0;
        //    if (ScheduleIsMine(id) == false) { return Json(""); }
        //    var v = from a in _context.Schedules select a;
        //    v = v.Where(w => w.Id == id);
        //    //SORT
        //    if (!(string.IsNullOrEmpty(param.columns[param.order[0].column].data) && string.IsNullOrEmpty(param.order[0].dir)))
        //    {
        //        v = v.OrderBy(param.columns[param.order[0].column].data + " " + param.order[0].dir);
        //    }

        //    recordsTotal = v.Count();
        //    var data = v.Skip(param.start).Take(param.length).ToListAsync();

        //    return Json(new { draw = param.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        //}


        // GET: api/Schedules/5
        [HttpGet("/api/Schedule/{id}")]
        public async Task<ActionResult<dtoUser>> GetSchedule(long id)
        {
            if (ScheduleIsMine(id) == false) { return NotFound(); }
            Schedule mySchedule = new Schedule();
            var Schedule = await _context.Schedules.FindAsync(id);

            if (Schedule == null)
            {
                return NotFound();
            }
            mySchedule.Id = Schedule.Id;
            mySchedule.MondayStart = Schedule.MondayStart;
            mySchedule.MondayEnd = Schedule.MondayEnd;
            mySchedule.TuesdayStart = Schedule.TuesdayStart;
            mySchedule.TuesdayEnd = Schedule.TuesdayEnd;
            mySchedule.WednesdayStart = Schedule.WednesdayStart;
            mySchedule.WednesdayEnd = Schedule.WednesdayEnd;
            mySchedule.ThursdayStart = Schedule.ThursdayStart;
            mySchedule.ThursdayEnd = Schedule.ThursdayEnd;
            mySchedule.FridayStart = Schedule.FridayStart;
            mySchedule.FridayEnd = Schedule.FridayEnd;
            mySchedule.SaturdayStart = Schedule.SaturdayStart;
            mySchedule.SaturdayEnd = Schedule.SaturdayEnd;
            mySchedule.SundayStart = Schedule.SundayStart;
            mySchedule.SundayEnd = Schedule.SundayEnd;
            mySchedule.Active = Schedule.Active;

            return Json(mySchedule);
        }

        // PUT: api/Schedules/5
        [HttpPut("/api/Schedule/{id}")]
        public async Task<IActionResult> PutSchedule(long id, Schedule Schedule)
        {
            if (id != Schedule.Id) { return BadRequest(); }
            if (ScheduleIsMine(id) == false) { return NotFound(); }
            _context.Entry(Schedule).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Schedules
        [HttpPost("/api/Schedule/{id}")]
        public async Task<ActionResult<Schedule>> PostSchedule(long id, Schedule Schedule)
        {
            if (Schedule.Id == 0)
            {
                if (ModelState.IsValid)
                {
                    Schedule.CreatedDate = DateTime.Now;
                    Schedule.CreatedUserID = _CurrentUserID();
                    _context.Schedules.Add(Schedule);
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
                if (ScheduleIsMine(id) == false) { return NotFound(); }
                var Scheduleindb = _context.Schedules.Single(c => c.Id == Schedule.Id);

                Scheduleindb.UpdatedUserID = _CurrentUserID();
                Scheduleindb.UpdatedDate = DateTime.Now;
                Scheduleindb.Active  = Schedule.Active;
                Scheduleindb.MondayStart = Schedule.MondayStart;
                Scheduleindb.MondayEnd = Schedule.MondayEnd;
                Scheduleindb.TuesdayStart = Schedule.TuesdayStart;
                Scheduleindb.TuesdayEnd = Schedule.TuesdayEnd;
                Scheduleindb.WednesdayStart = Schedule.WednesdayStart;
                Scheduleindb.WednesdayEnd = Schedule.WednesdayEnd;
                Scheduleindb.ThursdayStart = Schedule.ThursdayStart;
                Scheduleindb.ThursdayEnd = Schedule.ThursdayEnd;
                Scheduleindb.FridayStart = Schedule.FridayStart;
                Scheduleindb.FridayEnd = Schedule.FridayEnd;
                Scheduleindb.SaturdayStart = Schedule.SaturdayStart;
                Scheduleindb.SaturdayEnd = Schedule.SaturdayEnd;
                Scheduleindb.SundayStart = Schedule.SundayStart;
                Scheduleindb.SundayEnd = Schedule.SundayEnd;
                var utcDate = Schedule.SundayStart.ToUniversalTime();
                _context.Entry(Scheduleindb).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!ScheduleExists(Schedule.Id))
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

        // DELETE: api/Schedules/5
        [HttpDelete("/api/Schedule/{id}")]
        public async Task<ActionResult<Schedule>> DeleteSchedule(long id)
        {
            if (ScheduleIsMine(id) == false) { return NotFound(); }
            var Schedule = await _context.Schedules.FindAsync(id);
            if (Schedule == null) { return NotFound(); }

            _context.Schedules.Remove(Schedule);
            await _context.SaveChangesAsync();

            return Json("OK");
        }

        private bool ScheduleExists(long id)
        {
            return _context.Schedules.Any(e => e.Id == id);
        }
        private bool ScheduleIsMine(long id)
        {
            return (_context.Locations.Include(s => s.Schedule).Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Schedule.Id == id).Count() == 1);
        }

    }
}
