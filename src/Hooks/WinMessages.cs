using System;

namespace NotesAndTasks
{
    /// <summary>
    /// Contains Windows message constants used for low-level hooks and input handling.
    /// </summary>
    /// <remarks>
    /// This class provides a centralized location for all Windows message constants used in the application.
    /// The constants are organized into logical groups:
    /// - Hook-related constants (WH_*)
    /// - Keyboard message constants (WM_KEY*)
    /// - Mouse button message constants (WM_*BUTTON*)
    /// - Mouse button identifiers (XBUTTON*)
    /// - Mouse event flags (MOUSEEVENTF_*)
    /// - Input type identifiers (INPUT_*)
    /// - Base values for calculations
    /// </remarks>
    public static class WinMessages
    {
        /// <summary>
        /// Windows hook constant for low-level keyboard events.
        /// Used with SetWindowsHookEx to install a hook procedure that monitors low-level keyboard input events.
        /// </summary>
        public const int WH_KEYBOARD_LL = 13;

        /// <summary>
        /// Windows hook constant for low-level mouse events.
        /// Used with SetWindowsHookEx to install a hook procedure that monitors low-level mouse input events.
        /// </summary>
        public const int WH_MOUSE_LL = 14;

        /// <summary>
        /// Posted when a nonsystem key is pressed.
        /// A nonsystem key is a key that is pressed when the ALT key is not pressed.
        /// </summary>
        public const int WM_KEYDOWN = 0x0100;

        /// <summary>
        /// Posted when a nonsystem key is released.
        /// </summary>
        public const int WM_KEYUP = 0x0101;

        /// <summary>
        /// Posted when a system key is pressed (such as ALT).
        /// </summary>
        public const int WM_SYSKEYDOWN = 0x0104;

        /// <summary>
        /// Posted when a system key is released.
        /// </summary>
        public const int WM_SYSKEYUP = 0x0105;

        /// <summary>
        /// Posted when the left mouse button is pressed.
        /// </summary>
        public const int WM_LBUTTONDOWN = 0x0201;

        /// <summary>
        /// Posted when the left mouse button is released.
        /// </summary>
        public const int WM_LBUTTONUP = 0x0202;

        /// <summary>
        /// Posted when the right mouse button is pressed.
        /// </summary>
        public const int WM_RBUTTONDOWN = 0x0204;

        /// <summary>
        /// Posted when the right mouse button is released.
        /// </summary>
        public const int WM_RBUTTONUP = 0x0205;

        /// <summary>
        /// Posted when the middle mouse button is pressed.
        /// </summary>
        public const int WM_MBUTTONDOWN = 0x0207;

        /// <summary>
        /// Posted when the middle mouse button is released.
        /// </summary>
        public const int WM_MBUTTONUP = 0x0208;

        /// <summary>
        /// Posted when an X button is pressed.
        /// The specific button (XBUTTON1 or XBUTTON2) is indicated in the high-order word of the mouseData field.
        /// </summary>
        public const int WM_XBUTTONDOWN = 0x020B;

        /// <summary>
        /// Posted when an X button is released.
        /// The specific button (XBUTTON1 or XBUTTON2) is indicated in the high-order word of the mouseData field.
        /// </summary>
        public const int WM_XBUTTONUP = 0x020C;

        /// <summary>
        /// First X button identifier (typically the fourth mouse button).
        /// Used to identify which X button was pressed in WM_XBUTTONDOWN events.
        /// </summary>
        public const int XBUTTON1 = 0x0001;

        /// <summary>
        /// Second X button identifier (typically the fifth mouse button).
        /// Used to identify which X button was pressed in WM_XBUTTONDOWN events.
        /// </summary>
        public const int XBUTTON2 = 0x0002;

        /// <summary>
        /// Mouse event flag indicating relative mouse movement.
        /// When used with SendInput, specifies that dx and dy contain relative movement data.
        /// </summary>
        public const uint MOUSEEVENTF_MOVE = 0x0001;

        /// <summary>
        /// Input type constant indicating mouse input.
        /// Used with the INPUT structure to specify that the input event is mouse-related.
        /// </summary>
        public const uint INPUT_MOUSE = 0;

    }
} 