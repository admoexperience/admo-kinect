using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admo.Api.Dto
{
    class ClientVersion : BaseApiResult
    {
        public string Number { set; get; }

        public string CreatedAt { set; get; }

        public string LastReportedAt { set; get; }
    }
}
