using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KdajBi.Web
{
    
    public static class Utils
    {
        public static class CookieNames
        {
            public const string DefaultLocation = "DefaultLocation";
        }
        public static string GetCookieValueFromResponse(HttpResponse response, string cookieName)
        {
            foreach (var headers in response.Headers)
            {
                if (headers.Key != "Set-Cookie")
                    continue;
                string header = headers.Value;
                if (header.StartsWith($"{cookieName}="))
                {
                    var p1 = header.IndexOf('=');
                    var p2 = header.IndexOf(';');
                    return header.Substring(p1 + 1, p2 - p1 - 1);
                }
            }
            return null;
        }
    }
}
