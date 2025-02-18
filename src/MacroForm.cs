using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace NotesTasks
{
    public partial class MacroForm : Form
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_XBUTTONDOWN = 0x020B;
        private const int XBUTTON1 = 0x0001;
        private const int XBUTTON2 = 0x0002;

        private delegate IntPtr LowLevelHookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        };

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelHookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint INPUT_MOUSE = 0;

        private IntPtr keyboardHookID = IntPtr.Zero;
        private IntPtr mouseHookID = IntPtr.Zero;
        private readonly LowLevelHookProc keyboardProc;
        private readonly LowLevelHookProc mouseProc;

        private bool isMacroOn = false;
        private enum ToggleType
        {
            Keyboard,
            MouseMiddle,
            MouseX1,
            MouseX2
        }
        private ToggleType currentToggleType = ToggleType.Keyboard;
        private Keys toggleKey = Keys.F2;  // Default
        private int jitterStrength = 3;
        private bool isSettingKey = false;
        private System.Threading.Timer jitterTimer;
        private bool isJittering = false;
        private int currentStep = 0;
        private readonly object lockObject = new object();
        private bool leftButtonDown = false;
        private bool rightButtonDown = false;
        private bool isExiting = false;

        private readonly (int dx, int dy)[] jitterPattern = new[]
        {
            (7, 7), (-7, -7), (0, 7), (7, 7), (-7, -7),
            (0, 6), (7, 7), (-7, -7), (0, 7), (7, 7),
            (-7, -7), (0, 6), (7, 7), (-7, -7), (0, 6),
            (7, 7), (-7, -7), (0, 7), (7, 7), (-7, -7),
            (0, 6), (7, 7), (-7, -7), (0, 6)
        };

        public MacroForm()
        {
            keyboardProc = KeyboardHookCallback;
            mouseProc = MouseHookCallback;
            
            try
            {
                InitializeComponent();
                InitializeCustomComponents();

                // Set the icon from the executable itself
                try
                {
                    var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                    this.Icon = icon;
                    notifyIcon.Icon = icon;
                }
                catch (Exception ex)
                {
                    UpdateDebugInfo($"Error loading icon: {ex.Message}");
                }

                // Initialize tray icon behavior
                notifyIcon.DoubleClick += (s, e) => ShowWindow();
                showWindowMenuItem.Click += (s, e) => ShowWindow();
                exitMenuItem.Click += (s, e) => 
                {
                    CleanupAndExit();
                };

                this.FormClosing += (s, e) =>
                {
                    if (!isExiting && chkMinimizeToTray.Checked && e.CloseReason == CloseReason.UserClosing)
                    {
                        e.Cancel = true;
                        this.Hide();
                        notifyIcon.Visible = true;
                        UpdateDebugInfo("Application minimized to system tray");
                    }
                    else if (isExiting || !chkMinimizeToTray.Checked)
                    {
                        // Cleanup when actually closing
                        CleanupAndExit();
                    }
                };

                this.Resize += (s, e) =>
                {
                    // Adjust controls for new window size
                    int padding = mainPanel.Padding.Horizontal;
                    int availableWidth = mainPanel.Width - padding;
                    btnSetKey.Width = availableWidth;
                    trackBarJitter.Width = availableWidth;
                    btnToggleDebug.Width = availableWidth;
                };
                
                this.Load += (sender, e) => 
                {
                    try
                    {
                        InitializeHooks();
                        jitterTimer = new System.Threading.Timer(OnJitterTimer, null, Timeout.Infinite, 10);
                        UpdateTitle();
                    }
                    catch (Exception ex)
                    {
                        UpdateDebugInfo($"Error initializing hooks: {ex.Message}");
                    }
                };

                // Handle application exit
                Application.ApplicationExit += (s, e) =>
                {
                    if (!isExiting)
                    {
                        CleanupAndExit();
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeCustomComponents()
        {
            btnSetKey.Click += (sender, e) => 
            {
                isSettingKey = true;
                btnSetKey.Text = "Press any key...";
                btnSetKey.Enabled = false;
            };

            this.KeyDown += (sender, e) =>
            {
                // Removed event handler for setting toggle key and toggling macro
            };

            trackBarJitter.ValueChanged += (sender, e) =>
            {
                jitterStrength = trackBarJitter.Value;
                lblJitterStrength.Text = $"Jitter Strength: {jitterStrength}";
                UpdateDebugInfo($"Jitter strength set to {jitterStrength}");
            };

            btnToggleDebug.Click += (sender, e) =>
            {
                debugPanel.Visible = !debugPanel.Visible;
                btnToggleDebug.Text = debugPanel.Visible ? "Hide Debug Info" : "Show Debug Info";
            };
        }

        private void InitializeHooks()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                keyboardHookID = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardProc,
                    GetModuleHandle(curModule.ModuleName), 0);
                mouseHookID = SetWindowsHookEx(WH_MOUSE_LL, mouseProc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }

            if (keyboardHookID == IntPtr.Zero || mouseHookID == IntPtr.Zero)
            {
                throw new Exception("Failed to set keyboard or mouse hook");
            }
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    Keys key = (Keys)vkCode;

                    if (isSettingKey)
                    {
                        currentToggleType = ToggleType.Keyboard;
                        toggleKey = key;
                        isSettingKey = false;
                        btnSetKey.Text = "Set Toggle Key";
                        btnSetKey.Enabled = true;
                        lblCurrentKey.Text = $"Current Key: {key}";
                        UpdateDebugInfo($"Toggle key set to {key}");
                        return (IntPtr)1; // Handle the key
                    }
                    else if (currentToggleType == ToggleType.Keyboard && key == toggleKey)
                    {
                        isMacroOn = !isMacroOn;
                        UpdateTitle();
                        UpdateDebugInfo($"Macro turned {(isMacroOn ? "ON" : "OFF")}");
                        return (IntPtr)1; // Handle the key
                    }
                }
            }
            return CallNextHookEx(keyboardHookID, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (isSettingKey)
                {
                    int msg = wParam.ToInt32();
                    
                    // Don't allow LMB or RMB as they're used for macro activation
                    if (msg == WM_MBUTTONDOWN)
                    {
                        currentToggleType = ToggleType.MouseMiddle;
                        isSettingKey = false;
                        btnSetKey.Text = "Set Toggle Key";
                        btnSetKey.Enabled = true;
                        lblCurrentKey.Text = "Current Key: Mouse3 (Middle)";
                        UpdateDebugInfo("Toggle key set to Mouse3 (Middle)");
                        return (IntPtr)1; // Handle the key
                    }
                    else if (msg == WM_XBUTTONDOWN)
                    {
                        MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                        int xButton = (int)((hookStruct.mouseData >> 16) & 0xFFFF);
                        
                        if (xButton == XBUTTON1)
                        {
                            currentToggleType = ToggleType.MouseX1;
                            isSettingKey = false;
                            btnSetKey.Text = "Set Toggle Key";
                            btnSetKey.Enabled = true;
                            lblCurrentKey.Text = "Current Key: Mouse4";
                            UpdateDebugInfo("Toggle key set to Mouse4");
                            return (IntPtr)1; // Handle the key
                        }
                        else if (xButton == XBUTTON2)
                        {
                            currentToggleType = ToggleType.MouseX2;
                            isSettingKey = false;
                            btnSetKey.Text = "Set Toggle Key";
                            btnSetKey.Enabled = true;
                            lblCurrentKey.Text = "Current Key: Mouse5";
                            UpdateDebugInfo("Toggle key set to Mouse5");
                            return (IntPtr)1; // Handle the key
                        }
                    }
                }
                else
                {
                    // Check for toggle activation via mouse buttons
                    int msg = wParam.ToInt32();
                    if ((currentToggleType == ToggleType.MouseMiddle && msg == WM_MBUTTONDOWN) ||
                        (currentToggleType == ToggleType.MouseX1 && msg == WM_XBUTTONDOWN && IsXButton1(lParam)) ||
                        (currentToggleType == ToggleType.MouseX2 && msg == WM_XBUTTONDOWN && IsXButton2(lParam)))
                    {
                        isMacroOn = !isMacroOn;
                        UpdateTitle();
                        UpdateDebugInfo($"Macro turned {(isMacroOn ? "ON" : "OFF")}");
                        return (IntPtr)1; // Handle the key
                    }
                    else
                    {
                        switch ((int)wParam)
                        {
                            case WM_LBUTTONDOWN:
                                leftButtonDown = true;
                                CheckJitterState();
                                break;
                            case WM_LBUTTONUP:
                                leftButtonDown = false;
                                CheckJitterState();
                                break;
                            case WM_RBUTTONDOWN:
                                rightButtonDown = true;
                                CheckJitterState();
                                break;
                            case WM_RBUTTONUP:
                                rightButtonDown = false;
                                CheckJitterState();
                                break;
                        }
                    }
                }
            }
            return CallNextHookEx(mouseHookID, nCode, wParam, lParam);
        }

        private bool IsXButton1(IntPtr lParam)
        {
            MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            return ((hookStruct.mouseData >> 16) & 0xFFFF) == XBUTTON1;
        }

        private bool IsXButton2(IntPtr lParam)
        {
            MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            return ((hookStruct.mouseData >> 16) & 0xFFFF) == XBUTTON2;
        }

        private void CheckJitterState()
        {
            bool shouldJitter = isMacroOn && leftButtonDown && rightButtonDown;
            
            if (shouldJitter && !isJittering)
            {
                isJittering = true;
                jitterTimer.Change(0, 10);
                UpdateDebugInfo("Jitter started");
            }
            else if (!shouldJitter && isJittering)
            {
                isJittering = false;
                jitterTimer.Change(Timeout.Infinite, 10);
                UpdateDebugInfo("Jitter stopped");
            }
        }

        private void OnJitterTimer(object state)
        {
            if (!isJittering) return;

            try
            {
                lock (lockObject)
                {
                    var pattern = jitterPattern[currentStep];
                    INPUT input = new INPUT();
                    input.type = 0; // INPUT_MOUSE
                    input.mi.dx = (int)(pattern.dx * jitterStrength / 7);
                    input.mi.dy = (int)(pattern.dy * jitterStrength / 7);
                    input.mi.dwFlags = MOUSEEVENTF_MOVE;
                    SendInput(1, ref input, Marshal.SizeOf(input));
                    currentStep = (currentStep + 1) % jitterPattern.Length;
                }
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Jitter error: {ex.Message}");
            }
        }

        private void UpdateTitle()
        {
            this.Text = $"Mouse Macro [{(isMacroOn ? "ON" : "OFF")}]";
        }

        private void UpdateDebugInfo(string info)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateDebugInfo(info)));
                return;
            }

            debugLabel.Text = $"[{DateTime.Now:HH:mm:ss}] {info}\n{debugLabel.Text}";
            if (debugLabel.Text.Length > 500)
            {
                debugLabel.Text = debugLabel.Text.Substring(0, 500) + "...";
            }
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
            notifyIcon.Visible = false;
            UpdateDebugInfo("Application restored from system tray");
        }

        private void CleanupAndExit()
        {
            isExiting = true;
            
            // Stop timers and hooks
            try
            {
                if (jitterTimer != null)
                {
                    jitterTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    jitterTimer.Dispose();
                }

                if (keyboardHookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(keyboardHookID);
                    keyboardHookID = IntPtr.Zero;
                }

                if (mouseHookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(mouseHookID);
                    mouseHookID = IntPtr.Zero;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }

            // Cleanup tray icon
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
            }

            // Force process to exit
            try
            {
                Process.GetCurrentProcess().Kill();
            }
            catch
            {
                Environment.Exit(0);
            }
        }
    }
}
