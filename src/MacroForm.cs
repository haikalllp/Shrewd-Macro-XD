using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.ComponentModel;
using NotesAndTasks;

namespace NotesAndTasks
{
    /// <summary>
    /// Main form for the Notes&Tasks application that provides mouse input management functionality.
    /// This form handles both jitter and recoil reduction features with configurable hotkeys and settings.
    /// </summary>
    /// <remarks>
    /// The form implements two main features:
    /// 1. Jitter - Applies a complex movement pattern to the mouse cursor
    /// 2. Recoil Reduction - Provides vertical compensation with configurable strength
    /// 
    /// Both features are activated when left and right mouse buttons are held simultaneously.
    /// The application can be minimized to the system tray and supports various hotkeys for control.
    /// </remarks>
    public partial class MacroForm : Form
    {
        #region Fields
        private IntPtr keyboardHookID = IntPtr.Zero;
        private IntPtr mouseHookID = IntPtr.Zero;
        private readonly NativeMethods.LowLevelHookProc keyboardProc;
        private readonly NativeMethods.LowLevelHookProc mouseProc;
        private readonly IContainer components;
        private System.Threading.Timer jitterTimer;
        private readonly ToolTip toolTip;

        private bool isMacroOn = false;
        /// <summary>
        /// Defines the types of input that can be used to toggle the macro functionality.
        /// </summary>
        private enum ToggleType
        {
            /// <summary>Keyboard key input</summary>
            Keyboard,
            /// <summary>Middle mouse button input</summary>
            MouseMiddle,
            /// <summary>Mouse button 4 (XButton1) input</summary>
            MouseX1,
            /// <summary>Mouse button 5 (XButton2) input</summary>
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

        #endregion

        /// <summary>
        /// Initializes a new instance of the MacroForm class.
        /// Sets up all necessary components, hooks, and event handlers.
        /// </summary>
        public MacroForm()
        {
            components = new Container();
            toolTip = new ToolTip(components);
            keyboardProc = KeyboardHookCallback;
            mouseProc = MouseHookCallback;

            try
            {
                InitializeComponent();
                InitializeCustomComponents();

                // Initialize tray icon behavior
                notifyIcon.DoubleClick += (s, e) => ShowWindow();
                showWindowMenuItem.Click += (s, e) => ShowWindow();
                exitMenuItem.Click += (s, e) => CleanupAndExit();

                this.FormClosing += OnFormClosingHandler;
                this.Resize += OnResizeHandler;
                this.Load += OnLoadHandler;

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

        /// <summary>
        /// Handles the form closing event. If minimize to tray is enabled,
        /// the form will be hidden instead of closed when the user clicks the close button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data containing the close reason and cancellation option.</param>
        private void OnFormClosingHandler(object sender, FormClosingEventArgs e)
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
        }

        /// <summary>
        /// Handles form resize events by ensuring proper layout of controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void OnResizeHandler(object sender, EventArgs e)
        {
            // Let the anchor properties handle control resizing
            mainPanel.PerformLayout();
            this.PerformLayout();
        }

        /// <summary>
        /// Handles the form load event by initializing hooks and timers.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void OnLoadHandler(object sender, EventArgs e)
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
        }

        /// <summary>
        /// Releases all resources used by the form.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (components != null)
                {
                    components.Dispose();
                }

                if (jitterTimer != null)
                {
                    jitterTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    jitterTimer.Dispose();
                }

                if (keyboardHookID != IntPtr.Zero)
                {
                    NativeMethods.UnhookWindowsHookEx(keyboardHookID);
                    keyboardHookID = IntPtr.Zero;
                }

                if (mouseHookID != IntPtr.Zero)
                {
                    NativeMethods.UnhookWindowsHookEx(mouseHookID);
                    mouseHookID = IntPtr.Zero;
                }

                if (notifyIcon != null)
                {
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();
                }

