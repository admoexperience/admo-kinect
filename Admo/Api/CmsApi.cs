using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Admo.Api.Dto;
using AdmoShared.Utilities;
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
                await GetUrlContent(CmsUrl + "checkin.json");
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

        public async Task<List<PodApp>> GetAppList()
        {
            var json = await GetUrlContent(CmsUrl+"apps.json");
            return JsonHelper.ConvertFromApiRequest<List<PodApp>>(json);
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
