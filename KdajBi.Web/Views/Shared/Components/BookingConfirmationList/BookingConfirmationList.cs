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
                db.BookingConfirmations
                    .Include(b => b.AppointmentToken)
                    .Include(b => b.AppointmentToken.Client)
                    .Where(b => b.AppointmentToken.CompanyId == CompanyId)
                    .Where(b => b.Active == true)
                    .ToList()
                );
        }
    }
}
