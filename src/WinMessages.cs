using System;

namespace NotesAndTasks
{
    /// <summary>
    /// Contains Windows message constants used for low-level hooks and input handling.
    /// </summary>
    public static class WinMessages
    {
        // Keyboard and mouse hook constants
        public const int WH_KEYBOARD_LL = 13;
        public const int WH_MOUSE_LL = 14;

        // Keyboard messages
        public const int WM_KEYDOWN = 0x0100;

        // Mouse button messages
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_XBUTTONDOWN = 0x020B;

        // Mouse button identifiers
        public const int XBUTTON1 = 0x0001;
        public const int XBUTTON2 = 0x0002;

        // Mouse event flags
        public const uint MOUSEEVENTF_MOVE = 0x0001;
        public const uint INPUT_MOUSE = 0;

        // Base recoil and speed constants
        public const double BASE_RECOIL_STRENGTH = 0.75;
        public const double BASE_RECOIL_STRENGTH_2 = 2.0;
        public const double LOW_LEVEL_1_SPEED = 0.25;
        public const double LOW_LEVEL_2_SPEED = 0.5;
        public const double LOW_LEVEL_3_SPEED = 0.75;
    }
} 