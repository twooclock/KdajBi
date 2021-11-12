﻿using KdajBi.Core;
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
    public class ServicesController : _BaseController
    {
        public ServicesController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender)
            : base(context, userManager, signInManager, logger, emailSender)
        {

        }


        [HttpPost("/api/Servicestable")]
        public JsonResult ServicesTable([FromBody] DataTableAjaxPostModel param)
        {
            int recordsTotal = 0;
            //var user = await _userManager.GetUserAsync(HttpContext.User);

            var v = from a in _context.Services select a;
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


        [HttpGet("/api/Services")]
        public async Task<ActionResult<List<Service>>> GetServices()
        {
            List<Service> Service;
            try
            {
                Service = _context.Services.Where(c => c.CompanyId == _CurrentUserCompanyID()).OrderBy(t=>t.Name).ToList(); ;
            }
            catch (Exception ex)
            {
                throw;
            }

            if (Service == null)
            {
                return NotFound();
            }

            return Service;
        }

        [HttpGet("/api/Service/{id}")]
        public async Task<ActionResult<Service>> GetService(long id)
        {
            var Service = await _context.Services.FindAsync(id);

            if (Service == null)
            {
                return NotFound();
            }

            return Service;
        }

        [HttpPut("/api/Service/{id}")]
        public async Task<IActionResult> PutService(long id, Service Service)
        {
            if (id != Service.Id)
            {
                return BadRequest();
            }

            _context.Entry(Service).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(id))
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

        [HttpPost("/api/Service")]
        public async Task<ActionResult<Service>> PostService(Service Service)
        {
            if (Service.Id == 0)
            {
                Service.CreatedUserID = _CurrentUserID();
                Service.CompanyId = _CurrentUserCompanyID();
                _context.Services.Add(Service);
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

                var Serviceindb = _context.Services.Single(c => c.Id == Service.Id);

                Serviceindb.UpdatedUserID = _CurrentUserID();
                Serviceindb.UpdatedDate = DateTime.Now;
                Service.CompanyId = _CurrentUserCompanyID();
                Serviceindb.Name = Service.Name;
                Serviceindb.Minutes = Service.Minutes;
                Serviceindb.Active = Service.Active;

                _context.Entry(Serviceindb).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(Service.Id))
                    { return NotFound(); }
                    else
                    { throw; }
                }

            }
            return Json("OK");
        }

        [HttpDelete("/api/Service/{id}")]
        public async Task<ActionResult<Service>> DeleteService(long id)
        {
            if (ServiceIsMine(id) == false) { return NotFound(); }
            var Service = await _context.Services.FindAsync(id);

            if (Service == null)
            {
                return NotFound();
            }

            _context.Services.Remove(Service);

            await _context.SaveChangesAsync();

            return Json("OK");
        }

        private bool ServiceExists(long id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
        private bool ServiceIsMine(long id)
        {
            return _context.Services.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == id).Count() == 1;
        }
    }
}
