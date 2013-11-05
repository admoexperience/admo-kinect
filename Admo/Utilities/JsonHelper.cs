using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Admo.Utilities
{
    public class JsonHelper
    {
        public static T ConvertFromApiRequest<T>(String jsonString)
        {
            var objectList = JsonConvert.DeserializeObject<T>(jsonString, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new JsonLowerCaseUnderscoreContractResolver()
            });
            return objectList;
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
}
