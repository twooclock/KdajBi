using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KdajBi.Web.ViewComponents
{
    public class LocationsComboViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext db;

        public LocationsComboViewComponent(ApplicationDbContext context)
        {
            db = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int CompanyId)
        {
            var items =  GetItems(CompanyId);
            return View(items);
        }
        private IEnumerable<vmLocationsCombo> GetItems(int p_CompanyId)
        {
            string defaultLocation = "0";
            long idLocation = 0;
            var myLocations = db.Locations.Where(x => x.CompanyId == p_CompanyId).ToList();
            List<vmLocationsCombo> retval = new List<vmLocationsCombo>();
            defaultLocation = HttpContext.Request.Cookies[Utils.CookieNames.DefaultLocation];

            if (long.TryParse(defaultLocation, out idLocation) == false) { 
                idLocation = myLocations.First().Id;
                HttpContext.Response.Cookies.Append(Utils.CookieNames.DefaultLocation, myLocations.First().Id.ToString());
            }

            foreach (var item in myLocations)
            {
                retval.Add(new vmLocationsCombo(item.Id, item.Name, (item.Id == idLocation)));
            } 
            
            return retval;
        }
    }
}
