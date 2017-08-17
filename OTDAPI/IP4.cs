using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OTDAPI
{
    public static class IP4
    {
        public static string GetIP4Address(IPAddress remoteIpAddress)
        {
            string IP4Address = String.Empty;

            foreach(IPAddress IPA in Dns.GetHostAddresses(remoteIpAddress.ToString()))
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
