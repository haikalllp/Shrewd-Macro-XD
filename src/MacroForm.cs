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
        private Keys toggleKey = Keys.Capital;  // Default to Capital
        private Keys macroSwitchKey = Keys.Q;  // Default to Q
        private int jitterStrength = 3;  // Default to 3
        private int recoilReductionStrength = 1;  // Default to 1
        private bool isSettingKey = false;
        private bool isSettingMacroSwitchKey = false;
        private System.Threading.Timer jitterTimer;
        private bool isJittering = false;
        private int currentStep = 0;
        private readonly object lockObject = new object();
        private bool leftButtonDown = false;
        private bool rightButtonDown = false;
        private bool isExiting = false;
        private bool jitterEnabled = false;
        private bool alwaysJitterMode = false;
        private bool alwaysRecoilReductionMode = false;

        private readonly (int dx, int dy)[] jitterPattern = new[]
        {
            (7, 7), (-7, -7), (0, 7), (7, 7), (-7, -7),
            (0, 6), (7, 7), (-7, -7), (0, 7), (7, 7),
            (-7, -7), (0, 6), (7, 7), (-7, -7), (0, 6),
            (7, 7), (-7, -7), (0, 7), (7, 7), (-7, -7),
            (0, 6), (7, 7), (-7, -7), (0, 6)
        };

        private const int BASE_RECOIL_STRENGTH = 3;

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
                    trackBarRecoilReduction.Width = availableWidth;
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
            // Set initial text with bold formatting
            UpdateCurrentKey(toggleKey.ToString());

            btnSetKey.Click += (sender, e) =>
            {
                isSettingKey = true;
                btnSetKey.Text = "Press any key...";
                btnSetKey.Enabled = false;
            };

            btnSetMacroSwitch.Click += (sender, e) =>
            {
                isSettingMacroSwitchKey = true;
                btnSetMacroSwitch.Text = "Press any key...";
                btnSetMacroSwitch.Enabled = false;
            };

            trackBarJitter.ValueChanged += (sender, e) =>
            {
                jitterStrength = trackBarJitter.Value;
                UpdateJitterStrength(jitterStrength);
                UpdateDebugInfo($"Jitter strength set to {jitterStrength}");
            };

            trackBarRecoilReduction.ValueChanged += (sender, e) =>
            {
                recoilReductionStrength = trackBarRecoilReduction.Value;
                UpdateRecoilReductionStrength(recoilReductionStrength);
                UpdateDebugInfo($"Recoil reduction strength set to {recoilReductionStrength}");
            };

            chkAlwaysJitter.CheckedChanged += (sender, e) =>
            {
                if (chkAlwaysJitter.Checked)
                {
                    alwaysJitterMode = true;
                    alwaysRecoilReductionMode = false;
                    jitterEnabled = true;
                    chkAlwaysRecoilReduction.Checked = false;
                    btnSetMacroSwitch.Enabled = false;
                }
                else
                {
                    alwaysJitterMode = false;
                    btnSetMacroSwitch.Enabled = true;
                }
                UpdateTitle();
                UpdateModeLabels();
            };

            chkAlwaysRecoilReduction.CheckedChanged += (sender, e) =>
            {
                if (chkAlwaysRecoilReduction.Checked)
                {
                    alwaysRecoilReductionMode = true;
                    alwaysJitterMode = false;
                    jitterEnabled = false;
                    chkAlwaysJitter.Checked = false;
                    btnSetMacroSwitch.Enabled = false;
                }
                else
                {
                    alwaysRecoilReductionMode = false;
                    btnSetMacroSwitch.Enabled = true;
                }
                UpdateTitle();
                UpdateModeLabels();
            };

            btnToggleDebug.Click += (sender, e) =>
            {
                debugPanel.Visible = !debugPanel.Visible;
                btnToggleDebug.Text = debugPanel.Visible ? "Hide Debug Info" : "Show Debug Info";
            };

            // Make both panels visible
            strengthPanel1.Visible = true;
            strengthPanel2.Visible = true;

            // Initialize mode labels
            UpdateModeLabels();
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
                        UpdateCurrentKey(toggleKey.ToString());
                        UpdateDebugInfo($"Toggle key set to {key}");
                        return (IntPtr)1; // Handle the key
                    }
                    else if (isSettingMacroSwitchKey)
                    {
                        macroSwitchKey = key;
                        isSettingMacroSwitchKey = false;
                        btnSetMacroSwitch.Text = "Set Switch Key";
                        btnSetMacroSwitch.Enabled = true;
                        UpdateMacroSwitchKey(macroSwitchKey.ToString());
                        UpdateDebugInfo($"Macro switch key set to {key}");
                        return (IntPtr)1; // Handle the key
                    }
                    else if (currentToggleType == ToggleType.Keyboard && key == toggleKey)
                    {
                        ToggleMacro();
                        return (IntPtr)1; // Handle the key
                    }
                    else if (key == macroSwitchKey && !alwaysJitterMode && !alwaysRecoilReductionMode)
                    {
                        ToggleMacroMode();
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
                        UpdateCurrentKey("Mouse3 (Middle)");
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
                            UpdateCurrentKey("Mouse4");
                            UpdateDebugInfo("Toggle key set to Mouse4");
                            return (IntPtr)1; // Handle the key
                        }
                        else if (xButton == XBUTTON2)
                        {
                            currentToggleType = ToggleType.MouseX2;
                            isSettingKey = false;
                            btnSetKey.Text = "Set Toggle Key";
                            btnSetKey.Enabled = true;
                            UpdateCurrentKey("Mouse5");
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
                        ToggleMacro();
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
            // Always require both mouse buttons
            bool shouldActivate = isMacroOn && leftButtonDown && rightButtonDown;

            if (shouldActivate && !isJittering)
            {
                isJittering = true;
                jitterTimer.Change(0, 10);

                // Use alwaysJitterMode/alwaysRecoilReductionMode to determine which macro to run
                bool useJitter = alwaysJitterMode || (!alwaysRecoilReductionMode && jitterEnabled);

                if (useJitter)
                {
                    UpdateDebugInfo($"Jitter started - Mouse Buttons: LMB={leftButtonDown}, RMB={rightButtonDown}, Always Jitter={alwaysJitterMode}");
                }
                else
                {
                    UpdateDebugInfo($"Recoil reduction started - Mouse Buttons: LMB={leftButtonDown}, RMB={rightButtonDown}, Always Recoil Reduction={alwaysRecoilReductionMode}");
                }
            }
            else if (!shouldActivate && isJittering)
            {
                isJittering = false;
                jitterTimer.Change(Timeout.Infinite, 10);

                bool useJitter = alwaysJitterMode || (!alwaysRecoilReductionMode && jitterEnabled);

                if (useJitter)
                {
                    UpdateDebugInfo($"Jitter stopped - Mouse Buttons: LMB={leftButtonDown}, RMB={rightButtonDown}, Always Jitter={alwaysJitterMode}");
                }
                else
                {
                    UpdateDebugInfo($"Recoil reduction stopped - Mouse Buttons: LMB={leftButtonDown}, RMB={rightButtonDown}, Always Recoil Reduction={alwaysRecoilReductionMode}");
                }
            }
            UpdateModeLabels();
        }

        private void OnJitterTimer(object state)
        {
            if (!isJittering) return;

            try
            {
                lock (lockObject)
                {
                    INPUT input = new INPUT();
                    input.type = INPUT_MOUSE;

                    bool useJitter = alwaysJitterMode || (!alwaysRecoilReductionMode && jitterEnabled);

                    if (useJitter)
                    {
                        // Jitter mode
                        var pattern = jitterPattern[currentStep];
                        input.mi.dx = (int)(pattern.dx * jitterStrength / 7);
                        input.mi.dy = (int)(pattern.dy * jitterStrength / 7);
                        currentStep = (currentStep + 1) % jitterPattern.Length;
                    }
                    else
                    {
                        // Recoil reduction mode - constant downward movement
                        input.mi.dx = 0;
                        input.mi.dy = (int)(BASE_RECOIL_STRENGTH * recoilReductionStrength);
                    }

                    input.mi.dwFlags = MOUSEEVENTF_MOVE;
                    input.mi.mouseData = 0;
                    input.mi.time = 0;
                    input.mi.dwExtraInfo = IntPtr.Zero;

                    SendInput(1, ref input, Marshal.SizeOf(input));
                }
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Movement error: {ex.Message}");
            }
        }

        private void UpdateTitle()
        {
            string jitterMode;
            string recoilMode;
            if (alwaysJitterMode)
                jitterMode = "Always Jitter";
            else
                jitterMode = jitterEnabled ? "Jitter" : "Jitter (OFF)";

            if (alwaysRecoilReductionMode)
                recoilMode = "Always Recoil Reduction";
            else
                recoilMode = jitterEnabled ? "Recoil Reduction (OFF)" : "Recoil Reduction";

            this.Text = $"Notes&Tasks [{(isMacroOn ? "ON" : "OFF")}] - {jitterMode} / {recoilMode} Mode";
            UpdateModeLabels();
        }

        private void UpdateCurrentKey(string key)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateCurrentKey), key);
                return;
            }
            lblCurrentKeyValue.Text = key;
            UpdateDebugInfo($"Toggle key updated to: {key}");
        }

        private void UpdateJitterStrength(int strength)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(UpdateJitterStrength), strength);
                return;
            }
            lblJitterStrengthValue.Text = strength.ToString();
            UpdateDebugInfo($"Jitter strength updated to: {strength}");
        }

        private void UpdateRecoilReductionStrength(int strength)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(UpdateRecoilReductionStrength), strength);
                return;
            }
            lblRecoilReductionStrengthValue.Text = strength.ToString();
            UpdateDebugInfo($"Recoil reduction strength updated to: {strength}");
        }

        private void UpdateMacroSwitchKey(string key)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateMacroSwitchKey), key);
                return;
            }
            lblMacroSwitchKeyValue.Text = key;
            UpdateDebugInfo($"Macro switch key updated to: {key}");
        }

        private void UpdateDebugInfo(string message)
        {
            if (debugLabel.InvokeRequired)
            {
                debugLabel.Invoke(new Action(() => UpdateDebugInfo(message)));
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string newLine = $"[{timestamp}] {message}";

            // Keep last 100 lines of debug info
            var lines = debugLabel.Lines.ToList();
            lines.Add(newLine);
            if (lines.Count > 100)
            {
                lines.RemoveAt(0);
            }
            debugLabel.Lines = lines.ToArray();

            // Auto-scroll to bottom
            debugLabel.SelectionStart = debugLabel.TextLength;
            debugLabel.ScrollToCaret();
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

        private void ToggleMacro()
        {
            isMacroOn = !isMacroOn;
            UpdateTitle();
            string mode = jitterEnabled ? "Jitter" : "Recoil Reduction";
            string alwaysMode = alwaysJitterMode ? "Always Jitter" : (alwaysRecoilReductionMode ? "Always Recoil Reduction" : "Normal");
            UpdateDebugInfo($"Macro {(isMacroOn ? "Enabled" : "Disabled")} - Mode: {mode}, Always Mode: {alwaysMode}, Key: **{toggleKey}**");
            UpdateModeLabels();
        }

        private void ToggleMacroMode()
        {
            // If either always mode is on, we can't switch modes
            if (alwaysJitterMode || alwaysRecoilReductionMode)
                return;

            // Toggle between jitter and recoil reduction modes
            jitterEnabled = !jitterEnabled;

            UpdateTitle();
            string mode = jitterEnabled ? "Jitter" : "Recoil Reduction";
            UpdateDebugInfo($"Macro mode switched to: {mode}");
            UpdateModeLabels();
        }

        private void UpdateModeLabels()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateModeLabels));
                return;
            }

            lblRecoilReductionActive.Text = (!jitterEnabled && isMacroOn) ? "[Active]" : "";
            lblJitterActive.Text = (jitterEnabled && isMacroOn) ? "[Active]" : "";
        }

        private void lblJitterActive_Click(object sender, EventArgs e)
        {

        }

        private void lblCurrentKeyPrefix_Click(object sender, EventArgs e)
        {

        }

        private void strengthPanel2_Paint(object sender, PaintEventArgs e)
        {
            // This is an empty Paint event handler for strengthPanel2
        }
    }
}
