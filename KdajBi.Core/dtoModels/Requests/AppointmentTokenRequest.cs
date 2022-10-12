using System;
using System.Linq;
namespace KdajBi.Core.dtoModels.Requests
{
    public class AppointmentTokenRequest
    {
        public AppointmentTokenRequest(string service, long minutes, long companyId, long locationId, long clientId, long appUserId, long workplaceId)
        {
            this.Service = service;
            this.Minutes = minutes;
            this.CompanyId = companyId;
            this.LocationId = locationId;
            this.ClientId = clientId;
            this.AppUserId = appUserId;
            this.WorkplaceId = workplaceId;
        }

        public string Token { get; set; }
        public string Service { get; set; }
        public long Minutes { get; set; }
        public long CompanyId { get; set; }
        public long LocationId { get; set; }
        public long ClientId { get; set; }
        public long AppUserId { get; set; }
        public long WorkplaceId { get; set; }

    }

}