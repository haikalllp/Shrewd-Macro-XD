using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.ComponentModel;
using NotesAndTasks.Configuration;
using NotesAndTasks.Hooks;
using NotesAndTasks.Utilities;

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
        private readonly KeyboardHook keyboardHook;
        private readonly MouseHook mouseHook;
        private readonly MacroManager macroManager;
        private readonly HotkeyManager hotkeyManager;
        private readonly ToolTip toolTip;

        private bool isSettingKey = false;
        private bool isSettingMacroSwitchKey = false;
        private bool isExiting = false;
        private System.Threading.Timer jitterTimer;
        private int jitterStrength = 3;  // Default to 3
        private int recoilReductionStrength = 1;  // Default to 1
        private bool isJittering = false;
        private int currentStep = 0;
        private readonly object lockObject = new object();
        private bool leftButtonDown = false;
        private bool rightButtonDown = false;
        private bool jitterEnabled = false;
        private bool alwaysJitterMode = false;
        private bool alwaysRecoilReductionMode = false;
        private bool recoilReductionEnabled = false;

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
            // Initialize hooks and tools before InitializeComponent
            keyboardHook = new KeyboardHook();
            mouseHook = new MouseHook();
            macroManager = new MacroManager();
            hotkeyManager = new HotkeyManager();
            toolTip = new ToolTip();

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

                // Set up hook event handlers
                keyboardHook.KeyDown += OnKeyDown;
                mouseHook.MouseDown += OnMouseDown;
                mouseHook.MouseUp += OnMouseUp;

                // Set up macro manager event handlers
                macroManager.MacroStateChanged += OnMacroStateChanged;
                macroManager.ModeChanged += OnModeChanged;
                macroManager.JitterStarted += (s, e) => UpdateDebugInfo("Jitter started");
                macroManager.JitterStopped += (s, e) => UpdateDebugInfo("Jitter stopped");
                macroManager.RecoilReductionStarted += (s, e) => UpdateDebugInfo("Recoil reduction started");
                macroManager.RecoilReductionStopped += (s, e) => UpdateDebugInfo("Recoil reduction stopped");

                // Set up hotkey manager event handlers
                hotkeyManager.MacroKeyChanged += (s, key) => UpdateCurrentKey(key.ToString());
                hotkeyManager.SwitchKeyChanged += (s, key) => UpdateMacroSwitchKey(key.ToString());

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

        private void OnKeyDown(object sender, KeyboardHookEventArgs e)
        {
            try
            {
                if (isSettingKey)
                {
                    isSettingKey = false;
                    btnSetKey.Enabled = true;
                    btnSetKey.Text = "Set Toggle Key";
                    hotkeyManager.SetMacroKey(e.VirtualKeyCode, ToggleType.Keyboard);
                    UpdateDebugInfo($"Set toggle key to {e.VirtualKeyCode}");
                }
                else if (isSettingMacroSwitchKey)
                {
                    isSettingMacroSwitchKey = false;
                    btnSetMacroSwitch.Enabled = true;
                    btnSetMacroSwitch.Text = "Set Switch Key";
                    hotkeyManager.SetSwitchKey(e.VirtualKeyCode);
                    UpdateDebugInfo($"Set macro switch key to {e.VirtualKeyCode}");
                }
                else if (e.VirtualKeyCode == hotkeyManager.MacroKey && hotkeyManager.ToggleType == ToggleType.Keyboard)
                {
                    macroManager.ToggleMacro();
                }
                else if (e.VirtualKeyCode == hotkeyManager.SwitchKey)
                {
                    macroManager.SwitchMode();
                }
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error in OnKeyDown: {ex.Message}");
            }
        }

        private void OnMouseDown(object sender, MouseHookEventArgs e)
        {
            try
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                    case MouseButtons.Right:
                        macroManager.HandleMouseButton(e.Button, true);
                        break;
                    case MouseButtons.Middle:
                        if (isSettingKey)
                        {
                            isSettingKey = false;
                            btnSetKey.Enabled = true;
                            btnSetKey.Text = "Set Toggle Key";
                            hotkeyManager.SetMacroKey(Keys.MButton, ToggleType.MouseMiddle);
                            UpdateDebugInfo("Set toggle key to MButton");
                        }
                        else if (isSettingMacroSwitchKey)
                        {
                            isSettingMacroSwitchKey = false;
                            btnSetMacroSwitch.Enabled = true;
                            btnSetMacroSwitch.Text = "Set Switch Key";
                            hotkeyManager.SetSwitchKey(Keys.MButton);
                            UpdateDebugInfo("Set macro switch key to MButton");
                        }
                        else if (hotkeyManager.MacroKey == Keys.MButton)
                        {
                            macroManager.ToggleMacro();
                        }
                        else if (hotkeyManager.SwitchKey == Keys.MButton)
                        {
                            macroManager.SwitchMode();
                        }
                        break;
                    case MouseButtons.XButton1:
                    case MouseButtons.XButton2:
                        if (isSettingKey)
                        {
                            isSettingKey = false;
                            btnSetKey.Enabled = true;
                            btnSetKey.Text = "Set Toggle Key";
                            string buttonName = e.Button == MouseButtons.XButton1 ? "XButton1" : "XButton2";
                            Keys key = e.Button == MouseButtons.XButton1 ? Keys.XButton1 : Keys.XButton2;
                            var type = e.Button == MouseButtons.XButton1 ? 
                                ToggleType.MouseX1 : 
                                ToggleType.MouseX2;
                            hotkeyManager.SetMacroKey(key, type);
                            UpdateDebugInfo($"Set toggle key to {buttonName}");
                        }
                        else if (isSettingMacroSwitchKey)
                        {
                            isSettingMacroSwitchKey = false;
                            btnSetMacroSwitch.Enabled = true;
                            btnSetMacroSwitch.Text = "Set Switch Key";
                            string buttonName = e.Button == MouseButtons.XButton1 ? "XButton1" : "XButton2";
                            Keys key = e.Button == MouseButtons.XButton1 ? Keys.XButton1 : Keys.XButton2;
                            hotkeyManager.SetSwitchKey(key);
                            UpdateDebugInfo($"Set macro switch key to {buttonName}");
                        }
                        else if ((e.Button == MouseButtons.XButton1 && hotkeyManager.MacroKey == Keys.XButton1) ||
                                (e.Button == MouseButtons.XButton2 && hotkeyManager.MacroKey == Keys.XButton2))
                        {
                            macroManager.ToggleMacro();
                        }
                        else if ((e.Button == MouseButtons.XButton1 && hotkeyManager.SwitchKey == Keys.XButton1) ||
                                (e.Button == MouseButtons.XButton2 && hotkeyManager.SwitchKey == Keys.XButton2))
                        {
                            macroManager.SwitchMode();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error in OnMouseDown: {ex.Message}");
            }
        }

        private void OnMouseUp(object sender, MouseHookEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                {
                    macroManager.HandleMouseButton(e.Button, false);
                }
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error in OnMouseUp: {ex.Message}");
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
                // Start the hooks
                keyboardHook.Start();
                mouseHook.Start();
                
                // Update UI
                UpdateTitle();
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error initializing hooks: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles changes in the macro's enabled/disabled state.
        /// </summary>
        private void OnMacroStateChanged(object sender, bool isEnabled)
        {
            try
            {
                UpdateTitle();
                UpdateDebugInfo($"Macro {(isEnabled ? "enabled" : "disabled")}");
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error in OnMacroStateChanged: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles changes in the macro's mode (jitter/recoil reduction).
        /// </summary>
        private void OnModeChanged(object sender, bool isJitterMode)
        {
            try
            {
                UpdateTitle();
                UpdateModeLabels();
                UpdateDebugInfo($"Mode switched to {(isJitterMode ? "jitter" : "recoil reduction")}");
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error in OnModeChanged: {ex.Message}");
            }
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
            if (hotkeyManager.MacroKey != Keys.None)
                UpdateCurrentKey(hotkeyManager.MacroKey.ToString());
            if (hotkeyManager.SwitchKey != Keys.None)
                UpdateSwitchKey(hotkeyManager.SwitchKey.ToString());

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
                try
                {
                    Validation.ValidateStrength(trackBarJitter.Value, trackBarJitter.Minimum, trackBarJitter.Maximum, nameof(trackBarJitter.Value));
                    macroManager.SetJitterStrength(trackBarJitter.Value);
                    SettingsManager.CurrentSettings.JitterStrength = trackBarJitter.Value;
                    SettingsManager.SaveSettings();
                    UpdateJitterStrength(trackBarJitter.Value);
                    UpdateDebugInfo($"Jitter strength set to {trackBarJitter.Value}");
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    UpdateDebugInfo($"Error setting jitter strength: {ex.Message}");
                    trackBarJitter.Value = Math.Max(trackBarJitter.Minimum, Math.Min(trackBarJitter.Maximum, trackBarJitter.Value));
                }
            };

            trackBarRecoilReduction.ValueChanged += (sender, e) =>
            {
                try
                {
                    Validation.ValidateStrength(trackBarRecoilReduction.Value, trackBarRecoilReduction.Minimum, trackBarRecoilReduction.Maximum, nameof(trackBarRecoilReduction.Value));
                    macroManager.SetRecoilReductionStrength(trackBarRecoilReduction.Value);
                    SettingsManager.CurrentSettings.RecoilReductionStrength = trackBarRecoilReduction.Value;
                    SettingsManager.SaveSettings();
                    UpdateRecoilReductionStrength(trackBarRecoilReduction.Value);
                    UpdateDebugInfo($"Recoil reduction strength set to {trackBarRecoilReduction.Value}");
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    UpdateDebugInfo($"Error setting recoil reduction strength: {ex.Message}");
                    trackBarRecoilReduction.Value = Math.Max(trackBarRecoilReduction.Minimum, Math.Min(trackBarRecoilReduction.Maximum, trackBarRecoilReduction.Value));
                }
            };

            chkAlwaysJitter.CheckedChanged += (sender, e) =>
            {
                macroManager.SetAlwaysJitterMode(chkAlwaysJitter.Checked);
                btnSetMacroSwitch.Enabled = !chkAlwaysJitter.Checked;
                SettingsManager.CurrentSettings.AlwaysJitterMode = chkAlwaysJitter.Checked;
                SettingsManager.SaveSettings();
                UpdateTitle();
                UpdateModeLabels();
            };

            chkAlwaysRecoilReduction.CheckedChanged += (sender, e) =>
            {
                macroManager.SetAlwaysRecoilReductionMode(chkAlwaysRecoilReduction.Checked);
                btnSetMacroSwitch.Enabled = !chkAlwaysRecoilReduction.Checked;
                SettingsManager.CurrentSettings.AlwaysRecoilReductionMode = chkAlwaysRecoilReduction.Checked;
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
        /// Validates all settings before applying them and falls back to defaults if validation fails.
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                var settings = SettingsManager.CurrentSettings;
                Validation.ValidateNotNull(settings, nameof(settings));

                // Validate all settings before applying them
                if (!SettingsValidation.ValidateSettings(settings, 
                    Math.Min(trackBarJitter.Minimum, trackBarRecoilReduction.Minimum),
                    Math.Max(trackBarJitter.Maximum, trackBarRecoilReduction.Maximum)))
                {
                    UpdateDebugInfo("Invalid settings detected, resetting to defaults");
                    ResetToDefaultSettings();
                    return;
                }

                // Apply validated settings to UI
                trackBarJitter.Value = settings.JitterStrength;
                trackBarRecoilReduction.Value = settings.RecoilReductionStrength;
                chkAlwaysJitter.Checked = settings.AlwaysJitterMode;
                chkAlwaysRecoilReduction.Checked = settings.AlwaysRecoilReductionMode;
                chkMinimizeToTray.Checked = settings.MinimizeToTray;

                // Apply settings to MacroManager
                macroManager.SetJitterStrength(settings.JitterStrength);
                macroManager.SetRecoilReductionStrength(settings.RecoilReductionStrength);
                macroManager.SetAlwaysJitterMode(settings.AlwaysJitterMode);
                macroManager.SetAlwaysRecoilReductionMode(settings.AlwaysRecoilReductionMode);

                // Update UI elements
                UpdateTitle();
                UpdateModeLabels();
                UpdateDebugInfo("Settings loaded successfully");
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error loading settings: {ex.Message}");
                ResetToDefaultSettings();
            }
        }

        /// <summary>
        /// Saves the current settings to the settings manager.
        /// </summary>
        private void SaveCurrentSettings()
        {
            try
            {
                var settings = SettingsManager.CurrentSettings;
                settings.JitterStrength = trackBarJitter.Value;
                settings.RecoilReductionStrength = trackBarRecoilReduction.Value;
                settings.AlwaysJitterMode = chkAlwaysJitter.Checked;
                settings.AlwaysRecoilReductionMode = chkAlwaysRecoilReduction.Checked;
                settings.MinimizeToTray = chkMinimizeToTray.Checked;
                SettingsManager.SaveSettings();
                UpdateDebugInfo("Settings saved successfully");
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error saving settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Resets all settings to their default values.
        /// </summary>
        private void ResetToDefaultSettings()
        {
            try
            {
                // Reset UI controls
                trackBarJitter.Value = 3;
                trackBarRecoilReduction.Value = 1;
                chkAlwaysJitter.Checked = false;
                chkAlwaysRecoilReduction.Checked = false;
                chkMinimizeToTray.Checked = true;

                // Reset MacroManager
                macroManager.SetJitterStrength(3);
                macroManager.SetRecoilReductionStrength(1);
                macroManager.SetAlwaysJitterMode(false);
                macroManager.SetAlwaysRecoilReductionMode(false);

                // Reset hotkeys
                hotkeyManager.SetMacroKey(Keys.Capital, ToggleType.Keyboard);
                hotkeyManager.SetSwitchKey(Keys.Q);

                // Update UI
                UpdateCurrentKey(hotkeyManager.MacroKey.ToString());
                UpdateSwitchKey(hotkeyManager.SwitchKey.ToString());
                UpdateTitle();
                UpdateModeLabels();

                // Save settings
                SaveCurrentSettings();
                UpdateDebugInfo("Settings reset to defaults");
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error resetting settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the form title to reflect current macro state and mode.
        /// </summary>
        private void UpdateTitle()
        {
            string jitterMode = macroManager.IsAlwaysJitterMode ? "Always Jitter" :
                (macroManager.IsJitterEnabled ? "Jitter" : "Jitter (OFF)");

            string recoilMode = macroManager.IsAlwaysRecoilReductionMode ? "Always Recoil Reduction" :
                (macroManager.IsJitterEnabled ? "Recoil Reduction (OFF)" : "Recoil Reduction");

            this.Text = $"Notes&Tasks [{(macroManager.IsEnabled ? "ON" : "OFF")}] - {jitterMode} / {recoilMode} Mode";
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
            try
            {
                Validation.ValidateStrength(strength, trackBarJitter.Minimum, trackBarJitter.Maximum, nameof(strength));

                if (InvokeRequired)
                {
                    Invoke(new Action<int>(UpdateJitterStrength), strength);
                    return;
                }

                lblJitterStrengthValue.Text = strength.ToString();
                UpdateDebugInfo($"Jitter strength updated to: {strength}");
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error updating jitter strength: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the displayed recoil reduction strength value.
        /// Thread-safe method that can be called from any thread.
        /// </summary>
        /// <param name="strength">The strength value to display (1-20).</param>
        private void UpdateRecoilReductionStrength(int strength)
        {
            try
            {
                Validation.ValidateStrength(strength, trackBarRecoilReduction.Minimum, trackBarRecoilReduction.Maximum, nameof(strength));

                if (InvokeRequired)
                {
                    Invoke(new Action<int>(UpdateRecoilReductionStrength), strength);
                    return;
                }

                lblRecoilReductionStrengthValue.Text = strength.ToString();
                UpdateDebugInfo($"Recoil reduction strength updated to: {strength}");
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error updating recoil reduction strength: {ex.Message}");
            }
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
        /// Toggles the macro on/off state and updates the UI accordingly.
        /// </summary>
        private void ToggleMacro()
        {
            macroManager.ToggleMacro();
            string mode = macroManager.IsJitterEnabled ? "Jitter" : "Recoil Reduction";
            string alwaysMode = macroManager.IsAlwaysJitterMode ? "Always Jitter" : 
                (macroManager.IsAlwaysRecoilReductionMode ? "Always Recoil Reduction" : "Normal");
            UpdateDebugInfo($"Macro {(macroManager.IsEnabled ? "Enabled" : "Disabled")} - Mode: {mode}, Always Mode: {alwaysMode}, Key: **{hotkeyManager.MacroKey}**");
            UpdateModeLabels();
        }

        /// <summary>
        /// Handles switching between jitter and recoil reduction modes.
        /// Only works when neither "Always" mode is enabled.
        /// Validates mode state before switching.
        /// </summary>
        private void HandleModeSwitch()
        {
            try
            {
                // Validate current state
                if (isSettingKey || isSettingMacroSwitchKey)
                {
                    UpdateDebugInfo("Cannot switch modes while setting keys");
                    return;
                }

                // Don't switch if either "Always" mode is enabled
                if (alwaysJitterMode || alwaysRecoilReductionMode)
                {
                    UpdateDebugInfo("Cannot switch modes when 'Always' mode is enabled");
                    return;
                }

                // Toggle between jitter and recoil reduction with validation
                bool previousJitterState = jitterEnabled;
                bool previousRecoilState = recoilReductionEnabled;

                jitterEnabled = !jitterEnabled;
                recoilReductionEnabled = !jitterEnabled;

                // Validate the new state
                if (jitterEnabled == recoilReductionEnabled)
                {
                    // Invalid state detected, revert changes
                    jitterEnabled = previousJitterState;
                    recoilReductionEnabled = previousRecoilState;
                    UpdateDebugInfo("Error: Invalid mode state detected");
                    return;
                }

                // Save the new state
                try
                {
                    SettingsManager.CurrentSettings.JitterEnabled = jitterEnabled;
                    SettingsManager.CurrentSettings.RecoilReductionEnabled = recoilReductionEnabled;
                    SettingsManager.SaveSettings();
                }
                catch (Exception ex)
                {
                    // Revert changes if settings save fails
                    jitterEnabled = previousJitterState;
                    recoilReductionEnabled = previousRecoilState;
                    UpdateDebugInfo($"Error saving mode settings: {ex.Message}");
                    return;
                }

                UpdateTitle();
                UpdateModeLabels();
                UpdateDebugInfo($"Switched to {(jitterEnabled ? "Jitter" : "Recoil Reduction")} mode");
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error in HandleModeSwitch: {ex.Message}");
            }
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

            lblRecoilReductionActive.Text = (!macroManager.IsJitterEnabled && macroManager.IsEnabled) ? "[Active]" : "";
            lblJitterActive.Text = (macroManager.IsJitterEnabled && macroManager.IsEnabled) ? "[Active]" : "";
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
            UpdateCurrentKey(hotkeyManager.MacroKey.ToString());
            UpdateSwitchKey(hotkeyManager.SwitchKey.ToString());
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
            SaveCurrentSettings();
        }

        /// <summary>
        /// Performs cleanup operations before exiting the application.
        /// </summary>
        private void CleanupAndExit()
        {
            try
            {
                isExiting = true;
                SaveCurrentSettings();

                // Stop and dispose of hooks
                keyboardHook?.Dispose();
                mouseHook?.Dispose();

                // Stop and dispose of macro manager
                macroManager?.Dispose();

                // Clean up UI resources
                notifyIcon?.Dispose();
                toolTip?.Dispose();

                // Close the form
                this.Close();
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error during cleanup: {ex.Message}");
            }
        }
    }
}
