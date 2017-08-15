using System;
using System.Configuration;
using System.DirectoryServices;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace OTDServer
{
    internal static class Program
    {
        private static void Main()
        {
            Console.Title = "OTDServer";
            Task l = Listen();
            l.Wait();
        }

        private static async Task Listen()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(ConfigurationManager.AppSettings["otd:APIAddress"]);
            listener.Start();

            while(true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerRequest inputRequest = context.Request;
                HttpListenerResponse otdResponse = context.Response;

                string inputRequestBody = ReadRequestBody(inputRequest);

                if(isValidADLogon(inputRequestBody))
                {
                    string logon = inputRequestBody;
                    try
                    {
                        SearchResult userAccount = RetrieveUserInfo(logon);

                        //Check if user account exists, contains needed information and request was sent from user's workstation
                        if(userAccount != null && isContainsNeededAttributes(userAccount) && userAccount.Properties["networkAddress"][0].ToString() == IP4.GetIP4Address(inputRequest))
                        {
                            OpenTheDoor(userAccount);
                            otdResponse.StatusCode = (int)HttpStatusCode.OK;
                        }
                        else
                        {
                            otdResponse.StatusCode = (int)HttpStatusCode.Forbidden;
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else
                {
                    otdResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                }
                otdResponse.Close();
            }
        }

        private static SearchResult RetrieveUserInfo(string logon)
        {

            DirectoryEntry currentDomain = new DirectoryEntry();
            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            DirectorySearcher search = new DirectorySearcher(currentDomain);
            search.Filter = ("userPrincipalName=" + logon + "@" + domainName);
            foreach(string property in new string[] { "networkAddress", "physicalDeliveryOfficeName" })
                search.PropertiesToLoad.Add(property);
            SearchResult result = search.FindOne();
            return result;
        }

        private static void OpenTheDoor(SearchResult result)
        {
            Uri remoteServerGET = new Uri(FormatURL(result));
            HttpWebRequest outputRequest = WebRequest.CreateHttp(remoteServerGET);
            WebResponse outputRequestResponse = outputRequest.GetResponse();
            outputRequestResponse.Close();
        }

        private static string FormatURL(SearchResult result)
        {
            return ConfigurationManager.AppSettings["otd:remoteServerGET"].Replace("{ROOM}", (string)result.Properties["physicalDeliveryOfficeName"][0]);
        }

        //Check if inputRequestBody is valid Active Directory logon name:
        //it length less or equal 104 characters
        //and doesn't contain next characters: " / \ [ ] : ; | = , + * ? < >
        private static bool isValidADLogon(string inputRequestBody)
        {
            return inputRequestBody.Length <= 104 && inputRequestBody.IndexOfAny("\"/\\[]:;|=,+*?<>".ToCharArray()) == -1;
        }

        private static string ReadRequestBody(HttpListenerRequest inputRequest)
        {
            Stream inputStream = inputRequest.InputStream;
            Encoding encoding = inputRequest.ContentEncoding;
            StreamReader reader = new StreamReader(inputStream, encoding);
            return reader.ReadToEnd();
        }

        private static bool isContainsNeededAttributes (SearchResult result)
        {
            return result.Properties ["physicalDeliveryOfficeName"].Count > 0 && result.Properties ["networkAddress"].Count > 0;
        }
    }
}