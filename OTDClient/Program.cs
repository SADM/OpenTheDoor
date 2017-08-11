using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using static System.Environment;

namespace OpenTheDoor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            byte[] requestBody = Encoding.UTF8.GetBytes(UserName);
            HttpWebRequest otdRequest = HttpWebRequest.CreateHttp(ConfigurationManager.AppSettings["otd:APIAddress"]);
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