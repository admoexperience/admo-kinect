using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admo.Api.Dto
{
    public class Unit : BaseApiResult
    {
        public string ApiKey { set; get; }
        public string Name { set; get; }
        public string CreatedAt { set; get; }
    }
}
