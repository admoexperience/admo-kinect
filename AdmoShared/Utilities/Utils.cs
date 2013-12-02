using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AdmoShared.Utilities
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

        public static string ConvertToJson(Object obj)
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

        public static Dictionary<string, object> ParseJson(string result)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
        }

        public static string Sha256(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                var sha = new SHA256Managed();
                var hash = sha.ComputeHash(stream);

                var shaHash = BitConverter.ToString(hash).Replace("-", string.Empty);
                return shaHash.ToLowerInvariant();
            }
        }

        public static string BytesToHuman(long byteCount)
        {
            var suf = new [] {"B", "KB", "MB", "GB", "TB", "PB", "EB"}; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num) + suf[place];
        }

        public static bool CheckifAngleCanChange(double angleChangeTime, double getCurrentTimeInSeconds)
        {
            return ((getCurrentTimeInSeconds - angleChangeTime) > 1);
        }
    }
}
