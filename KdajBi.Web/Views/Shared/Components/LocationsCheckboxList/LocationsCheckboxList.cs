using KdajBi.Core;
using KdajBi.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KdajBi.Web.ViewComponents
{
    public class LocationsCheckboxListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext db;

        public LocationsCheckboxListViewComponent(ApplicationDbContext context)
        {
            db = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int CompanyId)
        {
            var items = await GetItemsAsync(CompanyId);
            return View(items);
        }
        private Task<List<Location>> GetItemsAsync(int p_CompanyId)
        {
            return db.Locations.Where(x => x.CompanyId== p_CompanyId).ToListAsync();
        }
    }
}
