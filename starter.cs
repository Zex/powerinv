/** starter.cs
 *
 * Author: Zex <top_zlynch@yahoo.com>
 *
 */
using System;
using System.Text.RegularExpressions;
using System.Management;
using System.IO;
using System.Runtime.InteropServices; 

namespace utilities
{          
/*
 *    public class DiskPartBlocker
 *    {
 *        public const string premount_dp_path = "C:\\premount.dp";
 *        public const string postmount_dp_path = "C:\\postmount.dp";
 *
 *        public static void generate_script(int disk_nr, int part_nr, Guid guid, string dp_path)
 *        {
 *            string[] premount = {
 *                "select disk " + disk_nr.ToString(),
 *                "select partition " + part_nr.ToString(),
 *                "",
 *                "set id=" + guid.ToString() + " override"
 *                };
 *
 *            Directory.CreateDirectory(Path.GetDirectoryName(dp_path));
 *
 *            using (StreamWriter o = new StreamWriter(dp_path))
 *            {
 *                foreach (string line in premount)
 *                    o.WriteLine(line);
 *            }
 *        }
 *
 *        public static void to_basic_guid()
 *        {
 *            if (!File.Exists(premount_dp_path))
 *                throw new Exception("Premount script not found");
 *
 *            ProcessStartInfo to_basic = new ProcessStartInfo();
 *            to_basic.FileName = "diskpart";
 *            to_basic.Arguments = "/s " + premount_dp_path;
 *
 *            Process.Start(to_basic).WaitForExit();
 *        }
 *
 *        public static void to_oem_guid()
 *        {
 *            if (!File.Exists(postmount_dp_path))
 *                throw new Exception("Postmount script not found");
 *
 *            ProcessStartInfo to_oem = new ProcessStartInfo();
 *            to_oem.FileName = "diskpart";
 *            to_oem.Arguments = "/s " + postmount_dp_path;
 *
 *            Process.Start(to_oem).WaitForExit();
 *        }
 *
 *        public static void cleanup()
 *        {
 *            if (File.Exists(premount_dp_path))
 *                File.Delete(premount_dp_path);
 *
 *            if (File.Exists(postmount_dp_path))
 *                File.Delete(postmount_dp_path);
 *        }
 *    }
 */
    public class SymLnk
    {
        [DllImport("kernel32.dll")]
        static extern int GetLastError();
    
        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363866%28v=vs.85%29.aspx
        enum SymLnk
        {
            SYMBOLIC_LINK_FLAG_FILE = 0,
            SYMBOLIC_LINK_FLAG_DIRECTORY = 1
        };
            
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(
        string lpSymlinkFileName, string lpTargetFileName, SymLnk dwFlags);

        public struct PartPosition
        {
            public int disk_nr;
            public int part_nr;
        }

        static void mksymlnk(string globalroot_path, string access_path, SymLnk ty)
        {
            bool ret;

            if (false == (ret = CreateSymbolicLink(access_path, globalroot_path, ty)))
            {
                throw new Exception("CreateSymbolicLink " + globalroot_path + " @ " + access_path + " fail, errno=" + GetLastError());
            }
        }
        
        static void mksymlnk(PartPosition pos, string access_path)
        {
            string globalroot_path = string.Format(@"\\?\GLOBALROOT\Device\Harddisk{0}\partition{1}\", pos.disk_nr, pos.part_nr);

            mksymlnk(globalroot_path, access_path, SymLnkType.SYMBOLIC_LINK_FLAG_DIRECTORY);
        }
    }
    
    public class DeviceIOCtrl
    {
        public const int GENERIC_READ = unchecked((int)0x80000000);
        //public const int GENERIC_WRITE = unchecked((int)0x40000000);
        public const int FILE_SHARE_READ = 1;
        public const int FILE_SHARE_WRITE = 2;
        public const int OPEN_EXISTING = 3;
        public const int ERROR_INSUFFICIENT_BUFFER = 122;
        public const int INVALID_PARTITION_INDEX = -1;
        public const int INVALID_DISK_INDEX = -1;

