using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NotesAndTasks.Models;

namespace NotesAndTasks.Configuration
{
    /// <summary>
    /// Manages event handler registration and lifecycle for the application.
    /// Provides centralized control over event subscriptions and cleanup.
    /// </summary>
    public class EventHandlerManager : IDisposable
    {
        private readonly Dictionary<string, List<Delegate>> eventHandlers;
        private readonly ConfigurationManager configManager;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the EventHandlerManager class.
        /// </summary>
        /// <param name="configManager">The configuration manager instance.</param>
        public EventHandlerManager(ConfigurationManager configManager)
        {
            this.configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            this.eventHandlers = new Dictionary<string, List<Delegate>>();
            RegisterConfigurationEvents();
        }

        /// <summary>
        /// Registers configuration-related event handlers.
        /// </summary>
        private void RegisterConfigurationEvents()
        {
            // Configuration change handlers
            RegisterEventHandler("ConfigChanged", 
                new ConfigurationChangedEventHandler(OnConfigurationChanged));
            configManager.ConfigurationChanged += OnConfigurationChanged;

            // Configuration validation handlers
            RegisterEventHandler("ConfigValidating",
                new ConfigurationValidationEventHandler(OnConfigurationValidating));
            configManager.ConfigurationValidating += OnConfigurationValidating;

            // Configuration backup handlers
            RegisterEventHandler("ConfigBackup",
                new ConfigurationBackupEventHandler(OnConfigurationBackupCompleted));
            configManager.ConfigurationBackupCompleted += OnConfigurationBackupCompleted;
        }

        /// <summary>
        /// Registers a control's event handlers.
        /// </summary>
        /// <param name="control">The control to register events for.</param>
        public void RegisterControlEvents<T>(T control) where T : Control
        {
            if (control == null) return;

            // Store event handlers
            var handlers = new List<Delegate>();
            eventHandlers[control.Name] = handlers;

            // Common events
            EventHandler clickHandler = (s, e) => OnControlClick(control, e);
            control.Click += clickHandler;
            handlers.Add(clickHandler);

            // Mouse events
            MouseEventHandler mouseDownHandler = (s, e) => OnControlMouseDown(control, e);
            control.MouseDown += mouseDownHandler;
            handlers.Add(mouseDownHandler);

            MouseEventHandler mouseUpHandler = (s, e) => OnControlMouseUp(control, e);
            control.MouseUp += mouseUpHandler;
            handlers.Add(mouseUpHandler);

            // Type-specific events
            if (control is TrackBar trackBar)
            {
                EventHandler valueChangedHandler = (s, e) => OnTrackBarValueChanged(trackBar, e);
                trackBar.ValueChanged += valueChangedHandler;
                handlers.Add(valueChangedHandler);

                EventHandler scrollHandler = (s, e) => OnTrackBarScroll(trackBar, e);
                trackBar.Scroll += scrollHandler;
                handlers.Add(scrollHandler);
            }
            else if (control is CheckBox checkBox)
            {
                EventHandler checkedChangedHandler = (s, e) => OnCheckBoxCheckedChanged(checkBox, e);
                checkBox.CheckedChanged += checkedChangedHandler;
                handlers.Add(checkedChangedHandler);
            }
            else if (control is TextBox textBox)
            {
                EventHandler textChangedHandler = (s, e) => OnTextBoxTextChanged(textBox, e);
                textBox.TextChanged += textChangedHandler;
                handlers.Add(textChangedHandler);

                KeyEventHandler keyDownHandler = (s, e) => OnTextBoxKeyDown(textBox, e);
                textBox.KeyDown += keyDownHandler;
                handlers.Add(keyDownHandler);
            }
        }

        /// <summary>
        /// Registers an event handler with tracking.
        /// </summary>
        private void RegisterEventHandler(string eventName, Delegate handler)
        {
            if (!eventHandlers.ContainsKey(eventName))
            {
                eventHandlers[eventName] = new List<Delegate>();
            }
            eventHandlers[eventName].Add(handler);
        }

        /// <summary>
        /// Unregisters all event handlers for a control.
        /// </summary>
        public void UnregisterControlEvents(Control control)
        {
            if (control == null) return;

            var handlersToRemove = new List<string>();
            foreach (var kvp in eventHandlers)
            {
                if (kvp.Key.StartsWith($"{control.Name}_"))
                {
                    handlersToRemove.Add(kvp.Key);
                }
            }

            foreach (var handlerName in handlersToRemove)
            {
                eventHandlers.Remove(handlerName);
            }
        }

