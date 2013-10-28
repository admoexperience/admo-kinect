using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Admo.classes.stats
{
    public class StatRequest
    {
        public String Event { get; set; }

        public Dictionary<String, object> Properties { get; set; }

        public String AsJson()
        {
            return Utils.ConvertToJson(this);
        }

        public String AsBase64()
        {
            var x = AsJson();
            var endcoded = Encoding.UTF8.GetBytes(x);
            return Convert.ToBase64String(endcoded);
        }
    }
}