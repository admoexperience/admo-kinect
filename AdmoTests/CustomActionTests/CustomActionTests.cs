using System;
using System.Threading;
using Admo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdmoInstallerCustomAction;
using System.Windows;

namespace AdmoTests.CustomActionTests
{
    [TestClass]
    public class CustomActionTests
    {
 
        [TestMethod]
        public void TestRunWPFOnSTAThread()
        {
          //Will have to use gui testing tools here

            Thread t = new Thread(() =>
            {
                var app = new Application();
                app.Run(new DownloadRuntimeWPF());
            });
            t.SetApartmentState(ApartmentState.STA);

            t.Start();
           t.Abort();

        }
    }
}
