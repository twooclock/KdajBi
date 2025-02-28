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
    public class PublicBookingListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext db;

        public PublicBookingListViewComponent(ApplicationDbContext context)
        {
            db = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int CompanyId)
        {
            
            
            return View(
                db.PublicBookings
                    .Include(b => b.Location)
                    .Include(b => b.Client)
                    .Include(b => b.Service)
                    .Include(b=>b.ClientWorkplace)
                    .Where(b => b.Location.CompanyId == CompanyId)
                    .Where(b => b.GCalId != null && b.Active==true && b.Status==0)
                    .ToList()
                );
        }
    }
}
