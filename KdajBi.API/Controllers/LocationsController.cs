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
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : _BaseController
    {
        

        public LocationsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender)
            : base(context, userManager, signInManager, logger, emailSender)
        {
            
        }


        [HttpPost]
        [Route("/api/Locationstable")]
        public JsonResult LocationsTable([FromBody] DataTableAjaxPostModel param)
        {
            int recordsTotal = 0;
            //var user = await _userManager.GetUserAsync(HttpContext.User);

            var v = from a in _context.Locations select a;

            //SORT
            if (!(string.IsNullOrEmpty(param.columns[param.order[0].column].data) && string.IsNullOrEmpty(param.order[0].dir)))
            {
                v = v.OrderBy(param.columns[param.order[0].column].data + " " + param.order[0].dir);
            }

            recordsTotal = v.Count();
            var data = v.Skip(param.start).Take(param.length).ToList();

            return Json(new { draw = param.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: api/Locations/
        
        [HttpGet]
        [Route("/api/Locations")]
        public async Task<ActionResult<List<Location>>> GetLocations()
        {
            
            List<Location> Location;
            try
            {
                Location = await _context.Locations.ToListAsync(); ;
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

        // GET: api/Locations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Location>> GetLocation(long id)
        {
            var Location = await _context.Locations.FindAsync(id);

            if (Location == null)
            {
                return NotFound();
            }

            return Location;
        }

        // PUT: api/Locations/5
        [HttpPut("{id}")]
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

        // POST: api/Locations
        [HttpPost]
        [Route("/api/Location")]
        public async Task<ActionResult<Location>> PostLocation(Location Location)
        {
            if (Location.Id == 0)
            {
                Location.CreatedUserID = _CurrentUserID();
                Location.CompanyId = _CurrentUserCompanyID();

                _context.Locations.Add(Location);
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

        // DELETE: api/Locations/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Location>> DeleteLocation(long id)
        {
            var Location = await _context.Locations.FindAsync(id);
            if (Location == null)
            {
                return NotFound();
            }

            _context.Locations.Remove(Location);
            await _context.SaveChangesAsync();

            return Json("OK");
        }

        private bool LocationExists(long id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }
    }
}
