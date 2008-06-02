using System.Windows;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using WinForms = System.Windows.Forms;

namespace BabySmash
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static IntPtr _hookID = IntPtr.Zero;
        static InterceptKeys.LowLevelKeyboardProc _proc = HookCallback;
        Window1 mainWindow = null;
        public App()
            : base()
        {
            _hookID = InterceptKeys.SetHook(_proc);
            try
            {
                this.ShutdownMode = ShutdownMode.OnLastWindowClose; //TODO: Should this be OnMainWindowClose?
                mainWindow = new Window1();
                MainWindow.WindowState = WindowState.Maximized; //Do it here, rather than in XAML otherwise multimon won't work.
                mainWindow.Show();
            }
            catch (Exception e)
            {
                //TODO: Logging?
                InterceptKeys.UnhookWindowsHookEx(_hookID);
            }
        }

        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                //Prevent ALT-TAB and CTRL-ESC by eating TAB and ESC. Also kill Windows Keys.
                int vkCode = Marshal.ReadInt32(lParam);
                if ((WinForms.Keys)vkCode == WinForms.Keys.LWin || 
                    (WinForms.Keys)vkCode == WinForms.Keys.RWin)
                {
                    //Debug.WriteLine("ATE: " + (WinForms.Keys)vkCode);
                    return (IntPtr)1; //handled
                }

                bool Alt = (System.Windows.Forms.Control.ModifierKeys & WinForms.Keys.Alt) != 0;
                bool Control = (System.Windows.Forms.Control.ModifierKeys & WinForms.Keys.Control) != 0;
 
                if (Alt && (WinForms.Keys)vkCode == WinForms.Keys.Tab)
                {
                    //Debug.WriteLine("ATE ALT-TAB: " + (WinForms.Keys)vkCode);
                    return (IntPtr)1; //handled
                }

                if (Control && (WinForms.Keys)vkCode == WinForms.Keys.Escape)
                {
                    //Debug.WriteLine("ATE CTRL-ESC: " + (WinForms.Keys)vkCode);
                    return (IntPtr)1; //handled
                }
                //Debug.WriteLine("HOOKED: " + (WinForms.Keys)vkCode);
            }
            return InterceptKeys.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //UNDONE: Make a Window instance for each screen, position them, show them, then maximize them. 
            //TODO: Now, how to respond to events on all screens at once?
            foreach (WinForms.Screen s in WinForms.Screen.AllScreens)
            {
                if (s.Primary == false)
                {
                    Window1 w = new Window1();
                    w.WindowStartupLocation = WindowStartupLocation.Manual; //key!
                    Debug.Write("Found screen: " + s.DeviceName);
                    w.Left = s.WorkingArea.Left;
                    Debug.Write("  Left: " + s.WorkingArea.Left);
                    w.Top = s.WorkingArea.Top;
                    Debug.Write("   Top: " + s.WorkingArea.Top);
                    w.Width = s.WorkingArea.Width;
                    Debug.Write(" Width: " + s.WorkingArea.Width);
                    w.Height = s.WorkingArea.Height;
                    Debug.Write("Height: " + s.WorkingArea.Height);
                    w.WindowStyle = WindowStyle.None;
                    w.Topmost = true;
                    w.Owner = mainWindow;
                    w.Show();
                    w.WindowState = WindowState.Maximized;
                }
                mainWindow.Focus(); 
            }; 
        }
    }
}
