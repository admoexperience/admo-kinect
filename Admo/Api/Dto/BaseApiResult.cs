using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admo.Api.Dto
{
    public class BaseApiResult
    {
        public string Error { set; get; }

        public bool ContainsErrors()
        {
            return !string.IsNullOrEmpty(Error);
        }
    }
}
