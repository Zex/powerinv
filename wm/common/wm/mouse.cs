/** Window Manager - Mouse
 * Author: Zex <top_zlynch AT yahoo.com>
 */
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using fw;

namespace wm
{
    public class Mouse : IDisposable
    {
        private static IntPtr _mouse_hk = IntPtr.Zero;
        private static lowlevel.HookDelegate _delegate;

        public static IntPtr MouseProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //Trace.WriteLine("Mouse.MouseProc()"); // too noisy here

            if (nCode >= 0)
            {
                lowlevel.MouseHookStruct mouse_data = (lowlevel.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(lowlevel.MouseHookStruct));
                lowlevel.POINT mouse = mouse_data.pt;

                switch ((UInt32)wParam)
                {
                    case lowlevel.WM_LBUTTONDBLCLK:
                    case lowlevel.WM_RBUTTONDBLCLK:
                    {
                        Trace.WriteLine(string.Format("Mouse.MouseProc() WM_LBUTTONDBLCLK | WM_RBUTTONDBLCLK position: ({0}, {1})", mouse.x, mouse.y));
                        return (IntPtr)1;
                    }
                    case lowlevel.WM_RBUTTONDOWN:
                    case lowlevel.WM_RBUTTONUP:
                    {
                        Trace.WriteLine(string.Format("Mouse.MouseProc() WM_RBUTTONDOWN | WM_RBUTTONUP position: ({0}, {1})", mouse.x, mouse.y));
                        return (IntPtr)1;
                    }
                    case lowlevel.WM_LBUTTONDOWN:
                    case lowlevel.WM_LBUTTONUP:
                    case lowlevel.WM_MOUSEMOVE:
                    case lowlevel.WM_MOUSEHOVER:
                    case lowlevel.WM_MOUSEACTIVATE:
                    case lowlevel.WM_MOUSEHWHEEL:
                    case lowlevel.WM_MOUSELEAVE:
                    {
                        break;
                    }
                    default:
                    {
                        Trace.WriteLine(string.Format("Mouse.MouseProc() Unhandled mouse action wParam = {0}, ({1}, {2})", (UInt32)wParam, mouse.x, mouse.y));
                        break;
                    }
                }
            }

            return lowlevel.CallNextHookEx(_mouse_hk, nCode, wParam, lParam);
        }

        public virtual void Initialize()
        {
            Trace.WriteLine("Mouse.Initialize()");
            _delegate = new lowlevel.HookDelegate(MouseProc);
            _mouse_hk = lowlevel.SetWindowsHookEx(lowlevel.WH_MOUSE_LL, _delegate,
                lowlevel.GetModuleHandle(null), 0);
        }

        public virtual void Dispose()
        {
            Trace.WriteLine("Mouse.Dispose()");
            lowlevel.UnhookWindowsHookEx(_mouse_hk);
        }

        public Mouse()
        {
            Trace.WriteLine("Mouse.Mouse()");
        }
    }
}
