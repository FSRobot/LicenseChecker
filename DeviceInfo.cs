using ECSCOMLib;
using System;
using System.IO;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;

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
            var ecsSerial = GetSerialNumber();
            var diskSerial = GetDriveSerialNumber();
            var serial = $"{ecsSerial}{productName}{diskSerial}";
            serial = Helpers.AesHelper.Encrypt(serial, "MachineCode");
            return serial;
        }

        public int GetSerialNumber()
        {
            var serial = _cnc.ECSCNCSerialNumber();
            return serial;
        }
        public string GetDiskDriveSerial()
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


        [StructLayout(LayoutKind.Sequential)]
        struct STORAGE_PROPERTY_QUERY
        {
            public STORAGE_PROPERTY_ID PropertyId;
            public STORAGE_QUERY_TYPE QueryType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public byte[] AdditionalParameters;
        }

        enum STORAGE_PROPERTY_ID
        {
            StorageDeviceProperty = 0
        }

        enum STORAGE_QUERY_TYPE
        {
            PropertyStandardQuery = 0
        }

        [StructLayout(LayoutKind.Sequential)]
        struct STORAGE_DEVICE_DESCRIPTOR
        {
            public uint Version;
            public uint Size;
            public byte DeviceType;
            public byte DeviceTypeModifier;
            public byte RemovableMedia;
            public byte CommandQueueing;
            public uint VendorIdOffset;
            public uint ProductIdOffset;
            public uint ProductRevisionOffset;
            public uint SerialNumberOffset;
            public STORAGE_BUS_TYPE BusType;
            public uint RawPropertiesLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            public byte[] RawDeviceProperties;
        }

        enum STORAGE_BUS_TYPE
        {
            BusTypeUnknown = 0x00
        }

        const uint IOCTL_STORAGE_QUERY_PROPERTY = 0x2D1400;
        const uint FILE_SHARE_READ = 0x00000001;
        const uint FILE_SHARE_WRITE = 0x00000002;
        const uint OPEN_EXISTING = 3;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            ref uint lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);
        
        string GetDriveSerialNumber()
        {
            IntPtr hDrive = CreateFile(
                @"\\.\PhysicalDrive0", // 物理驱动器
                0,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero,
                OPEN_EXISTING,
                0,
                IntPtr.Zero);

            if (hDrive == IntPtr.Zero)
            {
                throw new Exception("Failed to open drive.");
            }

            STORAGE_PROPERTY_QUERY query = new STORAGE_PROPERTY_QUERY
            {
                PropertyId = STORAGE_PROPERTY_ID.StorageDeviceProperty,
                QueryType = STORAGE_QUERY_TYPE.PropertyStandardQuery,
                AdditionalParameters = new byte[1]
            };

            int querySize = Marshal.SizeOf(query);
            IntPtr queryPtr = Marshal.AllocHGlobal(querySize);
            Marshal.StructureToPtr(query, queryPtr, true);

            int descriptorSize = Marshal.SizeOf(typeof(STORAGE_DEVICE_DESCRIPTOR)) + 1024;
            IntPtr descriptorPtr = Marshal.AllocHGlobal(descriptorSize);

            uint bytesReturned = 0;
            bool result = DeviceIoControl(
                hDrive,
                IOCTL_STORAGE_QUERY_PROPERTY,
                queryPtr,
                (uint)querySize,
                descriptorPtr,
                (uint)descriptorSize,
                ref bytesReturned,
                IntPtr.Zero);

            if (result)
            {
                STORAGE_DEVICE_DESCRIPTOR descriptor = Marshal.PtrToStructure<STORAGE_DEVICE_DESCRIPTOR>(descriptorPtr);
                int serialNumberOffset = (int)descriptor.SerialNumberOffset;
                int productIdOffset = (int)descriptor.ProductIdOffset;
                int versionOffset = (int)descriptor.Version;
                int productRevisionOffset = (int)descriptor.ProductRevisionOffset;
                if (serialNumberOffset != 0)
                {
                    string serialNumber = Marshal.PtrToStringAnsi(new IntPtr(descriptorPtr.ToInt64() + serialNumberOffset));
                    string productId = Marshal.PtrToStringAnsi(new IntPtr(descriptorPtr.ToInt64() + productIdOffset));
                    string version = Marshal.PtrToStringAnsi(new IntPtr(descriptorPtr.ToInt64() + versionOffset));
                    string productRevision = Marshal.PtrToStringAnsi(new IntPtr(descriptorPtr.ToInt64() + productRevisionOffset));
                    Marshal.FreeHGlobal(queryPtr);
                    Marshal.FreeHGlobal(descriptorPtr);
                    CloseHandle(hDrive);
                    return $"{serialNumber}{productId}{version}{productRevision}";
                }
                else
                {
                    throw new DriveNotFoundException("Serial number not found.");
                }
            }
            else
            {
                throw new DriveNotFoundException("Failed to get drive serial number.");
            }

            Marshal.FreeHGlobal(queryPtr);
            Marshal.FreeHGlobal(descriptorPtr);
            CloseHandle(hDrive);
            return string.Empty;
        }
    }
}
