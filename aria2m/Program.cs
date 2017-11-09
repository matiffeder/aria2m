using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Threading;
using System.Windows.Forms;
//using System.Drawing;
using System.Diagnostics;
using System.Net;
using System.IO;
using Microsoft.VisualBasic.ApplicationServices;

namespace aria2m
{
    class TrayManager : IDisposable
    {
        //private static NotifyIcon trayicon = new NotifyIcon();
        private readonly NotifyIcon trayicon;
        //private static MenuItem menuItem0;
        private readonly MenuItem menuItem0;
        private static bool? firstopen = true;

        public TrayManager()
        {
            menuItem0 = new MenuItem();
            menuItem0.Click += new EventHandler(Toggle_aria2);
            menuItem0.Text = "Close &aria2";
            trayicon = new NotifyIcon
            {
                Icon = Properties.Resources.Icon,
                Text = "aria2 is running.",
                Visible = true,
                ContextMenu = new ContextMenu(new[]
                {
                    menuItem0,
                    new MenuItem("Open &UI", new EventHandler(Open_UI)),
                    new MenuItem("-"),
                    new MenuItem("E&xit", new EventHandler((s, e) => Close_aria2(s, e, true))),
                }),
            };
            trayicon.DoubleClick += new EventHandler(Open_UI);
            //if BalloonTip shows and click trayicon once will also Open_UI
            //trayicon.BalloonTipClicked += new EventHandler(Open_UI);
            Toggle_aria2(null, null);
        }

        public void Arg_Call(string[] args)
        {
            //if (arg != null)
            //{
                /*string copytxt = "";
                for (int i = 0; i < args.Length; i++)
                {
                    copytxt = copytxt + args[i] + "\n";
                }*/
                //if (new Mutex(true, "{XXX}").WaitOne(TimeSpan.Zero, true))
                if (Process.GetProcessesByName("aria2c").Length == 0)
                    Toggle_aria2(null, null);
                //Clipboard.SetText(copytxt);
                //trayicon.ShowBalloonTip(3000, "Texts copied.", copytxt, ToolTipIcon.None);
                if (Call_RPC("addUri", args[0], args[1], args[2]))
                    trayicon.ShowBalloonTip(3000, "Link added.", args[0], ToolTipIcon.None);
                //copytxt = null;
            //}
        }

