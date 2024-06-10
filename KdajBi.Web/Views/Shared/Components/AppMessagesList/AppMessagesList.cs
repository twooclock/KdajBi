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
    public class AppMessagesListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext db;

        public AppMessagesListViewComponent(ApplicationDbContext context)
        {
            db = context;
        }

        public IViewComponentResult Invoke(int CompanyId)
        {
            if (User.HasClaim("Nadzornik", bool.TrueString))
            {
                return View(
                    db.UserAppMessages
                        .Include(b => b.AppMessage)
                        .Where(b => b.AppMessage.ToCompanyId == CompanyId)
                        .Where(b => b.Read == false)
                        .ToList()
                    );
            }
            else
            {
                return View(
                    db.UserAppMessages
                        .Include(b => b.AppMessage)
                        .Where(b => b.AppMessage.ToCompanyId == CompanyId)
                        .Where(b => b.Read == false && b.AppMessage.ForAdminOnly == false)
                        .ToList()
                    );
            }

        }
    }
}
