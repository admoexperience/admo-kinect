using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Admo.classes.lib
{
    class CmsAccountApi 
    {

        //CmsApi.BaseUri+
        public const String AccountsUri = CmsApi.BaseUri+"/account";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public String Email {get; set;}
        public String Password { get; set; }


        public async Task<String> RegisterDevice(String name)
        {
            var httpClient = new HttpClient();

            var dict = new Dictionary<String, String>
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

        public async Task<String> RegisterDevice()
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
