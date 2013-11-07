using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Admo.Utilities
{
    public class JsonHelper
    {

        //This method parses a json string in the format {key:{ actualObject}}
        //So this method simply parses it as a dict of those values and takes the first key
        public static T ConvertFromApiRequest<T>(String jsonString)
        {
            //Quick hack to handle errors.
            //All objects should contain the error field to directly parse into it.
            if (jsonString.StartsWith("{\"error\":"))
            {
                return JsonConvert.DeserializeObject<T>(jsonString, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new JsonLowerCaseUnderscoreContractResolver()
                });
            }

            //TODO: handle errors by returning a new object with the error value contained.
            var objectList = JsonConvert.DeserializeObject<Dictionary<String,T>>(jsonString, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new JsonLowerCaseUnderscoreContractResolver()
            });

            

            //No errors return the first found object
            return objectList.First().Value;
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
