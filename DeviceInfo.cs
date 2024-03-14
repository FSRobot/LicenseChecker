using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace LicenseChecker
{
    public static class DeviceInfo
    {
        public static bool Correct(string serial)
        {
            if (serial == null) return false;
            var mac = GetMacAddress();
            var memory = GetMemorySerialNumber();
            var board = GetMotherBoardSerialNumber();
            return (mac + memory + board).Equals(serial);
        }

        public static string SerialNumber
            => $"{GetMacAddress()}{GetMemorySerialNumber()}{GetMotherBoardSerialNumber()}";

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

                return Convert.ToInt64(serial, 16);
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

                return Convert.ToInt64(serial, 16);
            }
        }
    }
}
