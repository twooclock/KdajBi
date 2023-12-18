using KdajBi.Core;
using KdajBi.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace KdajBi.API.Controllers
{
    //[Authorize(Roles = "Super,Admin")]
    [Authorize]
    [ApiController]
    public class ServiceGroupsController : _BaseController
    {
        public ServiceGroupsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender)
            : base(context, userManager, signInManager, logger, emailSender)
        {

        }


        [HttpPost("/api/ServiceGroupstable/{locationid}")]
        public JsonResult ServiceGroupsTable(long locationid, [FromBody] DataTableAjaxPostModel param)
        {
            int recordsTotal = 0;
            //var user = await _userManager.GetUserAsync(HttpContext.User);

            var v = from a in _context.ServiceGroups select a;
            v = v.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId== locationid);
            //SORT
            if (!(string.IsNullOrEmpty(param.columns[param.order[0].column].data) && string.IsNullOrEmpty(param.order[0].dir)))
            {
                v = v.OrderBy(param.columns[param.order[0].column].data + " " + param.order[0].dir);
            }

            recordsTotal = v.Count();
            var data = v.Skip(param.start).Take(param.length).ToList();

            return Json(new { draw = param.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }


        [HttpGet("/api/ServiceGroups/{locationid}")]
        public async Task<ActionResult<List<ServiceGroup>>> GetServiceGroups(long locationid)
        {
            List<ServiceGroup> ServiceGroup=new List<ServiceGroup>();
            try
            {
                if (locationid > 0)
                { ServiceGroup = _context.ServiceGroups.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId == locationid).OrderBy(t => t.Name).ToList(); }
                else
                { ServiceGroup = _context.ServiceGroups.Where(c => c.CompanyId == _CurrentUserCompanyID()).OrderBy(t => t.Name).ToList(); }
            }
            catch (Exception ex)
            {
                throw;
            }

            return ServiceGroup;
        }

        [HttpGet("/api/ServiceGroup/{id}")]
        public async Task<ActionResult<ServiceGroup>> GetServiceGroup(long id)
        {
            var ServiceGroup = await _context.ServiceGroups.FindAsync(id);

            if (ServiceGroup == null)
            {
                return NotFound();
            }

            return ServiceGroup;
        }

        [HttpPut("/api/ServiceGroup/{id}")]
        public async Task<IActionResult> PutService(long id, ServiceGroup ServiceGroup)
        {
            if (id != ServiceGroup.Id)
            {
                return BadRequest();
            }

            _context.Entry(ServiceGroup).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceGroupExists(id))
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

        [HttpPost("/api/ServiceGroup")]
        public async Task<ActionResult<Service>> PostService(ServiceGroup ServiceGroup)
        {
            if (ServiceGroup.Id == 0)
            {
                ServiceGroup.CreatedUserID = _CurrentUserID();
                ServiceGroup.CompanyId = _CurrentUserCompanyID();
                _context.ServiceGroups.Add(ServiceGroup);
                await _context.SaveChangesAsync();

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

                var ServiceGroupindb = _context.ServiceGroups.Single(c => c.Id == ServiceGroup.Id);

                ServiceGroupindb.UpdatedUserID = _CurrentUserID();
                ServiceGroupindb.UpdatedDate = DateTime.Now;
                ServiceGroupindb.CompanyId = _CurrentUserCompanyID();
                ServiceGroupindb.LocationId = ServiceGroup.LocationId;
                ServiceGroupindb.SortPosition = ServiceGroup.SortPosition;
                ServiceGroupindb.Name = ServiceGroup.Name;
                
                ServiceGroupindb.Active = ServiceGroup.Active;

                _context.Entry(ServiceGroupindb).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceGroupExists(ServiceGroup.Id))
                    { return NotFound(); }
                    else
                    { throw; }
                }

            }
            return Json("OK");
        }

        [HttpDelete("/api/ServiceGroup/{id}")]
        public async Task<ActionResult<ServiceGroup>> DeleteService(long id)
        {
            if (ServiceGroupIsMine(id) == false) { return NotFound(); }
            var ServiceGroup = await _context.ServiceGroups.FindAsync(id);

            if (ServiceGroup == null) { return NotFound(); }
            _context.Database.ExecuteSqlRaw("UPDATE Services SET ServiceGroupId = NULL WHERE ServiceGroupId = @p0", id); ;
			_context.ServiceGroups.Remove(ServiceGroup);

            await _context.SaveChangesAsync();

            return Json("OK");
        }

        private bool ServiceGroupExists(long id)
        {
            return _context.ServiceGroups.Any(e => e.Id == id);
        }
        private bool ServiceGroupIsMine(long id)
        {
            return _context.ServiceGroups.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == id).Count() == 1;
        }
    }
}
