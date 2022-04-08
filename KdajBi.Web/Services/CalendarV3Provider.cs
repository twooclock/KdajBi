using KdajBi.GoogleHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace KdajBi.Web.Services
{
    public interface ICalendarV3Provider
    {
        CalendarV3Helper GetHelper();
    }

    public class CalendarV3ProviderSettings
    {
        public string jsonFileName { get; set; } 
    }


    public class CalendarV3Provider : ICalendarV3Provider
    {
        public readonly CalendarV3ProviderSettings _apiSettings; 
        protected readonly ILogger _logger;
        public CalendarV3Provider(IOptions<CalendarV3ProviderSettings> apiSettings, ILogger<ApiTokenProvider> logger)
        {
            _apiSettings = apiSettings.Value;
            _logger = logger;
        }


        CalendarV3Helper ICalendarV3Provider.GetHelper()
        {
            return new CalendarV3Helper(_apiSettings.jsonFileName);
        }
    }
}
