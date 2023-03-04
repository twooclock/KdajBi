using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.Web.Services;
using KdajBi.Web.ViewModels;
using KdajBi.GoogleHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;
using Newtonsoft.Json;
using System;

namespace KdajBi.Web.Controllers
{
    [Controller]
    public class PublicBookingController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected readonly ILogger<_BaseController> _logger;
        public PublicBookingController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
        {
            _context = context;
            _logger = logger;
        }

        [AllowAnonymous]
        [Route("/narocanje/{token}")]
        [Route("/book/{token}")]
        public IActionResult Index(string token)
        {
            var bookinglocation = _context.Locations.Where(l => l.PublicBookingToken == token).FirstOrDefault();
            vmPublicBooking vm = new vmPublicBooking();
            vm.token = token;
            vm.Location = bookinglocation;
            if (bookinglocation != null)
            { vm.CompanyName = _context.Companies.Find(bookinglocation.CompanyId).Name; }
            else
            {
                vm.ErrorMsg = "Naslov ne obstaja";
                return View("~/Views/Book/Error.cshtml", vm);
            }
            vm.Mobile = null;
            vm.PublicBooking_Text = SettingsHelper.getSetting(_context, bookinglocation.CompanyId, bookinglocation.Id, "PublicBooking_Text", "");
            return View("~/Views/Book/Auth.cshtml", vm);
        }

