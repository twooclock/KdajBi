using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KdajBi.Web
{


    public static class Utils
    {
        public static string[] siMobilePrefixes = new string[] { "030", "031", "040", "041", "051", "0590", "0591", "0592", "0593", "0596", "0597", "0598", "0599", "064", "0651", "06555", "06556", "06560", "06570", "06571", "06572", "06573", "06574", "06575", "06576", "06577", "06578", "06579", "0658", "068", "06910", "06919", "0695", "0696", "0697", "0698", "0699", "070", "071", "0816", "0817", "08180", "08181", "08182", "08184", "08188", "0820", "08221", "08225", "08227", "08228", "08229", "08280", "08281", "08288", "0833", "0838", "0839" };

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
