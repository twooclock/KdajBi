using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using NLog;

namespace KdajBi
{
    public interface ISmsInfo
    {
        int SmsLimitInfo(string p_CustomerCode, string p_TaxID);
        double SmsPriceInfo(string p_CustomerCode, string p_TaxID);
        string SmsMakeOrder(string p_CustomerCode, string p_TaxID, string p_KontaktGSM, int p_Kolicina, string p_Sporocilo);
    }
    public class SmsInfo : ISmsInfo
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private string smsInfoWS = "";
        public SmsInfo(string p_SmsInfoWS)
        {
            smsInfoWS = p_SmsInfoWS;
        }

        public int SmsLimitInfo(string p_CustomerCode, string p_TaxID)
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
                myXML = myXML + @"<ServiceRequest Sender=""" + p_CustomerCode + @""" Davcna=""" + p_TaxID + @""" RequestID=""" + DateTime.Now.ToString("yyyymmddhhMMss") + @""">";
                myXML = myXML + @"   <ServiceData ServiceName=""vrniStanjeSMS""></ServiceData>";
                myXML = myXML + @"</ServiceRequest>";
                var contentData = new StringContent(myXML, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(smsInfoWS, contentData).Result;
                string serviceResponse = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    XmlDocument retval=new XmlDocument();
                    retval.LoadXml(serviceResponse);
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

        public double SmsPriceInfo(string p_CustomerCode, string p_TaxID)
        {
            double price = -1;
            //get data from smsInfo web service
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(smsInfoWS);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string myXML = @"<?xml version=""1.0"" encoding=""UTF-8""?>";
                myXML = myXML + @"<ServiceRequest Sender=""" + p_CustomerCode + @""" Davcna=""" + p_TaxID + @""" RequestID=""" + DateTime.Now.ToString("yyyymmddhhMMss") + @""">";
                myXML = myXML + @"   <ServiceData ServiceName=""vrniCenoSMS""></ServiceData>";
                myXML = myXML + @"</ServiceRequest>";
                var contentData = new StringContent(myXML, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(smsInfoWS, contentData).Result;
                string serviceResponse = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    XmlDocument retval = new XmlDocument();
                    retval.LoadXml(serviceResponse);
                    XmlNode myNode = retval.SelectSingleNode("cenasms/CENA");
                    //_logger.Info("Price:" + myNode.InnerText.Replace(",","."));
                    price = double.Parse(myNode.InnerText.Replace(",", "."));
                    return price;
                }


            }
            catch (Exception ex)
            {
                return -1;
            }
            return -1;
        }

        public string SmsMakeOrder(string p_CustomerCode, string p_TaxID,string p_KontaktGSM, int p_Kolicina, string p_Sporocilo)
        {
            //get data from smsInfo web service
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(smsInfoWS);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string p_XML = @"<xmlOddajNarociloSMS Aplikacija=""KDAJBI"" CustomerCode=""" + p_CustomerCode + @""" KontaktGSM=""" + p_KontaktGSM + @""" Kolicina=""" + p_Kolicina.ToString() + @""" Sporocilo=""" + p_Sporocilo + @""" />";

                string myXML = @"<?xml version=""1.0"" encoding=""UTF-8""?>";
                myXML = myXML + @"<ServiceRequest Sender=""" + p_CustomerCode + @""" Davcna=""" + p_TaxID + @""" RequestID=""" + DateTime.Now.ToString("yyyymmddhhMMss") + @""">";
                myXML = myXML + @"   <ServiceData ServiceName=""oddajNarociloSMS"">"+p_XML+"</ServiceData>";
                myXML = myXML + @"</ServiceRequest>";
                var contentData = new StringContent(myXML, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(smsInfoWS, contentData).Result;
                string serviceResponse = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return serviceResponse;
                }
            }
            catch (Exception ex)
            {
                return "ERROR";
            }
            return "ERROR";
        }
    }
}
