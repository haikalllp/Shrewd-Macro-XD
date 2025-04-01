using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NotesAndTasks.Hooks
{
    /// <summary>
    /// Manages low-level mouse hook functionality.
    /// </summary>
    public class MouseHook : BaseHook
    {
        /// <summary>
        /// Event raised when a mouse button is pressed.
        /// </summary>
        public event EventHandler<MouseHookEventArgs> MouseDown;

        /// <summary>
        /// Event raised when a mouse button is released.
        /// </summary>
        public event EventHandler<MouseHookEventArgs> MouseUp;

        /// <summary>
        /// Gets the Windows hook type for mouse events.
        /// </summary>
        protected override int HookType => WinMessages.WH_MOUSE_LL;

        /// <summary>
        /// The hook callback that processes mouse input events.
        /// </summary>
        protected override IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
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
            return NativeMethods.CallNextHookEx(HookID, nCode, wParam, lParam);
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