        // DRIVE_LAYOUT_INFORMATION_EX_HEAD_SZ, number of bytes in DRIVE_LAYOUT_INFORMATION_EX 
        // up to the first PARTITION_INFORMATION_EX in the array, is 48 bytes.
        // PARTITION_INFORMATION_EX_SZ, size of each PARTITION_INFORMATION_EX, is 144 bytes.
        public const int DRIVE_LAYOUT_INFORMATION_EX_HEAD_SZ = 48;
        public const int PARTITION_INFORMATION_EX_SZ = 144;

        public const int IOCTL_DISK_GET_DRIVE_LAYOUT_EX = unchecked((int)0x00070050);

        // PARTITION_BASIC_DATA_GUID https://msdn.microsoft.com/en-us/library/windows/desktop/aa365449%28v=vs.85%29.aspx
        public static Guid PARTITION_BASIC_DATA_GUID = new Guid("ebd0a0a2-b9e5-4433-87c0-68b6b72699c7");

        // GPT_BASIC_DATA_ATTRIBUTE_NO_DRIVE_LETTER, 0x8000000000000000
        // GPT_ATTRIBUTE_PLATFORM_REQUIRED, 0x0000000000000001
        public static string FACTORY_GPT_ATTR = "0x8000000000000001";

        // PARTITION_STYLE https://msdn.microsoft.com/en-us/library/windows/desktop/aa365452%28v=vs.85%29.aspx
        // PARTITION_STYLE_MBR 0, Master boot record (MBR) format.
        // PARTITION_STYLE_GPT 1, GUID Partition Table (GPT) format.
        // PARTITION_STYLE_RAW 2, Partition not formatted in either of the recognized formatsMBR or GPT.
        public enum PARTITION_STYLE : int
        {
            MBR = 0,
            GPT = 1,
            RAW = 2
        }

        // Disk Partition Types https://msdn.microsoft.com/en-us/library/windows/desktop/aa363990%28v=vs.85%29.aspx
        public enum Partition : byte
        {
            ENTRY_UNUSED = 0,
            FAT_12 = 1,
            XENIX_1 = 2,
            XENIX_2 = 3,
            FAT_16 = 4,
            EXTENDED = 5,
            HUGE = 6,
            IFS = 7,
            OS2BOOTMGR = 0xa,
            FAT32 = 0xb,
            FAT32_XINT13 = 0xc,
            XINT13 = 0xe,
            XINT13_EXTENDED = 0xf,
            PREP = 0x41,
            LDM = 0x42,
            UNIX = 0x63
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            string fileName,
            int desiredAccess,
            int shareMode,
            IntPtr securityAttributes,
            int creationDisposition,
            int flagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeviceIoControl(
            SafeFileHandle hVol,
            int controlCode,
            IntPtr inBuffer,
            int inBufferSize,
            IntPtr outBuffer,
            int outBufferSize,
            ref int bytesReturned,
            IntPtr overlapped);

        // DRIVE_LAYOUT_INFORMATION_EX https://msdn.microsoft.com/en-us/library/windows/desktop/aa364001%28v=vs.85%29.aspx
        [StructLayout(LayoutKind.Explicit)]
        public struct DRIVE_LAYOUT_INFORMATION_EX
        {
            [FieldOffset(0)]
            public PARTITION_STYLE PartitionStyle;

            [FieldOffset(4)]
            public int PartitionCount;

            [FieldOffset(8)]
            public DRIVE_LAYOUT_INFORMATION_MBR Mbr;

            [FieldOffset(8)]
            public DRIVE_LAYOUT_INFORMATION_GPT Gpt;
        }

        // DRIVE_LAYOUT_INFORMATION_MBR https://msdn.microsoft.com/en-us/library/windows/desktop/aa364004%28v=vs.85%29.aspx
        public struct DRIVE_LAYOUT_INFORMATION_MBR
        {
            public uint Signature;
        }

        // DRIVE_LAYOUT_INFORMATION_GPT https://msdn.microsoft.com/en-us/library/windows/desktop/aa364003%28v=vs.85%29.aspx
        [StructLayout(LayoutKind.Sequential)]
        public struct DRIVE_LAYOUT_INFORMATION_GPT
        {
            public Guid DiskId;
            public long StartingUsableOffset;
            public long UsableLength;
            public int MaxPartitionCount;
        }

