using KdajBi.Core;
using KdajBi.Core.Models;
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
        private IEnumerable<Location> GetItems(int p_CompanyId)
        {
            IEnumerable<Location> retval = db.Locations.Where(x => x.CompanyId == p_CompanyId).ToList();
            HttpContext.Response.Cookies.Append(Utils.CookieNames.DefaultLocation , retval.First().Id.ToString());
            return retval;
        }
    }
}
