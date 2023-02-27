using KdajBi.Core;
using KdajBi.Core.dtoModels;
using KdajBi.Core.Models;
using KdajBi.GoogleHelper;
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
    public class WorkplacesController : _BaseController
    {
        //public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public WorkplacesController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<WorkplacesController> logger, IEmailSender emailSender)
            : base(context, userManager, signInManager, logger, emailSender)
        {
            
        }


        [HttpPost("/api/workplacestable/{locationid}")]
        public JsonResult WorkplacesTable(long locationid, [FromBody] DataTableAjaxPostModel param)
        {
            int recordsTotal = 0;

            var v = from a in _context.Workplaces select a;
            v = v.Where(w => w.LocationId == locationid);
            //SORT
            if (!(string.IsNullOrEmpty(param.columns[param.order[0].column].data) && string.IsNullOrEmpty(param.order[0].dir)))
            {
                v = v.OrderBy(param.columns[param.order[0].column].data + " " + param.order[0].dir);
            }

            recordsTotal = v.Count();
            var data = v.Skip(param.start).Take(param.length).ToList();

            //add google clendar names
            var gt = _CurrentUserGooToken();
            if (gt != null)
            {
                using (GoogleService service = new GoogleService(User.Identity.Name, gt))
                {
                    foreach (var gooCalendar in service.getCalendars().Items)
                    {
                        foreach (var item in data)
                        {
                            if (gooCalendar.Id == item.GoogleCalendarID)
                            { item.GoogleCalendarSummary = gooCalendar.Summary; }
                        }
                    }
                }
            }
            
            return Json(new { draw = param.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }


        // GET: api/workplaces/5
        [HttpGet("/api/workplace/{id}")]
        public async Task<ActionResult<dtoUser>> GetWorkplace(long id)
        {
            Workplace myWorkplace = new Workplace();
            var workplace = await _context.Workplaces.FindAsync(id);

            if (workplace == null)
            {
                return NotFound();
            }
            myWorkplace.Id = workplace.Id;
            myWorkplace.GoogleCalendarID = workplace.GoogleCalendarID;
            myWorkplace.Name = workplace.Name;
            myWorkplace.SortPosition = workplace.SortPosition;
            myWorkplace.Active = workplace.Active;

            return Json(myWorkplace);
        }

        // PUT: api/workplaces/5
        [HttpPut("/api/workplace/{id}")]
        public async Task<IActionResult> PutWorkplace(long id, Workplace workplace)
        {
            if (id != workplace.Id)
            {
                return BadRequest();
            }

            _context.Entry(workplace).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorkplaceExists(id))
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

        // POST: api/workplaces
        [HttpPost("/api/workplace/{locationid}")]
        public async Task<ActionResult<Workplace>> PostWorkplace(long locationid, Workplace workplace)
        {
            if (workplace.Id == 0)
            {
                if (ModelState.IsValid)
                {
                    workplace.CreatedDate = DateTime.Now;
                    workplace.CreatedUserID = _CurrentUserID();
                    workplace.LocationId = locationid;
                    _context.Workplaces.Add(workplace);
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

                var workplaceindb = _context.Workplaces.Single(c => c.Id == workplace.Id);

                workplaceindb.UpdatedUserID = _CurrentUserID();
                workplaceindb.UpdatedDate = DateTime.Now;
                workplaceindb.Active = workplace.Active;
                workplaceindb.GoogleCalendarID = workplace.GoogleCalendarID;
                workplaceindb.Name = workplace.Name;
                workplaceindb.SortPosition = workplace.SortPosition;

                _context.Entry(workplaceindb).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!WorkplaceExists(workplace.Id))
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

        // DELETE: api/workplaces/5
        [HttpDelete("/api/workplace/{id}")]
        public async Task<ActionResult<Workplace>> DeleteWorkplace(long id)
        {
            var workplace = await _context.Workplaces.FindAsync(id);
            if (workplace == null)
            {
                return NotFound();
            }

            _context.Workplaces.Remove(workplace);
            await _context.SaveChangesAsync();

            return Json("OK");
        }

        // GET: api/workplaceservices/5
        [HttpGet("/api/workplaceexcludedservices/{wpid}")]
        public async Task<ActionResult<dtoUser>> GetWorkplaceExcludedServices(long wpid)
        {
            var myExServices = _context.WorkplaceExcludedServices.Where(es => es.WorkplaceId == wpid).ToList();

            return Json(myExServices);
        }

        [HttpPost("/api/workplaceexcludedservices/{locationid}/{wpid}")]
        public async Task<ActionResult<Workplace>> PostWorkplaceServices(long locationid, long wpid, string[] p_Ids)
        {
            try
            {
                //remove all exclusions for a workplace
                _context.WorkplaceExcludedServices.RemoveRange(_context.WorkplaceExcludedServices.Where(u =>  u.WorkplaceId== wpid));
                _context.SaveChanges();

                if (p_Ids[0] != "0")
                {
                    foreach (var item in p_Ids)
                    {
                        _context.WorkplaceExcludedServices.Add(new WorkplaceExcludedService(wpid, long.Parse(item.Split('_')[1])));
                    }
                }
                _context.SaveChanges();
            }
            catch (Exception ex)
            {

                throw;
            }
            return Json("OK");

        }
        private bool WorkplaceExists(long id)
        {
            return _context.Workplaces.Any(e => e.Id == id);
        }


    }
}