        // PARTITION_INFORMATION_MBR https://msdn.microsoft.com/en-us/library/windows/desktop/aa365450%28v=vs.85%29.aspx
        [StructLayout(LayoutKind.Sequential)]
        public struct PARTITION_INFORMATION_MBR
        {
            public byte PartitionType;
            [MarshalAs(UnmanagedType.U1)]

            public bool BootIndicator;
            [MarshalAs(UnmanagedType.U1)]

            public bool RecognizedPartition;
            public UInt32 HiddenSectors;

            public Partition GetPartition()
            {
                const byte mask = 0x3f;
                return (Partition)(PartitionType & mask);
            }
        }

        // PARTITION_INFORMATION_GPT https://msdn.microsoft.com/en-us/library/windows/desktop/aa365449%28v=vs.85%29.aspx
        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        public struct PARTITION_INFORMATION_GPT
        {
            [FieldOffset(0)]
            public Guid PartitionType;

            [FieldOffset(16)]
            public Guid PartitionId;

            [FieldOffset(32)]
            public ulong Attributes;

            [FieldOffset(40)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
            public string Name;
        }

        // PARTITION_INFORMATION_EX https://msdn.microsoft.com/en-us/library/windows/desktop/aa365448%28v=vs.85%29.aspx
        [StructLayout(LayoutKind.Explicit)]
        public struct PARTITION_INFORMATION_EX
        {
            [FieldOffset(0)]
            public PARTITION_STYLE PartitionStyle;

            [FieldOffset(8)]
            public long StartingOffset;

            [FieldOffset(16)]
            public long PartitionLength;

            [FieldOffset(24)]
            public int PartitionNumber;

            [FieldOffset(28)]
            [MarshalAs(UnmanagedType.U1)]
            public bool RewritePartition;

            [FieldOffset(32)]
            public PARTITION_INFORMATION_MBR Mbr;

            [FieldOffset(32)]
            public PARTITION_INFORMATION_GPT Gpt;
        }

        public struct DiskInfo
        {
            public DRIVE_LAYOUT_INFORMATION_EX layout_info;
            public PARTITION_INFORMATION_EX[] part_info;
        }

        // Device ID is string like \\.\physicaldrive0, \\.\physicaldrive2, etc.
        public static DiskInfo get_drive_layout_ex(string deviceid)
        {
            DiskInfo disk_info = new DiskInfo();

            disk_info.layout_info = default(DRIVE_LAYOUT_INFORMATION_EX);
            disk_info.part_info = null;

            // "\\\\.\\PHYSICALDRIVE" + PhysicalDrive
            using (SafeFileHandle hDevice =
                CreateFile(deviceid, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero))
            {
                if (hDevice.IsInvalid)
                    throw new Win32Exception("Failed to get device handler");

                int numPartitions = 1;
                bool done = false;

                do
                {
                    int outBufferSize = DRIVE_LAYOUT_INFORMATION_EX_HEAD_SZ + (numPartitions * PARTITION_INFORMATION_EX_SZ);
                    IntPtr blob = default(IntPtr);
                    int bytesReturned = 0;
                    bool result = false;

                    try
                    {
                        blob = Marshal.AllocHGlobal(outBufferSize);
                        result = DeviceIoControl(hDevice, IOCTL_DISK_GET_DRIVE_LAYOUT_EX, IntPtr.Zero, 0, blob, outBufferSize, ref bytesReturned, IntPtr.Zero);

                        if (result == false)
                        {
                            if (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER)
                                throw new Win32Exception();

                            numPartitions += 1;
                        }
                        else
                        {
                            done = true;
                            disk_info.layout_info = (DRIVE_LAYOUT_INFORMATION_EX)Marshal.PtrToStructure(blob, typeof(DRIVE_LAYOUT_INFORMATION_EX));

                            disk_info.part_info = new PARTITION_INFORMATION_EX[disk_info.layout_info.PartitionCount];

                            for (int i = 0; i < disk_info.layout_info.PartitionCount; i++)
                            {
                                IntPtr offset = new IntPtr(blob.ToInt64() + DRIVE_LAYOUT_INFORMATION_EX_HEAD_SZ + (i * PARTITION_INFORMATION_EX_SZ));
                                disk_info.part_info[i] = (PARTITION_INFORMATION_EX)Marshal.PtrToStructure(offset, typeof(PARTITION_INFORMATION_EX));
                            }
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(blob);
                    }
                } while (!(done));
            }
            return disk_info;
        }

        // Descript partition position as [disk index, partition index] 
        public class PartitionPos
        {
            public int disk_ind;
            public int part_ind;
        }

        public static void list_partition(DiskInfo disk_info)
        {
            for (int i = 0; i < disk_info.layout_info.PartitionCount; i++)
            {
                var part = disk_info.part_info[i];

                switch (part.PartitionStyle)
                {
                    case PARTITION_STYLE.MBR:
                        {
                        var mbr = part.Mbr;
                        Console.WriteLine("Mbr PartitionType - raw value: {0}", mbr.PartitionType);
                        Console.WriteLine("Mbr BootIndicator : {0}", mbr.BootIndicator);
                        Console.WriteLine("Mbr RecognizedPartition : {0}", mbr.RecognizedPartition);
                        Console.WriteLine("Mbr HiddenSectors : {0}", mbr.HiddenSectors);
                            break;
                        }
                    case PARTITION_STYLE.GPT:
                        {
                            var gpt = part.Gpt;

                            Console.WriteLine("Gpt PartitionType: {0}", gpt.PartitionType);
                            Console.WriteLine("Gpt PartitionId: {0}", gpt.PartitionId);
                            Console.WriteLine("Gpt Attributes: {0}", gpt.Attributes);
                            Console.WriteLine("Gpt Name: {0}", gpt.Name);

                            break;
                        }
                    case PARTITION_STYLE.RAW:
                    default:
                        {
                            break;
                        }
                }
            }
        }

    }
    /*
     \code
    {
        string NamespacePath = "\\\\.\\ROOT\\Microsoft\\Windows\\Storage";
        string ClassName = "MSFT_Partition";// "MSFT_StorageObject";

        ManagementClass cls = new ManagementClass(NamespacePath + ":" + ClassName);
        //out string ret = string.Empty;

        Console.WriteLine(cls.InvokeMethod("AddAccessPath", new object[] { 
            "c:\\port-3", false, out string ret }));
        // ------------------
        Console.WriteLine(string.Format("Property Names in {0}: ", ClassName));

        foreach (PropertyData property in cls.Properties)
        {
            Console.WriteLine("name: {0}, Value: {1}", property.Name,
                 cls.GetPropertyValue(property.Name));
        }
        // ------------------      
    }
    \endcode
    */
    public class PartitionHelper
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool DeleteVolumeMountPoint(string lpszVolumeMountPoint);

        private PartitionHelper()
        {

        }

        static Regex RxGUID = new Regex("([0-9a-fA-F]){8}(-([0-9a-fA-F]){4}){3}-([0-9a-fA-F]){12}");

        public static void mount(string label, string dirbase)
        {
            if (0 < RxGUID.Match(label).Value.Length)
            {
                label = RxGUID.Match(label).Value;
            }

            string dest = Path.Combine(dirbase, label); 
            string req  = string.Format(@"select * from Win32_Volume where Label = '{0}' or DeviceID like '%{0}%'", label);

            ManagementObjectCollection objs = (new ManagementObjectSearcher(req)).Get();

            foreach (ManagementObject obj in objs)
            {
                DirectoryInfo di = Directory.CreateDirectory(dest);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

                obj.InvokeMethod("AddMountPoint", new object[] { dest });
            }
        }

        public static void umount(string label, string dirbase)
        {
            if (0 < RxGUID.Match(label).Value.Length)
            {
                label = RxGUID.Match(label).Value;
            }

            string dest = Path.Combine(dirbase, label);
            string req  = string.Format(@"select * from Win32_Volume where Label = '{0}' or DeviceID like '%{0}%'", label);

            ManagementObjectCollection objs = (new ManagementObjectSearcher(req)).Get();

            foreach (ManagementObject obj in objs)
            {
                if (!DeleteVolumeMountPoint(dest + "\\"))
                {
                    throw new Exception("Failed to delete mount point: errno=" + Marshal.GetLastWin32Error().ToString());
                }
                else
                {
                    obj.InvokeMethod("Dismount", new object[] { true, false });
                    Directory.Delete(dest, true);
                }
            }
        }

    }


    /**
     * public class BcdBrocker
     * {
     * }
     *
     */
}

