using KdajBi.Core;
using KdajBi.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace KdajBi.API.Controllers
{
    [Authorize(Roles = "Super,Admin")]
    [ApiController]
    public class LocationsController : _BaseController
    {
        public LocationsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender)
            : base(context, userManager, signInManager, logger, emailSender)
        {
            
        }


        [HttpPost("/api/Locationstable")]
        public JsonResult LocationsTable([FromBody] DataTableAjaxPostModel param)
        {
            int recordsTotal = 0;
            //var user = await _userManager.GetUserAsync(HttpContext.User);

            var v = from a in _context.Locations select a;
            v = v.Where(c => c.CompanyId == _CurrentUserCompanyID());
            //SORT
            if (!(string.IsNullOrEmpty(param.columns[param.order[0].column].data) && string.IsNullOrEmpty(param.order[0].dir)))
            {
                v = v.OrderBy(param.columns[param.order[0].column].data + " " + param.order[0].dir);
            }

            recordsTotal = v.Count();
            var data = v.Skip(param.start).Take(param.length).ToList();

            return Json(new { draw = param.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        
        [HttpGet("/api/Locations")]
        public async Task<ActionResult<List<Location>>> GetLocations()
        {
            
            List<Location> Location;
            try
            {
                Location =  _context.Locations.Where(c => c.CompanyId == _CurrentUserCompanyID()).ToList(); ;
            }
            catch (Exception ex)
            {
                throw;
            }

            if (Location == null)
            {
                return NotFound();
            }

            return Location;
        }

        [HttpGet("/api/Location/{id}")]
        public async Task<ActionResult<Location>> GetLocation(long id)
        {
            var Location = await _context.Locations.FindAsync(id);

            if (Location == null)
            {
                return NotFound();
            }

            return Location;
        }

        [HttpPut("/api/Location/{id}")]
        public async Task<IActionResult> PutLocation(long id, Location Location)
        {
            if (id != Location.Id)
            {
                return BadRequest();
            }

            _context.Entry(Location).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocationExists(id))
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

        [HttpPost("/api/Location")]
        public async Task<ActionResult<Location>> PostLocation(Location Location)
        {
            if (Location.Id == 0)
            {
                Location.CreatedUserID = _CurrentUserID();
                Location.CompanyId = _CurrentUserCompanyID();
                _context.Locations.Add(Location);
                await _context.SaveChangesAsync();
                //add workplaces for all employees
                List<AppUser> myEmployees = _context.Users.Where(l => l.CompanyId == Location.CompanyId).ToList();
                foreach (AppUser item in myEmployees)
                {

                    Workplace myworkplace = new Workplace
                    {
                        CreatedDate = DateTime.Now,
                        CreatedUserID = _CurrentUserID(),
                        Name = item.FirstName,
                        LocationId = Location.Id,
                        UserId = item.Id
                    };
                    //sortposition?
                    _context.Workplaces.Add(myworkplace);
                }
                
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            else
            {

                var Locationindb = _context.Locations.Single(c=> c.Id==Location.Id);

                Locationindb.UpdatedUserID = _CurrentUserID();
                Locationindb.UpdatedDate = DateTime.Now;
                Locationindb.Name = Location.Name;
                Locationindb.Tel = Location.Tel;
                Locationindb.Active = Location.Active;

                _context.Entry(Locationindb).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationExists(Location.Id))
                    { return NotFound(); }
                    else
                    { throw; }
                }

            }
            return Json("OK");
        }

        [HttpDelete("/api/Location/{id}")]
        public async Task<ActionResult<Location>> DeleteLocation(long id)
        {
            if (LocationIsMine(id))
            {
                return NotFound();
            }
            var Location = await _context.Locations.Include(s=>s.Schedule).Where(l=>l.Id==id).FirstOrDefaultAsync();
            
            if (Location == null)
            {
                return NotFound();
            }

            _context.Workplaces.RemoveRange(_context.Workplaces.Where(w=>w.LocationId==id));
            _context.Schedules.Remove(_context.Schedules.Find(Location.Schedule.Id));
            _context.Locations.Remove(Location);

            await _context.SaveChangesAsync();

            return Json("OK");
        }


        [HttpPost("/api/GetLocationScheduleExceptions/{locationid}/{date}")]
        public async Task<ActionResult<Schedule>> GetLocationScheduleExceptions(long locationid, string date)
        {
            //get schedule Exceptions
            if (LocationIsMine(locationid))
            {
                DateTime d = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);
                DateTime firstDayOfMonth = new DateTime(d.Year, d.Month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddTicks(-1);

                var myLocation = _context.Locations.Include(w => w.Workplaces).FirstOrDefault(x => x.Id == locationid);
                List<dynamic> workplaces = new List<dynamic>();
                foreach (var wp in myLocation.Workplaces)
                {
                    List<dynamic> wpExceptions = new List<dynamic>();
                    var neki = _context.WorkplaceScheduleExceptions.Where(wse => wse.WorkplaceId == wp.Id && (wse.Date >= firstDayOfMonth && wse.Date <= lastDayOfMonth));
                    foreach (var item in neki)
                    {
                        wpExceptions.Add(new { date = item.Date, eventsJson = item.EventsJson });
                    }
                    workplaces.Add(new { id = wp.Id, exceptions = wpExceptions });
                }

                return Json(workplaces);
            }

            return Json("[]");

        }
        private bool LocationExists(long id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }
        private bool LocationIsMine(long id)
        {
            return (_context.Locations.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == id).Count() == 1);
        }
    }
}
