using KdajBi.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KdajBi.GoogleHelper;

namespace KdajBi.Web.ViewModels
{
    public class vmPublicBooking : _BaseViewModel
    {
        public string token;
        public Location Location;
        public string CompanyName;
        public string PublicBooking_Text;
        public string Mobile;
        public long PublicBoookingId;
        public string ErrorMsg;
        public bool EnterClientName;
    }
}