        [AllowAnonymous]
        [Route("/narocanje/auth/mobile")]
        [Route("/book/auth/mobile")]
        [HttpPost]
        public IActionResult mobile(string token, string inputMobile, string inputClientFirstName, string inputClientLastName, string inputPIN, string pbid)
        {
            var bookinglocation = _context.Locations.Include(l=>l.Workplaces).Where(l => l.PublicBookingToken == token).FirstOrDefault();
            
            vmPublicBooking vm = new vmPublicBooking();
            vm.token = token;
            vm.Location = bookinglocation;
            if (bookinglocation != null)
            {
                var myCompany = _context.Companies.Find(bookinglocation.CompanyId);
                vm.CompanyName = myCompany.Name;
                _context.Entry(myCompany).State = EntityState.Detached;
            }
            else
            {
                vm.ErrorMsg = "Naslov ne obstaja";
                return View("~/Views/Book/Error.cshtml", vm);
            }
            vm.Mobile = inputMobile;
            vm.PublicBooking_Text = SettingsHelper.getSetting(_context, bookinglocation.CompanyId, bookinglocation.Id, "PublicBooking_Text", "");


            if (inputMobile != null && inputPIN == null)
            {
                var newbooking = new PublicBooking();

                if (bookinglocation != null)
                {
                    //allow only 3 bookings from one mobile number a day
                    if (_context.PublicBookings.Where(pb => pb.Token == token && pb.Mobile == inputMobile && pb.CreatedDate.Value.Date == DateTime.Now.Date).Count() < 3)
                    {
                        //allow only slovenian mobile numbers
                        if (Utils.siMobilePrefixes.Any(x => inputMobile.StartsWith(x)))
                        {
                            //create public booking 
                            newbooking.Token = token;
                            newbooking.Mobile = inputMobile;
                            Random _rdm = new Random();
                            int myPIN = _rdm.Next(1000, 9999);
                            newbooking.PIN = myPIN;
                            newbooking.LocationId = bookinglocation.Id;

                            var myClient = _context.Clients.Where(c => c.CompanyId == bookinglocation.CompanyId && c.Mobile.EndsWith(inputMobile.Substring(1))).FirstOrDefault();
                            if (myClient != null)
                            {
                                newbooking.ClientId = myClient.Id;
                            }
                            else
                            { vm.EnterClientName = true; }

                            _context.PublicBookings.Add(newbooking);
                            _context.SaveChanges();

                            // obvesti stranko prek sms (TODO: naredi prek service)
                            SmsCampaign newSmsCampaign = new SmsCampaign();
                            newSmsCampaign.Company.Id = bookinglocation.CompanyId;
                            newSmsCampaign.LocationId = bookinglocation.Id;
                            newSmsCampaign.AppUser.Id = _context.Users.AsNoTracking().Where(u => u.CompanyId == bookinglocation.CompanyId).First().Id;

                            newSmsCampaign.MsgTxt = @"PIN za prijavo je " + myPIN.ToString() + ". Lep pozdrav! ";
                            if (string.IsNullOrEmpty(bookinglocation.Tel) == false)
                            { newSmsCampaign.MsgTxt += Environment.NewLine + "Za več informacij nas pokličite na " + bookinglocation.Tel; }
                            var mySmsInfo = new SmsCounter(newSmsCampaign.MsgTxt);

                            newSmsCampaign.MsgSegments = mySmsInfo.Messages;
                            newSmsCampaign.Name = "PublicBookingAuthorization";
                            newSmsCampaign.Recipients.Add(new SmsMsg(inputMobile, (newbooking.ClientId.HasValue ? newbooking.ClientId.Value : 0)));

                            newSmsCampaign.SendAfter = DateTime.Now;
                            newSmsCampaign.ApprovedAt = DateTime.Now;


                            _context.Attach(newSmsCampaign.Company);
                            _context.Attach(newSmsCampaign.AppUser);
                            _context.SmsCampaigns.Add(newSmsCampaign);
                            try
                            {
                                _context.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "/api/publicbooking-confirmation");
                                throw;
                            }
                        }
                        else
                        {
                            vm.ErrorMsg = "Neveljavna mobilna številka? Pokličte nas.";
                            return View("~/Views/Book/Error.cshtml", vm);
                        }
                    }
                    else
                    {
                        vm.ErrorMsg = "Iz varnostnih razlogov so dovoljene le tri prijave na dan. Hvala za razumevanje, pokličite nas ali poskusite spet jutri!";
                        return View("~/Views/Book/Error.cshtml", vm);
                    }
                }
                
                vm.PublicBoookingId = newbooking.Id;

                return View("~/Views/Book/Auth.cshtml", vm);
            }
            else
            {
                //check pin
                var myPB = _context.PublicBookings.Find(long.Parse(pbid));
                if (myPB != null)
                {
                    if (string.IsNullOrEmpty(myPB.GCalId) == false) {
                        //this public booking has already been used
                        return Index(token);
                    }
                    if (inputPIN == myPB.PIN.ToString())
                    {
                        long clientid = 0;
                        //ok mobile is authorized
                        if (string.IsNullOrEmpty(inputClientFirstName)==false || string.IsNullOrEmpty(inputClientLastName) == false)
                        {
                            try
                            {
                                //store client
                                Client myClient = new Client();
                                myClient.LastName = (string.IsNullOrEmpty(inputClientLastName) ? "" : inputClientLastName);
                                myClient.FirstName = (string.IsNullOrEmpty(inputClientFirstName) ? "" : inputClientFirstName);
                                myClient.Mobile = inputMobile;
                                myClient.CompanyId = bookinglocation.CompanyId;
                                myClient.LocationId = bookinglocation.Id;
                                myClient.Sex = "F"; //mandatory - default is female
                                _context.Clients.Add(myClient);
                                _context.SaveChanges();
                                clientid = myClient.Id;
                            }
                            catch (Exception ex)
                            {

                                //ignore as its not needed...
                            }
                        }
                        myPB.Authorized = DateTime.Now;
                        if (clientid > 0) { myPB.ClientId = clientid; }
                        _context.SaveChanges();
                        vm.PublicBoookingId = myPB.Id;

                        //remove any workplaces that have no services
                        var locationservices = _context.Services.Where(s => s.LocationId == bookinglocation.Id && s.Active == true).ToList();
                        for (int i = vm.Location.Workplaces.Count-1; i >= 0; i--)
                        {
                            var wp = vm.Location.Workplaces.ElementAt(i);
                            var wpExServices = _context.WorkplaceExcludedServices.Where(w => w.WorkplaceId == wp.Id).ToList();
                            if (wpExServices.Count>0)
                            { 
                                var wpServices = locationservices.Where(p => wpExServices.All(p2 => p2.ServiceId != p.Id)).ToList();
                                if (wpServices.Count == 0)
                                { vm.Location.Workplaces.Remove(wp); }
                            }
                        }
                        

                        return View("~/Views/Book/index.cshtml", vm);
                    }
                    else
                    {
                        //wrong pin
                        vm.ErrorMsg = "Napačen PIN!";
                        return View("~/Views/Book/Error.cshtml", vm);
                    } 
                }
                else {
                    //not found
                    vm.ErrorMsg = "Nekaj je šlo narobe...!";
                    return View("~/Views/Book/Error.cshtml", vm);
                }
            }
        }

        [AllowAnonymous]
        [Route("/booked")]
        public IActionResult Success()
        {
            return View("~/Views/Book/Success.cshtml");
        }

        [AllowAnonymous]
        [Route("/book-error")]
        public IActionResult BookError(string token)
        {
            vmPublicBooking vm = new vmPublicBooking();
            vm.token = token;
            vm.ErrorMsg = "Naročilo ni bilo sprejeto, nekaj je šlo narobe... :-(";
            return View("~/Views/Book/Error.cshtml", vm);
        }
    }
}
