using System;
using System.Net;

namespace OTDServer
{
    public static class IP4
    {
        public static string GetIP4Address(HttpListenerRequest request)
        {
            string IP4Address = String.Empty;

            foreach(IPAddress IPA in Dns.GetHostAddresses(request.UserHostAddress))
            {
                if(IPA.AddressFamily.ToString() == "InterNetwork")
                {
                    IP4Address = IPA.ToString();
                    break;
                }
            }

            if(IP4Address != String.Empty)
            {
                return IP4Address;
            }

            foreach(IPAddress IPA in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if(IPA.AddressFamily.ToString() == "InterNetwork")
                {
                    IP4Address = IPA.ToString();
                    break;
                }
            }

            return IP4Address;
        }
    }
}