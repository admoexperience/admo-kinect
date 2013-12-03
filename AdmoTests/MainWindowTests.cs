using System;
using System.Threading;
using System.Windows;
using Admo;
using AdmoInstallerCustomAction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdmoTests
{
    [TestClass]
    public class MainWindowTests
    {
        [TestMethod]
        public void OpenAndAbortMainWindow()
        {

            Thread t = new Thread(() =>
            {
                var app = new Application();
                app.Run(new MainWindow());
            });
            t.SetApartmentState(ApartmentState.STA);

            t.Start();
            //     t.Join();
            
            t.Abort();

        }
      
    }
}
