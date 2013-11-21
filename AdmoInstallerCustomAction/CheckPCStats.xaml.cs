using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AdmoInstallerCustomAction
{
    /// <summary>
    /// Interaction logic for CheckPCStats.xaml
    /// </summary>
    public partial class CheckPCStats : Window
    {
        public CheckPCStats()
        {
            InitializeComponent();

            Closed += DownloadRuntimeWPF_Closed;
            NextBox.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(Next_MouseUp), true);
            var systeminfo = HardwareUtils.GetSystemInfo();
            PcStatsTextBlock.AppendText("====HARDWAREINFO===" + Environment.NewLine);
            PcStatsTextBlock.AppendText(String.Format("Manufacturer: {0}, Model: {1}, CPU: {2}", systeminfo.Manufacturer, systeminfo.Model, systeminfo.ProcessorType) + Environment.NewLine);
            PcStatsTextBlock.AppendText(String.Format("OSVersion: {0}, Raw: {1}, Is64Bit: {2}", systeminfo.OsVersion, systeminfo.OsVersionRaw, systeminfo.Is64BitOperatingSystem) + Environment.NewLine );
           // PcStatsTextBlock.AppendText(String.Format("TotalMemory: {0}", Utils.BytesToHuman(systeminfo.TotalMemory)));
        }

        private void DownloadRuntimeWPF_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();

           
            PcStatsTextBlock.AppendText("====HARDWAREINFO===");

        }

        private void Next_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
