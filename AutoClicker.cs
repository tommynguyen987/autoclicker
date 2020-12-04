using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using System.Text;
namespace MyUtility
{
    public partial class AutoClicker : Form
    {
        #region Properties

        static string dataPath = Application.StartupPath + "\\data.txt";
        public static string logFilePath = Application.StartupPath + "\\logs.txt";
        public static string usedProxyPath = Application.StartupPath + "\\usedproxylist.txt";
        static string proxyPath = Application.StartupPath + "\\validproxylist.txt";
                                                            //"\\proxylist.txt"; 
                                                            //"\\xroxy.com.txt";
                                                            //"\\dogdevproxy.net.txt";
        const int START_AUTO_CLICK = 1;
        const int START_CHECK_PROXY = -1;
        static string appPath1 = "C:\\Program Files\\Mozilla Firefox\\firefox.exe -p User120";
        static string appPath2 = "C:\\Program Files (x86)\\Mozilla Firefox\\firefox.exe -p User120";
        static string[] arrApps = { 
            //"C:\\Users\\"+Environment.UserName.ToLower()+"\\AppData\\Local\\CocCoc\\Browser\\Application\\browser.exe", 
            "C:\\Program Files (x86)\\Mozilla Firefox\\firefox.exe",
            "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe", 
            "C:\\Program Files (x86)\\Internet Explorer\\iexplore.exe", 
            "C:\\Program Files (x86)\\Safari\\Safari.exe", 
            "C:\\Program Files (x86)\\Opera\\opera.exe" 
        };

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        [DllImport("user32.dll")]
        static extern bool AnimateWindow(System.IntPtr hWnd, int time, AnimateWindowFlags flags);
        [System.Flags]
        enum AnimateWindowFlags
        {
            AW_HOR_POSITIVE = 0x00000001,
            AW_HOR_NEGATIVE = 0x00000002,
            AW_VER_POSITIVE = 0x00000004,
            AW_VER_NEGATIVE = 0x00000008,
            AW_CENTER = 0x00000010,
            AW_HIDE = 0x00010000,
            AW_ACTIVATE = 0x00020000,
            AW_SLIDE = 0x00040000,
            AW_BLEND = 0x00080000
        }
        
        public AutoClicker()
        {
            InitializeComponent();
            //StartUpManager.AddApplicationToCurrentUserStartup("Auto Clicker");
        }

        #endregion

        #region Methods

        // Put application into system tray
        private void WindowsInSystemTray(bool inTray)
        {
            if (inTray)
            {
                this.ShowInTaskbar = false;
                AnimateWindow(this.Handle, 50, AnimateWindowFlags.AW_BLEND | AnimateWindowFlags.AW_HIDE);
                myNotifyIcon.Visible = true;
                myNotifyIcon.ShowBalloonTip(500);
            }
            else
            {
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                AnimateWindow(this.Handle, 700, AnimateWindowFlags.AW_BLEND | AnimateWindowFlags.AW_ACTIVATE);
                this.Activate();
                myNotifyIcon.Visible = false;
            }
        }
       
