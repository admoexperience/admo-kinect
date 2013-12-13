using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Admo.Api.Dto;
using AdmoShared.Utilities;
using NLog;

namespace Admo.Api
{
    public class CmsApi
    {
        public readonly string BaseUri = "https://cms.admoexperience.com/api/v1";
        public readonly string CmsUrl; 
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _apiKey;

        public CmsApi(string apiKey,string baseUri)
        {
            _apiKey = apiKey;
            BaseUri = baseUri;
            CmsUrl = BaseUri + "/unit/";
        }

        public async void CheckIn()
        {
            try
            {
                Logger.Debug("Checking into the CMS");
                await GetUrlContent(CmsUrl + "checkin.json");
            }
            catch (Exception e)
            {
                //Happens when the unit is offline
                Logger.Warn("Unable to check in", e);
            }
        }

        public async Task<string> GetConfig()
        {
            return await GetUrlContent(CmsUrl + "config.json");
        }

        public async Task<string> PostScreenShot(Image img)
        {
            var httpClient = new HttpClient();
            string url = CmsUrl + "screenshot.json";
            // httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
            httpClient.DefaultRequestHeaders.Add("Api-Key", _apiKey);


            var form = new MultipartFormDataContent();
            var stream = new MemoryStream();
            img.Save(stream, ImageFormat.Jpeg);
            HttpContent c = new ByteArrayContent(stream.ToArray());
            form.Add(c, "screenshot",  "screenshot.jpeg");
            var responseMessage = httpClient.PostAsync(url, form).Result;

            var data = await responseMessage.Content.ReadAsStringAsync();
            return data;
        }

        public async Task<List<PodApp>> GetAppList()
        {
            var json = await GetUrlContent(CmsUrl+"apps.json");
            return JsonHelper.ConvertFromApiRequest<List<PodApp>>(json);
        }

        private async Task<string> GetUrlContent(string url)
        {
            var httpClient = new HttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            // Add our custom headers
            requestMessage.Headers.Add("Api-Key", _apiKey);

            // Send the request to the server
            var response = await httpClient.SendAsync(requestMessage);

            // Just as an example I'm turning the response into a string here
            var responseAsString = await response.Content.ReadAsStringAsync();
            return responseAsString;
        }

        public async Task<String> RegisterDeviceVersion()
        {
            var httpClient = new HttpClient();
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            var clientVersion = new ClientVersion
                {
                    Number=version
                };
            httpClient.DefaultRequestHeaders.Add("Api-Key", _apiKey);

            // Send the request to the server
            var response = await httpClient.PostAsync(CmsUrl + "client_version.json", new StringContent(
                Utils.ConvertToJson(clientVersion),
                Encoding.UTF8,
                "application/json"));

            //Just send the username/password with out the device

            var responseAsString = await response.Content.ReadAsStringAsync();
            return responseAsString;
        }

    }
}