                if (toolTip != null)
                {
                    toolTip.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Initializes custom components including icons, hotkeys, tooltips, and loads saved settings.
        /// </summary>
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

        /// <summary>
        /// Initializes all event handlers for form controls.
        /// Sets up click events, value change events, and checkbox state change events.
        /// </summary>
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

        /// <summary>
        /// Loads saved settings from the settings manager and applies them to the form.
        /// This includes hotkeys, strength values, and UI preferences.
        /// </summary>
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

        /// <summary>
        /// Saves current settings to persistent storage through the settings manager.
        /// </summary>
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

        /// <summary>
        /// Initializes low-level keyboard and mouse hooks for input monitoring.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when hook initialization fails.</exception>
        private void InitializeHooks()
        {
            try
            {
                using var curProcess = Process.GetCurrentProcess();
                using var curModule = curProcess.MainModule;

                keyboardHookID = NativeMethods.SetWindowsHookEx(WinMessages.WH_KEYBOARD_LL, keyboardProc,
                    NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
                mouseHookID = NativeMethods.SetWindowsHookEx(WinMessages.WH_MOUSE_LL, mouseProc,
                    NativeMethods.GetModuleHandle(curModule.ModuleName), 0);

                if (keyboardHookID == IntPtr.Zero || mouseHookID == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Failed to set windows hook");
                }
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error in InitializeHooks: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Callback function for the low-level mouse hook.
        /// Handles mouse button events and manages macro activation states.
        /// </summary>
        /// <param name="nCode">Hook code; if less than zero, the hook procedure must pass the message to CallNextHookEx.</param>
        /// <param name="wParam">Message identifier.</param>
        /// <param name="lParam">Pointer to a MSLLHOOKSTRUCT structure.</param>
        /// <returns>If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx.</returns>
        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var hookStruct = (NativeMethods.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(NativeMethods.MSLLHOOKSTRUCT));

                switch ((int)wParam)
                {
                    case WinMessages.WM_LBUTTONDOWN:
                        leftButtonDown = true;
                        break;

                    case WinMessages.WM_LBUTTONUP:
                        leftButtonDown = false;
                        break;

                    case WinMessages.WM_RBUTTONDOWN:
                        rightButtonDown = true;
                        break;

                    case WinMessages.WM_RBUTTONUP:
                        rightButtonDown = false;
                        break;
                }

                if (wParam == (IntPtr)WinMessages.WM_XBUTTONDOWN)
                {
                    bool isXButton1 = (hookStruct.mouseData >> 16) == WinMessages.XBUTTON1;
                    bool isXButton2 = (hookStruct.mouseData >> 16) == WinMessages.XBUTTON2;

                    if (isSettingKey)
                    {
                        isSettingKey = false;
                        btnSetKey.Enabled = true;
                        btnSetKey.Text = "Set Toggle Key";
                        string buttonName = isXButton1 ? "XButton1" : "XButton2";
                        currentMacroKey = isXButton1 ? Keys.XButton1 : Keys.XButton2;
                        currentToggleType = isXButton1 ? ToggleType.MouseX1 : ToggleType.MouseX2;
                        UpdateCurrentKey(buttonName);
                        SaveCurrentSettings();
                        UpdateDebugInfo($"Set toggle key to {buttonName}");
                    }
                    else if (isSettingMacroSwitchKey)
                    {
                        isSettingMacroSwitchKey = false;
                        btnSetMacroSwitchKey.Enabled = true;
                        btnSetMacroSwitchKey.Text = "Set Switch Key";
                        string buttonName = isXButton1 ? "XButton1" : "XButton2";
                        currentSwitchKey = isXButton1 ? Keys.XButton1 : Keys.XButton2;
                        UpdateMacroSwitchKey(buttonName);
                        SaveCurrentSettings();
                        UpdateDebugInfo($"Set macro switch key to {buttonName}");
                    }
                    else if ((isXButton1 && currentMacroKey == Keys.XButton1) ||
                            (isXButton2 && currentMacroKey == Keys.XButton2))
                    {
                        ToggleMacro();
                    }
                    else if ((isXButton1 && currentSwitchKey == Keys.XButton1) ||
                            (isXButton2 && currentSwitchKey == Keys.XButton2))
                    {
                        HandleModeSwitch();
                    }
                }
                else if (wParam == (IntPtr)WinMessages.WM_MBUTTONDOWN)
                {
                    if (isSettingKey)
                    {
                        isSettingKey = false;
                        btnSetKey.Enabled = true;
                        btnSetKey.Text = "Set Toggle Key";
                        currentMacroKey = Keys.MButton;
                        currentToggleType = ToggleType.MouseMiddle;
                        UpdateCurrentKey("MButton");
                        SaveCurrentSettings();
                        UpdateDebugInfo("Set toggle key to MButton");
                    }
                    else if (isSettingMacroSwitchKey)
                    {
                        isSettingMacroSwitchKey = false;
                        btnSetMacroSwitchKey.Enabled = true;
                        btnSetMacroSwitchKey.Text = "Set Switch Key";
                        currentSwitchKey = Keys.MButton;
                        UpdateMacroSwitchKey("MButton");
                        SaveCurrentSettings();
                        UpdateDebugInfo("Set macro switch key to MButton");
                    }
                    else if (currentMacroKey == Keys.MButton)
                    {
                        ToggleMacro();
                    }
                    else if (currentSwitchKey == Keys.MButton)
                    {
                        HandleModeSwitch();
                    }
                }

                CheckJitterState();
            }

            return NativeMethods.CallNextHookEx(mouseHookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Callback function for the low-level keyboard hook.
        /// Handles keyboard events for macro toggling and mode switching.
        /// </summary>
        /// <param name="nCode">Hook code; if less than zero, the hook procedure must pass the message to CallNextHookEx.</param>
        /// <param name="wParam">Message identifier.</param>
        /// <param name="lParam">Pointer to a KBDLLHOOKSTRUCT structure.</param>
        /// <returns>If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx.</returns>
        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WinMessages.WM_KEYDOWN)
                {
                    var vkCode = Marshal.ReadInt32(lParam);

                    if (isSettingKey)
                    {
                        isSettingKey = false;
                        btnSetKey.Enabled = true;
                        btnSetKey.Text = "Set Toggle Key";
                        currentMacroKey = (Keys)vkCode;
                        currentToggleType = ToggleType.Keyboard;
                        UpdateCurrentKey(currentMacroKey.ToString());
                        SaveCurrentSettings();
                        UpdateDebugInfo($"Set toggle key to {currentMacroKey}");
                    }
                    else if (isSettingMacroSwitchKey)
                    {
                        isSettingMacroSwitchKey = false;
                        btnSetMacroSwitchKey.Enabled = true;
                        btnSetMacroSwitchKey.Text = "Set Switch Key";
                        currentSwitchKey = (Keys)vkCode;
                        UpdateMacroSwitchKey(currentSwitchKey.ToString());
                        SaveCurrentSettings();
                        UpdateDebugInfo($"Set macro switch key to {currentSwitchKey}");
                    }
                    else if ((Keys)vkCode == currentMacroKey && currentToggleType == ToggleType.Keyboard)
                    {
                        ToggleMacro();
                    }
                    else if ((Keys)vkCode == currentSwitchKey)
                    {
                        HandleModeSwitch();
                    }
                }
            }

            return NativeMethods.CallNextHookEx(keyboardHookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Checks and updates the jitter/recoil reduction state based on mouse button states.
        /// Activates or deactivates the jitter timer based on current conditions.
        /// </summary>
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

        /// <summary>
        /// Timer callback that handles mouse movement for both jitter and recoil reduction modes.
        /// </summary>
        /// <param name="state">State object (not used).</param>
        private void OnJitterTimer(object state)
        {
            if (!isJittering) return;

            try
            {
                lock (lockObject)
                {
                    var input = new NativeMethods.INPUT
                    {
                        type = WinMessages.INPUT_MOUSE,
                        mi = new NativeMethods.MOUSEINPUT
                        {
                            mouseData = 0,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    };

                    bool useJitter = alwaysJitterMode || (!alwaysRecoilReductionMode && jitterEnabled);

                    if (useJitter)
                    {
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
                            if (recoilReductionStrength == 1)
                            {
                                // Special case for strength 1 - minimal movement
                                input.mi.dy = Math.Max(1, (int)Math.Round(BASE_RECOIL_STRENGTH * 0.3));
                            }
                            else
                            {
                                double logBase = 1.5;
                                input.mi.dy = Math.Max(1, (int)Math.Round(BASE_RECOIL_STRENGTH * Math.Log(recoilReductionStrength + 1, logBase)));
                            }
                        }
                        // Mid Range (7-13): Standard recoil control with linear scaling
                        else if (recoilReductionStrength <= 14)
                        {
                            input.mi.dy = Math.Max(1, (int)Math.Round(BASE_RECOIL_STRENGTH * recoilReductionStrength * 1.2));
                        }
                        // High Range (14-20): Aggressive compensation with enhanced exponential scaling
                        else
                        {
                            // Using an even more aggressive scaling factor for maximum effect
                            double baseValue = BASE_RECOIL_STRENGTH * 20.0; // Increased base value for stronger effect
                            double scalingFactor = 1.3; // Higher scaling factor for more dramatic progression
                            double exponentialBoost = 1.2; // Additional multiplier for enhanced scaling
                            input.mi.dy = Math.Max(1, (int)Math.Round(
                                baseValue *
                                Math.Pow(scalingFactor, recoilReductionStrength - 13) *
                                Math.Pow(exponentialBoost, (recoilReductionStrength - 13) / 2)
                            ));
                        }
                    }

                    input.mi.dwFlags = WinMessages.MOUSEEVENTF_MOVE;
                    NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
                }
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Movement error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the form title to reflect current macro state and mode.
        /// </summary>
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

        /// <summary>
        /// Updates the displayed current key binding for macro toggle.
        /// </summary>
        /// <param name="key">The key name to display.</param>
        private void UpdateCurrentKey(string key)
        {
            if (lblCurrentKeyValue != null)
            {
                lblCurrentKeyValue.Text = key;
            }
        }

        /// <summary>
        /// Updates the displayed key binding for mode switching.
        /// </summary>
        /// <param name="key">The key name to display.</param>
        private void UpdateSwitchKey(string key)
        {
            if (lblMacroSwitchKeyValue != null)
            {
                lblMacroSwitchKeyValue.Text = key;
            }
        }

        /// <summary>
        /// Updates the displayed jitter strength value.
        /// Thread-safe method that can be called from any thread.
        /// </summary>
        /// <param name="strength">The strength value to display (1-20).</param>
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

        /// <summary>
        /// Updates the displayed recoil reduction strength value.
        /// Thread-safe method that can be called from any thread.
        /// </summary>
        /// <param name="strength">The strength value to display (1-20).</param>
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

        /// <summary>
        /// Updates the displayed macro switch key value.
        /// Thread-safe method that can be called from any thread.
        /// </summary>
        /// <param name="key">The key name to display.</param>
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

        /// <summary>
        /// Adds a debug message to the debug panel with timestamp.
        /// Thread-safe method that can be called from any thread.
        /// </summary>
        /// <param name="message">The debug message to display.</param>
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

        /// <summary>
        /// Shows and activates the main window when restored from system tray.
        /// </summary>
        private void ShowWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
            notifyIcon.Visible = false;
            UpdateDebugInfo("Application restored from system tray");
        }

        /// <summary>
        /// Performs cleanup and exits the application.
        /// Ensures all resources are properly disposed and hooks are unregistered.
        /// </summary>
        private void CleanupAndExit()
        {
            if (isExiting) return;
            isExiting = true;

            try
            {
                // Use Dispose pattern for cleanup
                this.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }

            // Force process to exit
            try
            {
                using (var process = Process.GetCurrentProcess())
                {
                    process.Kill();
                }
            }
            catch
            {
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Toggles the macro on/off state and updates the UI accordingly.
        /// </summary>
        private void ToggleMacro()
        {
            isMacroOn = !isMacroOn;
            UpdateTitle();
            string mode = jitterEnabled ? "Jitter" : "Recoil Reduction";
            string alwaysMode = alwaysJitterMode ? "Always Jitter" : (alwaysRecoilReductionMode ? "Always Recoil Reduction" : "Normal");
            UpdateDebugInfo($"Macro {(isMacroOn ? "Enabled" : "Disabled")} - Mode: {mode}, Always Mode: {alwaysMode}, Key: **{toggleKey}**");
            UpdateModeLabels();
        }

        /// <summary>
        /// Handles switching between jitter and recoil reduction modes.
        /// Only works when neither "Always" mode is enabled.
        /// </summary>
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

        /// <summary>
        /// Updates the active mode labels in the UI.
        /// Thread-safe method that can be called from any thread.
        /// </summary>
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

        /// <summary>
        /// Initializes the application icon for both the main window and system tray.
        /// </summary>
        private void InitializeIcon()
        {
            try
            {
                using var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (icon != null)
                {
                    this.Icon = (Icon)icon.Clone();
                    notifyIcon.Icon = (Icon)icon.Clone();
                }
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error loading icon: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes default hotkey bindings and updates the UI to reflect them.
        /// </summary>
        private void InitializeHotkeys()
        {
            // Initialize default hotkeys
            UpdateCurrentKey(currentMacroKey.ToString());
            UpdateSwitchKey(currentSwitchKey.ToString());
        }

        /// <summary>
        /// Initializes tooltips for various UI controls to provide user guidance.
        /// </summary>
        private void InitializeTooltips()
        {
            // Initialize tooltips for controls using the managed ToolTip instance
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
