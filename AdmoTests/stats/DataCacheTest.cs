using System.IO;
using Admo.classes.stats;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdmoTests.stats
{
    [TestClass]
    public class DataCacheTest
    {
        [TestMethod]
        public void TestExists()
        {
            var db = CreateDb();
            Assert.IsTrue(db.Exists());
        }

        [TestMethod]
        public void TestInsertData()
        {
            var db = CreateDb();

            Assert.AreEqual(0,db.GetRowCount());
            db.InsertData("datatesting");
            Assert.AreEqual(1, db.GetRowCount());

        }

        [TestMethod]
        public void GetFirstValue()
        {
            var db = CreateDb();
            db.InsertData("1");
            db.InsertData("2");
            db.InsertData("3");
            var data = db.PopData();
            Assert.AreEqual("1",data);
            Assert.AreEqual(2, db.GetRowCount());
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
