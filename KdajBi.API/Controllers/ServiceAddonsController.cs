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
using System.Threading.Tasks;

namespace KdajBi.API.Controllers
{
    [Authorize]
    [ApiController]
    public class ServiceAddonsController : _BaseController
    {
        public ServiceAddonsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender)
            : base(context, userManager, signInManager, logger, emailSender)
        {
        }

        [HttpGet("/api/ServiceAddons/{serviceId}")]
        public ActionResult<List<ServiceAddon>> GetServiceAddons(long serviceId)
        {
            var addons = _context.ServiceAddons
                .Where(a => a.ServiceId == serviceId &&
                            _context.Services.Any(s => s.Id == serviceId && s.CompanyId == _CurrentUserCompanyID()))
                .OrderBy(a => a.Name)
                .ToList();
            return addons;
        }

        [HttpGet("/api/ServiceAddon/{id}")]
        public async Task<ActionResult<ServiceAddon>> GetServiceAddon(long id)
        {
            if (!ServiceAddonIsMine(id)) return NotFound();
            var addon = await _context.ServiceAddons.FindAsync(id);
            if (addon == null) return NotFound();
            return addon;
        }

        [HttpPost("/api/ServiceAddon")]
        public async Task<ActionResult<ServiceAddon>> PostServiceAddon(ServiceAddon serviceAddon)
        {
            if (!ServiceIsMine(serviceAddon.ServiceId)) return NotFound();

            if (serviceAddon.Id == 0)
            {
                serviceAddon.CreatedUserID = _CurrentUserID();
                _context.ServiceAddons.Add(serviceAddon);
                await _context.SaveChangesAsync();
            }
            else
            {
                var addonInDb = _context.ServiceAddons.Single(a => a.Id == serviceAddon.Id);

                addonInDb.UpdatedUserID = _CurrentUserID();
                addonInDb.UpdatedDate = DateTime.Now;
                addonInDb.Name = serviceAddon.Name;
                addonInDb.Minutes = serviceAddon.Minutes;
                addonInDb.PriceDescription = serviceAddon.PriceDescription;
                addonInDb.Notes = serviceAddon.Notes;
                addonInDb.Active = serviceAddon.Active;

                _context.Entry(addonInDb).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceAddonExists(serviceAddon.Id)) return NotFound();
                    else throw;
                }
            }
            return Json("OK");
        }

        [HttpDelete("/api/ServiceAddon/{id}")]
        public async Task<ActionResult<ServiceAddon>> DeleteServiceAddon(long id)
        {
            if (!ServiceAddonIsMine(id)) return NotFound();
            var addon = await _context.ServiceAddons.FindAsync(id);
            if (addon == null) return NotFound();
            _context.ServiceAddons.Remove(addon);
            await _context.SaveChangesAsync();
            return Json("OK");
        }

        private bool ServiceAddonExists(long id)
        {
            return _context.ServiceAddons.Any(a => a.Id == id);
        }

        private bool ServiceIsMine(long serviceId)
        {
            return _context.Services.Any(s => s.Id == serviceId && s.CompanyId == _CurrentUserCompanyID());
        }

        private bool ServiceAddonIsMine(long id)
        {
            return (from a in _context.ServiceAddons
                    join s in _context.Services on a.ServiceId equals s.Id
                    where a.Id == id && s.CompanyId == _CurrentUserCompanyID()
                    select a).Any();
        }
    }
}
