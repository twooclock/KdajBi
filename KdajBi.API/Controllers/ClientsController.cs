using AutoMapper;
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
        private IMapper _mapper;
        public ClientsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<ClientsController> logger, IEmailSender emailSender, IMapper mapper)
            : base(context, userManager, signInManager, logger, emailSender)
        {
            _mapper = mapper;
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
            
            //search
            if (string.IsNullOrEmpty(param.search.value)==false)
            { v = v.Where(w => w.FirstName.Contains(param.search.value) || w.LastName.Contains(param.search.value));  }

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
        public async Task<ActionResult<Client>> GetClients(long locationid)
        {
            if (LocationIsMine(locationid) == false) { return NotFound(); }
            var Clients =  await _context.Clients.Where(c => c.LocationId == locationid).Include(t => t.ClientTags).ThenInclude(t => t.Tag).ToListAsync();

            if (Clients == null) { return NotFound(); }

            return Json(Clients);
        }

        // GET: api/Clients/5
        [HttpGet("/api/Clients/getclientslist/{locationid}")]
        public async Task<ActionResult<Client>> GetClientsList(long locationid)
        {
            if (LocationIsMine(locationid) == false) { return NotFound(); }
            var Clients = _context.Clients.Where(c => c.LocationId == locationid).OrderBy(o=>o.FirstName).ThenBy(o=>o.LastName).Select(p => new { value=p.Id, label=p.FullName }).ToList();

            if (Clients == null) { return NotFound(); }

            return Json(Clients);
        }

        // GET: api/Clients/5
        [HttpGet("/api/Client/{id}")]
        public async Task<ActionResult<Client>> GetClient(long id)
        {
            var Client = await _context.Clients.Include(t => t.ClientTags).ThenInclude(t => t.Tag).Where(c => c.Id == id).FirstOrDefaultAsync();

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
        public async Task<ActionResult<dtoClient>> PostClient(dtoClient p_Client)
        {
            long retval = 0;
            if (p_Client.Id == 0)
            {
                if (ModelState.IsValid)
                {
                    Client Client = _mapper.Map<dtoClient, Client>(p_Client);

                    Client.CreatedDate = DateTime.Now;
                    Client.CreatedUserID = _CurrentUserID();
                    Client.CompanyId = _CurrentUserCompanyID();
                    _context.Clients.Add(Client);
                    try
                    {
                        await _context.SaveChangesAsync();
                        retval = Client.Id;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
            else
            {
                if (ClientIsMine(p_Client.Id) == false) { return NotFound(); }
                var Clientindb = _context.Clients.Include(ct=>ct.ClientTags).Single(c => c.Id == p_Client.Id);

                Clientindb.UpdatedUserID = _CurrentUserID();
                Clientindb.UpdatedDate = DateTime.Now;
                Clientindb.Address = p_Client.Address;
                Clientindb.AllowsEmail = p_Client.AllowsEmail;
                Clientindb.AllowsSMS = p_Client.AllowsSMS;
                Clientindb.Birthday = p_Client.Birthday;
                //Clientindb.CompanyId = _CurrentUserCompanyID();
                Clientindb.CompanyName = p_Client.CompanyName;
                Clientindb.Email = p_Client.Email;
                Clientindb.FirstName = p_Client.FirstName;
                Clientindb.IsCompany = p_Client.IsCompany;
                Clientindb.LastName = p_Client.LastName;
                Clientindb.LocationId = p_Client.LocationId;
                Clientindb.Mobile = p_Client.Mobile;
                Clientindb.Notes = p_Client.Notes;
                Clientindb.TaxId = p_Client.TaxId;
                Clientindb.ZipCode = p_Client.ZipCode;

                // Update / Insert ClientTags
                foreach (var childModel in Clientindb.ClientTags)
                {
                    var existingChild = p_Client.ClientTags
                        .Where(c => c.TagId == childModel.TagId)
                        .SingleOrDefault();

                    if (existingChild != null)
                    {
                        // Update child
                        continue;
                    }
                    else
                    {
                        // zbriši
                        _context.ClientTags.Remove(childModel);

                    }
                }


                foreach (var child in p_Client.ClientTags)
                {
                    if (Clientindb.ClientTags.All(x => x.TagId != child.TagId))
                    {
                        // This input child doesn't exist in entity.Children -- dodaj
                        if (child.TagId > 0)
                        { Clientindb.ClientTags.Add(new ClientTag(child.ClientId, child.TagId)); }
                        else
                        {
                            Tag newtag = new Tag(_CurrentUserCompanyID(), child.Tag.Title);
                            _context.Tags.Add(newtag);
                            _context.SaveChanges();
                            _context.ClientTags.Add(new ClientTag(child.ClientId, newtag.Id)); }
                        continue;
                    }
                }


                _context.Entry(Clientindb).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    retval = Clientindb.Id;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!ClientExists(p_Client.Id))
                    { return NotFound(); }
                    else
                    { throw; }
                }
                catch (Exception ex)
                {
                    throw;
                }

            }
            return Json(new { result="OK", id=retval });

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
