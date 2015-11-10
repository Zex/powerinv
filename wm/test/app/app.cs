/** wm - app
 * Author: Zex <top_zlynch AT yahoo.com>
 */
using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace wmapp
{
    public class BritainClient : wm.Client
    {
        public void OnQuitBtn(object sender, EventArgs e)
        {
            Trace.WriteLine(string.Format("Client.OnQuitBtn : {0}", base.Name));
            base.Close();
        }

        public BritainClient(string name) : base(name)
        {
            int id = Convert.ToInt32(name.Split(new char[] { '_' })[1]);
            System.Drawing.Color color = System.Drawing.Color.DodgerBlue;
            
            switch (id%3)
            {
                case 0: color = System.Drawing.Color.DeepSkyBlue; break;
                case 1: color = System.Drawing.Color.Goldenrod; break;
                case 2: color = System.Drawing.Color.Firebrick; break;
            }
            this.BackColor = color;
            
            TextBox tbox = new TextBox();
            tbox.Text = "@" + name;
            this.Controls.Add(tbox);
            
            Button btn = new Button();
            btn.Text = name;
            btn.Click += new EventHandler(OnQuitBtn);
            btn.Left = tbox.Width+5;
            this.Controls.Add(btn);

            Button icon = new Button();
            try
            {
                icon.Image = System.Drawing.Image.FromFile(
                    Path.Combine (Path.GetDirectoryName (Assembly.GetExecutingAssembly().Location),
                        @"bigeyes.jpg"));
            }
            catch (FileNotFoundException)
            {
                Trace.WriteLine("Button image file not found");
            }
            icon.Width = 100;
            icon.Height = 80;
            icon.Top = tbox.Height + 3;
            this.Controls.Add(icon);

            this.Height = 100;
            this.Width = 150;
            
            if (id < 8)
            {
                this.Left = 50 + this.Width * (id);
                this.Top = 50 + this.Height * (id);
            }
            else
            {
                this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width * (id-6);
                this.Top = 10 + this.Height * (id-7);
            }
        }
    }

    public class wmapp
    {
        public static string trace_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
   System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".trace");
        public static TextWriterTraceListener _tracer = new TextWriterTraceListener(Console.Out);
        
        protected static void InitTrace()
        {
            //_tracer.TraceOutputOptions |= TraceOptions.DateTime;//| TraceOptions ;
            Trace.Listeners.Add(_tracer);
        }

        public static void Main()
        {
            InitTrace();
            
            try
            {
                for (int i = 0; i < 15; i++)
                {
                    BritainClient cli = new BritainClient(string.Format("Client_{0}", i));
                    wm.Manager.BuildWindow(cli);
                }

                for (int i = 20; i > 0; i--)
                {
                    Thread.Sleep(1000);
                    wm.Manager.ActivateWindow(string.Format("Client_{0}", i%3));
                }

                for (int i = 0; i < 15; i++)
                {
                    wm.Manager.DestroyWindow(string.Format("Client_{0}", i));
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }
        }
    }
}
