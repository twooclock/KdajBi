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
    [Authorize(Roles = "Super, Admin")]
    [ApiController]
    public class AppUsersController : _BaseController
    {
        

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public AppUsersController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender)
            : base(context, userManager, signInManager, logger, emailSender)
        {
           
        }


        [HttpPost]
        [Route("/api/appuserstable")]
        public JsonResult AppUsersTable([FromBody] DataTableAjaxPostModel param)
        {
            int recordsTotal = 0;

            var v = from a in _context.Users select a;
            v = v.Include(c => c.Company);
            //SORT
            if (!(string.IsNullOrEmpty(param.columns[param.order[0].column].data) && string.IsNullOrEmpty(param.order[0].dir)))
            {
                v = v.OrderBy(param.columns[param.order[0].column].data + " " + param.order[0].dir);
            }

            recordsTotal = v.Count();
            var data = v.Skip(param.start).Take(param.length).ToListAsync();

            return Json(new { draw = param.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }


        // GET: api/AppUsers/5
        [HttpGet("/api/appuser/{id}")]
        public async Task<ActionResult<dtoUser>> GetAppUser(int id)
        {
            dtoUser mydtoUser = new dtoUser();
            var appuser = await _context.Users.FindAsync(id);

            if (appuser == null)
            {
                return NotFound();
            }
            mydtoUser.Id = appuser.Id;
            mydtoUser.Email = appuser.Email;
            mydtoUser.FirstName = appuser.FirstName;
            mydtoUser.LastName = appuser.LastName;
            mydtoUser.Company = new dtoCompany(appuser.CompanyId, _context.Companies.Find(appuser.CompanyId).Name);

            return Json(mydtoUser);
        }

        // PUT: api/AppUsers/5
        [HttpPut("/api/appuser/{id}")]
        public async Task<IActionResult> PutAppUser(int id, AppUser appuser)
        {
            if (id != appuser.Id)
            {
                return BadRequest();
            }

            _context.Entry(appuser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppUserExists(id))
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

        // POST: api/AppUsers
        [HttpPost("/api/appuser")]
        public async Task<ActionResult<AppUser>> PostAppUser(AppUser appuser)
        {
            if (appuser.Id == 0)
            {
                //returnUrl = returnUrl ?? Url.Content("~/");
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
                if (ModelState.IsValid)
                {
                    appuser.CreatedDate = DateTime.Now;
                    appuser.CreatedUserID = _CurrentUserID();
                    var result = await _userManager.CreateAsync(appuser, "1234tomaz");
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");


                        //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        //var callbackUrl = Url.Page(
                        //    "/Account/ConfirmEmail",
                        //    pageHandler: null,
                        //    values: new { area = "Identity", userId = user.Id, code = code },
                        //    protocol: Request.Scheme);

                        //await _emailSender.SendEmailAsync(appuser.Email, "Confirm your email",
                        //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        //if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        //{
                        //    return RedirectToPage("RegisterConfirmation", new { email = appuser.Email });
                        //}
                        //else
                        //{
                        //    //await _signInManager.SignInAsync(user, isPersistent: false);
                        //    //return LocalRedirect(returnUrl);
                        //}
                    }
                    //konec
                    //appuser.CreatedUserID = _CurrentUserID;


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

                var appuserindb = _context.Users.Single(c => c.Id == appuser.Id);

                appuserindb.UpdatedUserID = _CurrentUserID();
                appuserindb.UpdatedDate = DateTime.Now;

                _context.Entry(appuserindb).State = EntityState.Modified;



                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!AppUserExists(appuser.Id))
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

        // DELETE: api/AppUsers/5
        [HttpDelete("/api/appuser/{id}")]
        public async Task<ActionResult<AppUser>> DeleteAppUser(int id)
        {
            var appuser = await _context.Users.FindAsync(id);
            if (appuser == null)
            {
                return NotFound();
            }

            _context.Users.Remove(appuser);
            await _context.SaveChangesAsync();

            return Json("OK");
        }

        private bool AppUserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }



        #region "myUsers"
        [HttpPost]
        [Route("/api/myappuserstable")]
        public async Task<ActionResult<IQueryable<AppUser>>> MyAppUsersTable([FromBody] DataTableAjaxPostModel param)
        {
            int recordsTotal = 0;

            var v = (from a in _context.Users select a).Where(c => c.CompanyId == _CurrentUserCompanyID());
            v = v.Include(c => c.Company);
            //SORT
            if (!(string.IsNullOrEmpty(param.columns[param.order[0].column].data) && string.IsNullOrEmpty(param.order[0].dir)))
            {
                v = v.OrderBy(param.columns[param.order[0].column].data + " " + param.order[0].dir);
            }

            recordsTotal = v.Count();
            var data = v.Skip(param.start).Take(param.length).ToList();

            return Json(new { draw = param.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }


        // GET: api/AppUsers/5
        [HttpGet("/api/myappuser/{id}")]
        public async Task<ActionResult<dtoUser>> GetMyAppUser(int id)
        {
            dtoUser mydtoUser = new dtoUser();
            var appuser = _context.Users.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == id).FirstOrDefault();

            if (appuser == null)
            {
                return NotFound();
            }
            mydtoUser.Id = appuser.Id;
            mydtoUser.Email = appuser.Email;
            mydtoUser.FirstName = appuser.FirstName;
            mydtoUser.LastName = appuser.LastName;
            mydtoUser.Company = new dtoCompany(appuser.CompanyId, _context.Companies.Find(appuser.CompanyId).Name);

            return Json(mydtoUser);
        }

        // PUT: api/AppUsers/5
        [HttpPut("/api/myappuser/{id}")]
        public async Task<IActionResult> PutMyAppUser(int id, AppUser appuser)
        {
            if (id != appuser.Id)
            {
                return BadRequest();
            }
            appuser.CompanyId = _CurrentUserCompanyID();
            appuser.UpdatedDate = DateTime.Now;
            appuser.UpdatedUserID = _CurrentUserID();
            _context.Entry(appuser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppUserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return BadRequest();
        }

        // POST: api/AppUsers
        [HttpPost("/api/myappuser")]
        public async Task<ActionResult<AppUser>> PostMyAppUser(AppUser appuser)
        {
            appuser.CompanyId = _CurrentUserCompanyID();
            if (appuser.Id == 0)
            {
                //returnUrl = returnUrl ?? Url.Content("~/");
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
                if (ModelState.IsValid)
                {
                    appuser.CreatedDate = DateTime.Now;
                    appuser.CreatedUserID = _CurrentUserID();
                    var result = await _userManager.CreateAsync(appuser, "1234tomaz");
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");
                        //create appropriate workplaces
                        //for all locations

                        List<Location> myLocations = _context.Locations.Where(l => l.CompanyId == appuser.CompanyId).ToList();
                        foreach (Location item in myLocations)
                        {

                            Workplace myworkplace = new Workplace
                            {
                                CreatedDate = DateTime.Now,
                                CreatedUserID = _CurrentUserID(),
                                Name = appuser.FirstName,
                                LocationId = item.Id,
                                UserId = appuser.Id
                            };
                            //sortposition?
                            _context.Workplaces.Add(myworkplace);
                        }

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
            }
            else
            {

                var appuserindb = _context.Users.Single(c => c.Id == appuser.Id);

                appuserindb.UpdatedUserID = _CurrentUserID();
                appuserindb.UpdatedDate = DateTime.Now;
                appuserindb.FirstName = appuser.FirstName;
                appuserindb.LastName = appuser.LastName;

                _context.Entry(appuserindb).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!AppUserExists(appuser.Id))
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

        // DELETE: api/AppUsers/5
        [HttpDelete("/api/myappuser/{id}")]
        public async Task<ActionResult<AppUser>> DeleteMyAppUser(int id)
        {
            var appuser =  _context.Users.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.Id == id).FirstOrDefault();
            if (appuser == null)
            {
                return NotFound();
            }

            _context.Users.Remove(appuser);

            //delete workspaces
            _context.Workplaces.RemoveRange(_context.Workplaces.Where(u => u.UserId == id).ToList());

            await _context.SaveChangesAsync();

            return Json("OK");
        }


        #endregion
    }
}
