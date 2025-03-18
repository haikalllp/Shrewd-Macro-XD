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
                notifyIcon.DoubleClick += (s, e) => uiManager.ShowWindow();
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
            if (!isExiting && chkMinimizeToTray.Checked && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                notifyIcon.Visible = true;
                uiManager.UpdateDebugInfo("Application minimized to system tray");
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
                uiManager.UpdateTitle();
                
                // Complete the migration by removing the legacy settings file
                FinalizeMigration();
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

            chkAlwaysJitter.CheckedChanged += (sender, e) =>
            {
                macroManager.SetAlwaysJitterMode(chkAlwaysJitter.Checked);
                btnSetMacroSwitch.Enabled = !chkAlwaysJitter.Checked;
                ConfigurationManager.Instance.CurrentSettings.MacroSettings.AlwaysJitterMode = chkAlwaysJitter.Checked;
                ConfigurationManager.Instance.SaveConfiguration();
                uiManager.UpdateTitle();
                uiManager.UpdateModeLabels();
            };

            chkAlwaysRecoilReduction.CheckedChanged += (sender, e) =>
            {
                macroManager.SetAlwaysRecoilReductionMode(chkAlwaysRecoilReduction.Checked);
                btnSetMacroSwitch.Enabled = !chkAlwaysRecoilReduction.Checked;
                ConfigurationManager.Instance.CurrentSettings.MacroSettings.AlwaysRecoilReductionMode = chkAlwaysRecoilReduction.Checked;
                ConfigurationManager.Instance.SaveConfiguration();
                uiManager.UpdateTitle();
                uiManager.UpdateModeLabels();
            };

            chkMinimizeToTray.CheckedChanged += (sender, e) =>
            {
                ConfigurationManager.Instance.CurrentSettings.UISettings.MinimizeToTray = chkMinimizeToTray.Checked;
                ConfigurationManager.Instance.SaveConfiguration();
            };

            trackBarJitter.ValueChanged += (sender, e) =>
            {
                macroManager.SetJitterStrength(trackBarJitter.Value);
                lblJitterStrengthValue.Text = trackBarJitter.Value.ToString();
                ConfigurationManager.Instance.CurrentSettings.MacroSettings.JitterStrength = trackBarJitter.Value;
                ConfigurationManager.Instance.SaveConfiguration();
            };

            trackBarRecoilReduction.ValueChanged += (sender, e) =>
            {
                macroManager.SetRecoilReductionStrength(trackBarRecoilReduction.Value);
                lblRecoilReductionStrengthValue.Text = trackBarRecoilReduction.Value.ToString();
                ConfigurationManager.Instance.CurrentSettings.MacroSettings.RecoilReductionStrength = trackBarRecoilReduction.Value;
                ConfigurationManager.Instance.SaveConfiguration();
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
                var settings = ConfigurationManager.Instance.CurrentSettings;
                Validation.ValidateNotNull(settings, nameof(settings));

                // Validate all settings before applying them
                if (!ValidateSettings(settings))
                {
                    uiManager.UpdateDebugInfo("Invalid settings detected, resetting to defaults");
                    ResetToDefaultSettings();
                    return;
                }

                // Apply validated settings to UI
                trackBarJitter.Value = settings.MacroSettings.JitterStrength;
                trackBarRecoilReduction.Value = settings.MacroSettings.RecoilReductionStrength;
                chkAlwaysJitter.Checked = settings.MacroSettings.AlwaysJitterMode;
                chkAlwaysRecoilReduction.Checked = settings.MacroSettings.AlwaysRecoilReductionMode;
                chkMinimizeToTray.Checked = settings.UISettings.MinimizeToTray;

                // Update UI elements
                uiManager.UpdateTitle();
                uiManager.UpdateModeLabels();
                uiManager.UpdateDebugInfo("Settings loaded successfully");
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Error loading settings: {ex.Message}");
                ResetToDefaultSettings();
            }
        }

        /// <summary>
        /// Validates settings to ensure they're within expected ranges
        /// </summary>
        private bool ValidateSettings(AppSettings settings)
        {
            if (settings == null) return false;
            
            return settings.MacroSettings.JitterStrength >= trackBarJitter.Minimum && 
                   settings.MacroSettings.JitterStrength <= trackBarJitter.Maximum &&
                   settings.MacroSettings.RecoilReductionStrength >= trackBarRecoilReduction.Minimum &&
                   settings.MacroSettings.RecoilReductionStrength <= trackBarRecoilReduction.Maximum;
        }

        /// <summary>
        /// Saves the current settings to the settings manager.
        /// </summary>
        private void SaveCurrentSettings()
        {
            try
            {
                // Save to new configuration system
                var settings = ConfigurationManager.Instance.CurrentSettings;
                settings.MacroSettings.JitterStrength = trackBarJitter.Value;
                settings.MacroSettings.RecoilReductionStrength = trackBarRecoilReduction.Value;
                settings.MacroSettings.AlwaysJitterMode = chkAlwaysJitter.Checked;
                settings.MacroSettings.AlwaysRecoilReductionMode = chkAlwaysRecoilReduction.Checked;
                settings.UISettings.MinimizeToTray = chkMinimizeToTray.Checked;
                ConfigurationManager.Instance.SaveConfiguration();
                
                uiManager.UpdateDebugInfo("Settings saved successfully");
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Error saving settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Finalizes the migration by removing legacy settings files.
        /// </summary>
        private void FinalizeMigration()
        {
            try
            {
                if (ConfigurationManager.Instance.FinalizeMigration())
                {
                    uiManager.UpdateDebugInfo("Settings migration completed successfully");
                }
                else
                {
                    uiManager.UpdateDebugInfo("Unable to remove legacy settings file");
                }
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Error during migration finalization: {ex.Message}");
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
                hotkeyManager.ResetToDefaults();

                // Update UI
                uiManager.UpdateCurrentKey(hotkeyManager.MacroKey.ToString());
                uiManager.UpdateSwitchKey(hotkeyManager.SwitchKey.ToString());
                uiManager.UpdateTitle();
                uiManager.UpdateModeLabels();

                // Save settings
                SaveCurrentSettings();
                uiManager.UpdateDebugInfo("Settings reset to defaults");
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Error resetting settings: {ex.Message}");
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
                uiManager?.Dispose();

                // Close the form
                this.Close();
            }
            catch (Exception ex)
            {
                uiManager.UpdateDebugInfo($"Error during cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Override of OnFormClosing to ensure settings are saved.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveCurrentSettings();
            base.OnFormClosing(e);
        }
    }
}
