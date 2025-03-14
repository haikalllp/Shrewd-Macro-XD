using System;
using System.Collections.Generic;
using System.Windows.Forms;

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
        public void RegisterControlEvents(Control control)
        {
            if (control == null) throw new ArgumentNullException(nameof(control));

            // Register common control events
            RegisterEventHandler($"{control.Name}_Click",
                new EventHandler(control.Click));

            // Register specific control type events
            switch (control)
            {
                case TrackBar trackBar:
                    RegisterTrackBarEvents(trackBar);
                    break;
                case CheckBox checkBox:
                    RegisterCheckBoxEvents(checkBox);
                    break;
                case Button button:
                    RegisterButtonEvents(button);
                    break;
                case TextBox textBox:
                    RegisterTextBoxEvents(textBox);
                    break;
            }
        }

        /// <summary>
        /// Registers events specific to TrackBar controls.
        /// </summary>
        private void RegisterTrackBarEvents(TrackBar trackBar)
        {
            RegisterEventHandler($"{trackBar.Name}_ValueChanged",
                new EventHandler(trackBar.ValueChanged));
            RegisterEventHandler($"{trackBar.Name}_Scroll",
                new EventHandler(trackBar.Scroll));
        }

        /// <summary>
        /// Registers events specific to CheckBox controls.
        /// </summary>
        private void RegisterCheckBoxEvents(CheckBox checkBox)
        {
            RegisterEventHandler($"{checkBox.Name}_CheckedChanged",
                new EventHandler(checkBox.CheckedChanged));
        }

        /// <summary>
        /// Registers events specific to Button controls.
        /// </summary>
        private void RegisterButtonEvents(Button button)
        {
            RegisterEventHandler($"{button.Name}_MouseDown",
                new MouseEventHandler(button.MouseDown));
            RegisterEventHandler($"{button.Name}_MouseUp",
                new MouseEventHandler(button.MouseUp));
        }

        /// <summary>
        /// Registers events specific to TextBox controls.
        /// </summary>
        private void RegisterTextBoxEvents(TextBox textBox)
        {
            RegisterEventHandler($"{textBox.Name}_TextChanged",
                new EventHandler(textBox.TextChanged));
            RegisterEventHandler($"{textBox.Name}_KeyDown",
                new KeyEventHandler(textBox.KeyDown));
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
            if (config.JitterSettings.AlwaysEnabled && config.RecoilSettings.AlwaysEnabled)
            {
                e.IsValid = false;
                e.Message = "Both jitter and recoil reduction cannot be in always-enabled mode";
                return;
            }

            // Validate hotkey conflicts
            if (config.HotkeySettings.MacroToggleKey == config.HotkeySettings.ModeSwitchKey)
            {
                e.IsValid = false;
                e.Message = "Macro toggle key and mode switch key cannot be the same";
                return;
            }

            // Validate backup settings
            if (config.BackupSettings.AutoBackupEnabled)
            {
                if (config.BackupSettings.BackupIntervalHours < 1)
                {
                    e.IsValid = false;
                    e.Message = "Backup interval must be at least 1 hour";
                    return;
                }
                if (config.BackupSettings.MaxBackupCount < 1)
                {
                    e.IsValid = false;
                    e.Message = "Maximum backup count must be at least 1";
                    return;
                }
            }
        }

        /// <summary>
        /// Handles changes to jitter settings.
        /// </summary>
        private void HandleJitterSettingsChanged(AppConfiguration previousConfig, AppConfiguration newConfig)
        {
            if (previousConfig?.JitterSettings.Strength != newConfig.JitterSettings.Strength)
            {
                System.Diagnostics.Debug.WriteLine($"Jitter strength changed from {previousConfig?.JitterSettings.Strength} to {newConfig.JitterSettings.Strength}");
            }
            if (previousConfig?.JitterSettings.IsEnabled != newConfig.JitterSettings.IsEnabled)
            {
                System.Diagnostics.Debug.WriteLine($"Jitter enabled state changed from {previousConfig?.JitterSettings.IsEnabled} to {newConfig.JitterSettings.IsEnabled}");
            }
        }

        /// <summary>
        /// Handles changes to recoil reduction settings.
        /// </summary>
        private void HandleRecoilSettingsChanged(AppConfiguration previousConfig, AppConfiguration newConfig)
        {
            if (previousConfig?.RecoilSettings.Strength != newConfig.RecoilSettings.Strength)
            {
                System.Diagnostics.Debug.WriteLine($"Recoil reduction strength changed from {previousConfig?.RecoilSettings.Strength} to {newConfig.RecoilSettings.Strength}");
            }
            if (previousConfig?.RecoilSettings.IsEnabled != newConfig.RecoilSettings.IsEnabled)
            {
                System.Diagnostics.Debug.WriteLine($"Recoil reduction enabled state changed from {previousConfig?.RecoilSettings.IsEnabled} to {newConfig.RecoilSettings.IsEnabled}");
            }
        }

        /// <summary>
        /// Handles changes to hotkey settings.
        /// </summary>
        private void HandleHotkeySettingsChanged(AppConfiguration previousConfig, AppConfiguration newConfig)
        {
            if (previousConfig?.HotkeySettings.MacroToggleKey != newConfig.HotkeySettings.MacroToggleKey)
            {
                System.Diagnostics.Debug.WriteLine($"Macro toggle key changed from {previousConfig?.HotkeySettings.MacroToggleKey} to {newConfig.HotkeySettings.MacroToggleKey}");
            }
            if (previousConfig?.HotkeySettings.ModeSwitchKey != newConfig.HotkeySettings.ModeSwitchKey)
            {
                System.Diagnostics.Debug.WriteLine($"Mode switch key changed from {previousConfig?.HotkeySettings.ModeSwitchKey} to {newConfig.HotkeySettings.ModeSwitchKey}");
            }
        }

        /// <summary>
        /// Handles changes to UI settings.
        /// </summary>
        private void HandleUISettingsChanged(AppConfiguration previousConfig, AppConfiguration newConfig)
        {
            if (previousConfig?.UISettings.MinimizeToTray != newConfig.UISettings.MinimizeToTray)
            {
                System.Diagnostics.Debug.WriteLine($"Minimize to tray setting changed from {previousConfig?.UISettings.MinimizeToTray} to {newConfig.UISettings.MinimizeToTray}");
            }
            if (previousConfig?.UISettings.ShowDebugPanel != newConfig.UISettings.ShowDebugPanel)
            {
                System.Diagnostics.Debug.WriteLine($"Debug panel visibility changed from {previousConfig?.UISettings.ShowDebugPanel} to {newConfig.UISettings.ShowDebugPanel}");
            }
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