        //private static void Toggle_aria2(object Sender, EventArgs e)
        private void Toggle_aria2(object Sender, EventArgs e)
        {
            if (Process.GetProcessesByName("aria2c").Length == 0)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "aria2c.exe",
                    Arguments = "--conf-path=aria2.conf",
                    //when use arg to open program, the WorkingDirectory is different, so process will not open
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    //more memory be used than CreateNoWindow
                    //WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    //if ProcessWindowStyle.Hidden set this false will show aria2 window, default is true
                    UseShellExecute = false
                });
                if (firstopen == null)
                    trayicon.ShowBalloonTip(3000, "", "aria2 opened.", ToolTipIcon.None);
                //maybe reduce memory usage
                trayicon.Text = null;
                trayicon.Text = "aria2 is running.";
                menuItem0.Text = null;
                menuItem0.Text = "Close &aria2";
                firstopen = null;
            }
            else if (firstopen == true)
            {
                trayicon.ShowBalloonTip(3000, "", "aria2 is \"already\" running.", ToolTipIcon.None);
                firstopen = null;
            }
            else
            {
                Close_aria2(Sender, e, false);
                trayicon.ShowBalloonTip(3000, "", "aria2 closed.", ToolTipIcon.None);
                trayicon.Text = null;
                menuItem0.Text = null;
                trayicon.Text = "aria2 already closed.";
                menuItem0.Text = "Open &aria2";
            }
        }

        private static void Open_UI(object Sender, EventArgs e)
        {
            //Start("chrome", @"ui\index.html") make ui\index.html as a external links, Firefox not
            Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"ui\index.html");
        }

        private static void Close_aria2(object Sender, EventArgs e, bool exit)
        {
            //aria2c checked in Toggle_aria2 when !exit || sometimes aria2c has closed when exit is true
            if (!exit || Process.GetProcessesByName("aria2c").Length > 0)
            {
                //aria2.shutdown need wait 3 seconds
                Call_RPC("saveSession", null, null, null);
                //Thread.Sleep(1000);
                //if aria2c not open at this time in the program, there is no aria2c
                //aria2c.CloseMainWindow();
                //nothing happend
                //aria2c.Close();
                //foreach (var x in Process.GetProcesses().Where(x => x.ProcessName.ToLower().StartsWith("aria2c")).ToList())
                foreach (var x in Process.GetProcessesByName("aria2c"))
                {
                    x.CloseMainWindow();
                    if (!x.HasExited)
                        x.Kill();
                }
            }
            if (exit)
            {
                //trayicon.Visible = false;
                //trayicon.Dispose();
                //Application.Exit();
                Application.ExitThread();
            }
        }

        private static bool Call_RPC(string method, string uri, string refer, string dir)
        {
            string secret = "";
            foreach (var line in File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "aria2.conf"))
            {
                if (line.StartsWith("rpc-secret"))
                {
                    secret = line.Substring(11);
                    break;
                }
            }
            string port = "6800";
            foreach (var line in File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "aria2.conf"))
            {
                if (line.StartsWith("rpc-listen-port"))
                {
                    port = line.Substring(16);
                    break;
                }
            }
            if (dir != null)
                dir = "\"dir\": \"" + dir.Replace("\\", "/") + "\"";
            try
            {
                //string json = JsonConvert.SerializeObject(new JObject { ["jsonrpc"] = "2.0", ["id"] = "m", ["method"] = "aria2." + method, ["params"] = new JArray { "token:secret", new JArray { "https://github.com/master.zip" } } });
                using (var webClient = new WebClient())
                {
                    webClient.Encoding = System.Text.Encoding.UTF8;
                    webClient.UploadString("http://localhost:" + port + "/jsonrpc", "POST", "{ \"jsonrpc\": \"2.0\", \"id\": \"m\", \"method\": \"aria2." + method + "\", \"params\": [\"token:" + secret + "\", [\"" + uri + "\"], {\"referer\": \"" + refer + "\", " + dir + " } ] }");
                    secret = null;
                    port = null;
                    return true;
                }
                //json = null;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "RPC Error", MessageBoxButtons.OK);
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            trayicon.Visible = false;
        }

        ~TrayManager()
        {
            Dispose(false);
        }
    }

    class TrayOnlyApplication : WindowsFormsApplicationBase
    {
        public TrayOnlyApplication()
        {
            IsSingleInstance = true;
            MainForm = new Form { ShowInTaskbar = false, WindowState = FormWindowState.Minimized };

            // Default behavior for single-instance is to activate main form
            // of original instance when second instance is run, which will show
            // the window (i.e. reset Visible to true) and restore the window
            // (i.e. reset WindowState to Normal). For a tray-only program,
            // we need to force the dummy window to stay invisible.
            MainForm.VisibleChanged += (s, e) => MainForm.Visible = false;
            MainForm.Resize += (s, e) => MainForm.WindowState = FormWindowState.Minimized;
        }
    }

    static class Program
    {
        [STAThread]

        static void Main(string[] args)
        {
            TrayManager trayManager = null;
            TrayOnlyApplication app = new TrayOnlyApplication();

            // Startup is raised only when no other instance of the
            // program is already running.
            app.Startup += (s, e) => trayManager = new TrayManager();
            if (args.Length > 0)
                app.Startup += (s, e) => trayManager.Arg_Call(new string[] {
                    e.CommandLine[0],
                    e.CommandLine.Count > 1 ? e.CommandLine[1] : null,
                    e.CommandLine.Count > 2 ? e.CommandLine[2] : null
                });
            /*void start_Call(object sender, StartupEventArgs e)
            {
                trayManager = new TrayManager();
                if (e.CommandLine.Count > 0)
                    trayManager.Arg_Call(e.CommandLine[0]);
            }
            app.Startup += new StartupEventHandler(start_Call);*/

            // StartNextInstance is run when the program if a
            // previously -run instance is still running.
            /*app.StartupNextInstance += (s, e) => trayManager.Arg_Call
            (
                e.CommandLine.Count > 0 ? e.CommandLine[0] : null
            );*/
            void next_Call(object sender, StartupNextInstanceEventArgs e)
            {
                if (e.CommandLine.Count > 0)
                    trayManager.Arg_Call(new string[] {
                        e.CommandLine[0],
                        e.CommandLine.Count > 1 ? e.CommandLine[1] : null,
                        e.CommandLine.Count > 2 ? e.CommandLine[2] : null
                    });
            }
            app.StartupNextInstance += new StartupNextInstanceEventHandler(next_Call);

            try
            {
                app.Run(args);
            }
            finally
            {
                trayManager?.Dispose();
            }
        }
    }
}


