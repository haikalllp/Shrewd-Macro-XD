using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NotesAndTasks.Hooks
{
    /// <summary>
    /// Provides low-level keyboard hook functionality for capturing keyboard input events.
    /// </summary>
    public class KeyboardHook : BaseHook
    {
        /// <summary>
        /// Event raised when a key is pressed.
        /// </summary>
        public event EventHandler<KeyboardHookEventArgs> KeyDown;

        /// <summary>
        /// Event raised when a key is released.
        /// </summary>
        public event EventHandler<KeyboardHookEventArgs> KeyUp;

        /// <summary>
        /// Gets the Windows hook type for keyboard events.
        /// </summary>
        protected override int HookType => WinMessages.WH_KEYBOARD_LL;

        /// <summary>
        /// The hook callback that processes keyboard input events.
        /// </summary>
        protected override IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var hookStruct = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);

                if (wParam == (IntPtr)WinMessages.WM_KEYDOWN || wParam == (IntPtr)WinMessages.WM_SYSKEYDOWN)
                {
                    KeyDown?.Invoke(this, new KeyboardHookEventArgs((Keys)hookStruct.vkCode));
                }
                else if (wParam == (IntPtr)WinMessages.WM_KEYUP || wParam == (IntPtr)WinMessages.WM_SYSKEYUP)
                {
                    KeyUp?.Invoke(this, new KeyboardHookEventArgs((Keys)hookStruct.vkCode));
                }
            }

            return NativeMethods.CallNextHookEx(HookID, nCode, wParam, lParam);
        }
    }

    /// <summary>
    /// Event arguments for keyboard hook events.
    /// </summary>
    public class KeyboardHookEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the virtual key code of the key.
        /// </summary>
        public Keys VirtualKeyCode { get; }

        /// <summary>
        /// Initializes a new instance of the KeyboardHookEventArgs class.
        /// </summary>
        /// <param name="virtualKeyCode">The virtual key code.</param>
        public KeyboardHookEventArgs(Keys virtualKeyCode)
        {
            VirtualKeyCode = virtualKeyCode;
        }
    }
} 