using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.DirectoryServices;
using System.Net;
using System.Net.NetworkInformation;

namespace OTDAPI.Controllers
{
    [Route("")]
    public class OTDController : Controller
    {
        private readonly Settings _settings;

        public OTDController(IOptionsSnapshot<Settings> optionsAccessor)
        {
            _settings = optionsAccessor.Value;
        }

        //GET otd
        [HttpGet]
        public string Get()
        {
            return "OTD is running";
        }

        // POST otd
        [HttpPost]
        public IActionResult Post([FromBody]string value)
        {
            if(isValidADLogon(value))
            {
                string logon = value;
                try
                {
                    SearchResult userAccount = RetrieveUserInfo(logon);
                    if(userAccount != null && userAccount.Properties["networkAddress"][0].ToString() == IP4.GetIP4Address(HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress))
                    {
                        OpenTheDoor(userAccount);
                        return Ok();
                    }
                    else
                        return Forbid();
                }
                catch(Exception e)
                {
                    return StatusCode(424);
                }
            }
            else
                return BadRequest();
        }

        //Check if inputRequestBody is valid Active Directory logon name:
        //it length less or equal 104 characters
        //and doesn't contain next characters: " / \ [ ] : ; | = , + * ? < >
        private static bool isValidADLogon(string inputRequestBody)
        {
            return inputRequestBody.Length <= 104 && inputRequestBody.IndexOfAny("\"/\\[]:;|=,+*?<>".ToCharArray()) == -1;
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
            if(result.Properties["physicalDeliveryOfficeName"].Count > 0 && result.Properties["networkAddress"].Count > 0)
                return result;
            else
                return null;
        }

        private void OpenTheDoor(SearchResult result)
        {
            Uri remoteServerGET = new Uri(FormatURL(result));
            HttpWebRequest outputRequest = WebRequest.CreateHttp(remoteServerGET);
            WebResponse outputRequestResponse = outputRequest.GetResponse();
            outputRequestResponse.Close();
        }

        private string FormatURL(SearchResult result)
        {
            return _settings.RemoteServerGet.Replace("{ROOM}", (string)result.Properties["physicalDeliveryOfficeName"][0]);
        }
    }
}