        /// <summary>
        /// Configuration changed event handler.
        /// </summary>
        private void OnConfigurationChanged(object sender, ConfigurationChangedEventArgs e)
        {
            try
            {
                // Handle configuration changes based on section
                switch (e.Section)
                {
                    case "JitterSettings":
                        HandleJitterSettingsChanged(e.PreviousConfig, e.NewConfig);
                        break;
                    case "RecoilSettings":
                        HandleRecoilSettingsChanged(e.PreviousConfig, e.NewConfig);
                        break;
                    case "HotkeySettings":
                        HandleHotkeySettingsChanged(e.PreviousConfig, e.NewConfig);
                        break;
                    case "UISettings":
                        HandleUISettingsChanged(e.PreviousConfig, e.NewConfig);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling configuration change: {ex.Message}");
            }
        }

        /// <summary>
        /// Configuration validating event handler.
        /// </summary>
        private void OnConfigurationValidating(object sender, ConfigurationValidationEventArgs e)
        {
            try
            {
                // Perform additional validation if needed
                if (e.Configuration != null)
                {
                    ValidateConfigurationConsistency(e);
                }
            }
            catch (Exception ex)
            {
                e.IsValid = false;
                e.Message = $"Validation error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error during configuration validation: {ex.Message}");
            }
        }

