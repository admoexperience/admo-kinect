using System;
using System.IO;
using Admo.classes;
using Admo.classes.lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AdmoTests
{
    [TestClass]
    public class ConfigTest
    {
        private static string TempPath;

        [TestInitialize]
        public void Startup()
        {
            Config.OverrideBaseConfigPath = null;
            var tempPath = Path.GetTempPath();
            TempPath = Path.Combine(tempPath, Path.GetRandomFileName());
            Directory.CreateDirectory(TempPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Config.OverrideBaseConfigPath = null;
            Directory.Delete(TempPath,true);
        }

        [TestMethod]
        public void DefaultConfigPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var defaultPath = Path.Combine(appData, "Admo");
            Assert.AreEqual(defaultPath, Config.GetBaseConfigPath());
        }

         [TestMethod]
        public void UsesBasePathIfSet(){
            Config.OverrideBaseConfigPath = "c://";
            Assert.AreEqual("c://", Config.GetBaseConfigPath());
        }

        [TestMethod]
        public void LocalOnlyConfigFromFile()
        {
            Assert.AreEqual(false,Config.IsLocalOnly());
            Config.OverrideBaseConfigPath = TempPath;
            var cmsUrlPath = Config.GetLocalConfig("BaseCmsUrl");
            File.WriteAllText(cmsUrlPath,@"local");
            Assert.AreEqual(true, Config.IsLocalOnly());
        }

        [TestMethod]
        public void CmsUrlCanBeRead()
        {
            Assert.AreEqual(false, Config.IsLocalOnly());
            Config.OverrideBaseConfigPath = TempPath;
            var cmsUrlPath = Config.GetLocalConfig("BaseCmsUrl");
            File.WriteAllText(cmsUrlPath, @"https://moo.com/asdf/");
            Assert.AreEqual(@"https://moo.com/asdf/", Config.GetBaseCmsUrl());
        }
    }
}
