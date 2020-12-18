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
    [Authorize(Roles = "Super,Admin")]
    [ApiController]
    public class TagsController : _BaseController
    {
        public TagsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender)
            : base(context, userManager, signInManager, logger, emailSender)
        {

        }


        [HttpPost("/api/Tagstable")]
        public JsonResult TagsTable([FromBody] DataTableAjaxPostModel param)
        {
            int recordsTotal = 0;
            //var user = await _userManager.GetUserAsync(HttpContext.User);

            var v = from a in _context.Tags select a;
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


        [HttpGet("/api/Tags")]
        public async Task<ActionResult<List<Tag>>> GetTags()
        {
            List<Tag> Tag;
            try
            {
                Tag = _context.Tags.Where(c => c.CompanyId == _CurrentUserCompanyID()).OrderBy(t=>t.Title).ToList(); ;
            }
            catch (Exception ex)
            {
                throw;
            }

            if (Tag == null)
            {
                return NotFound();
            }

            return Tag;
        }

        [HttpGet("/api/Tag/{id}")]
        public async Task<ActionResult<Tag>> GetTag(long id)
        {
            var Tag = await _context.Tags.FindAsync(id);

            if (Tag == null)
            {
                return NotFound();
            }

            return Tag;
        }

        [HttpPut("/api/Tag/{id}")]
        public async Task<IActionResult> PutTag(long id, Tag Tag)
        {
            if (id != Tag.Id)
            {
                return BadRequest();
            }

            _context.Entry(Tag).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(id))
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

        [HttpPost("/api/Tag")]
        public async Task<ActionResult<Tag>> PostTag(Tag Tag)
        {
            if (Tag.Id == 0)
            {
                Tag.CreatedUserID = _CurrentUserID();
                Tag.CompanyId = _CurrentUserCompanyID();
                _context.Tags.Add(Tag);
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

                var Tagindb = _context.Tags.Single(c => c.Id == Tag.Id);

                Tagindb.UpdatedUserID = _CurrentUserID();
                Tagindb.UpdatedDate = DateTime.Now;
                Tag.CompanyId = _CurrentUserCompanyID();
                Tagindb.Title = Tag.Title;
                Tagindb.Active = Tag.Active;

                _context.Entry(Tagindb).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagExists(Tag.Id))
                    { return NotFound(); }
                    else
                    { throw; }
                }

            }
            return Json("OK");
        }

        [HttpDelete("/api/Tag/{id}")]
        public async Task<ActionResult<Tag>> DeleteTag(long id)
        {
            if (TagIsMine(id) == false) { return NotFound(); }
            var Tag = await _context.Tags.FindAsync(id);

            if (Tag == null)
            {
                return NotFound();
            }

            _context.Tags.Remove(Tag);

            await _context.SaveChangesAsync();

            return Json("OK");
        }

        private bool TagExists(long id)
        {
            return _context.Tags.Any(e => e.Id == id);
        }
        private bool TagIsMine(long id)
        {
            return _context.Tags.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == id).Count() == 1;
        }
    }
}
