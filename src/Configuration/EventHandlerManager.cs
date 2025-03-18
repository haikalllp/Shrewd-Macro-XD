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
            // Settings change handlers
            RegisterEventHandler("SettingsChanged", 
                new EventHandler<SettingsChangedEventArgs>(OnSettingsChanged));
            configManager.SettingsChanged += OnSettingsChanged;

            // Settings validation handlers
            RegisterEventHandler("SettingsValidating",
                new EventHandler<SettingsValidationEventArgs>(OnSettingsValidating));
            configManager.SettingsValidating += OnSettingsValidating;

            // Settings backup handlers
            RegisterEventHandler("SettingsBackup",
                new EventHandler<SettingsBackupEventArgs>(OnSettingsBackupCompleted));
            configManager.SettingsBackupCompleted += OnSettingsBackupCompleted;
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
        /// Handles settings changes
        /// </summary>
        private void OnSettingsChanged(object sender, SettingsChangedEventArgs e)
        {
            if (e.Section == "All")
            {
                // Process all sections
                HandleMacroSettingsChanges(e.PreviousSettings, e.NewSettings);
                HandleHotkeySettingsChanges(e.PreviousSettings, e.NewSettings);
                HandleUISettingsChanges(e.PreviousSettings, e.NewSettings);
            }
            else if (e.Section == "MacroSettings")
            {
                HandleMacroSettingsChanges(e.PreviousSettings, e.NewSettings);
            }
            else if (e.Section == "HotkeySettings")
            {
                HandleHotkeySettingsChanges(e.PreviousSettings, e.NewSettings);
            }
            else if (e.Section == "UISettings")
            {
                HandleUISettingsChanges(e.PreviousSettings, e.NewSettings);
            }
        }

        /// <summary>
        /// Handles settings validation
        /// </summary>
        private void OnSettingsValidating(object sender, SettingsValidationEventArgs e)
        {
            var settings = e.Settings;
            if (settings == null)
            {
                e.IsValid = false;
                e.Message = "Settings cannot be null";
                return;
            }

            // Validate macro settings
            if (settings.MacroSettings.AlwaysJitterMode && settings.MacroSettings.AlwaysRecoilReductionMode)
            {
                e.IsValid = false;
                e.Message = "Cannot enable both AlwaysJitterMode and AlwaysRecoilReductionMode simultaneously";
                return;
            }

            // Validate hotkey settings
            if (settings.HotkeySettings.MacroKey.Key == settings.HotkeySettings.SwitchKey.Key &&
                settings.HotkeySettings.MacroKey.Type == settings.HotkeySettings.SwitchKey.Type)
            {
                e.IsValid = false;
                e.Message = "Macro key and switch key cannot be the same";
                return;
            }

            // Validate UI settings
            if (settings.UISettings.WindowSize.Width <= 0 || settings.UISettings.WindowSize.Height <= 0)
            {
                e.IsValid = false;
                e.Message = "Window size dimensions must be positive";
                return;
            }
        }

        /// <summary>
        /// Handles settings backup completion
        /// </summary>
        private void OnSettingsBackupCompleted(object sender, SettingsBackupEventArgs e)
        {
            if (e.Success)
            {
                System.Diagnostics.Debug.WriteLine($"Backup completed successfully: {e.BackupPath}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Backup failed: {e.ErrorMessage}");
            }
        }

        /// <summary>
        /// Handles changes to macro settings.
        /// </summary>
        private void HandleMacroSettingsChanges(AppSettings previousSettings, AppSettings newSettings)
        {
            if (previousSettings?.MacroSettings.JitterStrength != newSettings.MacroSettings.JitterStrength)
            {
                System.Diagnostics.Debug.WriteLine($"Jitter strength changed from {previousSettings?.MacroSettings.JitterStrength} to {newSettings.MacroSettings.JitterStrength}");
            }

            if (previousSettings?.MacroSettings.JitterEnabled != newSettings.MacroSettings.JitterEnabled)
            {
                System.Diagnostics.Debug.WriteLine($"Jitter enabled changed from {previousSettings?.MacroSettings.JitterEnabled} to {newSettings.MacroSettings.JitterEnabled}");
            }

            if (previousSettings?.MacroSettings.AlwaysJitterMode != newSettings.MacroSettings.AlwaysJitterMode)
            {
                System.Diagnostics.Debug.WriteLine($"Always jitter mode changed from {previousSettings?.MacroSettings.AlwaysJitterMode} to {newSettings.MacroSettings.AlwaysJitterMode}");
            }

            if (previousSettings?.MacroSettings.RecoilReductionStrength != newSettings.MacroSettings.RecoilReductionStrength)
            {
                System.Diagnostics.Debug.WriteLine($"Recoil reduction strength changed from {previousSettings?.MacroSettings.RecoilReductionStrength} to {newSettings.MacroSettings.RecoilReductionStrength}");
            }

            if (previousSettings?.MacroSettings.RecoilReductionEnabled != newSettings.MacroSettings.RecoilReductionEnabled)
            {
                System.Diagnostics.Debug.WriteLine($"Recoil reduction enabled changed from {previousSettings?.MacroSettings.RecoilReductionEnabled} to {newSettings.MacroSettings.RecoilReductionEnabled}");
            }

            if (previousSettings?.MacroSettings.AlwaysRecoilReductionMode != newSettings.MacroSettings.AlwaysRecoilReductionMode)
            {
                System.Diagnostics.Debug.WriteLine($"Always recoil reduction mode changed from {previousSettings?.MacroSettings.AlwaysRecoilReductionMode} to {newSettings.MacroSettings.AlwaysRecoilReductionMode}");
            }
        }

        /// <summary>
        /// Handles changes to hotkey settings.
        /// </summary>
        private void HandleHotkeySettingsChanges(AppSettings previousSettings, AppSettings newSettings)
        {
            if (previousSettings?.HotkeySettings.MacroKey.Key != newSettings.HotkeySettings.MacroKey.Key || 
                previousSettings?.HotkeySettings.MacroKey.Type != newSettings.HotkeySettings.MacroKey.Type)
            {
                System.Diagnostics.Debug.WriteLine($"Macro toggle key changed from {previousSettings?.HotkeySettings.MacroKey.DisplayName} to {newSettings.HotkeySettings.MacroKey.DisplayName}");
            }

            if (previousSettings?.HotkeySettings.SwitchKey.Key != newSettings.HotkeySettings.SwitchKey.Key ||
                previousSettings?.HotkeySettings.SwitchKey.Type != newSettings.HotkeySettings.SwitchKey.Type)
            {
                System.Diagnostics.Debug.WriteLine($"Mode switch key changed from {previousSettings?.HotkeySettings.SwitchKey.DisplayName} to {newSettings.HotkeySettings.SwitchKey.DisplayName}");
            }
        }

        /// <summary>
        /// Handles changes to UI settings.
        /// </summary>
        private void HandleUISettingsChanges(AppSettings previousSettings, AppSettings newSettings)
        {
            if (previousSettings?.UISettings.MinimizeToTray != newSettings.UISettings.MinimizeToTray)
            {
                System.Diagnostics.Debug.WriteLine($"Minimize to tray changed from {previousSettings?.UISettings.MinimizeToTray} to {newSettings.UISettings.MinimizeToTray}");
            }

            if (previousSettings?.UISettings.ShowDebugPanel != newSettings.UISettings.ShowDebugPanel)
            {
                System.Diagnostics.Debug.WriteLine($"Show debug panel changed from {previousSettings?.UISettings.ShowDebugPanel} to {newSettings.UISettings.ShowDebugPanel}");
            }

            if (previousSettings?.UISettings.ShowStatusInTitle != newSettings.UISettings.ShowStatusInTitle)
            {
                System.Diagnostics.Debug.WriteLine($"Show status in title changed from {previousSettings?.UISettings.ShowStatusInTitle} to {newSettings.UISettings.ShowStatusInTitle}");
            }

            if (previousSettings?.UISettings.ShowTrayNotifications != newSettings.UISettings.ShowTrayNotifications)
            {
                System.Diagnostics.Debug.WriteLine($"Show tray notifications changed from {previousSettings?.UISettings.ShowTrayNotifications} to {newSettings.UISettings.ShowTrayNotifications}");
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
                    configManager.SettingsChanged -= OnSettingsChanged;
                    configManager.SettingsValidating -= OnSettingsValidating;
                    configManager.SettingsBackupCompleted -= OnSettingsBackupCompleted;

                    eventHandlers.Clear();
                }

                isDisposed = true;
            }
        }
    }
} 