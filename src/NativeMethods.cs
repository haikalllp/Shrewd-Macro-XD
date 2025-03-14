using System;
using System.Runtime.InteropServices;

namespace NotesAndTasks
{
    /// <summary>
    /// Contains P/Invoke declarations for Windows API functions and related structures.
    /// This class follows security best practices by isolating native method declarations.
    /// </summary>
    /// <remarks>
    /// This class encapsulates all external Windows API calls used by the application.
    /// It implements the recommended pattern for P/Invoke declarations by:
    /// - Using internal access to limit exposure
    /// - Properly declaring structures with correct marshaling attributes
    /// - Including security-related attributes where needed
    /// - Providing comprehensive documentation for each member
    /// </remarks>
    internal static class NativeMethods
    {
        #region Delegates
        /// <summary>
        /// Delegate for low-level keyboard and mouse hook callbacks.
        /// </summary>
        /// <param name="nCode">Hook code. If nCode is less than zero, the hook procedure must pass the message to CallNextHookEx.</param>
        /// <param name="wParam">Message identifier.</param>
        /// <param name="lParam">Pointer to a KBDLLHOOKSTRUCT or MSLLHOOKSTRUCT structure.</param>
        /// <returns>If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx.</returns>
        internal delegate IntPtr LowLevelHookProc(int nCode, IntPtr wParam, IntPtr lParam);
        #endregion

        #region Structures
        /// <summary>
        /// Represents a point in a two-dimensional coordinate system.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            /// <summary>The x-coordinate of the point.</summary>
            public int X;
            /// <summary>The y-coordinate of the point.</summary>
            public int Y;
        }

        /// <summary>
        /// Contains information about a low-level mouse input event.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MSLLHOOKSTRUCT
        {
            /// <summary>The x- and y-coordinates of the cursor, in screen coordinates.</summary>
            public POINT pt;
            /// <summary>
            /// Additional information about the mouse event.
            /// If the message is WM_XBUTTONDOWN, WM_XBUTTONUP, WM_XBUTTONDBLCLK, WM_NCXBUTTONDOWN, WM_NCXBUTTONUP,
            /// or WM_NCXBUTTONDBLCLK, the high-order word specifies which X button was pressed or released.
            /// </summary>
            public uint mouseData;
            /// <summary>The event-injected flag.</summary>
            public uint flags;
            /// <summary>The time stamp for this message.</summary>
            public uint time;
            /// <summary>Additional information associated with the message.</summary>
            public IntPtr dwExtraInfo;
        }

