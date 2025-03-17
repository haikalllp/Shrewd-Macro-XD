using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NotesAndTasks
{
    /// <summary>
    /// Manages low-level keyboard hook functionality.
    /// </summary>
    public class KeyboardHook : IDisposable
    {
        private IntPtr hookID = IntPtr.Zero;
        private readonly NativeMethods.LowLevelHookProc hookProc;
        private bool disposed;

        /// <summary>
        /// Event raised when a key is pressed.
        /// </summary>
        public event EventHandler<KeyEventArgs> KeyDown;

        /// <summary>
        /// Event raised when a key is released.
        /// </summary>
        public event EventHandler<KeyEventArgs> KeyUp;

        /// <summary>
        /// Initializes a new instance of the KeyboardHook class.
        /// </summary>
        public KeyboardHook()
        {
            hookProc = HookCallback;
            Start();
        }

        /// <summary>
        /// Starts the keyboard hook.
        /// </summary>
        public void Start()
        {
            if (hookID == IntPtr.Zero)
            {
                using var curProcess = Process.GetCurrentProcess();
                using var curModule = curProcess.MainModule;

                if (curModule != null)
                {
                    hookID = NativeMethods.SetWindowsHookEx(
                        WinMessages.WH_KEYBOARD_LL,
                        hookProc,
                        NativeMethods.GetModuleHandle(curModule.ModuleName),
                        0);

                    if (hookID == IntPtr.Zero)
                    {
                        throw new InvalidOperationException("Failed to initialize keyboard hook");
                    }
                }
            }
        }

        /// <summary>
        /// Stops the keyboard hook.
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
                var keyboardData = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
                bool isKeyDown = false;

                switch ((int)wParam)
                {
                    case WinMessages.WM_KEYDOWN:
                    case WinMessages.WM_SYSKEYDOWN:
                        isKeyDown = true;
                        break;
                    case WinMessages.WM_KEYUP:
                    case WinMessages.WM_SYSKEYUP:
                        break;
                    default:
                        return NativeMethods.CallNextHookEx(hookID, nCode, wParam, lParam);
                }

                var args = new KeyEventArgs(keyboardData.vkCode);
                if (isKeyDown)
                    KeyDown?.Invoke(this, args);
                else
                    KeyUp?.Invoke(this, args);
            }
            return NativeMethods.CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Disposes of the keyboard hook resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the keyboard hook resources.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources.</param>
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
        /// Finalizer to ensure hook is unregistered.
        /// </summary>
        ~KeyboardHook()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Event arguments for keyboard events.
    /// </summary>
    public class KeyEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the virtual key code of the key.
        /// </summary>
        public uint VirtualKeyCode { get; }

        /// <summary>
        /// Initializes a new instance of the KeyEventArgs class.
        /// </summary>
        /// <param name="virtualKeyCode">The virtual key code.</param>
        public KeyEventArgs(uint virtualKeyCode)
        {
            VirtualKeyCode = virtualKeyCode;
        }
    }
} 