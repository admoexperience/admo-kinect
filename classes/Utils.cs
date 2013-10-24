using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admo.classes
{
    class Utils
    {
        public static double GetCurrentTimeInSeconds()
        {
            return Convert.ToDouble(DateTime.Now.Ticks) / 10000 / 1000;
        }

        public static long AsEpocTime(DateTime dateTime)
        {
            var span = (dateTime
                        - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            return (long) span.TotalSeconds;
        }
    }
}
