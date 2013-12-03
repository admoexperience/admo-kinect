using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdmoShared.Utilities;

namespace Admo.classes.stats
{
    public class Mixpanel
    {
        public readonly string Token;
        public readonly string ApiKey;
        public readonly string UnitName;
        public const string BaseUrl = "https://api.mixpanel.com";
        public const string ImportUri = "/import/";
        public const string TrackUri = "/track/";
        public Boolean Debug = false;

        public Mixpanel(string apiKey, string token, string unitName)
        {
            ApiKey = apiKey;
            Token = token;
            UnitName = unitName;
        }

        public String FormatRequestForTrack(StatRequest request)
        {
            //Embed the trackingtoken
            request.Properties.Add("token",Token);
            //Reference the unit which it was tracked on
            request.Properties.Add("unitName", UnitName);
            var url = BaseUrl + TrackUri + "?"+
                "data=" + request.AsBase64();
            if (Debug)
            {
                url += "&verbose=1&test=1";
            }
            return url;
        }

        public String FormatRequestForImport(StatRequest request)
        {
            //Embed the trackingtoken
            request.Properties.Add("token", Token);
            var url = BaseUrl + ImportUri + "?api_key="+ApiKey+"&" +
                "data=" + request.AsBase64();
            if (Debug)
            {
                url += "&verbose=1&test=1";
            }
            return url;
        }


        public long AsEpocTime(DateTime dateTime)
        {
            return Utils.AsEpochTime(dateTime);
        }
    }
}
