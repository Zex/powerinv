using System;

namespace sysinfo
{
    class reginfo
    {
        public void get_items()
        {
            foreach (var k in
                Microsoft.Win32.Registry.LocalMachine.GetSubKeyNames())
            {
                Console.WriteLine("SubKey: {0}", k);
            }

            Console.WriteLine("---------------------------------------");

            Microsoft.Win32.RegistryKey rkey =
                Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Google", true);

            if (rkey != null)
            {
                foreach (var k in rkey.GetSubKeyNames())
                {
                    Console.WriteLine("SubKeyName : {0}", k);
                }

                Console.WriteLine("---------------------------------------");

                foreach (var k in rkey.GetValueNames())
                {
                    Console.WriteLine("{0}: {1}", k, rkey.GetValue(k));
                }

            }
        }
    }      

    class utils
    {
        public void mount(string label, string mount_base)
        {
            System.Management.Instrumentation.WmiConfigurationAttribute
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        sysinfo.reginfo ri = new sysinfo.reginfo();
        ri.get_items();
    }
}
