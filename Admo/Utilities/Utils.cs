using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Admo.Utilities
{
    public class Utils
    {
        public static double GetCurrentTimeInSeconds()
        {
            return Convert.ToDouble(DateTime.Now.Ticks) / 10000 / 1000;
        }

        public static long AsEpochTime(DateTime dateTime)
        {
            var span = (dateTime
                        - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            return (long) span.TotalSeconds;
        }

        public static String ConvertToJson(Object obj)
        {
            return JsonConvert.SerializeObject(obj,
                                       Formatting.None,
                                       new JsonSerializerSettings
                                       {
                                           NullValueHandling = NullValueHandling.Ignore,
                                           Formatting = Formatting.None,
                                           ContractResolver = new CamelCasePropertyNamesContractResolver()
                                       });
        }

        public static Dictionary<String, object> ParseJson(string result)
        {
            return JsonConvert.DeserializeObject<Dictionary<String, object>>(result);
        }
    }
}
