/** Window Manager - Screen
 * Author: Zex <top_zlynch AT yahoo.com>
 */
using System;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using fw;

namespace wm
{
    public class Screens : HashSet<Client>
    {
        public static void GoBack()
        {
            // TODO
        }

        public static void GoHome()
        {
            // TODO
        }
    }
    
    public class Client : Form
    {
        private Manager _mgr = Singleton<Manager>.Instance;

        public Client(string name)
        {
            Trace.WriteLine("Client() : name=" + name);
            this.Name = name;
            Initialize();
        }

        public void Initialize()
        {
            // Default form style
            this.StartPosition = FormStartPosition.Manual;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.BackColor = System.Drawing.Color.CadetBlue;
        }
    }
    /**
     * \sa https://msdn.microsoft.com/en-us/library/system.windows.forms.nativewindow%28v=vs.110%29.aspx
     */
    public class Manager : NativeWindow
    {
        protected static Screens _clients = new Screens();
        private static Keyboard _wm_kb;
        private static Mouse _wm_mouse;

        public HashSet<Client> clients { get { return _clients; } }

        public Manager()
        {
            Initialize();
        }

        internal void OnHandleCreated(object sender, EventArgs e)
        {
            Trace.WriteLine(string.Format("Manager.OnHandleCreated() : {0}, {1}", sender, e));
            AssignHandle(((Client)sender).Handle);
        }

        internal void OnHandleDestroyed(object sender, EventArgs e)
        {
            Trace.WriteLine(string.Format("Manager.OnHandleDestroyed() : {0}, {1}", sender, e));
            ReleaseHandle();
        }

        protected override void WndProc(ref Message m)
        {
            //Trace.WriteLine(string.Format("Manager.WndProc() : {0}", m)); // too noisy here
            
            switch (m.Msg)
            {   
                case lowlevel.WM_SETFOCUS:
                    Trace.WriteLine(string.Format("Manager.WndProc() : lowlevel.WM_SETFOCUS (int)m.WParam != 0 {0}", ((int)m.WParam != 0)));
                    break;
                case lowlevel.WM_ACTIVATEAPP:
                    Trace.WriteLine(string.Format("Manager.WndProc() : lowlevel.WM_ACTIVATEAPP (int)m.WParam != 0 {0}", ((int)m.WParam != 0)));
                    break;
                case lowlevel.WM_CLOSE:
                    Trace.WriteLine(string.Format("Manager.WndProc() : lowlevel.WM_CLOSE {0}", m));
                    break;
                default:
                    //Trace.WriteLine(string.Format("Unhandled message {0}", m)); // too noisy here
                    break;
            }
            base.WndProc(ref m);
        }

        protected void Register(Client cli)
        {
            Trace.WriteLine(string.Format("Manager.Register() : name={0}", cli.Name));
            if (_clients.Count < 1)
            {
                cli.HandleCreated += new EventHandler(this.OnHandleCreated);
                cli.HandleDestroyed += new EventHandler(this.OnHandleDestroyed);
            }
            _clients.Add(cli);
            Trace.WriteLine(string.Format("Client count: {0}", _clients.Count));
        }

        protected void Initialize()
        {
            Trace.WriteLine("Manager.Initialize()");
            
            _wm_kb = new Keyboard();
            _wm_kb.Initialize();
            _wm_mouse = new Mouse();
            _wm_mouse.Initialize();
        }

        public void Dispose()
        {
            Trace.WriteLine("Manager.Dispose()");
            _wm_mouse.Dispose();
            _wm_kb.Dispose();
            _clients.Clear();
        }

        public static void ActivateWindow(string name)
        {
            foreach (var cli in _clients)
            {
                if (cli.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    cli.Activate();
                    break;
                }
            }
        }

        public static void DestroyWindow(string name)
        {
            foreach (var cli in _clients)
            {
                if (cli.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    cli.Close();
                    break;
                }
            }
        }

        protected void NewFormProc(object data)
        {
            Trace.WriteLine("Manager.NewFormProc()");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Client cli = (Client)data;
            Register(cli);
            
            Application.Run(cli);
        }

        public static void BuildWindow(Client cli)
        {
            Trace.WriteLine(string.Format("Manager.BuildWindow() : name={0}", cli.Name));
            Thread thr = new Thread(new ParameterizedThreadStart(fw.Singleton<Manager>.Instance.NewFormProc));
            thr.Start(cli);
        }

    } /** class Manager  */

}
