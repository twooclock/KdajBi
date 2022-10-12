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
    [Authorize]
    [ApiController]
    public class AppMessagesController : _BaseController
    {
        public AppMessagesController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender)
            : base(context, userManager, signInManager, logger, emailSender)
        {

        }


        [HttpPost("/api/AppMessagestable")]
        public JsonResult AppMessagesTable([FromBody] DataTableAjaxPostModel param)
        {
            int recordsTotal = 0;
            //var user = await _userManager.GetUserAsync(HttpContext.User);
            var v = from a in _context.UserAppMessages select a;
            v = v.Include(b => b.AppMessage);
            v = v.Where(c => c.AppMessage.ToCompanyId == _CurrentUserCompanyID());
            if (User.IsInRole("Admin") == false)
            { v = v.Where(c => c.UserId == _CurrentUserID()); }
            //SORT
            if (!(string.IsNullOrEmpty(param.columns[param.order[0].column].data) && string.IsNullOrEmpty(param.order[0].dir)))
            {
                v = v.OrderBy(param.columns[param.order[0].column].data + " " + param.order[0].dir);
            }

            recordsTotal = v.Count();
            var data = v.Skip(param.start).Take(param.length).ToList();

            return Json(new { draw = param.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }


        [HttpGet("/api/AppMessages")]
        public async Task<ActionResult<List<UserAppMessage>>> GetAppMessages()
        {
            List<UserAppMessage> AppMessage;
            try
            {
                AppMessage = _context.UserAppMessages
                    .Include(b=>b.AppMessage)
                    .Where(c => c.AppMessage.ToCompanyId == _CurrentUserCompanyID()).OrderBy(t=>t.AppMessage.MessageDate).ToList(); ;
            }
            catch (Exception ex)
            {
                throw;
            }

            if (AppMessage == null)
            {
                return NotFound();
            }

            return AppMessage;
        }

        [HttpGet("/api/AppMessage/{id}")]
        public async Task<ActionResult<AppMessage>> GetAppMessage(long id)
        {
            var AppMessage = await _context.AppMessages.FindAsync(id);

            if (AppMessage == null)
            {
                return NotFound();
            }

            return AppMessage;
        }

        [HttpPut("/api/UserAppMessageRead/{id}")]
        public async Task<IActionResult> Read(long id)
        {
            UserAppMessage am = _context.UserAppMessages.Find(id);
            if (am == null)
            {
                return BadRequest();
            }
            am.Read = true;
            am.DateRead = DateTime.Now;
            am.UpdatedDate = DateTime.Now;
            am.UpdatedUserID = _CurrentUserID();
            _context.Entry(am).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppMessageExists(id))
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

        [HttpPost("/api/AppMessage")]
        public async Task<ActionResult<AppMessage>> PostAppMessage(AppMessage AppMessage)
        {
            if (AppMessage.Id == 0)
            {
                AppMessage.CreatedUserID = _CurrentUserID();
                AppMessage.ToCompanyId = _CurrentUserCompanyID();
                _context.AppMessages.Add(AppMessage);
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

                var AppMessageindb = _context.AppMessages.Single(c => c.Id == AppMessage.Id);

                AppMessageindb.UpdatedUserID = _CurrentUserID();
                AppMessageindb.UpdatedDate = DateTime.Now;
                AppMessage.ToCompanyId = _CurrentUserCompanyID();
                AppMessageindb.Subject = AppMessage.Subject;
                AppMessageindb.Active = AppMessage.Active;

                _context.Entry(AppMessageindb).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppMessageExists(AppMessage.Id))
                    { return NotFound(); }
                    else
                    { throw; }
                }

            }
            return Json("OK");
        }

        [HttpDelete("/api/AppMessage/{id}")]
        public async Task<ActionResult<AppMessage>> DeleteAppMessage(long id)
        {
            if (AppMessageIsMine(id) == false) { return NotFound(); }
            var AppMessage = await _context.AppMessages.FindAsync(id);

            if (AppMessage == null)
            {
                return NotFound();
            }

            _context.AppMessages.Remove(AppMessage);

            await _context.SaveChangesAsync();

            return Json("OK");
        }

        private bool AppMessageExists(long id)
        {
            return _context.AppMessages.Any(e => e.Id == id);
        }
        private bool AppMessageIsMine(long id)
        {
            return _context.AppMessages.Where(c => c.ToCompanyId == _CurrentUserCompanyID() && c.Id == id).Count() == 1;
        }
    }
}
