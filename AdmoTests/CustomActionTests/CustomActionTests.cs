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
        public void TestRunWpfOnSTAThread()
        {
          //Will have to use gui testing tools here

            Thread t = new Thread(() =>
            {
                var app = new Application();
                app.Run(new DownloadRuntimeWPF());
            });
            t.SetApartmentState(ApartmentState.STA);

            t.Start();
      //     t.Join();
           t.Abort();

        }
        [TestMethod]
        public void TestRunWpfOnSTAThreadPcStats()
        {
            //Will have to use gui testing tools here

            Thread t = new Thread(() =>
            {
                var app = new Application();
                app.Run(new CheckPCStats());
            });
            t.SetApartmentState(ApartmentState.STA);

            t.Start();
         //   t.Join();
            t.Abort();

        }
    }
}
