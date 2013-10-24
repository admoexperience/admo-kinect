﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admo.classes.stats
{
    public class Mixpanel
    {
        public readonly String Token;
        public readonly String ApiKey;
        public const String BaseUrl = "https://api.mixpanel.com";
        public const String ImportUri = "/import/";
        public const String TrackUri = "/track/";

        public Mixpanel(String apiKey, String token)
        {
            ApiKey = apiKey;
            Token = token;
        }

        public String FormatRequestForTrack(StatRequest request)
        {
            //Embed the trackingtoken
            request.Properties.Add("token",Token);
            var url = BaseUrl + TrackUri + "?verbose=1&test=1&"+
                "data=" + request.AsBase64();
            return url;
        }

        public String FormatRequestForImport(StatRequest request)
        {
            //Embed the trackingtoken
            request.Properties.Add("token", Token);
            var url = BaseUrl + ImportUri + "?api_key="+ApiKey+"&verbose=1&test=1&" +
                "data=" + request.AsBase64();
            return url;
        }


        public long AsEpocTime(DateTime dateTime)
        {
            var span = (dateTime
                - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            return (long)span.TotalSeconds;
        }
    }
}
