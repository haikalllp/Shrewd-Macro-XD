using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace NotesAndTasks.Hooks
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
        /// Gets the Windows hook identifier.
        /// </summary>
        public IntPtr HookID => hookID;

        /// <summary>
        /// Event raised when a mouse button is pressed.
        /// </summary>
        public event EventHandler<MouseHookEventArgs> MouseDown;

        /// <summary>
        /// Event raised when a mouse button is released.
        /// </summary>
        public event EventHandler<MouseHookEventArgs> MouseUp;

        /// <summary>
        /// Initializes a new instance of the MouseHook class.
        /// </summary>
        public MouseHook()
        {
            hookProc = HookCallback;
        }

        /// <summary>
        /// Sets up the mouse hook with the specified module handle.
        /// </summary>
        /// <param name="moduleHandle">The handle to the module containing the hook procedure.</param>
        public void SetHook(IntPtr moduleHandle)
        {
            if (hookID != IntPtr.Zero)
                throw new InvalidOperationException("Hook is already set");

            hookID = NativeMethods.SetWindowsHookEx(WinMessages.WH_MOUSE_LL, hookProc, moduleHandle, 0);
            if (hookID == IntPtr.Zero)
                throw new InvalidOperationException("Failed to set mouse hook");
        }

        /// <summary>
        /// Starts monitoring mouse events.
        /// </summary>
        public void Start()
        {
            if (hookID == IntPtr.Zero)
            {
                using var curProcess = Process.GetCurrentProcess();
                using var curModule = curProcess.MainModule;
                if (curModule == null)
                    throw new InvalidOperationException("Failed to get current module");

                SetHook(NativeMethods.GetModuleHandle(curModule.ModuleName));
            }
        }

        /// <summary>
        /// Stops monitoring mouse events.
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
                    var args = new MouseHookEventArgs(button, mouseData.pt.X, mouseData.pt.Y);
                    if (isDown)
                        MouseDown?.Invoke(this, args);
                    else
                        MouseUp?.Invoke(this, args);
                }
            }
            return NativeMethods.CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the MouseHook.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the MouseHook and optionally releases the managed resources.
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
        /// Finalizes an instance of the MouseHook class.
        /// </summary>
        ~MouseHook()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Event arguments for mouse hook events.
    /// </summary>
    public class MouseHookEventArgs : EventArgs
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
        /// Initializes a new instance of the MouseHookEventArgs class.
        /// </summary>
        /// <param name="button">The mouse button.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public MouseHookEventArgs(MouseButtons button, int x, int y)
        {
            Button = button;
            X = x;
            Y = y;
        }
    }
} 