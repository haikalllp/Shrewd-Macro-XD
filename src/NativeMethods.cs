using System;
using System.Runtime.InteropServices;

namespace NotesAndTasks
{
    /// <summary>
    /// Contains P/Invoke declarations for Windows API functions and related structures.
    /// This class follows security best practices by isolating native method declarations.
    /// </summary>
    internal static class NativeMethods
    {
        #region Delegates
        /// <summary>
        /// Delegate for low-level keyboard and mouse hook callbacks
        /// </summary>
        internal delegate IntPtr LowLevelHookProc(int nCode, IntPtr wParam, IntPtr lParam);
        #endregion

        #region Structures
        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        #endregion

        #region User32.dll Functions
        /// <summary>
        /// Sets a Windows hook that monitors low-level keyboard or mouse input events
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(
            int idHook,
            LowLevelHookProc lpfn,
            IntPtr hMod,
            uint dwThreadId);

        /// <summary>
        /// Removes a previously set Windows hook
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// Passes the hook information to the next hook procedure in the current hook chain
        /// </summary>
        [DllImport("user32.dll")]
        internal static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        /// <summary>
        /// Retrieves the current cursor position
        /// </summary>
        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(out POINT lpPoint);

        /// <summary>
        /// Synthesizes mouse motion and button clicks
        /// </summary>
        [DllImport("user32.dll")]
        internal static extern void mouse_event(
            uint dwFlags,
            int dx,
            int dy,
            uint dwData,
            int dwExtraInfo);

        /// <summary>
        /// Synthesizes keystrokes, mouse motions, and button clicks
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint SendInput(
            uint nInputs,
            ref INPUT pInputs,
            int cbSize);
        #endregion

        #region Kernel32.dll Functions
        /// <summary>
        /// Retrieves a module handle for the specified module
        /// </summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion
    }
} 