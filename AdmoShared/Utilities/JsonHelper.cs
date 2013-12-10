using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AdmoShared.Utilities
{
    public class JsonHelper
    {
        private static readonly JsonSerializerSettings LowerCaseUnderscore = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new JsonLowerCaseUnderscoreContractResolver()
        };

        private static readonly JsonSerializerSettings CamelCaseSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        private static readonly JsonSerializerSettings UnderScoreSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            ContractResolver = new JsonUnderscoreLowerCaseContractResolver()
        };

        public static string ConvertToUnderScore(object cont)
        {
            var x = JsonConvert.SerializeObject(cont, Formatting.None, UnderScoreSettings);
            return x;
        }
        //Converts data to json
        public static string ConvertToCamelCase(object cont)
        {
            var x = JsonConvert.SerializeObject(cont, Formatting.None, CamelCaseSettings);
            return x;
        }

        //This method parses a json string in the format {key:{ actualObject}}
        //So this method simply parses it as a dict of those values and takes the first key
        public static T ConvertFromApiRequest<T>(string jsonString)
        {
            //Quick hack to handle errors.
            //All objects should contain the error field to directly parse into it.
            if (jsonString.StartsWith("{\"error\":"))
            {
                return JsonConvert.DeserializeObject<T>(jsonString, LowerCaseUnderscore);
            }

            //TODO: handle errors by returning a new object with the error value contained.
            var objectList = JsonConvert.DeserializeObject<Dictionary<string, T>>(jsonString, LowerCaseUnderscore);



            //No errors return the first found object
            return objectList.First().Value;
        }

        public static T DeserializeObject<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }

    public class JsonLowerCaseUnderscoreContractResolver : DefaultContractResolver
    {
        private readonly Regex _regex = new Regex("(?!(^[A-Z]))([A-Z])");

        protected override string ResolvePropertyName(string propertyName)
        {
            return _regex.Replace(propertyName, "_$2").ToLower();
        }
    }

    public class JsonUnderscoreLowerCaseContractResolver : DefaultContractResolver
    {
     //   private readonly Regex _regex = new Regex("(?!(^[A-Z]))([A-Z])");

        protected override string ResolvePropertyName(string propertyName)
        {
            return Regex.Replace(propertyName, "([A-Z])", " $1", RegexOptions.Compiled).Trim().ToLower().Replace(" ","_");
        }
    }
}