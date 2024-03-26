using KdajBi.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KdajBi.GoogleHelper;

namespace KdajBi.Web.ViewModels
{
    public class vmBooking : _BaseViewModel
    {
        public AppointmentToken token;
        public GoogleAuthToken gt;
        public int PublicBoooking_MaxDays;
        public string PublicBoooking_CSS;
    }
}
