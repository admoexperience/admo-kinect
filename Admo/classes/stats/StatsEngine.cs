using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Newtonsoft.Json;

namespace Admo.classes.stats
{
    public class StatsEngine
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Mixpanel _mixpanel;
        private readonly DataCache _dataCache;
        public StatsEngine(DataCache dataCache, Mixpanel mixpanel)
        {
            _mixpanel = mixpanel;
            _dataCache = dataCache;
        }


        public async void Track(StatRequest request)
        {
            if (Config.IsOnline)
            {
               await SendDataToMixpanel(request);
            }
            else
            {
                var json = Utils.ConvertToJson(request);
                _dataCache.InsertData(json);
            }
        }

        //When the unit comes backonline 
        public async void ProcessOfflineCache()
        {
            //Proccess them one request at a time while the unit is online.
            //At worst we should loose one or 2 requests.
            //TODO: Handle failed request and add them back to cache with max retries or something.
            while (_dataCache.GetRowCount() > 0 && Config.IsOnline)
            {
                var statToTrack = _dataCache.PopData();
                var request = JsonConvert.DeserializeObject<StatRequest>(statToTrack);
                await SendDataToMixpanel(request);
            }
        }


        private async Task<string> SendDataToMixpanel(StatRequest request)
        {
            var url = _mixpanel.FormatRequestForTrack(request);
            var httpClient = new HttpClient();

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            // Send the request to the server
            var response = await httpClient.SendAsync(requestMessage);

            // Just as an example I'm turning the response into a string here
            var responseAsString =  response.Content.ReadAsStringAsync().Result;
            return responseAsString;
        }
    }
}