        /// <summary>
        /// Contains information about simulated mouse or keyboard input events.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            /// <summary>The type of the input event (1 for mouse input, 2 for keyboard input).</summary>
            public uint type;
            /// <summary>The mouse input data when the type is INPUT_MOUSE.</summary>
            public MOUSEINPUT mi;
        }

        /// <summary>
        /// Contains information about a simulated mouse event.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            /// <summary>The absolute position of the mouse, or the amount of motion since the last mouse event.</summary>
            public int dx;
            /// <summary>The absolute position of the mouse, or the amount of motion since the last mouse event.</summary>
            public int dy;
            /// <summary>If the mouse wheel is moved, indicates the amount of wheel movement.</summary>
            public uint mouseData;
            /// <summary>A set of bit flags that specify various aspects of mouse motion and button clicks.</summary>
            public uint dwFlags;
            /// <summary>The time stamp for the event, in milliseconds.</summary>
            public uint time;
            /// <summary>Additional information associated with the event.</summary>
            public IntPtr dwExtraInfo;
        }
        #endregion

        #region User32.dll Functions
        /// <summary>
        /// Sets a Windows hook that monitors low-level keyboard or mouse input events.
        /// </summary>
        /// <param name="idHook">The type of hook to be installed (WH_KEYBOARD_LL or WH_MOUSE_LL).</param>
        /// <param name="lpfn">A pointer to the hook procedure.</param>
        /// <param name="hMod">A handle to the DLL containing the hook procedure.</param>
        /// <param name="dwThreadId">The identifier of the thread with which the hook procedure is to be associated.</param>
        /// <returns>If successful, returns a handle to the hook procedure. If unsuccessful, returns IntPtr.Zero.</returns>
        /// <remarks>
        /// The hook procedure should process the message and return the value returned by CallNextHookEx.
        /// For low-level hooks, the hook procedure must be in the same desktop as the running application.
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(
            int idHook,
            LowLevelHookProc lpfn,
            IntPtr hMod,
            uint dwThreadId);

        /// <summary>
        /// Removes a previously set Windows hook.
        /// </summary>
        /// <param name="hhk">A handle to the hook to be removed. This parameter is obtained by a previous call to SetWindowsHookEx.</param>
        /// <returns>If successful, returns true. If unsuccessful, returns false.</returns>
        /// <remarks>
        /// The hook procedure must be in the state to be removed, that is, it must not be in the middle of processing a message.
        /// This function must be called on the same thread that installed the hook.
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// Passes the hook information to the next hook procedure in the current hook chain.
        /// </summary>
        /// <param name="hhk">This parameter is ignored.</param>
        /// <param name="nCode">The hook code passed to the current hook procedure.</param>
        /// <param name="wParam">The wParam value passed to the current hook procedure.</param>
        /// <param name="lParam">The lParam value passed to the current hook procedure.</param>
        /// <returns>The value returned by the next hook procedure in the chain.</returns>
        /// <remarks>
        /// This function must be called by the hook procedure if it is not processing the message.
        /// The hook procedure can modify the values pointed to by wParam and lParam before passing them to CallNextHookEx.
        /// </remarks>
        [DllImport("user32.dll")]
        internal static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        /// <summary>
        /// Retrieves the current cursor position.
        /// </summary>
        /// <param name="lpPoint">A pointer to a POINT structure that receives the screen coordinates of the cursor.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        /// <remarks>
        /// The cursor position is always specified in screen coordinates and is not affected by the mapping mode of the window that contains the cursor.
        /// </remarks>
        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(out POINT lpPoint);

        /// <summary>
        /// Synthesizes mouse motion and button clicks.
        /// </summary>
        /// <param name="dwFlags">A set of bit flags that specify various aspects of mouse motion and button clicking.</param>
        /// <param name="dx">The mouse's absolute position along the x-axis or its amount of motion since the last mouse event.</param>
        /// <param name="dy">The mouse's absolute position along the y-axis or its amount of motion since the last mouse event.</param>
        /// <param name="dwData">If dwFlags contains MOUSEEVENTF_WHEEL, then dwData specifies the amount of wheel movement.</param>
        /// <param name="dwExtraInfo">Additional information associated with the mouse event.</param>
        /// <remarks>
        /// The mouse_event function has been superseded by SendInput. Use SendInput instead.
        /// </remarks>
        [DllImport("user32.dll")]
        internal static extern void mouse_event(
            uint dwFlags,
            int dx,
            int dy,
            uint dwData,
            int dwExtraInfo);

        /// <summary>
        /// Synthesizes keystrokes, mouse motions, and button clicks.
        /// </summary>
        /// <param name="nInputs">The number of structures in the pInputs array.</param>
        /// <param name="pInputs">An array of INPUT structures. Each structure represents an event to be inserted into the keyboard or mouse input stream.</param>
        /// <param name="cbSize">The size, in bytes, of an INPUT structure.</param>
        /// <returns>The function returns the number of events that it successfully inserted into the keyboard or mouse input stream.</returns>
        /// <remarks>
        /// This function is the preferred method for synthesizing input, as it supports more features and is more efficient than mouse_event and keybd_event.
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint SendInput(
            uint nInputs,
            ref INPUT pInputs,
            int cbSize);
        #endregion

        #region Kernel32.dll Functions
        /// <summary>
        /// Retrieves a module handle for the specified module.
        /// </summary>
        /// <param name="lpModuleName">The name of the loaded module (either a .dll or .exe file).</param>
        /// <returns>If successful, returns a handle to the specified module. If unsuccessful, returns IntPtr.Zero.</returns>
        /// <remarks>
        /// If lpModuleName is NULL, GetModuleHandle returns a handle to the file used to create the calling process (.exe file).
        /// The returned handle is not global or inheritable. It cannot be duplicated or used by another process.
        /// </remarks>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion
    }
} 