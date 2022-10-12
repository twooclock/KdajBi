using KdajBi.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KdajBi.Web.ViewModels
{
    public class vmClient : _BaseViewModel
    {
        public string ClientsJson;
        public List<Tuple<string, string, long>> GoogleCalendars = new List<Tuple<string, string, long>>(); //new Dictionary<string, string>();

    }


}
