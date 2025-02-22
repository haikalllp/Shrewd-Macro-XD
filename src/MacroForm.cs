using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;
using Macro;

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

        // Surpress warnings as first time build might not know about these
#pragma warning disable CS0414
        private ToggleType currentToggleType = ToggleType.Keyboard;
        private Keys toggleKey = Keys.Capital;  // Default to Capital
        private Keys macroSwitchKey = Keys.Q;  // Default to Q
#pragma warning restore CS0414

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
        private bool recoilReductionEnabled = false;
        private Keys currentMacroKey = Keys.Capital;  // Default to Caps Lock
        private Keys currentSwitchKey = Keys.Q;      // Default to Q

        private readonly (int dx, int dy)[] jitterPattern = new[]
        {
            (7, 7), (-7, -7), (0, 7), (7, 7), (-7, -7),
            (0, 6), (7, 7), (-7, -7), (0, 7), (7, 7),
            (-7, -7), (0, 6), (7, 7), (-7, -7), (0, 6),
            (7, 7), (-7, -7), (0, 7), (7, 7), (-7, -7),
            (0, 6), (7, 7), (-7, -7), (0, 6)
        };

        private const double BASE_RECOIL_STRENGTH = 0.75;
        private const double BASE_RECOIL_STRENGTH_2 = 2.0;
        private const double LOW_LEVEL_1_SPEED = 0.25;
        private const double LOW_LEVEL_2_SPEED = 0.5;
        private const double LOW_LEVEL_3_SPEED = 0.75;

        public MacroForm()
        {
            keyboardProc = KeyboardHookCallback;
            mouseProc = MouseHookCallback;

            try
            {
                InitializeComponent();
                InitializeCustomComponents();

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
                    // Let the anchor properties handle control resizing
                    // Controls will maintain their relative positions and sizes
                    mainPanel.PerformLayout();
                    this.PerformLayout();
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
            InitializeIcon();
            InitializeHotkeys();
            InitializeTooltips();
            LoadSettings();

            // Set initial text with bold formatting
            if (currentMacroKey != Keys.None)
                UpdateCurrentKey(currentMacroKey.ToString());
            if (currentSwitchKey != Keys.None)
                UpdateSwitchKey(currentSwitchKey.ToString());

            // Initialize event handlers
            InitializeEventHandlers();
        }

        private void InitializeEventHandlers()
        {
            btnToggleDebug.Click += (sender, e) =>
            {
                debugPanel.Visible = !debugPanel.Visible;
                btnToggleDebug.Text = debugPanel.Visible ? "Hide Debug Info" : "Show Debug Info";
                UpdateDebugInfo("Debug panel visibility toggled");
            };

            btnSetKey.Click += (sender, e) =>
            {
                isSettingKey = true;
                btnSetKey.Text = "Press any key...";
                btnSetKey.Enabled = false;
                UpdateDebugInfo("Waiting for new toggle key...");
            };

            trackBarJitter.ValueChanged += (sender, e) =>
            {
                jitterStrength = trackBarJitter.Value;
                SettingsManager.CurrentSettings.JitterStrength = jitterStrength;
                SettingsManager.SaveSettings();
                UpdateJitterStrength(jitterStrength);
                UpdateDebugInfo($"Jitter strength set to {jitterStrength}");
            };

            trackBarRecoilReduction.ValueChanged += (sender, e) =>
            {
                recoilReductionStrength = trackBarRecoilReduction.Value;
                SettingsManager.CurrentSettings.RecoilReductionStrength = recoilReductionStrength;
                SettingsManager.SaveSettings();
                UpdateRecoilReductionStrength(recoilReductionStrength);
                UpdateDebugInfo($"Recoil reduction strength set to {recoilReductionStrength}");
            };

            chkAlwaysJitter.CheckedChanged += (sender, e) =>
            {
                alwaysJitterMode = chkAlwaysJitter.Checked;
                if (alwaysJitterMode)
                {
                    jitterEnabled = true;
                    recoilReductionEnabled = false;
                    chkAlwaysRecoilReduction.Checked = false;
                    btnSetMacroSwitch.Enabled = false;
                }
                else
                {
                    btnSetMacroSwitch.Enabled = true;
                }
                SettingsManager.CurrentSettings.AlwaysJitterMode = alwaysJitterMode;
                SettingsManager.CurrentSettings.JitterEnabled = jitterEnabled;
                SettingsManager.CurrentSettings.RecoilReductionEnabled = recoilReductionEnabled;
                SettingsManager.SaveSettings();
                UpdateTitle();
                UpdateModeLabels();
            };

            chkAlwaysRecoilReduction.CheckedChanged += (sender, e) =>
            {
                alwaysRecoilReductionMode = chkAlwaysRecoilReduction.Checked;
                if (alwaysRecoilReductionMode)
                {
                    recoilReductionEnabled = true;
                    jitterEnabled = false;
                    chkAlwaysJitter.Checked = false;
                    btnSetMacroSwitch.Enabled = false;
                }
                else
                {
                    btnSetMacroSwitch.Enabled = true;
                }
                SettingsManager.CurrentSettings.AlwaysRecoilReductionMode = alwaysRecoilReductionMode;
                SettingsManager.CurrentSettings.RecoilReductionEnabled = recoilReductionEnabled;
                SettingsManager.CurrentSettings.JitterEnabled = jitterEnabled;
                SettingsManager.SaveSettings();
                UpdateTitle();
                UpdateModeLabels();
            };

            chkMinimizeToTray.CheckedChanged += (sender, e) =>
            {
                SettingsManager.CurrentSettings.MinimizeToTray = chkMinimizeToTray.Checked;
                SettingsManager.SaveSettings();
            };

            btnSetMacroSwitch.Click += (sender, e) =>
            {
                isSettingMacroSwitchKey = true;
                btnSetMacroSwitch.Text = "Press any key...";
                btnSetMacroSwitch.Enabled = false;
                UpdateDebugInfo("Waiting for new switch key...");
            };
        }

        private void LoadSettings()
        {
            var settings = SettingsManager.CurrentSettings;

            // Load all saved settings
            trackBarJitter.Value = settings.JitterStrength;
            chkAlwaysJitter.Checked = settings.AlwaysJitterMode;

            trackBarRecoilReduction.Value = settings.RecoilReductionStrength;
            chkAlwaysRecoilReduction.Checked = settings.AlwaysRecoilReductionMode;

            // Load UI preferences
            chkMinimizeToTray.Checked = settings.MinimizeToTray;

            // Update variables
            jitterStrength = settings.JitterStrength;
            jitterEnabled = settings.JitterEnabled;
            alwaysJitterMode = settings.AlwaysJitterMode;
            recoilReductionStrength = settings.RecoilReductionStrength;
            recoilReductionEnabled = settings.RecoilReductionEnabled;
            alwaysRecoilReductionMode = settings.AlwaysRecoilReductionMode;

            // Load hotkeys
            if (!string.IsNullOrEmpty(settings.MacroToggleKey))
                currentMacroKey = (Keys)Enum.Parse(typeof(Keys), settings.MacroToggleKey);
            if (!string.IsNullOrEmpty(settings.ModeSwitchKey))
                currentSwitchKey = (Keys)Enum.Parse(typeof(Keys), settings.ModeSwitchKey);

            // Update strength labels
            UpdateJitterStrength(jitterStrength);
            UpdateRecoilReductionStrength(recoilReductionStrength);

            UpdateTitle();
            UpdateModeLabels();
        }

        private void SaveCurrentSettings()
        {
            var settings = SettingsManager.CurrentSettings;

            settings.JitterStrength = jitterStrength;
            settings.JitterEnabled = jitterEnabled;
            settings.AlwaysJitterMode = alwaysJitterMode;
            settings.RecoilReductionStrength = recoilReductionStrength;
            settings.RecoilReductionEnabled = recoilReductionEnabled;
            settings.AlwaysRecoilReductionMode = alwaysRecoilReductionMode;
            settings.MinimizeToTray = chkMinimizeToTray.Checked;
            settings.MacroToggleKey = currentMacroKey.ToString();
            settings.ModeSwitchKey = currentSwitchKey.ToString();

            SettingsManager.SaveSettings();
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

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                // Handle LMB and RMB for macro activation
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

                // Handle extra mouse buttons for toggle/switch
                if (wParam == (IntPtr)WM_XBUTTONDOWN)
                {
                    bool isXButton1 = (hookStruct.mouseData >> 16) == 1;
                    bool isXButton2 = (hookStruct.mouseData >> 16) == 2;

                    if (isSettingKey)
                    {
                        isSettingKey = false;
                        btnSetKey.Text = "Set Toggle Key";
                        btnSetKey.Enabled = true;
                        string buttonName = isXButton1 ? "XButton1" : "XButton2";
                        currentMacroKey = isXButton1 ? Keys.XButton1 : Keys.XButton2;
                        UpdateCurrentKey(buttonName);
                        SettingsManager.CurrentSettings.MacroToggleKey = buttonName;
                        SettingsManager.SaveSettings();
                        UpdateDebugInfo($"Toggle key set to {buttonName}");
                        return (IntPtr)1;
                    }
                    else if (isSettingMacroSwitchKey)
                    {
                        isSettingMacroSwitchKey = false;
                        btnSetMacroSwitch.Text = "Set Switch Key";
                        btnSetMacroSwitch.Enabled = true;
                        string buttonName = isXButton1 ? "XButton1" : "XButton2";
                        currentSwitchKey = isXButton1 ? Keys.XButton1 : Keys.XButton2;
                        UpdateSwitchKey(buttonName);
                        SettingsManager.CurrentSettings.ModeSwitchKey = buttonName;
                        SettingsManager.SaveSettings();
                        UpdateDebugInfo($"Switch key set to {buttonName}");
                        return (IntPtr)1;
                    }
                    else if ((isXButton1 && currentMacroKey == Keys.XButton1) ||
                            (isXButton2 && currentMacroKey == Keys.XButton2))
                    {
                        ToggleMacro();
                        return (IntPtr)1;
                    }
                    else if ((isXButton1 && currentSwitchKey == Keys.XButton1) ||
                            (isXButton2 && currentSwitchKey == Keys.XButton2))
                    {
                        if (!alwaysJitterMode && !alwaysRecoilReductionMode)
                        {
                            HandleModeSwitch();
                        }
                        return (IntPtr)1;
                    }
                }
                else if (wParam == (IntPtr)WM_MBUTTONDOWN)
                {
                    if (isSettingKey)
                    {
                        isSettingKey = false;
                        btnSetKey.Text = "Set Toggle Key";
                        btnSetKey.Enabled = true;
                        currentMacroKey = Keys.MButton;
                        UpdateCurrentKey("MButton");
                        SettingsManager.CurrentSettings.MacroToggleKey = "MButton";
                        SettingsManager.SaveSettings();
                        UpdateDebugInfo("Toggle key set to Middle Mouse Button");
                        return (IntPtr)1;
                    }
                    else if (isSettingMacroSwitchKey)
                    {
                        isSettingMacroSwitchKey = false;
                        btnSetMacroSwitch.Text = "Set Switch Key";
                        btnSetMacroSwitch.Enabled = true;
                        currentSwitchKey = Keys.MButton;
                        UpdateSwitchKey("MButton");
                        SettingsManager.CurrentSettings.ModeSwitchKey = "MButton";
                        SettingsManager.SaveSettings();
                        UpdateDebugInfo("Switch key set to Middle Mouse Button");
                        return (IntPtr)1;
                    }
                    else if (currentMacroKey == Keys.MButton)
                    {
                        ToggleMacro();
                        return (IntPtr)1;
                    }
                    else if (currentSwitchKey == Keys.MButton)
                    {
                        if (!alwaysJitterMode && !alwaysRecoilReductionMode)
                        {
                            HandleModeSwitch();
                        }
                        return (IntPtr)1;
                    }
                }
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                // Don't allow LMB or RMB as they are reserved for activation
                if (key == Keys.LButton || key == Keys.RButton)
                {
                    if (isSettingKey || isSettingMacroSwitchKey)
                    {
                        UpdateDebugInfo("Left and Right mouse buttons cannot be used as they are reserved for activation");
                        if (isSettingKey)
                        {
                            isSettingKey = false;
                            btnSetKey.Text = "Set Toggle Key";
                            btnSetKey.Enabled = true;
                        }
                        if (isSettingMacroSwitchKey)
                        {
                            isSettingMacroSwitchKey = false;
                            btnSetMacroSwitch.Text = "Set Switch Key";
                            btnSetMacroSwitch.Enabled = true;
                        }
                        return (IntPtr)1;
                    }
                    return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
                }

                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    if (isSettingKey)
                    {
                        isSettingKey = false;
                        btnSetKey.Text = "Set Toggle Key";
                        btnSetKey.Enabled = true;
                        currentMacroKey = key;
                        UpdateCurrentKey(key.ToString());
                        SettingsManager.CurrentSettings.MacroToggleKey = key.ToString();
                        SettingsManager.SaveSettings();
                        UpdateDebugInfo($"Toggle key set to {key}");
                        return (IntPtr)1;
                    }
                    else if (isSettingMacroSwitchKey)
                    {
                        isSettingMacroSwitchKey = false;
                        btnSetMacroSwitch.Text = "Set Switch Key";
                        btnSetMacroSwitch.Enabled = true;
                        currentSwitchKey = key;
                        UpdateSwitchKey(key.ToString());
                        SettingsManager.CurrentSettings.ModeSwitchKey = key.ToString();
                        SettingsManager.SaveSettings();
                        UpdateDebugInfo($"Switch key set to {key}");
                        return (IntPtr)1;
                    }
                    else if (key == currentMacroKey)
                    {
                        ToggleMacro();
                        return (IntPtr)1;
                    }
                    else if (key == currentSwitchKey && !alwaysJitterMode && !alwaysRecoilReductionMode)
                    {
                        HandleModeSwitch();
                        return (IntPtr)1;
                    }
                }
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
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
                        // Low Range (1-6): Ultra-precise movements with logarithmic scaling
                        if (recoilReductionStrength <= 6)
                        {
                            double logBase = 1.5;
                            input.mi.dy = Math.Max(1, (int)Math.Round(BASE_RECOIL_STRENGTH * Math.Log(recoilReductionStrength + 1, logBase)));
                        }
                        // Mid Range (7-13): Standard recoil control with linear scaling
                        else if (recoilReductionStrength <= 13)
                        {
                            input.mi.dy = Math.Max(1, (int)Math.Round(BASE_RECOIL_STRENGTH * recoilReductionStrength * 1.2));
                        }
                        // High Range (14-20): Aggressive compensation with enhanced exponential scaling
                        else
                        {
                            // Using a more aggressive scaling factor to ensure consistent speed increase
                            double baseValue = BASE_RECOIL_STRENGTH * 15.6; // Starting from level 13's approximate value
                            double scalingFactor = 1.3; // Increased scaling factor for more aggressive progression
                            input.mi.dy = Math.Max(1, (int)Math.Round(baseValue * Math.Pow(scalingFactor, recoilReductionStrength - 13)));
                        }
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
            if (lblCurrentKeyValue != null)
            {
                lblCurrentKeyValue.Text = key;
            }
        }

        private void UpdateSwitchKey(string key)
        {
            if (lblMacroSwitchKeyValue != null)
            {
                lblMacroSwitchKeyValue.Text = key;
            }
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

        private void HandleModeSwitch()
        {
            // Don't switch if either "Always" mode is enabled
            if (alwaysJitterMode || alwaysRecoilReductionMode)
                return;

            // Toggle between jitter and recoil reduction
            jitterEnabled = !jitterEnabled;
            recoilReductionEnabled = !jitterEnabled;

            // Save the new state
            SettingsManager.CurrentSettings.JitterEnabled = jitterEnabled;
            SettingsManager.CurrentSettings.RecoilReductionEnabled = recoilReductionEnabled;
            SettingsManager.SaveSettings();

            UpdateTitle();
            UpdateModeLabels();
            UpdateDebugInfo($"Switched to {(jitterEnabled ? "Jitter" : "Recoil Reduction")} mode");
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveCurrentSettings();
            base.OnFormClosing(e);
        }

        private void InitializeIcon()
        {
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
        }

        private void InitializeHotkeys()
        {
            // Initialize default hotkeys
            UpdateCurrentKey(currentMacroKey.ToString());
            UpdateSwitchKey(currentSwitchKey.ToString());
        }

        private void InitializeTooltips()
        {
            // Initialize tooltips for controls
            var toolTip = new ToolTip();
            toolTip.SetToolTip(chkAlwaysJitter, "Always keep Jitter enabled");
            toolTip.SetToolTip(trackBarJitter, "Adjust Jitter strength");
            toolTip.SetToolTip(chkAlwaysRecoilReduction, "Always keep Recoil Reduction enabled");
            toolTip.SetToolTip(trackBarRecoilReduction, "Adjust Recoil Reduction strength");
            toolTip.SetToolTip(chkMinimizeToTray, "Minimize to system tray when closing");
        }

        private void chkMinimizeToTray_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
