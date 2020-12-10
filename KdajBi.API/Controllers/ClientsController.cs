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
    //[Authorize(Roles = "Super, Admin")]
    [ApiController]
    public class ClientsController : _BaseController
    {

        public ClientsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<ClientsController> logger, IEmailSender emailSender)
            : base(context, userManager, signInManager, logger, emailSender)
        {
            
        }


        [HttpPost]
        [Route("/api/Clientstable/{locationid}")]
        public JsonResult ClientsTable(long locationid, [FromBody] DataTableAjaxPostModel param)
        {
            int recordsTotal = 0;
            
            var v = from a in _context.Clients select a;
            if (locationid != 0)
            { v = v.Where(w => w.CompanyId == _CurrentUserCompanyID() && w.LocationId == locationid); }
            else
            { v = v.Where(w => w.CompanyId == _CurrentUserCompanyID()); }
            //SORT
            if (!(string.IsNullOrEmpty(param.columns[param.order[0].column].data) && string.IsNullOrEmpty(param.order[0].dir)))
            {
                v = v.OrderBy(param.columns[param.order[0].column].data + " " + param.order[0].dir);
            }

            recordsTotal = v.Count();
            var data = v.Skip(param.start).Take(param.length).ToList();

            return Json(new { draw = param.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }


        // GET: api/Clients/5
        [HttpGet("/api/Clients/{locationid}")]
        public async Task<ActionResult<dtoUser>> GetClients(long locationid)
        {
            if (LocationIsMine(locationid) == false) { return NotFound(); }
            var Clients =  _context.Clients.Where(c => c.LocationId == locationid).ToList();

            if (Clients == null) { return NotFound(); }

            return Json(Clients);
        }

        // GET: api/Clients/5
        [HttpGet("/api/Clients/getclientslist/{locationid}")]
        public async Task<ActionResult<dtoUser>> GetClientsList(long locationid)
        {
            if (LocationIsMine(locationid) == false) { return NotFound(); }
            var Clients = _context.Clients.Where(c => c.LocationId == locationid).Select(p => new { value=p.Id, label=p.FullName }).ToList();

            if (Clients == null) { return NotFound(); }

            return Json(Clients);
        }

        // GET: api/Clients/5
        [HttpGet("/api/Client/{id}")]
        public async Task<ActionResult<dtoUser>> GetClient(long id)
        {
            var Client = _context.Clients.Find(id);

            if (Client == null) { return NotFound(); }

            return Json(Client);
        }

        // PUT: api/Clients/5
        [HttpPut("/api/Client/{id}")]
        public async Task<IActionResult> PutClient(long id, Client Client)
        {
            if (id != Client.Id) { return BadRequest(); }
            if (ClientIsMine(id) == false) { return NotFound(); }
            _context.Entry(Client).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(id))
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

        // POST: api/Clients
        [HttpPost("/api/Client")]
        public async Task<ActionResult<Client>> PostClient( Client Client)
        {
            if (Client.Id == 0)
            {
                if (ModelState.IsValid)
                {
                    Client.CreatedDate = DateTime.Now;
                    Client.CreatedUserID = _CurrentUserID();
                    Client.CompanyId = _CurrentUserCompanyID();
                    _context.Clients.Add(Client);
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
                if (ClientIsMine(Client.Id) == false) { return NotFound(); }
                var Clientindb = _context.Clients.Single(c => c.Id == Client.Id);

                Clientindb.UpdatedUserID = _CurrentUserID();
                Clientindb.UpdatedDate = DateTime.Now;
                Clientindb.Active  = Client.Active;
                Clientindb.Address = Client.Address;
                Clientindb.AllowsEmail = Client.AllowsEmail;
                Clientindb.AllowsSMS = Client.AllowsSMS;
                Clientindb.Birthday = Client.Birthday;
                //Clientindb.CompanyId = _CurrentUserCompanyID();
                Clientindb.CompanyName = Client.CompanyName;
                Clientindb.Email = Client.Email;
                Clientindb.FirstName = Client.FirstName;
                Clientindb.IsCompany = Client.IsCompany;
                Clientindb.LastName = Client.LastName;
                Clientindb.LocationId = Client.LocationId;
                Clientindb.Mobile = Client.Mobile;
                Clientindb.Notes = Client.Notes;
                Clientindb.TaxId = Client.TaxId;
                Clientindb.ZipCode = Client.ZipCode;

                _context.Entry(Clientindb).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!ClientExists(Client.Id))
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

        // DELETE: api/Clients/5
        [HttpDelete("/api/Client/{id}")]
        public async Task<ActionResult<Client>> DeleteClient(long id)
        {
            if (ClientIsMine(id) == false) { return NotFound(); }
            var Client = await _context.Clients.FindAsync(id);
            if (Client == null) { return NotFound(); }

            _context.Clients.Remove(Client);
            await _context.SaveChangesAsync();

            return Json("OK");
        }

        private bool ClientExists(long id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
        private bool ClientIsMine(long id)
        {
            return (_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == id).Count() == 1);
        }
        private bool LocationIsMine(long id)
        {
            return (_context.Locations.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == id).Count() == 1);
        }
    }
}
