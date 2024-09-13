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
        public int PublicBooking_MaxDays;
        public bool PublicBooking_AllowCurrentDay;
        public bool PublicBooking_AlertMeWithSMS;
        public string PublicBooking_CSS;
    }
}
