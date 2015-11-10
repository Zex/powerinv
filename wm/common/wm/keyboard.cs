/** Window Manager - Keyboard
 * Author: Zex <top_zlynch AT yahoo.com>
 */
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using fw;

namespace wm
{
    /**
     * \sa http://autohotkey.com/board/topic/1738-comprehensive-list-of-windows-hotkeys
     */
    public class Keyboard : IDisposable
    {
        private static IntPtr _keyboard_hk = IntPtr.Zero;
        private static lowlevel.HookDelegate _delegate;

        public static IntPtr keyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            Trace.WriteLine(string.Format("Keyboard.keyboardProc()"));

            if (nCode >= 0 && (wParam == (IntPtr)lowlevel.WM_KEYDOWN || wParam == (IntPtr)lowlevel.WM_SYSKEYDOWN))
            {
                lowlevel.KBDLLHOOKSTRUCT key = (lowlevel.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(lowlevel.KBDLLHOOKSTRUCT));

                if ((0 != (key.flags & lowlevel.KBDLLHOOKSTRUCTFlags.LLKHF_ALTDOWN)))
                {
                    switch ((Keys)key.vkCode)
                    {
                        case Keys.Tab:      // TAB + ALT
                        case Keys.F4:       // F4 + ALT
                        case Keys.Escape:   // ESC + ALT
                        {
                            Trace.WriteLine(string.Format("Keyboard.keyboardProc(): ALT + {0}", (Keys)key.vkCode));
                            return (IntPtr)1;
                        }
                    }
                }
                
                switch ((Keys)key.vkCode)
                {
                    case Keys.LWin:
                    case Keys.RWin:
                    {
                        Trace.WriteLine(string.Format("Keyboard.keyboardProc(): WIN + {0}", (Keys)key.vkCode));
                        return (IntPtr)1;
                    }
                }

                // TODO: take control
            }
            
            return lowlevel.CallNextHookEx(_keyboard_hk, nCode, wParam, lParam);
        }

        public virtual void Initialize()
        {
            Trace.WriteLine("Keyboard.Initialize()");
            _delegate = new lowlevel.HookDelegate(keyboardProc);
            _keyboard_hk = lowlevel.SetWindowsHookEx(lowlevel.WH_KEYBOARD_LL, _delegate,
                lowlevel.GetModuleHandle(null), 0);
        }

        public virtual void Dispose()
        {
            Trace.WriteLine("Keyboard.Dispose()");
            lowlevel.UnhookWindowsHookEx(_keyboard_hk);
        }

        public Keyboard()
        {
            Trace.WriteLine("Keyboard.Keyboard()");
        }
    }
}
