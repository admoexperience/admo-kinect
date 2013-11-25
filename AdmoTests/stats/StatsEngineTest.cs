using System;
using System.Collections.Generic;
using System.IO;
using Admo.classes;
using Admo.classes.stats;
using Admo.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdmoTests.stats
{
    [TestClass]
    public class StatsEngineTest
    {
        [TestMethod]
        public void TestDataIsAddedToCacheWhenOffline()
        {
            Config.IsOnline = false;
            var datacache = CreateDb();
            var statsEngine = new StatsEngine(datacache, new Mixpanel("na","na","na"));
            var s = new StatRequest
            {
                Event = "test",
                Properties = new Dictionary<string, Object> { { "key", "value" }, { "key2", "value2" } }
            };
            var json = Utils.ConvertToJson(s);
            statsEngine.Track(s);
            Assert.AreEqual(1,datacache.GetRowCount());
            Assert.AreEqual(json, datacache.PopData());
        }

        private static DataCache CreateDb()
        {
            var tempPath = Path.GetTempPath();
            var folder = Path.Combine(tempPath, Path.GetRandomFileName());
            Directory.CreateDirectory(folder);
            var db = new DataCache(folder);
            return db;
        } 
    }

}
