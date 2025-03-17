using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NotesAndTasks
{
    /// <summary>
    /// Manages low-level mouse hook functionality.
    /// </summary>
    public class MouseHook : IDisposable
    {
        private IntPtr hookID = IntPtr.Zero;
        private readonly NativeMethods.LowLevelHookProc hookProc;
        private bool disposed;

        /// <summary>
        /// Event raised when a mouse button is pressed.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseDown;

        /// <summary>
        /// Event raised when a mouse button is released.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseUp;

        /// <summary>
        /// Initializes a new instance of the MouseHook class.
        /// </summary>
        public MouseHook()
        {
            hookProc = HookCallback;
            Start();
        }

        /// <summary>
        /// Starts the mouse hook.
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
                        WinMessages.WH_MOUSE_LL,
                        hookProc,
                        NativeMethods.GetModuleHandle(curModule.ModuleName),
                        0);

                    if (hookID == IntPtr.Zero)
                    {
                        throw new InvalidOperationException("Failed to initialize mouse hook");
                    }
                }
            }
        }

        /// <summary>
        /// Stops the mouse hook.
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
                var mouseData = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
                var button = MouseButtons.None;
                bool isDown = false;

                switch ((int)wParam)
                {
                    case WinMessages.WM_LBUTTONDOWN:
                        button = MouseButtons.Left;
                        isDown = true;
                        break;
                    case WinMessages.WM_LBUTTONUP:
                        button = MouseButtons.Left;
                        break;
                    case WinMessages.WM_RBUTTONDOWN:
                        button = MouseButtons.Right;
                        isDown = true;
                        break;
                    case WinMessages.WM_RBUTTONUP:
                        button = MouseButtons.Right;
                        break;
                    case WinMessages.WM_MBUTTONDOWN:
                        button = MouseButtons.Middle;
                        isDown = true;
                        break;
                    case WinMessages.WM_MBUTTONUP:
                        button = MouseButtons.Middle;
                        break;
                    case WinMessages.WM_XBUTTONDOWN:
                        button = (mouseData.mouseData >> 16) == WinMessages.XBUTTON1 ? 
                            MouseButtons.XButton1 : MouseButtons.XButton2;
                        isDown = true;
                        break;
                    case WinMessages.WM_XBUTTONUP:
                        button = (mouseData.mouseData >> 16) == WinMessages.XBUTTON1 ? 
                            MouseButtons.XButton1 : MouseButtons.XButton2;
                        break;
                }

                if (button != MouseButtons.None)
                {
                    var args = new MouseEventArgs(button, mouseData.pt.X, mouseData.pt.Y);
                    if (isDown)
                        MouseDown?.Invoke(this, args);
                    else
                        MouseUp?.Invoke(this, args);
                }
            }
            return NativeMethods.CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Disposes of the mouse hook resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the mouse hook resources.
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
        ~MouseHook()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Event arguments for mouse events.
    /// </summary>
    public class MouseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the mouse button that triggered the event.
        /// </summary>
        public MouseButtons Button { get; }

        /// <summary>
        /// Gets the X coordinate of the mouse cursor.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the Y coordinate of the mouse cursor.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Initializes a new instance of the MouseEventArgs class.
        /// </summary>
        /// <param name="button">The mouse button.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public MouseEventArgs(MouseButtons button, int x, int y)
        {
            Button = button;
            X = x;
            Y = y;
        }
    }
} 