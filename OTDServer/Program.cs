using System;
using System.Configuration;
using System.DirectoryServices;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
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

                //read input request
                string inputRequestBody;
                Stream inputStream = inputRequest.InputStream;
                Encoding encoding = inputRequest.ContentEncoding;
                StreamReader reader = new StreamReader(inputStream, encoding);
                inputRequestBody = reader.ReadToEnd();

                //Check if inputRequestBody is valid Active Directory logon name:
                //it length less or equal 104 characters
                //and doesn't contain next characters: " / \ [ ] : ; | = , + * ? < >
                if(inputRequestBody.Length <= 104 && inputRequestBody.IndexOfAny("\"/\\[]:;|=,+*?<>".ToCharArray()) == -1)
                {
                    try
                    {
                        string logon = inputRequestBody;
                        DirectoryEntry currentDomain = new DirectoryEntry();
                        string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                        DirectorySearcher search = new DirectorySearcher(currentDomain);
                        search.Filter = ("userPrincipalName=" + logon + "@" + domainName);
                        foreach(string property in new string[] { "networkAddress", "physicalDeliveryOfficeName" })
                            search.PropertiesToLoad.Add(property);
                        SearchResult result = search.FindOne();
                        if(result != null)
                        {
                            if(result.Properties["physicalDeliveryOfficeName"].Count > 0 && result.Properties["networkAddress"][0].ToString() == IP4.GetIP4Address(inputRequest))
                            {
                                //make output request
                                Uri remoteServerGET = new Uri(Regex.Replace(ConfigurationManager.AppSettings["otd:remoteServerGET"], "{ROOM}", (string)result.Properties["physicalDeliveryOfficeName"][0]));
                                HttpWebRequest outputRequest = WebRequest.CreateHttp(remoteServerGET);
                                WebResponse outputRequestResponse = outputRequest.GetResponse();
                                outputRequestResponse.Close();

                                //send response
                                otdResponse.StatusCode = (int)HttpStatusCode.OK;
                                otdResponse.Close();
                            }
                            else
                            {
                                otdResponse.StatusCode = (int)HttpStatusCode.Forbidden;
                                otdResponse.Close();
                            }
                        }
                        else
                        {
                            otdResponse.StatusCode = (int)HttpStatusCode.Forbidden;
                            otdResponse.Close();
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
                    otdResponse.Close();
                }
            }
        }
    }
}