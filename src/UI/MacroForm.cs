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
using NotesAndTasks.UI;
using NotesAndTasks.Models;

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
        private readonly UIManager uiManager;
        private readonly ToolTip toolTip;
        private bool isExiting = false;
        private readonly EventHandler alwaysJitterCheckedChanged;
        private readonly EventHandler alwaysRecoilReductionCheckedChanged;
        #endregion

        /// <summary>
        /// Initializes a new instance of the MacroForm class.
        /// Sets up all necessary components, hooks, and event handlers.
        /// </summary>
        public MacroForm()
        {
            try
            {
                // Initialize managers and hooks
                macroManager = new MacroManager();
                hotkeyManager = new HotkeyManager(macroManager);
                keyboardHook = new KeyboardHook();
                mouseHook = new MouseHook();
                toolTip = new ToolTip();

                InitializeComponent();

                // Initialize UI Manager after components are initialized
                uiManager = new UIManager(
                    this,
                    macroManager,
                    hotkeyManager,
                    debugLabel,
                    lblJitterActive,
                    lblRecoilReductionActive,
                    lblCurrentKeyValue,
                    lblMacroSwitchKeyValue,
                    lblJitterStrengthValue,
                    lblRecoilReductionStrengthValue,
                    notifyIcon,
                    toolTip
                );

                // Initialize components and settings
                InitializeCustomComponents();

                // Initialize tray icon behavior
                notifyIcon.Click += (s, e) => 
                {
                    // Only respond to left clicks
                    if (((MouseEventArgs)e).Button == MouseButtons.Left)
                    {
                        uiManager.ShowWindow();
                    }
                };
                showWindowMenuItem.Click += (s, e) => uiManager.ShowWindow();
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
                macroManager.JitterStarted += (s, e) => uiManager.UpdateDebugInfo("Jitter started");
                macroManager.JitterStopped += (s, e) => uiManager.UpdateDebugInfo("Jitter stopped");
                macroManager.RecoilReductionStarted += (s, e) => uiManager.UpdateDebugInfo("Recoil reduction started");
                macroManager.RecoilReductionStopped += (s, e) => uiManager.UpdateDebugInfo("Recoil reduction stopped");

                // Set up hotkey manager event handlers
                hotkeyManager.MacroKeyChanged += (s, key) => uiManager.UpdateCurrentKey(key.ToString());
                hotkeyManager.SwitchKeyChanged += (s, key) => uiManager.UpdateMacroSwitchKey(key.ToString());
                hotkeyManager.KeySettingStateChanged += OnKeySettingStateChanged;
                hotkeyManager.DebugInfoUpdated += (s, info) => uiManager.UpdateDebugInfo(info);

                // Handle application exit
                Application.ApplicationExit += (s, e) =>
                {
                    if (!isExiting)
                    {
                        CleanupAndExit();
                    }
                };

                // Initialize event handlers
                alwaysJitterCheckedChanged = (sender, e) =>
                {
                    try
                    {
                        if (chkAlwaysJitter.Checked)
                        {
                            chkAlwaysRecoilReduction.Checked = false;
                            macroManager.SetAlwaysJitterMode(true);
                            ConfigurationManager.Instance.CurrentSettings.MacroSettings.AlwaysJitterMode = true;
                            ConfigurationManager.Instance.SaveSettings();
                            uiManager.UpdateTitle();
                            uiManager.UpdateModeLabels();
                        }
                        else
                        {
                            macroManager.SetAlwaysJitterMode(false);
                            ConfigurationManager.Instance.CurrentSettings.MacroSettings.AlwaysJitterMode = false;
                            ConfigurationManager.Instance.SaveSettings();
                            uiManager.UpdateTitle();
                            uiManager.UpdateModeLabels();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error toggling jitter: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        chkAlwaysJitter.Checked = false;
                    }
                };

                alwaysRecoilReductionCheckedChanged = (sender, e) =>
                {
                    try
                    {
                        if (chkAlwaysRecoilReduction.Checked)
                        {
                            chkAlwaysJitter.Checked = false;
                            macroManager.SetAlwaysRecoilReductionMode(true);
                            ConfigurationManager.Instance.CurrentSettings.MacroSettings.AlwaysRecoilReductionMode = true;
                            ConfigurationManager.Instance.SaveSettings();
                            uiManager.UpdateTitle();
                            uiManager.UpdateModeLabels();
                        }
                        else
                        {
                            macroManager.SetAlwaysRecoilReductionMode(false);
                            ConfigurationManager.Instance.CurrentSettings.MacroSettings.AlwaysRecoilReductionMode = false;
                            ConfigurationManager.Instance.SaveSettings();
                            uiManager.UpdateTitle();
                            uiManager.UpdateModeLabels();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error toggling recoil reduction: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        chkAlwaysRecoilReduction.Checked = false;
                    }
                };

                // Subscribe to events
                chkAlwaysJitter.CheckedChanged += alwaysJitterCheckedChanged;
                chkAlwaysRecoilReduction.CheckedChanged += alwaysRecoilReductionCheckedChanged;
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
                hotkeyManager.HandleKeyDown(e.VirtualKeyCode);
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Error in OnKeyDown: {ex.Message}");
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
                    case MouseButtons.XButton1:
                    case MouseButtons.XButton2:
                        hotkeyManager.HandleMouseButton(e.Button);
                        break;
                }
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Error in OnMouseDown: {ex.Message}");
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
                uiManager.UpdateDebugInfo($"Error in OnMouseUp: {ex.Message}");
            }
        }

        private void OnKeySettingStateChanged(object sender, bool isSettingKey)
        {
            try
            {
                btnSetKey.Enabled = !isSettingKey;
                btnSetMacroSwitch.Enabled = !isSettingKey;

                if (hotkeyManager.IsSettingMacroKey)
                {
                    btnSetKey.Text = "Press any key...";
                    btnSetMacroSwitch.Text = "Set Switch Key";
                }
                else if (hotkeyManager.IsSettingSwitchKey)
                {
                    btnSetKey.Text = "Set Toggle Key";
                    btnSetMacroSwitch.Text = "Press any key...";
                }
                else
                {
                    btnSetKey.Text = "Set Toggle Key";
                    btnSetMacroSwitch.Text = "Set Switch Key";
                }
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Error in OnKeySettingStateChanged: {ex.Message}");
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
            // Only minimize to tray if explicitly requested by user closing AND minimizeToTray is checked
            if (!isExiting && chkMinimizeToTray.Checked && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                notifyIcon.Visible = true;
                uiManager.UpdateDebugInfo("Application minimized to system tray");
            }
            else
            {
                // Always perform cleanup when actually closing (not minimizing)
                if (!isExiting)
                {
                    SaveCurrentSettings();
                    PerformCleanup();
                }
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
                uiManager.UpdateTitle();
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Error initializing hooks: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles changes in the macro's enabled/disabled state.
        /// </summary>
        private void OnMacroStateChanged(object sender, bool isEnabled)
        {
            try
            {
                uiManager.UpdateTitle();
                uiManager.UpdateDebugInfo($"Macro {(isEnabled ? "enabled" : "disabled")}");
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Error in OnMacroStateChanged: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles changes in the macro's mode (jitter/recoil reduction).
        /// </summary>
        private void OnModeChanged(object sender, bool isJitterMode)
        {
            try
            {
                uiManager.UpdateTitle();
                uiManager.UpdateModeLabels();
                uiManager.UpdateDebugInfo($"Mode switched to {(isJitterMode ? "jitter" : "recoil reduction")}");
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Error in OnModeChanged: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes custom components including icons, hotkeys, tooltips, and loads saved settings.
        /// </summary>
        private void InitializeCustomComponents()
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
                uiManager.UpdateDebugInfo($"Error loading icon: {ex.Message}");
            }

            // Set initial text with bold formatting
            if (hotkeyManager.MacroKey != Keys.None)
                uiManager.UpdateCurrentKey(hotkeyManager.MacroKey.ToString());
            if (hotkeyManager.SwitchKey != Keys.None)
                uiManager.UpdateSwitchKey(hotkeyManager.SwitchKey.ToString());

            // Initialize tooltips
            toolTip.SetToolTip(chkAlwaysJitter, "Always keep Jitter enabled");
            toolTip.SetToolTip(trackBarJitter, "Adjust Jitter strength");
            toolTip.SetToolTip(chkAlwaysRecoilReduction, "Always keep Recoil Reduction enabled");
            toolTip.SetToolTip(trackBarRecoilReduction, "Adjust Recoil Reduction strength");
            toolTip.SetToolTip(chkMinimizeToTray, "Minimize to system tray when closing");

            // Initialize event handlers
            InitializeEventHandlers();

            // Load settings
            LoadSettings();
        }

        private void InitializeEventHandlers()
        {
            btnToggleDebug.Click += (sender, e) =>
            {
                debugPanel.Visible = !debugPanel.Visible;
                btnToggleDebug.Text = debugPanel.Visible ? "Hide Debug Info" : "Show Debug Info";
                uiManager.UpdateDebugInfo("Debug panel visibility toggled");
            };

            btnSetKey.Click += (sender, e) =>
            {
                hotkeyManager.StartSettingMacroKey();
            };

            btnSetMacroSwitch.Click += (sender, e) =>
            {
                hotkeyManager.StartSettingSwitchKey();
            };

            chkMinimizeToTray.CheckedChanged += (sender, e) =>
            {
                ConfigurationManager.Instance.CurrentSettings.UISettings.MinimizeToTray = chkMinimizeToTray.Checked;
                ConfigurationManager.Instance.SaveSettings();
            };

            trackBarJitter.ValueChanged += (sender, e) =>
            {
                macroManager.SetJitterStrength(trackBarJitter.Value);
                lblJitterStrengthValue.Text = trackBarJitter.Value.ToString();
                ConfigurationManager.Instance.CurrentSettings.MacroSettings.JitterStrength = trackBarJitter.Value;
                ConfigurationManager.Instance.SaveSettings();
            };

            trackBarRecoilReduction.ValueChanged += (sender, e) =>
            {
                macroManager.SetRecoilReductionStrength(trackBarRecoilReduction.Value);
                lblRecoilReductionStrengthValue.Text = trackBarRecoilReduction.Value.ToString();
                ConfigurationManager.Instance.CurrentSettings.MacroSettings.RecoilReductionStrength = trackBarRecoilReduction.Value;
                ConfigurationManager.Instance.SaveSettings();
            };
        }

        /// <summary>
        /// Validates and corrects settings to ensure they're within valid ranges and follow app rules
        /// </summary>
        private void ValidateAndCorrectSettings(AppSettings settings)
        {
            if (settings == null) return;

            try
            {
                // Validate and correct MacroSettings
                if (settings.MacroSettings != null)
                {
                    // Ensure jitter strength is within valid range
                    if (settings.MacroSettings.JitterStrength < 1)
                    {
                        uiManager.UpdateDebugInfo($"Correcting invalid JitterStrength: {settings.MacroSettings.JitterStrength} to 1");
                        settings.MacroSettings.JitterStrength = 1;
                    }
                    else if (settings.MacroSettings.JitterStrength > 20)
                    {
                        uiManager.UpdateDebugInfo($"Correcting invalid JitterStrength: {settings.MacroSettings.JitterStrength} to 20");
                        settings.MacroSettings.JitterStrength = 20;
                    }

                    // Ensure recoil reduction strength is within valid range
                    if (settings.MacroSettings.RecoilReductionStrength < 1)
                    {
                        uiManager.UpdateDebugInfo($"Correcting invalid RecoilReductionStrength: {settings.MacroSettings.RecoilReductionStrength} to 1");
                        settings.MacroSettings.RecoilReductionStrength = 1;
                    }
                    else if (settings.MacroSettings.RecoilReductionStrength > 20)
                    {
                        uiManager.UpdateDebugInfo($"Correcting invalid RecoilReductionStrength: {settings.MacroSettings.RecoilReductionStrength} to 20");
                        settings.MacroSettings.RecoilReductionStrength = 20;
                    }

                    // Ensure we don't have both always modes enabled simultaneously
                    if (settings.MacroSettings.AlwaysJitterMode && settings.MacroSettings.AlwaysRecoilReductionMode)
                    {
                        uiManager.UpdateDebugInfo("Correcting conflict: Both AlwaysJitterMode and AlwaysRecoilReductionMode were enabled");
                        settings.MacroSettings.AlwaysJitterMode = true;
                        settings.MacroSettings.AlwaysRecoilReductionMode = false;
                    }
                }
                else
                {
                    uiManager.UpdateDebugInfo("Missing MacroSettings, creating defaults");
                    settings.MacroSettings = new MacroSettings();
                }

                // Validate and correct UISettings
                if (settings.UISettings != null)
                {
                    // Ensure window position is valid
                    if (settings.UISettings.WindowPosition.IsEmpty || 
                        settings.UISettings.WindowPosition.X < 0 || 
                        settings.UISettings.WindowPosition.Y < 0)
                    {
                        uiManager.UpdateDebugInfo($"Correcting invalid WindowPosition: {settings.UISettings.WindowPosition}");
                        settings.UISettings.WindowPosition = new System.Drawing.Point(100, 100);
                    }

                    // Ensure window size is valid
                    if (settings.UISettings.WindowSize.IsEmpty || 
                        settings.UISettings.WindowSize.Width < 300 || 
                        settings.UISettings.WindowSize.Height < 200)
                    {
                        uiManager.UpdateDebugInfo($"Correcting invalid WindowSize: {settings.UISettings.WindowSize}");
                        settings.UISettings.WindowSize = new System.Drawing.Size(800, 600);
                    }
                }
                else
                {
                    uiManager.UpdateDebugInfo("Missing UISettings, creating defaults");
                    settings.UISettings = new UISettings();
                }

                // Validate and correct HotkeySettings
                if (settings.HotkeySettings != null)
                {
                    // Validate MacroKey
                    if (settings.HotkeySettings.MacroKey == null)
                    {
                        uiManager.UpdateDebugInfo("Missing MacroKey, creating default");
                        settings.HotkeySettings.MacroKey = new InputBinding(Keys.Capital, InputType.Keyboard);
                    }
                    else if (settings.HotkeySettings.MacroKey.Key == Keys.None)
                    {
                        uiManager.UpdateDebugInfo("Invalid MacroKey, resetting to default");
                        settings.HotkeySettings.MacroKey.Key = Keys.Capital;
                        settings.HotkeySettings.MacroKey.Type = InputType.Keyboard;
                        settings.HotkeySettings.MacroKey.DisplayName = Keys.Capital.ToString();
                    }

                    // Validate SwitchKey
                    if (settings.HotkeySettings.SwitchKey == null)
                    {
                        uiManager.UpdateDebugInfo("Missing SwitchKey, creating default");
                        settings.HotkeySettings.SwitchKey = new InputBinding(Keys.Q, InputType.Keyboard);
                    }
                    else if (settings.HotkeySettings.SwitchKey.Key == Keys.None)
                    {
                        uiManager.UpdateDebugInfo("Invalid SwitchKey, resetting to default");
                        settings.HotkeySettings.SwitchKey.Key = Keys.Q;
                        settings.HotkeySettings.SwitchKey.Type = InputType.Keyboard;
                        settings.HotkeySettings.SwitchKey.DisplayName = Keys.Q.ToString();
                    }

                    // Ensure MacroKey and SwitchKey DisplayNames match their respective Keys
                    if (string.IsNullOrEmpty(settings.HotkeySettings.MacroKey.DisplayName) ||
                        settings.HotkeySettings.MacroKey.DisplayName != settings.HotkeySettings.MacroKey.Key.ToString())
                    {
                        uiManager.UpdateDebugInfo("Correcting MacroKey DisplayName");
                        settings.HotkeySettings.MacroKey.DisplayName = settings.HotkeySettings.MacroKey.Key.ToString();
                    }

                    if (string.IsNullOrEmpty(settings.HotkeySettings.SwitchKey.DisplayName) ||
                        settings.HotkeySettings.SwitchKey.DisplayName != settings.HotkeySettings.SwitchKey.Key.ToString())
                    {
                        uiManager.UpdateDebugInfo("Correcting SwitchKey DisplayName");
                        settings.HotkeySettings.SwitchKey.DisplayName = settings.HotkeySettings.SwitchKey.Key.ToString();
                    }

                    // Ensure MacroKey and SwitchKey are not the same when they have the same input type
                    if (settings.HotkeySettings.MacroKey.Key == settings.HotkeySettings.SwitchKey.Key &&
                        settings.HotkeySettings.MacroKey.Type == settings.HotkeySettings.SwitchKey.Type)
                    {
                        uiManager.UpdateDebugInfo("Correcting conflicting hotkeys: MacroKey and SwitchKey were the same");
                        settings.HotkeySettings.SwitchKey.Key = Keys.Q;
                        settings.HotkeySettings.SwitchKey.Type = InputType.Keyboard;
                        settings.HotkeySettings.SwitchKey.DisplayName = Keys.Q.ToString();
                    }
                }
                else
                {
                    uiManager.UpdateDebugInfo("Missing HotkeySettings, creating defaults");
                    settings.HotkeySettings = new HotkeySettings();
                }
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Error in ValidateAndCorrectSettings: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads saved settings from the settings manager and applies them to the form.
        /// Validates all settings before applying them and falls back to defaults if validation fails.
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                // Force reload settings from file to ensure we have the latest values
                ConfigurationManager.Instance.LoadSettings();
                var settings = ConfigurationManager.Instance.CurrentSettings;
                
                if (settings == null)
                {
                    uiManager.UpdateDebugInfo("Invalid settings detected, resetting to defaults");
                    ResetToDefaultSettings();
                    return;
                }

                // Log the actual values from settings for debugging
                uiManager.UpdateDebugInfo($"Loading settings - JitterStrength: {settings.MacroSettings.JitterStrength}, " +
                                          $"RecoilReduction: {settings.MacroSettings.RecoilReductionStrength}, " +
                                          $"AlwaysJitter: {settings.MacroSettings.AlwaysJitterMode}, " +
                                          $"AlwaysRecoil: {settings.MacroSettings.AlwaysRecoilReductionMode}, " +
                                          $"MinimizeToTray: {settings.UISettings.MinimizeToTray}");

                // Validate and correct settings before applying them
                ValidateAndCorrectSettings(settings);

                // Safety check for trackbar values to prevent exception
                int jitterStrength = settings.MacroSettings.JitterStrength;
                if (jitterStrength < trackBarJitter.Minimum) jitterStrength = trackBarJitter.Minimum;
                if (jitterStrength > trackBarJitter.Maximum) jitterStrength = trackBarJitter.Maximum;
                
                int recoilStrength = settings.MacroSettings.RecoilReductionStrength;
                if (recoilStrength < trackBarRecoilReduction.Minimum) recoilStrength = trackBarRecoilReduction.Minimum;
                if (recoilStrength > trackBarRecoilReduction.Maximum) recoilStrength = trackBarRecoilReduction.Maximum;

                // Apply validated settings to UI with exception handling for each control
                try
                {
                    trackBarJitter.Value = jitterStrength;
                    lblJitterStrengthValue.Text = jitterStrength.ToString();
                }
                catch (Exception ex)
                {
                    uiManager.UpdateDebugInfo($"Error setting jitter trackbar: {ex.Message}");
                    trackBarJitter.Value = trackBarJitter.Minimum;
                }

                try
                {
                    trackBarRecoilReduction.Value = recoilStrength;
                    lblRecoilReductionStrengthValue.Text = recoilStrength.ToString();
                }
                catch (Exception ex)
                {
                    uiManager.UpdateDebugInfo($"Error setting recoil trackbar: {ex.Message}");
                    trackBarRecoilReduction.Value = trackBarRecoilReduction.Minimum;
                }

                try
                {
                    chkAlwaysJitter.Checked = settings.MacroSettings.AlwaysJitterMode;
                }
                catch (Exception ex)
                {
                    uiManager.UpdateDebugInfo($"Error setting AlwaysJitter checkbox: {ex.Message}");
                    chkAlwaysJitter.Checked = false;
                }

                try
                {
                    chkAlwaysRecoilReduction.Checked = settings.MacroSettings.AlwaysRecoilReductionMode;
                }
                catch (Exception ex)
                {
                    uiManager.UpdateDebugInfo($"Error setting AlwaysRecoilReduction checkbox: {ex.Message}");
                    chkAlwaysRecoilReduction.Checked = false;
                }

                try
                {
                    chkMinimizeToTray.Checked = settings.UISettings.MinimizeToTray;
                }
                catch (Exception ex)
                {
                    uiManager.UpdateDebugInfo($"Error setting MinimizeToTray checkbox: {ex.Message}");
                    chkMinimizeToTray.Checked = false;
                }

                // Apply settings to macro manager with validation
                try
                {
                    macroManager.SetJitterStrength(jitterStrength);
                    macroManager.SetRecoilReductionStrength(recoilStrength);
                    macroManager.SetAlwaysJitterMode(settings.MacroSettings.AlwaysJitterMode);
                    macroManager.SetAlwaysRecoilReductionMode(settings.MacroSettings.AlwaysRecoilReductionMode);
                }
                catch (Exception ex)
                {
                    uiManager.UpdateDebugInfo($"Error applying settings to macro manager: {ex.Message}");
                }

                // Apply hotkey settings with validation
                try
                {
                    if (settings.HotkeySettings?.MacroKey != null && settings.HotkeySettings?.SwitchKey != null)
                    {
                        // Convert InputType to ToggleType for the HotkeyManager methods
                        var macroKeyToggleType = ConvertInputTypeToToggleType(settings.HotkeySettings.MacroKey.Type);
                        var switchKeyToggleType = ConvertInputTypeToToggleType(settings.HotkeySettings.SwitchKey.Type);
                        
                        hotkeyManager.SetMacroKey(settings.HotkeySettings.MacroKey.Key, macroKeyToggleType);
                        hotkeyManager.SetSwitchKey(settings.HotkeySettings.SwitchKey.Key, switchKeyToggleType);
                    }
                }
                catch (Exception ex)
                {
                    uiManager.UpdateDebugInfo($"Error applying hotkey settings: {ex.Message}");
                    hotkeyManager.ResetToDefaults();
                }

                // Update UI elements
                try
                {
                    uiManager.UpdateTitle();
                    uiManager.UpdateModeLabels();
                }
                catch (Exception ex)
                {
                    uiManager.UpdateDebugInfo($"Error updating UI elements: {ex.Message}");
                }
                
                // Verify that applied settings match the values in the file
                uiManager.UpdateDebugInfo($"Applied settings - JitterStrength: {trackBarJitter.Value}, " +
                                          $"RecoilReduction: {trackBarRecoilReduction.Value}, " +
                                          $"AlwaysJitter: {chkAlwaysJitter.Checked}, " +
                                          $"AlwaysRecoil: {chkAlwaysRecoilReduction.Checked}, " +
                                          $"MinimizeToTray: {chkMinimizeToTray.Checked}");
                
                uiManager.UpdateDebugInfo("Settings loaded successfully");
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Error loading settings: {ex.Message}");
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
                uiManager.UpdateDebugInfo("Saving current settings...");
                
                var settings = ConfigurationManager.Instance.CurrentSettings;
                if (settings == null)
                {
                    uiManager.UpdateDebugInfo("Error: Current settings object is null");
                    return;
                }
                
                // Save current UI values to settings
                try
                {
                    settings.MacroSettings.JitterStrength = trackBarJitter.Value;
                    settings.MacroSettings.RecoilReductionStrength = trackBarRecoilReduction.Value;
                    settings.MacroSettings.AlwaysJitterMode = chkAlwaysJitter.Checked;
                    settings.MacroSettings.AlwaysRecoilReductionMode = chkAlwaysRecoilReduction.Checked;
                    settings.UISettings.MinimizeToTray = chkMinimizeToTray.Checked;
                }
                catch (Exception ex)
                {
                    uiManager.UpdateDebugInfo($"Error preparing settings for save: {ex.Message}");
                    return;
                }
                
                // Validate and correct any issues before saving
                ValidateAndCorrectSettings(settings);
                
                // Log the values we're about to save
                uiManager.UpdateDebugInfo($"Saving settings - JitterStrength: {settings.MacroSettings.JitterStrength}, " +
                                          $"RecoilReduction: {settings.MacroSettings.RecoilReductionStrength}, " +
                                          $"AlwaysJitter: {settings.MacroSettings.AlwaysJitterMode}, " +
                                          $"AlwaysRecoil: {settings.MacroSettings.AlwaysRecoilReductionMode}, " +
                                          $"MinimizeToTray: {settings.UISettings.MinimizeToTray}");
                
                // Save the settings
                try
                {
                    ConfigurationManager.Instance.SaveSettings();
                    uiManager.UpdateDebugInfo("Settings saved successfully");
                }
                catch (Exception ex)
                {
                    uiManager.UpdateDebugInfo($"Error saving settings: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Critical error in SaveCurrentSettings: {ex.Message}");
            }
        }

        /// <summary>
        /// Resets all settings to their default values.
        /// </summary>
        private void ResetToDefaultSettings()
        {
            try
            {
                uiManager.UpdateDebugInfo("Resetting to default settings...");
                
                // Reset MacroManager first (safely)
                try
                {
                    macroManager.SetJitterStrength(3);
                    macroManager.SetRecoilReductionStrength(1);
                    macroManager.SetAlwaysJitterMode(false);
                    macroManager.SetAlwaysRecoilReductionMode(false);
                    uiManager.UpdateDebugInfo("Reset MacroManager settings");
                }
                catch (Exception ex)
                {
                    uiManager.UpdateDebugInfo($"Error resetting MacroManager: {ex.Message}");
                }

                // Reset HotkeyManager safely
                try
                {
                    hotkeyManager.ResetToDefaults();
                    uiManager.UpdateDebugInfo("Reset hotkey settings");
                }
                catch (Exception ex)
                {
                    uiManager.UpdateDebugInfo($"Error resetting HotkeyManager: {ex.Message}");
                }

                // Reset UI controls safely with proper bounds checking
                try
                {
                    // Default values
                    int defaultJitterStrength = 3;
                    int defaultRecoilStrength = 1;
                    
                    // Ensure values are within trackbar bounds
                    if (defaultJitterStrength < trackBarJitter.Minimum) defaultJitterStrength = trackBarJitter.Minimum;
                    if (defaultJitterStrength > trackBarJitter.Maximum) defaultJitterStrength = trackBarJitter.Maximum;
                    
                    if (defaultRecoilStrength < trackBarRecoilReduction.Minimum) defaultRecoilStrength = trackBarRecoilReduction.Minimum;
                    if (defaultRecoilStrength > trackBarRecoilReduction.Maximum) defaultRecoilStrength = trackBarRecoilReduction.Maximum;
                    
                    // Apply default values to trackbars
                    trackBarJitter.Value = defaultJitterStrength;
                    lblJitterStrengthValue.Text = defaultJitterStrength.ToString();
                    
                    trackBarRecoilReduction.Value = defaultRecoilStrength;
                    lblRecoilReductionStrengthValue.Text = defaultRecoilStrength.ToString();
                    
                    // Reset checkboxes
                    chkAlwaysJitter.Checked = false;
                    chkAlwaysRecoilReduction.Checked = false;
                    chkMinimizeToTray.Checked = false;
                    
                    uiManager.UpdateDebugInfo("Reset UI controls");
                }
                catch (Exception ex)
                {
                    uiManager.UpdateDebugInfo($"Error resetting UI controls: {ex.Message}");
                }

                // Create a fresh default settings object
                try
                {
                    var defaultSettings = new AppSettings();
                    
                    // Set default values
                    defaultSettings.MacroSettings.JitterStrength = 3;
                    defaultSettings.MacroSettings.RecoilReductionStrength = 1;
                    defaultSettings.MacroSettings.AlwaysJitterMode = false;
                    defaultSettings.MacroSettings.AlwaysRecoilReductionMode = false;
                    defaultSettings.MacroSettings.JitterEnabled = false;
                    defaultSettings.MacroSettings.RecoilReductionEnabled = false;
                    defaultSettings.UISettings.MinimizeToTray = false;
                    defaultSettings.UISettings.ShowStatusInTitle = true;
                    defaultSettings.UISettings.ShowTrayNotifications = true;
                    defaultSettings.UISettings.WindowPosition = new System.Drawing.Point(100, 100);
                    defaultSettings.UISettings.WindowSize = new System.Drawing.Size(800, 600);
                    defaultSettings.HotkeySettings.MacroKey = new InputBinding(Keys.Capital, InputType.Keyboard);
                    defaultSettings.HotkeySettings.SwitchKey = new InputBinding(Keys.Q, InputType.Keyboard);
                    
                    // Validate the new settings
                    ValidateAndCorrectSettings(defaultSettings);
                    
                    // Set as current settings and save
                    var configManager = ConfigurationManager.Instance;
                    
                    // Use reflection to set the private field directly if necessary
                    var fieldInfo = configManager.GetType().GetField("_currentSettings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (fieldInfo != null)
                    {
                        fieldInfo.SetValue(configManager, defaultSettings);
                        uiManager.UpdateDebugInfo("Set new default settings using reflection");
                    }
                    
                    // Save the settings
                    configManager.SaveSettings();
                    uiManager.UpdateDebugInfo("Saved default settings");
                }
                catch (Exception ex)
                {
                    uiManager.UpdateDebugInfo($"Error creating or saving default settings: {ex.Message}");
                    
                    // Fallback: Just save current UI values
                    try
                    {
                        SaveCurrentSettings();
                    }
                    catch
                    {
                        // Last resort: do nothing more
                    }
                }

                // Update UI
                try
                {
                    uiManager.UpdateTitle();
                    uiManager.UpdateModeLabels();
                    uiManager.UpdateDebugInfo("Settings reset to defaults");
                }
                catch (Exception ex)
                {
                    uiManager.UpdateDebugInfo($"Error updating UI after reset: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Critical error in ResetToDefaultSettings: {ex.Message}");
            }
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
            uiManager.UpdateDebugInfo("Application restored from system tray");
        }

        /// <summary>
        /// Performs cleanup operations before exiting the application.
        /// </summary>
        private void CleanupAndExit()
        {
            if (isExiting) return; // Prevent recursive calls
            
            try
            {
                isExiting = true;
                SaveCurrentSettings();
                PerformCleanup();
                
                // Close the form
                this.Close();
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Error during cleanup: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Performs resource cleanup, separating it from the exit logic
        /// </summary>
        private void PerformCleanup()
        {
            try
            {
                // Stop and dispose of hooks
                if (keyboardHook != null)
                {
                    keyboardHook.Stop();
                    keyboardHook.Dispose();
                }
                
                if (mouseHook != null)
                {
                    mouseHook.Stop();
                    mouseHook.Dispose();
                }

                // Stop and dispose of macro manager
                macroManager?.Dispose();

                // Clean up UI resources
                if (notifyIcon != null)
                {
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();
                }
                
                toolTip?.Dispose();
                uiManager?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Override of OnFormClosing to ensure settings are saved and proper cleanup is performed.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Let the handler do its work
            base.OnFormClosing(e);
            
            // If we're really closing (not cancelled), ensure cleanup happens
            if (!e.Cancel && !isExiting)
            {
                isExiting = true;
                SaveCurrentSettings();
                PerformCleanup();
            }
        }

        /// <summary>
        /// Converts InputType to ToggleType for use with HotkeyManager
        /// </summary>
        private NotesAndTasks.Utilities.ToggleType ConvertInputTypeToToggleType(NotesAndTasks.Models.InputType inputType)
        {
            if (inputType == NotesAndTasks.Models.InputType.Keyboard)
                return NotesAndTasks.Utilities.ToggleType.Keyboard;
            else
                return NotesAndTasks.Utilities.ToggleType.MouseMiddle; // Default to middle mouse button for mouse input
        }
    }
}
