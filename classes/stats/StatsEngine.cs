using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Admo.classes.stats
{
    public class StatsEngine
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Mixpanel _mixpanel;
        public StatsEngine(Mixpanel mixpanel)
        {
            _mixpanel = mixpanel;
        }


        public async void Track(StatRequest request)
        {
            var url = _mixpanel.FormatRequestForTrack(request);
            var httpClient = new HttpClient();

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            // Send the request to the server
            var response = await httpClient.SendAsync(requestMessage);

            // Just as an example I'm turning the response into a string here
            var responseAsString = await response.Content.ReadAsStringAsync();
            Logger.Debug(responseAsString);
        }
    }
}
