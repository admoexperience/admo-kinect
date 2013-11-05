using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Admo.Api
{
    public class CmsApi
    {
        public const String BaseUri = "https://cms.admoexperience.com/api/v1";
        public const String CmsUrl = BaseUri +"/unit/";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly String _apiKey;

        public CmsApi(String apiKey)
        {
            _apiKey = apiKey;
        }

        public async void CheckIn()
        {
            try
            {
                Logger.Debug("Checking into the CMS");
                // You need to add a reference to System.Net.Http to declare client.
                var httpClient = new HttpClient();
                var requestMessage = new HttpRequestMessage(HttpMethod.Get,
                                                            CmsApi.CmsUrl + "checkin.json");
                // Add our custom headers
                requestMessage.Headers.Add("Api-Key", _apiKey);

                // Send the request to the server
                var response = await httpClient.SendAsync(requestMessage);


                var responseAsString = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                //Happens when the unit is offline
                Logger.Warn("Unable to check in", e);
            }
        }

        public async Task<String> GetConfig()
        {
            return await GetUrlContent(CmsUrl + "config.json");
        }

        public async Task<String> PostScreenShot(Image img)
        {
            var httpClient = new HttpClient();
            const string url = CmsUrl + "screenshot.json";
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

        public async Task<String> GetAppList()
        {
            return await GetUrlContent(CmsUrl+"apps.json");
        }

        private async Task<String> GetUrlContent(String url)
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
    }
}
