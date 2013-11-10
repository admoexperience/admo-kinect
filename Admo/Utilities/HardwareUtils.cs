using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace Admo.Utilities
{
    public class HardwareUtils
    {
        public class UsbDevice
        {
            public string Manufacturer { set; get; }
            public string DeviceId { set; get; }
            public string Description { set; get; }
            public string VersionVendor { set; get; }
        }

        public class SystemInfo
        {
            public string OsVersionRaw { set; get; }
            public string Manufacturer { set; get; }
            public string OsVersion { set; get; }
            public string Model { set; get; }
            public long TotalMemory { set; get; }
            public string ProcessorType { set; get; }
            public bool Is64BitOperatingSystem { set; get; }
        }

        public static SystemInfo GetSystemInfo()
        {
            var osVersionRaw = Environment.OSVersion.ToString();
            var query1 =
                new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            var queryCollection1 = query1.Get();

            var qmotherBoard = queryCollection1.GetEnumerator();
            qmotherBoard.MoveNext();
            var motherBoard = qmotherBoard.Current;

            //Processor type and speed
            var qqcpu = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            var qcpu = qqcpu.Get().GetEnumerator();
            qcpu.MoveNext();
            var cpu = qcpu.Current;

            var osVersion = (from x in new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem").Get().OfType<ManagementObject>() select x.GetPropertyValue("Caption")).First();

            return new SystemInfo
            {
                Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                OsVersionRaw = osVersionRaw,
                OsVersion = Convert.ToString(osVersion),
                Manufacturer = Convert.ToString(motherBoard["manufacturer"]),
                Model = Convert.ToString(motherBoard["model"]),
                TotalMemory = Convert.ToInt64(motherBoard["totalphysicalmemory"]),
                ProcessorType = Convert.ToString(cpu["Name"])
            };
        }

        public static List<UsbDevice> GetUsbDevices()
        {
            var deviceList = new List<UsbDevice>();
            var scope = new ManagementScope("root\\CIMV2") {Options = {EnablePrivileges = true}};
            const string win32UsbControlerDevice = "Select * From Win32_USBControllerDevice";
            var query = new ObjectQuery(win32UsbControlerDevice);
            var searcher = new ManagementObjectSearcher(scope, query);
            foreach (var mgmtObj in searcher.Get())
            {
                var strDeviceName = mgmtObj["Dependent"].ToString();
                const string strQuotes = "'";
                strDeviceName = strDeviceName.Replace("\"", strQuotes);
                var arrDeviceName = strDeviceName.Split('=');
                strDeviceName = arrDeviceName[1];
                var win32PnPEntity = "Select * From Win32_PnPEntity "
                                         + "Where DeviceID =" + strDeviceName;
                var mySearcher =
                    new ManagementObjectSearcher(win32PnPEntity);
                foreach (var mobj in mySearcher.Get())
                {
                    var strDeviceID = mobj["DeviceID"].ToString();
                    var arrDeviceID = strDeviceID.Split('\\');
                    var device = new UsbDevice
                    {
                        VersionVendor = arrDeviceID[1],
                        Manufacturer = Convert.ToString(mobj["Manufacturer"]),
                        DeviceId = arrDeviceID[2].Trim('{', '}'),
                        Description = Convert.ToString(mobj["Description"])

                    };
                deviceList.Add(device);
                }
            }
            return deviceList;
        }

        
    }
}
