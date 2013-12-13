using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using AdmoShared.Utilities;
using NLog;

namespace Admo.Api
{
    public class CmsAccountApi 
    {

        //CmsApi.BaseUri+
        public readonly string AccountsUri;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string Email {get; set;}
        public string Password { get; set; }

        public CmsAccountApi(string accountUri)
        {
            AccountsUri = accountUri+"/account";
        }

        public async Task<string> RegisterDevice(string name)
        {
            var httpClient = new HttpClient();

            var dict = new Dictionary<string, string>
                {
                    {"email", Email},
                    {"password",Password},
                    {"name",name}
                };

            // Send the request to the server
            var response = await httpClient.PostAsync(AccountsUri + "/register_unit.json",new StringContent(
                Utils.ConvertToJson(dict),
                Encoding.UTF8,
                "application/json"));

            //Just send the username/password with out the device

            var responseAsString = await response.Content.ReadAsStringAsync();
            return responseAsString;
        }

        public async Task<string> RegisterDevice()
        {
            var httpClient = new HttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, AccountsUri + "/register_unit.json");

            // Send the request to the server
            var response = await httpClient.SendAsync(requestMessage);

            // Just as an example I'm turning the response into a string here
            var responseAsString = await response.Content.ReadAsStringAsync();
            return responseAsString;
        }
    }
}