        // Handle mouse click event
        private string GetAppPath()
        {
            if (File.Exists(appPath1))
            {
                return appPath1;
            }
            else if (File.Exists(appPath2))
            {
                return appPath2;
            }
            else
            {
                return string.Empty;
            }
        }
        private void DoMouseClick(Point pt)
        {
            //Call the imported function with the cursor's current position
            Cursor.Position = pt;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)pt.X, (uint)pt.Y, 0, 0);
        }
        private void SetWaitCursor(bool isWaitEnabled)
        {
            if (isWaitEnabled)
            {
                Application.UseWaitCursor = isWaitEnabled;//from the Form/Window instance
                Cursor.Current = Cursors.WaitCursor;
            }
            else
            {
                Application.UseWaitCursor = isWaitEnabled;//from the Form/Window instance
                Cursor.Current = Cursors.Default;
            }
            Application.DoEvents();//messages pumped to update controls    
        }
        private void AutoClickerByScreen(string url)
        {
            int x = 0, y = 0;
            int screenWidth = 0;
            int screenHeight = 0;
            string appPath = "", os = "";
            StringBuilder sbOperation = new StringBuilder();
            screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            DoMouseClick(new Point(x, y));

            appPath = GetAppPath();
            if (string.IsNullOrEmpty(appPath))
            {
                MessageBox.Show("Bạn chưa cài đặt trình duyệt Firefox!" + Environment.NewLine + "Hãy cài đặt trình duyệt và chạy lại chương trình nhé.");
                return;
            }
            
            ProxyServerConfigurator.SetProxyForFirefox(Handler.GetProxy(proxyPath)[Handler.GetProxyIndex(url, proxyPath)], Handler.ProxyConfigPath);
            bool isInternetConnected = true;
            do
            {
                isInternetConnected = Handler.NetworkIsAvailable();
            } while (!isInternetConnected);

            switch (screenWidth)
            {
                #region Resolution 1920 x 1080
                case 1920:
                    {
                        x = 827;
                        y = 408;
                        //for (int i = 0; i < arrApps.Length; i++)
                        //{
                        try
                        {
                            //if (File.Exists(arrApps[i]))
                            if (!string.IsNullOrEmpty(appPath))
                            {
                                Process process = new Process();
                                Handler.StartProcess(process, appPath, Handler.GetData(dataPath)[Handler.RandomIndex(Handler.GetDataLength(dataPath))]);
                                Thread.Sleep(30000);
                                SetWaitCursor(false);
                                if (Handler.IsWindowsXP) // WIndows XP
                                {

                                }
                                else if (Handler.IsWindows7) // Windows 7
                                {

                                }
                                else // Windows 8
                                {                                        
                                //if (i == 1) // google chrome
                                //{
                                //    y -= 25;
                                //}
                                //if (i == 1)
                                //{
                                //    y += 13;
                                //}
                                //if (i == 2) // internet explore
                                //{
                                //    x += 6;
                                //    y += 5;                                            
                                //}
                                }
                                DoMouseClick(new Point(x, y));
                                SetWaitCursor(true);
                                System.Threading.Thread.Sleep(30000);
                                //StopProcess(process, i);
                                Handler.StopProcess(process, appPath);
                                DoMouseClick(new Point(0, 0));
                                //ProxyServerConfigurator.SetProxyForFirefox(GetProxy()[GetProxyIndex(url)], AutoClickerHandler.ProxyConfigPath);
                                System.Threading.Thread.Sleep(900000); // 3 minutes
                            }
                        }
                        catch (Exception)
                        {
                            Handler.ClearCacheLocalAll();
                            //continue;
                        }
                        //}
                        break;
                    }
                #endregion

                #region Resolution 1366 x 768
                case 1366:
                    {
                        x = 547;
                        y = 395;
                        //for (int i = 0; i < arrApps.Length; i++)
                        //{
                            try
                            {
                                //if (File.Exists(arrApps[i]))
                                if(!string.IsNullOrEmpty(appPath))
                                {
                                    Process process = new Process();
                                    //StartProcess(process, arrApps[i], GetData()[RandomIndex(DataLength)]);
                                    string link = Handler.GetData(dataPath)[Handler.RandomIndex(Handler.GetDataLength(dataPath))];

                                    sbOperation.AppendLine("<--- Start process --->");                                    
                                    sbOperation.AppendLine(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"));
                                    sbOperation.AppendLine("Start process: " + appPath);
                                    sbOperation.AppendLine("Link: " + link);
                                    Handler.Log(sbOperation.ToString());

                                    Handler.StartProcess(process, appPath, link);
                                    Thread.Sleep(30000);
                                    SetWaitCursor(false);
                                    if (Handler.IsWindowsXP) // WIndows XP
                                    {
                                        os = "Windows XP";
                                    }
                                    else if (Handler.IsWindows7) // Windows 7
                                    {
                                        os = "Windows 7";
                                        if (screenHeight == 663)
                                        {
                                            x -= 250;
                                            y -= 200;
                                        }
                                        
                                    }
                                    else // Windows 8
                                    {
                                       os = "Windows 8";
                                    }
                                    DoMouseClick(new Point(x, y));

                                    // Log handle mouse click
                                    sbOperation = new StringBuilder();
                                    sbOperation.AppendLine(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"));
                                    sbOperation.AppendLine("Do event mouse click on " + os);
                                    sbOperation.AppendLine("Position ( x: "  + x + ", y: " + y + " )");
                                    Handler.Log(sbOperation.ToString());

                                    SetWaitCursor(true);
                                    Thread.Sleep(30000);
                                    //StopProcess(process, i);
                                    Handler.StopProcess(process, appPath);
                                    // Log end process
                                    sbOperation = new StringBuilder();
                                    sbOperation.AppendLine(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"));
                                    sbOperation.AppendLine("End process: " + appPath);
                                    sbOperation.AppendLine("Link: " + link);
                                    Handler.Log(sbOperation.ToString());
                                    
                                    DoMouseClick(new Point(0, 0));
                                    //ProxyServerConfigurator.SetProxy(GetProxy()[GetProxyIndex(url)]);
                                    Thread.Sleep(900000); // 15 minutes
                                }
                            }
                            catch (Exception ex)
                            {
                                Handler.ClearCacheLocalAll();
                                //continue;
                                // Log error
                                sbOperation = new StringBuilder();
                                sbOperation.AppendLine(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"));
                                sbOperation.AppendLine("Error: " + ex.Message);
                                Handler.Log(sbOperation.ToString());
                            }
                        //}
                        break;
                    }
                #endregion

                #region Resolution 1280 x 720
                case 1280:
                    {
                        x = 507;
                        y = 400;
                        //for (int i = 0; i < arrApps.Length; i++)
                        //{
                            try
                            {
                                //if (File.Exists(arrApps[i]))
                                if (!string.IsNullOrEmpty(appPath))
                                {
                                    Process process = new Process();
                                    //StartProcess(process, arrApps[i], GetData()[RandomIndex(DataLength)]);
                                    Handler.StartProcess(process, appPath, Handler.GetData(dataPath)[Handler.RandomIndex(Handler.GetDataLength(dataPath))]);
                                    Thread.Sleep(40000);
                                    if (Handler.IsWindowsXP) // WIndows XP
                                    {

                                    }
                                    else if (Handler.IsWindows7) // Windows 7
                                    {

                                    }
                                    else // Windows 8
                                    {
                                        //if (i == 1)
                                        //{
                                        //    y += 30;
                                        //}
                                        //if (i == 2)
                                        //{
                                        //    x += 6;
                                        //    y -= 40;
                                        //}
                                    }
                                    DoMouseClick(new Point(x, y));
                                    SetWaitCursor(true);
                                    Thread.Sleep(40000);
                                    //StopProcess(process, i);
                                    Handler.StopProcess(process, appPath);
                                    DoMouseClick(new Point(0, 0));
                                    //ProxyServerConfigurator.SetProxy(GetProxy()[GetProxyIndex(url)]);
                                    Thread.Sleep(900000);
                                }
                            }
                            catch (Exception)
                            {
                                Handler.ClearCacheLocalAll();
                                //continue;
                            }
                        //}
                        break;
                    }
                #endregion

                #region Resolution 1024 x 768
                case 1024:
                    {
                        x = 507;
                        y = 380;
                        //for (int i = 0; i < arrApps.Length; i++)
                        //{
                        try
                        {
                            //if (File.Exists(arrApps[i]))
                            if (!string.IsNullOrEmpty(appPath))
                            {
                                Process process = new Process();
                                //StartProcess(process, arrApps[i], GetData()[RandomIndex(DataLength)]);
                                Handler.StartProcess(process, appPath, Handler.GetData(dataPath)[Handler.RandomIndex(Handler.GetDataLength(dataPath))]);
                                Thread.Sleep(40000);
                                if (Handler.IsWindowsXP) // WIndows XP
                                {

                                }
                                else if (Handler.IsWindows7) // Windows 7
                                {

                                }
                                else // Windows 8
                                {
                                    //if (i == 1)
                                    //{
                                    //    y += 30;
                                    //}
                                    //if (i == 2)
                                    //{
                                    //    x += 6;
                                    //    y -= 40;
                                    //}
                                }
                                DoMouseClick(new Point(x, y));
                                SetWaitCursor(true);
                                Thread.Sleep(40000);
                                //StopProcess(process, i);
                                Handler.StopProcess(process, appPath);
                                DoMouseClick(new Point(0, 0));
                                //ProxyServerConfigurator.SetProxy(GetProxy()[GetProxyIndex(url)]);
                                Thread.Sleep(900000);
                            }
                        }
                        catch (Exception)
                        {
                            Handler.ClearCacheLocalAll();
                            //continue;
                        }
                        //}
                        break;
                    }
                #endregion
            }
        }
        
        // Load application and repeat by the time
        private void AutoClicker_Load(object sender, EventArgs e)
        {
            SetWaitCursor(true);
            //myNotifyIcon.BalloonTipText = "Your application is still working" + System.Environment.NewLine + "Double click into icon to show application.";
            //WindowsInSystemTray(true);
            if (setAutoToolStripMenuItem.Checked)
            {
                myBackgroundWorker.RunWorkerAsync();
                timer1.Enabled = true;
                timer1.Interval = 1800000; // 30 minutes
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            myBackgroundWorker.RunWorkerAsync();
        }       

        // Methods of menu strip
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Start the BackgroundWorker.
            myBackgroundWorker.RunWorkerAsync();            
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
            timer1.Enabled = true;
            timer1.Interval = 1800000; // 30 minutes
        }
        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myBackgroundWorker.CancelAsync();
            myBackgroundWorker.Dispose();
            startToolStripMenuItem.Enabled = true;
            stopToolStripMenuItem.Enabled = false;
        }
        private void setAutoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (setAutoToolStripMenuItem.Checked)
            {
                startToolStripMenuItem.Enabled = false;
            }
            else
            {
                startToolStripMenuItem.Enabled = true;
            }
        }
        private void SetEnableMenuItem(bool isChecked, bool isEnabled)
        {
            startToolStripMenuItem.Enabled = isEnabled;
            stopToolStripMenuItem.Enabled = isEnabled;
        }
        private void showMainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowsInSystemTray(false);
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
            myNotifyIcon.Dispose();
        }
        private void AutoClicker_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            WindowsInSystemTray(true);
        }
        private void AutoClicker_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                WindowsInSystemTray(true);
            }
        }
        private void myNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            WindowsInSystemTray(false);
        }
        private void myBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (!e.Cancel)
            {
                Thread.Sleep(60000); // 1 minutes    
                AutoClickerByScreen(Handler.Ouo);                                             
            }
            SetEnableMenuItem(true, true);
        }

        #endregion        
    }   
}