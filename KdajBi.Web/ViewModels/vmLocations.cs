using KdajBi.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KdajBi.Web.ViewModels
{
    public class vmLocations : _BaseViewModel
    {
        public List<Location> Locations;
    }

    public class vmLocation : _BaseViewModel
    {
        public Location Location;
        public Dictionary<string,string> GoogleCalendars = new Dictionary<string, string>();
        public string calWEvents;
    }
}
