using System;
using System.Collections.Generic;
using Admo.classes.stats;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdmoTests.stats
{
    [TestClass]
    public class StatsRequestTest
    {
        [TestMethod]
        public void TestAsJson()
        {
            var s = new StatRequest
                {
                    Event = "test", 
                    Properties = new Dictionary<string, Object> {{"key", "value"},{"key2","value2"}}
                };
            Assert.AreEqual("{\"event\":\"test\","+
                "\"properties\":{\"key\":\"value\",\"key2\":\"value2\"}"+
                "}",s.AsJson());
        }

        [TestMethod]
        public void TestAsBase64()
        {
            //Taken from https://mixpanel.com/docs/api-documentation/importing-events-older-than-31-days
            //As a test vector
            var s = new StatRequest
            {
                Event = "$born",
                Properties = new Dictionary<String, Object>
                    {
                        { "distinct_id", 481 }, { "time", 1321499371 },
                        {"token","13fe3ddc86eb6f90c4ee7d0d47563150"}
                    }
            };
            Assert.AreEqual("eyJldmVudCI6IiRib3JuIiwicHJvcGVydGllcyI6eyJkaXN0aW5jdF9pZCI6NDgxLCJ0aW1lIjoxMzIxNDk5MzcxLCJ0b2tlbiI6IjEzZmUzZGRjODZlYjZmOTBjNGVlN2QwZDQ3NTYzMTUwIn19", s.AsBase64());
        }
    }
}