        /// <summary>
        /// Configuration backup completed event handler.
        /// </summary>
        private void OnConfigurationBackupCompleted(object sender, ConfigurationBackupEventArgs e)
        {
            try
            {
                if (e.Success)
                {
                    System.Diagnostics.Debug.WriteLine($"Configuration backup created successfully at: {e.BackupPath}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Configuration backup failed: {e.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling backup completion: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates configuration consistency.
        /// </summary>
        private void ValidateConfigurationConsistency(ConfigurationValidationEventArgs e)
        {
            var config = e.Configuration;

            // Validate mode consistency
            if (config.MacroSettings.AlwaysJitterMode && config.MacroSettings.AlwaysRecoilReductionMode)
            {
                e.IsValid = false;
                e.Message = "Both jitter and recoil reduction cannot be in always-enabled mode";
                return;
            }

            // Validate hotkey conflicts
            if (config.HotkeySettings.MacroKey == config.HotkeySettings.SwitchKey)
            {
                e.IsValid = false;
                e.Message = "Macro toggle key and mode switch key cannot be the same";
                return;
            }

            // Validate UI settings
            if (config.UISettings.WindowSize.Width <= 0 || config.UISettings.WindowSize.Height <= 0)
            {
                e.IsValid = false;
                e.Message = "Window size must be positive";
                return;
            }
        }

        /// <summary>
        /// Handles changes to jitter settings.
        /// </summary>
        private void HandleJitterSettingsChanged(AppSettings previousConfig, AppSettings newConfig)
        {
            if (previousConfig?.MacroSettings.JitterStrength != newConfig.MacroSettings.JitterStrength)
            {
                System.Diagnostics.Debug.WriteLine($"Jitter strength changed from {previousConfig?.MacroSettings.JitterStrength} to {newConfig.MacroSettings.JitterStrength}");
            }

            if (previousConfig?.MacroSettings.JitterEnabled != newConfig.MacroSettings.JitterEnabled)
            {
                System.Diagnostics.Debug.WriteLine($"Jitter enabled changed from {previousConfig?.MacroSettings.JitterEnabled} to {newConfig.MacroSettings.JitterEnabled}");
            }

            if (previousConfig?.MacroSettings.AlwaysJitterMode != newConfig.MacroSettings.AlwaysJitterMode)
            {
                System.Diagnostics.Debug.WriteLine($"Always jitter mode changed from {previousConfig?.MacroSettings.AlwaysJitterMode} to {newConfig.MacroSettings.AlwaysJitterMode}");
            }
        }

        /// <summary>
        /// Handles changes to recoil settings.
        /// </summary>
        private void HandleRecoilSettingsChanged(AppSettings previousConfig, AppSettings newConfig)
        {
            if (previousConfig?.MacroSettings.RecoilReductionStrength != newConfig.MacroSettings.RecoilReductionStrength)
            {
                System.Diagnostics.Debug.WriteLine($"Recoil reduction strength changed from {previousConfig?.MacroSettings.RecoilReductionStrength} to {newConfig.MacroSettings.RecoilReductionStrength}");
            }

            if (previousConfig?.MacroSettings.RecoilReductionEnabled != newConfig.MacroSettings.RecoilReductionEnabled)
            {
                System.Diagnostics.Debug.WriteLine($"Recoil reduction enabled changed from {previousConfig?.MacroSettings.RecoilReductionEnabled} to {newConfig.MacroSettings.RecoilReductionEnabled}");
            }

            if (previousConfig?.MacroSettings.AlwaysRecoilReductionMode != newConfig.MacroSettings.AlwaysRecoilReductionMode)
            {
                System.Diagnostics.Debug.WriteLine($"Always recoil reduction mode changed from {previousConfig?.MacroSettings.AlwaysRecoilReductionMode} to {newConfig.MacroSettings.AlwaysRecoilReductionMode}");
            }
        }

        /// <summary>
        /// Handles changes to hotkey settings.
        /// </summary>
        private void HandleHotkeySettingsChanged(AppSettings previousConfig, AppSettings newConfig)
        {
            if (previousConfig?.HotkeySettings.MacroKey != newConfig.HotkeySettings.MacroKey)
            {
                System.Diagnostics.Debug.WriteLine($"Macro toggle key changed from {previousConfig?.HotkeySettings.MacroKey} to {newConfig.HotkeySettings.MacroKey}");
            }

            if (previousConfig?.HotkeySettings.SwitchKey != newConfig.HotkeySettings.SwitchKey)
            {
                System.Diagnostics.Debug.WriteLine($"Mode switch key changed from {previousConfig?.HotkeySettings.SwitchKey} to {newConfig.HotkeySettings.SwitchKey}");
            }
        }

        /// <summary>
        /// Handles changes to UI settings.
        /// </summary>
        private void HandleUISettingsChanged(AppSettings previousConfig, AppSettings newConfig)
        {
            if (previousConfig?.UISettings.MinimizeToTray != newConfig.UISettings.MinimizeToTray)
            {
                System.Diagnostics.Debug.WriteLine($"Minimize to tray changed from {previousConfig?.UISettings.MinimizeToTray} to {newConfig.UISettings.MinimizeToTray}");
            }

            if (previousConfig?.UISettings.ShowDebugPanel != newConfig.UISettings.ShowDebugPanel)
            {
                System.Diagnostics.Debug.WriteLine($"Show debug panel changed from {previousConfig?.UISettings.ShowDebugPanel} to {newConfig.UISettings.ShowDebugPanel}");
            }

            if (previousConfig?.UISettings.ShowStatusInTitle != newConfig.UISettings.ShowStatusInTitle)
            {
                System.Diagnostics.Debug.WriteLine($"Show status in title changed from {previousConfig?.UISettings.ShowStatusInTitle} to {newConfig.UISettings.ShowStatusInTitle}");
            }

            if (previousConfig?.UISettings.ShowTrayNotifications != newConfig.UISettings.ShowTrayNotifications)
            {
                System.Diagnostics.Debug.WriteLine($"Show tray notifications changed from {previousConfig?.UISettings.ShowTrayNotifications} to {newConfig.UISettings.ShowTrayNotifications}");
            }
        }

        // Event handler methods
        private void OnControlClick(Control control, EventArgs e)
        {
            // Handle click event
        }

        private void OnControlMouseDown(Control control, MouseEventArgs e)
        {
            // Handle mouse down
        }

        private void OnControlMouseUp(Control control, MouseEventArgs e)
        {
            // Handle mouse up
        }

        private void OnTrackBarValueChanged(TrackBar trackBar, EventArgs e)
        {
            // Handle value changed
        }

        private void OnTrackBarScroll(TrackBar trackBar, EventArgs e)
        {
            // Handle scroll
        }

        private void OnCheckBoxCheckedChanged(CheckBox checkBox, EventArgs e)
        {
            // Handle checked changed
        }

        private void OnTextBoxTextChanged(TextBox textBox, EventArgs e)
        {
            // Handle text changed
        }

        private void OnTextBoxKeyDown(TextBox textBox, KeyEventArgs e)
        {
            // Handle key down
        }

        /// <summary>
        /// Disposes of the event handler manager and unregisters all events.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    // Unregister all event handlers
                    configManager.ConfigurationChanged -= OnConfigurationChanged;
                    configManager.ConfigurationValidating -= OnConfigurationValidating;
                    configManager.ConfigurationBackupCompleted -= OnConfigurationBackupCompleted;

                    eventHandlers.Clear();
                }

                isDisposed = true;
            }
        }
    }
} 