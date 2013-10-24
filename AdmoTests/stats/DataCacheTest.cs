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
            db.CreateDataCache();
            Assert.IsTrue(db.Exists());
        }

        [TestMethod]
        public void TestInsertData()
        {
            var db = CreateDb();
            db.CreateDataCache();

            Assert.AreEqual(0,db.GetRowCount());
            db.InsertData("datatesting");
            Assert.AreEqual(0, db.GetRowCount());

        }

        [TestMethod]
        public void GetFirstValue()
        {
            var db = CreateDb();
            db.CreateDataCache();
            db.InsertData("1");
            db.InsertData("2");
            db.InsertData("3");
            var data = db.PopData();
           // Assert.AreEqual("1",data);


        }

        private static DataCache CreateDb()
        {
            var tempPath = Path.GetTempFileName();
            var db = new DataCache(tempPath);
            return db;
        }
    }
}
