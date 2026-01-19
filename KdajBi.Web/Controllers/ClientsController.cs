using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.GoogleHelper;
using KdajBi.Web.Services;
using KdajBi.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KdajBi.Web.Controllers
{
    //[Authorize(Roles = "Super,Admin")]
    [Controller]
    public class ClientsController : _BaseController
    {

        public ClientsController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AppUsersController> logger, IEmailSender emailSender, IApiTokenProvider apiTokenProvider)
            : base(context, userManager, signInManager, logger, emailSender, apiTokenProvider)
        {
        }




        [Route("/Clients/List")]
        public async Task<IActionResult> List()
        {
            long defLocId = await DefaultLocationId();
            if (await LocationIsMine(defLocId))
            {
                vmClient myVM = new vmClient();
                myVM.ClientsJson = JsonSerializer.Serialize(_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId == defLocId).OrderBy(o => o.FirstName).ThenBy(o => o.LastName).Select(p => new { value = p.Id, label = p.FullName }).ToList()).Replace(@"\", @"\\");
                myVM.Token = await _GetToken();
                myVM.UserUIShow = await _UserUIShow();
                return View(myVM);
            }
            return NotFound();
        }


        [Route("/Clients")]
        public async Task<IActionResult> Index()
        {
            long defLocId = await DefaultLocationId();
            if (await LocationIsMine(defLocId))
            {
                vmClient myVM = new vmClient();
                myVM.ClientsJson = JsonSerializer.Serialize(_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId == defLocId).OrderBy(o => o.FirstName).ThenBy(o => o.LastName).Select(p => new { value = p.Id, label = (p.FullName + " " + p.Mobile) }).ToList()).Replace(@"\", @"\\");
                var myLocation = _context.Locations.Include(s => s.Schedule).Include(w => w.Workplaces).FirstOrDefault(x => x.Id == defLocId);
                if (myLocation != null)
                {
                    //load google calendars
                    var gt = await _CurrentUserGooToken();
                    if (gt != null)
                    {
                        using (GoogleService service = new GoogleService(User.Identity.Name, gt))
                        {
                            var cals = service.getCalendars().Items;
                            if (cals != null)
                            {
                                for (int i = myLocation.Workplaces.Count - 1; i >= 0; i--)
                                {
                                    var item = myLocation.Workplaces.ElementAt(i);
                                    if (cals.Where(c => c.Id == item.GoogleCalendarID).Count() == 0) { item.GoogleCalendarID = null; }
                                    if (item.GoogleCalendarID != null)
                                    {
                                        myVM.GoogleCalendars.Add(new Tuple<string, string, long>(item.GoogleCalendarID, item.Name, item.Id));
                                    }
                                    else
                                    {
                                        myLocation.Workplaces.Remove(item);
                                    }

                                }
                            }
                            else
                            {
                                //didnt get any google calendars
                                //either user has any
                                //or google error occured
                                _logger.LogInformation("No google calendars for " + User.Identity.Name);
                            }
                        }
                    }
                }
                myVM.Token = await _GetToken();
                myVM.UserUIShow = await _UserUIShow();
                return View(myVM);
            }
            return NotFound();
        }

        [Route("/Clients/Notification")]
        public async Task<IActionResult> Notification()
        {
            long defLocId = await DefaultLocationId();
            if (await LocationIsMine(defLocId))
            {
                vmClient myVM = new vmClient();
                myVM.ClientsJson = JsonSerializer.Serialize(_context.Clients.Where(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId == defLocId  && c.AllowsSMS == true && c.Mobile != "").OrderBy(o => o.FirstName).ThenBy(o => o.LastName).Select(p => new { Id = p.Id, FullName = p.FullName, ct = "#" + String.Join("#", p.ClientTags.Select(t => t.TagId.ToString())) + "#" }).ToList()).Replace(@"\", @"\\");
                myVM.Token = await _GetToken();
                myVM.UserUIShow = await _UserUIShow();
                return View(myVM);
            }
            return NotFound();
        }



        /// <summary>
        /// Exports selected customers to CSV file in Google Contacts format
        /// </summary>
        /// <returns>CSV file download</returns>
        [Route("/Clients/ExportClientsToGoogleContacts")]
        [HttpPost]
        public async Task<IActionResult> ExportClientsToGoogleContacts()
        {
            try
            {
                long defLocId = await DefaultLocationId();
                if (await LocationIsMine(defLocId))
                {
                    // Check if user has permission to view personal data
                    //if (User.IsInRole("User") && GetSetting("SkrijOsebnePodatkeStranke") != "0")
                    //{
                    //    return new HttpStatusCodeResult(403, "Access denied");
                    //}

                    //if (selectedCustomerIds == null || selectedCustomerIds.Length == 0)
                    //{
                    //    return Json(new { success = false, message = "No customers selected" });
                    //}

                    // Fetch selected customers from database
                    var customers = _context.Clients
                    .Where(c => c.CompanyId == _CurrentUserCompanyID() && c.LocationId == defLocId)
                    .ToList();

                // Build CSV content
                var csvContent = BuildGoogleContactsCsv(customers);

                // Convert to UTF-8 bytes
                byte[] fileBytes = Encoding.UTF8.GetBytes(csvContent);
                byte[] preamble = Encoding.UTF8.GetPreamble();
                byte[] finalBytes = new byte[preamble.Length + fileBytes.Length];
                Array.Copy(preamble, finalBytes, preamble.Length);
                Array.Copy(fileBytes, 0, finalBytes, preamble.Length, fileBytes.Length);

                // Return file for download
                return File(finalBytes, "text/csv", "Stranke.csv");
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                // Log error
                return Json(new { success = false, message = "Error exporting customers: " + ex.Message });
            }
        }

        /// <summary>
        /// Builds CSV content in Google Contacts format
        /// </summary>
        private string BuildGoogleContactsCsv(List<Client> customers)
        {
            var sb = new StringBuilder();

            // Header row
            sb.AppendLine(string.Join("\t", new[]
            {
                "Name Prefix", "First Name", "Middle Name", "Last Name", "Name Suffix",
                "Phonetic First Name", "Phonetic Middle Name", "Phonetic Last Name",
                "Nickname", "File As", "E-mail 1 - Label", "E-mail 1 - Value",
                "Phone 1 - Label", "Phone 1 - Value", "Address 1 - Label", "Address 1 - Country",
                "Address 1 - Street", "Address 1 - Extended Address", "Address 1 - City",
                "Address 1 - Region", "Address 1 - Postal Code", "Address 1 - PO Box",
                "Organization Name", "Organization Title", "Organization Department",
                "Birthday", "Event 1 - Label", "Event 1 - Value", "Relation 1 - Label",
                "Relation 1 - Value", "Website 1 - Label", "Website 1 - Value",
                "Custom Field 1 - Label", "Custom Field 1 - Value", "Notes", "Labels"
            }));

            // Data rows
            foreach (var customer in customers)
            {
                var (postalCode, city) = SplitPostalCodeCity(customer.ZipCode);
                var birthday = FormatBirthday(customer.Birthday );

                var row = new[]
                {
                    "", // Name Prefix
                    customer.FirstName,
                    "", // Middle Name
                    customer.LastName,
                    "", // Name Suffix
                    "", // Phonetic First Name
                    "", // Phonetic Middle Name
                    "", // Phonetic Last Name
                    "", // Nickname
                    "", // File As
                    "", // E-mail 1 - Label
                    customer.Email ?? "",
                    "", // Phone 1 - Label
                    customer.Mobile ?? "",
                    "", // Address 1 - Label
                    "", // Address 1 - Country
                    customer.Address ?? "",
                    "", // Address 1 - Extended Address
                    city,
                    "", // Address 1 - Region
                    postalCode,
                    "", // Address 1 - PO Box
                    "", // Organization Name
                    "", // Organization Title
                    "", // Organization Department
                    birthday,
                    "", // Event 1 - Label
                    "", // Event 1 - Value
                    "", // Relation 1 - Label
                    "", // Relation 1 - Value
                    "", // Website 1 - Label
                    "", // Website 1 - Value
                    "", // Custom Field 1 - Label
                    "", // Custom Field 1 - Value
                    EscapeCsvField(customer.Notes  ?? ""),
                    "" // Labels
                };

                sb.AppendLine(string.Join("\t", row));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Splits postal code and city
        /// </summary>
        private (string postalCode, string city) SplitPostalCodeCity(string posta)
        {
            if (string.IsNullOrWhiteSpace(posta))
                return ("", "");

            var trimmedPosta = posta.Trim();
            var spaceIndex = trimmedPosta.IndexOf(' ');

            if (spaceIndex > 0)
            {
                var postalCode = trimmedPosta.Substring(0, spaceIndex).Trim();
                var city = trimmedPosta.Substring(spaceIndex).Trim();
                return (postalCode, city);
            }

            return (trimmedPosta, "");
        }

        /// <summary>
        /// Formats birthday in Google Contacts format (yyyy-MM-dd)
        /// </summary>
        private string FormatBirthday(DateTime? birthday)
        {
            if (birthday.HasValue)
                return birthday.Value.ToString("yyyy-MM-dd");

            return "";
        }

        /// <summary>
        /// Escapes CSV field by wrapping in quotes if necessary
        /// </summary>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // Wrap in quotes if contains special characters
            if (field.Contains("\"") || field.Contains("\t") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }

    }
}
