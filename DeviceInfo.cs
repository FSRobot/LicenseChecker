using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using EcsReader;

namespace LicenseChecker
{
    public class DeviceInfo
    {
        private readonly EcsSpy _spy;

        public DeviceInfo(EcsSpy spy)
        {
            _spy = spy;
        }

        public string SerialNumber()
        {
            var serial = $"{_spy.GetSerialNumber()}{GetDiskDriveSerial().Trim()}";
            serial = Convert.ToBase64String(Encoding.UTF8.GetBytes(serial));
            return serial;
        }

        public static string GetDiskDriveSerial()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
            {
                foreach (var info in searcher.Get())
                {
                    if (info["SerialNumber"] != null)
                        return info["SerialNumber"].ToString().Replace("-", "");
                }
            }

            return string.Empty;
        }

        public static long GetMacAddress()
        {
            try
            {
                string mac = string.Empty;
                using (ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
                {
                    var list = mc.GetInstances();
                    foreach (var management in list)
                    {
                        if ((bool)management["IPEnabled"] == true)
                        {
                            mac = management["MacAddress"].ToString();
                            break;
                        }
                    }

                    return Convert.ToInt64(mac.Replace(":", ""), 16);
                }
            }
            catch
            {
                return -1;
            }
        }

        public static long GetMemorySerialNumber()
        {
            using (ManagementClass mc = new ManagementClass("Win32_PhysicalMemory"))
            {
                var list = mc.GetInstances();
                var serial = string.Empty;

                foreach (var management in list)
                {
                    serial = management.Properties["SerialNumber"].Value.ToString();
                    break;
                }

                return Convert.ToInt64(serial.Trim(), 16);
            }
        }

        public static long GetMotherBoardSerialNumber()
        {
            using (ManagementClass mc = new ManagementClass("Win32_BaseBoard"))
            {
                var list = mc.GetInstances();
                var serial = string.Empty;

                foreach (var management in list)
                {
                    serial = management.Properties["SerialNumber"].Value.ToString();
                    break;
                }

                return Convert.ToInt64(serial.Trim(), 16);
            }
        }
    }
}
