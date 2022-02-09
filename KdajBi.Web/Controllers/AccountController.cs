﻿using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.Models;
using KdajBi.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace KdajBi.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : _BaseController
    {

        public AccountController(ApplicationDbContext context, UserManager<AppUser> _userManager, SignInManager<AppUser> _signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
            : base(context, _userManager, _signInManager, logger, emailSender, apiTokenProvider)
        {

        }

        [HttpPost("Account/Sendmail/{locationid}")]
        public async Task<IActionResult> SendMail(long locationid, [FromBody] string pMessage)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (LocationIsMine(locationid))
                {
                    return await SendMail(_userManager.GetUserName(User), _CurrentUserCompanyID(), locationid, _emailSender.emailSettings().AdminMail, 0, 0, pMessage);
                }
                else
                { return await SendMail(_userManager.GetUserName(User), _CurrentUserCompanyID(), 0, _emailSender.emailSettings().AdminMail, 0,  0, pMessage); }
            }
            else
            { return await SendMail("unauthenticated@kdajbi.si", 0, 0, _emailSender.emailSettings().AdminMail, 0, 0, pMessage); }
            
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SendMail([FromBody] string pMessage)
        {
            return await SendMail("anonymous@kdajbi.si", 0, 0,_emailSender.emailSettings().AdminMail, 0, 0, pMessage);
        }

        private async Task<IActionResult> SendMail(string fromEmail, long fromCompanyId, long fromLocationId, string toEmail, long toCompanyId, long toLocationId, string p_message)
        {
            bool mailsent = false;
            try
            {
                mailsent = await _emailSender.SendEmailAsync(fromEmail, toEmail, "KdajBi.si ("+fromEmail+")", p_message);
            }
            catch (Exception ex)
            {
                mailsent = false;
                _logger.LogError(ex, "SendMail error sending mail");
                
            }
            ContactMail newMail = new ContactMail();
            newMail.FromEmail = fromEmail;
            newMail.FromCompanyId = fromCompanyId;
            newMail.FromEmail = fromEmail;
            newMail.FromLocationId = fromLocationId;
            newMail.ToEmail = toEmail;
            newMail.ToCompanyId = toCompanyId;
            newMail.ToLocationId = toLocationId;
            newMail.Message = p_message;
            newMail.EmailSent = mailsent;

            _context.ContactMails.Add(newMail);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendMail({0}, {1}, {2}, {3}, {4}, {5}, {6})", fromEmail,  fromCompanyId,  fromLocationId,  toEmail,  toCompanyId,  toLocationId,  p_message);
            }
            return Json("OK");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            //return RedirectToAction("Index", "Home");
            return Redirect("~/LandingPage/index.html");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult GoogleLogin()
        {
            string redirectUrl = Url.Action("GoogleResponse", "Account");
            AuthenticationProperties properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            
            return new ChallengeResult("Google", properties);
        }



        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
            bool canContinue = false;
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction(nameof(Login));
            AppUser appUser = await _userManager.FindByNameAsync(info.Principal.FindFirst(ClaimTypes.Email).Value);

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            if (result != null)
            {
                canContinue = result.Succeeded;
                if (appUser != null && result.Succeeded == false)
                {
                    var identResult = await _userManager.AddLoginAsync(appUser, info);
                    if (identResult != null) 
                    { canContinue = identResult.Succeeded;  }
                    else
                    { canContinue = false;  }
                }
            }
            else { canContinue = false;  }

            if (canContinue==true)
            {
                //returning user
                var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);

                Claim myClaim = new Claim("picture", info.Principal.FindFirst("urn:google:picture").Value);
                Claim existingClaim = claimsPrincipal.Claims.FirstOrDefault(r => r.Type == "picture");
                if (existingClaim == null)
                {
                    //add picture to claims  (not into db)
                    await _userManager.AddClaimAsync(appUser, myClaim);
                }
                else
                { await _userManager.ReplaceClaimAsync(appUser, existingClaim, myClaim); }

                myClaim = new Claim("CompanyId", appUser.CompanyId.ToString());
                existingClaim = claimsPrincipal.Claims.FirstOrDefault(r => r.Type == "CompanyId");
                if (existingClaim == null)
                {
                    //add CompanyId to claims
                    await _userManager.AddClaimAsync(appUser, myClaim);
                }
                else
                { await _userManager.ReplaceClaimAsync(appUser, existingClaim, myClaim); }

                myClaim = new Claim("GooToken", JsonSerializer.Serialize(info.AuthenticationTokens.ToDictionary(x => x.Name, y => y.Value)));
                existingClaim = claimsPrincipal.Claims.FirstOrDefault(r => r.Type == "GooToken");
                if (existingClaim == null)
                {
                    //add Google token to claims
                    await _userManager.AddClaimAsync(appUser, myClaim);
                }
                else
                {
                    await _userManager.ReplaceClaimAsync(appUser, existingClaim, myClaim);
                }

                var authProperties = new AuthenticationProperties { IsPersistent = false };
                await _signInManager.SignInAsync(appUser, authProperties);

                return Redirect("~/Home/Index");
            }
            else
            {
                //first time user --> register
                AppUser user = new AppUser
                {
                    Email = info.Principal.FindFirst(ClaimTypes.Email).Value,
                    UserName = info.Principal.FindFirst(ClaimTypes.Email).Value,
                    FirstName = info.Principal.FindFirst(ClaimTypes.GivenName).Value,
                    LastName = (info.Principal.FindFirst(ClaimTypes.Surname) != null) ? info.Principal.FindFirst(ClaimTypes.Surname).Value : ""
                };


                return Register(user);
            }
        }

        private IActionResult Register(AppUser user)
        {
            return View("Register", user);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(string p_email, string p_firstname, string p_lastname, string p_davcna, string p_naziv, string p_nazivsalona)
        {
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction(nameof(Login));

            AppUser user = new AppUser
            {
                Email = info.Principal.FindFirst(ClaimTypes.Email).Value,
                UserName = info.Principal.FindFirst(ClaimTypes.Email).Value,
                FirstName = p_firstname,
                LastName = p_lastname,
                CreatedDate = DateTime.Now
            };

            Company company = new Company
            {
                Davcna = p_davcna,
                Name = p_naziv != null ? p_naziv.Split('|')[0] : "",
                Active = true
            };



            //create company
            _context.Companies.Add(company);
            _context.SaveChanges();
            user.CompanyId = company.Id;

            IdentityResult identResult = await _userManager.CreateAsync(user);
            if (identResult.Succeeded)
            {
                identResult = await _userManager.AddLoginAsync(user, info);
                if (identResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    var currentUser = await _userManager.FindByNameAsync(user.UserName);

                    await _userManager.AddToRoleAsync(currentUser, "Admin");

                    Location salon = new Location
                    {
                        Name = p_nazivsalona
                    };
                    salon.CompanyId = company.Id;
                    salon.Schedule = new Schedule { };

                    _context.Locations.Add(salon);

                    try
                    {
                        await _context.SaveChangesAsync();
                        //add users workplace
                        Workplace wp = new Workplace
                        {
                            LocationId = salon.Id,
                            UserId = user.Id,
                            Name = user.FirstName
                        };
                        _context.Workplaces.Add(wp);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Register - error");
                        throw;
                    }

                    //add picture to claims (not into database)
                    Claim myClaim = new Claim("picture", "");
                    foreach (Claim c in info.Principal.Claims)
                    {
                        if (c.Type == "urn:google:picture")
                        {
                            myClaim = new Claim("picture", info.Principal.FindFirst("urn:google:picture").Value);

                        }
                    }
                    await _userManager.AddClaimAsync(currentUser, myClaim);
                    //add CompanyId to claims
                    myClaim = new Claim("CompanyId", company.Id.ToString());
                    await _userManager.AddClaimAsync(currentUser, myClaim);

                    var authProperties = new AuthenticationProperties { IsPersistent = false };
                    await _signInManager.SignInAsync(currentUser, authProperties);

                    return Redirect("~/Home/Index");
                }
            }

            //something went wrong
            return AccessDenied();
        }

        public async Task<IActionResult> FlipNadzornik(string secret = "")
        {
            // Get User and a claims-based identity
            var identity = new ClaimsIdentity(User.Identity);

            var existingClaim = identity.FindFirst("Nadzornik");
            if (existingClaim != null)
            {
                identity.RemoveClaim(existingClaim);
                identity.AddClaim(new Claim("Nadzornik", (!bool.Parse(existingClaim.Value)).ToString()));
            }
            else
            {
                identity.AddClaim(new Claim("Nadzornik", true.ToString()));
            }

            var authProperties = new AuthenticationProperties { IsPersistent = false };
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(identity), authProperties);

            return Redirect("~/Home/Index");
        }
    }
}
