using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace NotesAndTasks.Hooks
{
    /// <summary>
    /// Provides low-level keyboard hook functionality for capturing keyboard input events.
    /// </summary>
    public class KeyboardHook : IDisposable
    {
        private IntPtr hookID = IntPtr.Zero;
        private readonly NativeMethods.LowLevelHookProc hookCallback;
        private bool disposed;

        /// <summary>
        /// Gets the Windows hook identifier.
        /// </summary>
        public IntPtr HookID => hookID;

        /// <summary>
        /// Event raised when a key is pressed.
        /// </summary>
        public event EventHandler<KeyboardHookEventArgs> KeyDown;

        /// <summary>
        /// Event raised when a key is released.
        /// </summary>
        public event EventHandler<KeyboardHookEventArgs> KeyUp;

        /// <summary>
        /// Initializes a new instance of the KeyboardHook class.
        /// </summary>
        public KeyboardHook()
        {
            hookCallback = HookCallback;
        }

        /// <summary>
        /// Sets up the keyboard hook with the specified module handle.
        /// </summary>
        /// <param name="moduleHandle">The handle to the module containing the hook procedure.</param>
        public void SetHook(IntPtr moduleHandle)
        {
            if (hookID != IntPtr.Zero)
                throw new InvalidOperationException("Hook is already set");

            hookID = NativeMethods.SetWindowsHookEx(WinMessages.WH_KEYBOARD_LL, hookCallback, moduleHandle, 0);
            if (hookID == IntPtr.Zero)
                throw new InvalidOperationException("Failed to set keyboard hook");
        }

        /// <summary>
        /// Starts monitoring keyboard events.
        /// </summary>
        public void Start()
        {
            if (hookID == IntPtr.Zero)
            {
                using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
                using var curModule = curProcess.MainModule;
                if (curModule == null)
                    throw new InvalidOperationException("Failed to get current module");

                SetHook(NativeMethods.GetModuleHandle(curModule.ModuleName));
            }
        }

        /// <summary>
        /// Stops monitoring keyboard events.
        /// </summary>
        public void Stop()
        {
            if (hookID != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(hookID);
                hookID = IntPtr.Zero;
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
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

            return NativeMethods.CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the KeyboardHook.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the KeyboardHook and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Stop();
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the KeyboardHook class.
        /// </summary>
        ~KeyboardHook()
        {
            Dispose(false);
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