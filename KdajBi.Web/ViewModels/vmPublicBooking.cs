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
        public long PublicBookingId;
        public string ErrorMsg;
        public bool EnterClientName;
        public int PublicBooking_MaxDays;
        public bool PublicBooking_AllowCurrentDay;
        public bool PublicBooking_ShowAnyone;
        public bool PublicBooking_AlertMeWithSMS;
        public bool PublicBooking_AuthorizeAfterSlotSelection;
        public bool PublicBooking_ClientDataIsMandatory;
        public string PublicBooking_TOS;
        public string PublicBooking_CSS;
        public bool PublicBooking_AllowClientNotes;
        public string PublicBooking_ClientNotes;
        public long PublicBooking_ClientWPID;
        public bool PublicBooking_AutoApprove;


        public long wpid;
        public long sid;
        public string date;
        public string timeslot;
    }
}
