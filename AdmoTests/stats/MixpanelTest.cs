using System;
using System.Collections.Generic;
using Admo.classes.stats;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdmoTests.stats
{
    [TestClass]
    public class MixpanelTest
    {
        private const long DefaultDate = 1321499371;
        //Taken from https://mixpanel.com/docs/api-documentation/importing-events-older-than-31-days
        //As a test vector
        [TestMethod]
        public void TestFormattingOfRequestForTrack()
        {
            var mp = CreateMp();

            var s = CreateStatRequest(DefaultDate);

            var request = mp.FormatRequestForTrack(s);

            Assert.AreEqual("https://api.mixpanel.com/track/?verbose=1&test=1&" +
                "data=eyJldmVudCI6IiRib3JuIiwicHJvcGVydGllcyI6eyJkaXN0aW5jdF9pZCI6NDgxLCJ0aW1lIjoxMzIxNDk5MzcxLCJ0b2tlbiI6IjEzZmUzZGRjODZlYjZmOTBjNGVlN2QwZDQ3NTYzMTUwIn19", request);
        }

        [TestMethod]
        public void TestFormattingOfRequestForImport()
        {
            var mp = CreateMp();

            var s = CreateStatRequest(DefaultDate);


            var request = mp.FormatRequestForImport(s);

            Assert.AreEqual("https://api.mixpanel.com/import/"+
                "?api_key=7a7727f7880dc39463f99475e7cefcf8&verbose=1&test=1&" +
                "data=eyJldmVudCI6IiRib3JuIiwicHJvcGVydGllcyI6eyJkaXN0aW5jdF9pZCI6NDgxLCJ0aW1lIjoxMzIxNDk5MzcxLCJ0b2tlbiI6IjEzZmUzZGRjODZlYjZmOTBjNGVlN2QwZDQ3NTYzMTUwIn19", request);
        }


        [TestMethod]
        public void TestDateConversion()
        {
            //Test vector.
            var mp = CreateMp();
            var date = new DateTime(2011, 11, 17, 3, 9, 31, DateTimeKind.Utc);
            Assert.AreEqual(1321499371, mp.AsEpocTime(date));
        }



        private static Mixpanel CreateMp()
        {
            //Test values from their website
            return new Mixpanel("7a7727f7880dc39463f99475e7cefcf8", "13fe3ddc86eb6f90c4ee7d0d47563150");
        }

        private static StatRequest CreateStatRequest(long time)
        {
            //Example from website
            return new StatRequest
            {
                Event = "$born",
                Properties = new Dictionary<String, Object>
                    {
                        { "distinct_id", 481 }, { "time", time },
                       //Should auto add the token
                    }
            };
        }
    }
}
