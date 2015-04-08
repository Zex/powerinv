/** starter.cs
 *
 * Author: Zex <top_zlynch@yahoo.com>
 */
using System.Management;
using System.IO;
using System.Runtime.InteropServices; 

namespace utils
{
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool DeleteVolumeMountPoint(string lpszVolumeMountPoint);

    public static void mount(string label, string dirbase)
    {
        string dest = Path.Combine(dirbase, label);

        ManagementObjectSearcher objs = new ManagementObjectSearcher(
            @"select * from Win32_Volume where Label = '" + label + "'");

        foreach (ManagementObject obj in objs.Get())
        {
            DirectoryInfo di = Directory.CreateDirectory(dest);
            di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            obj.InvokeMethod("AddMountPoint", new object[] { dest });
        }
    }

    public static void umount(string label, string dirbase)
    {
        string dest = Path.Combine(dirbase, label);

        ManagementObjectSearcher objs = new ManagementObjectSearcher(
            @"select * from Win32_Volume where Label = '" + label + "'");

        foreach (ManagementObject obj in objs.Get())
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
