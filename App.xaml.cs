using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using WinForms = System.Windows.Forms;
using System.Diagnostics;

namespace BabySmash
{
   public partial class App : Application
   {
      private static readonly InterceptKeys.LowLevelKeyboardProc _proc = HookCallback;
      private static IntPtr _hookID = IntPtr.Zero;

      private void Application_Startup(object sender, StartupEventArgs e)
      {
         Controller c = new Controller();
         c.Launch();
      }

      public App()
      {
         ShutdownMode = ShutdownMode.OnExplicitShutdown;
         try
         {
            _hookID = InterceptKeys.SetHook(_proc);
         }
         catch
         {
            if (_hookID != IntPtr.Zero)
               InterceptKeys.UnhookWindowsHookEx(_hookID);
         }
      }

      public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
      {
         if (nCode >= 0)
         {
            bool Alt = (WinForms.Control.ModifierKeys & Keys.Alt) != 0;
            bool Control = (WinForms.Control.ModifierKeys & Keys.Control) != 0;

            //Prevent ALT-TAB and CTRL-ESC by eating TAB and ESC. Also kill Windows Keys.
            int vkCode = Marshal.ReadInt32(lParam);
            Keys key = (Keys)vkCode;

            if (Alt && key == Keys.F4)
            {
               Application.Current.Shutdown();
               return (IntPtr)1; //handled
            }
            if (key == Keys.LWin ||key == Keys.RWin) return (IntPtr)1; //handled
            if (Alt && key == Keys.Tab)  return (IntPtr)1; //handled
            if (Alt && key == Keys.Space) return (IntPtr)1; //handled
            if (Control && key == Keys.Escape)return (IntPtr)1;
            if (key == Keys.None) return (IntPtr)1; //handled
            if (key <= Keys.Back) return (IntPtr)1; //handled
            if (key == Keys.Menu ) return (IntPtr)1; //handled
            if (key == Keys.Pause) return (IntPtr)1; //handled
            if (key == Keys.Help) return (IntPtr)1; //handled
            if (key == Keys.Sleep) return (IntPtr)1; //handled
            if (key == Keys.Apps) return (IntPtr)1; //handled
            if (key >= Keys.KanaMode && key <= Keys.HanjaMode) return (IntPtr)1; //handled
            if (key >= Keys.IMEConvert && key <= Keys.IMEModeChange) return (IntPtr)1; //handled
            if (key >= Keys.BrowserBack && key <= Keys.BrowserHome) return (IntPtr)1; //handled
            if (key >= Keys.MediaNextTrack && key <= Keys.OemClear) return (IntPtr)1; //handled

            Debug.WriteLine(vkCode.ToString() + " " + key);


         }
         return InterceptKeys.CallNextHookEx(_hookID, nCode, wParam, lParam);
      }
   }
}