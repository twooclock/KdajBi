using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;

namespace KdajBi
{
    public interface ISmsInfo
    {
        int SmsLimitInfo(string p_ClientID, string p_TaxID);
    }
    public class SmsInfo : ISmsInfo
    {
        private string smsInfoWS = "";
        public SmsInfo(string p_SmsInfoWS)
        {
            smsInfoWS = p_SmsInfoWS;
        }

        public int SmsLimitInfo(string p_ClientID, string p_TaxID)
        {
            int limit = 0;
            int kupljenih = 0;
            //get data from smsInfo web service
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(smsInfoWS);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string myXML = @"<?xml version=""1.0"" encoding=""UTF-8""?>";
                myXML = myXML + @"<ServiceRequest Sender=""" + p_ClientID + @""" Davcna=""" + p_TaxID + @""" RequestID=""" + DateTime.Now.ToString("yyyymmddhhMMss") + @""">";
                myXML = myXML + @"   <ServiceData ServiceName=""vrniStanjeSMS""></ServiceData>";
                myXML = myXML + @"</ServiceRequest>";
                var contentData = new StringContent(myXML, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(smsInfoWS, contentData).Result;
                string jsonstringJWT = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    XmlDocument retval=new XmlDocument();
                    retval.LoadXml(jsonstringJWT);
                    XmlNode myNode = retval.SelectSingleNode("stanjesms/LIMIT_SMS");
                    limit = int.Parse(myNode.InnerText);
                    myNode = retval.SelectSingleNode("stanjesms/KUPLJENIH_SMS");
                    kupljenih = int.Parse(myNode.InnerText);
                    return limit + kupljenih;
                }

                
            }
            catch (Exception ex)
            {
                return -1;
            }
            return 0;
        }
    }
}
