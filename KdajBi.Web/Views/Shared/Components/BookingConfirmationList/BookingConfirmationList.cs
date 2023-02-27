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
    public class BookingConfirmationListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext db;

        public BookingConfirmationListViewComponent(ApplicationDbContext context)
        {
            db = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int CompanyId)
        {
            return View(
                db.AppointmentTokens
                    .Include(b => b.Client)
                    .Where(b => b.CompanyId == CompanyId)
                    .Where(b => b.GCalId!=null && b.Status==0)
                    .ToList()
                );
        }
    }
}
