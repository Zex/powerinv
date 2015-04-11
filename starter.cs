/** starter.cs
 *
 * Author: Zex <top_zlynch@yahoo.com>
 */
using System;
using System.Text.RegularExpressions;
using System.Management;
using System.IO;
using System.Runtime.InteropServices; 

namespace utilities
{          
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
}
