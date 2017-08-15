using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System;

namespace OpenTheDoor
{
    static class Program
    {
        private static void Main()
        {
            byte[] requestBody = Encoding.UTF8.GetBytes(Environment.UserName);
            HttpWebRequest otdRequest = WebRequest.CreateHttp(ConfigurationManager.AppSettings["otd:APIAddress"]);
            otdRequest.UserAgent = "OTDClient";
            otdRequest.Method = "POST";
            otdRequest.ContentLength = requestBody.Length;
            Stream dataStream = otdRequest.GetRequestStream();
            dataStream.Write(requestBody, 0, requestBody.Length);
            dataStream.Close();
            WebResponse otdResponse = otdRequest.GetResponse();
            otdResponse.Close();
        }
    }
}