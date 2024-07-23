using ECSCOMLib;
using System;
using System.Management;
using System.Reflection;

namespace LicenseChecker
{
    public class DeviceInfo
    {
        private readonly Cnc _cnc = new Cnc();
        public DeviceInfo()
        {
        }

        public string SerialNumber(string productName)
        {
            var serial = $"{GetSerialNumber()}{productName}{GetDiskDriveSerial().Trim()}";
            serial = Helpers.AesHelper.Encrypt(serial, "MachineCode");
            return serial;
        }

        public int GetSerialNumber()
        {
            var serial = _cnc.ECSCNCSerialNumber();
            return serial;
        }
        public static string GetDiskDriveSerial()
        {
            WqlObjectQuery wqlQuery =
                new WqlObjectQuery("SELECT * FROM Win32_LogicalDisk");
            using ManagementObjectSearcher searcher =
                new ManagementObjectSearcher(wqlQuery);

            bool fund = false;
            var serialStr = string.Empty;
            foreach (var o in searcher.Get())
            {
                var disk = (ManagementObject)o;
                foreach (PropertyData prop in disk.Properties)
                {
                    if (prop.Name.Equals("DeviceID") && prop.Value.Equals("C:"))
                    {
                        fund = true;
                    }

                    if (fund)
                    {
                        if (prop.Name.Equals("VolumeSerialNumber")
                            || prop.Name.Equals("Size")
                            || prop.Name.Equals("SystemName"))
                        {
                            serialStr += prop.Value.ToString();
                        }
                    }

                    if (fund && prop.Name.Equals("Access"))
                    {
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(serialStr))
                throw new Exception("cannot get machine code!");
            return serialStr;
        }

        //public static long GetMacAddress()
        //{
        //    try
        //    {
        //        string mac = string.Empty;
        //        using (ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
        //        {
        //            var list = mc.GetInstances();
        //            foreach (var management in list)
        //            {
        //                if ((bool)management["IPEnabled"] == true)
        //                {
        //                    mac = management["MacAddress"].ToString();
        //                    break;
        //                }
        //            }

        //            return Convert.ToInt64(mac.Replace(":", ""), 16);
        //        }
        //    }
        //    catch
        //    {
        //        return -1;
        //    }
        //}

        //public static long GetMemorySerialNumber()
        //{
        //    using (ManagementClass mc = new ManagementClass("Win32_PhysicalMemory"))
        //    {
        //        var list = mc.GetInstances();
        //        var serial = string.Empty;

        //        foreach (var management in list)
        //        {
        //            serial = management.Properties["SerialNumber"].Value.ToString();
        //            break;
        //        }

        //        return Convert.ToInt64(serial.Trim(), 16);
        //    }
        //}

        //public static long GetMotherBoardSerialNumber()
        //{
        //    using (ManagementClass mc = new ManagementClass("Win32_BaseBoard"))
        //    {
        //        var list = mc.GetInstances();
        //        var serial = string.Empty;

        //        foreach (var management in list)
        //        {
        //            serial = management.Properties["SerialNumber"].Value.ToString();
        //            break;
        //        }

        //        return Convert.ToInt64(serial.Trim(), 16);
        //    }
        //}
    }
}
