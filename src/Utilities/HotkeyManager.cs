using System;
using System.Windows.Forms;
using NotesAndTasks.Configuration;
using NotesAndTasks.Models;

namespace NotesAndTasks.Utilities
{
    /// <summary>
    /// Manages hotkeys and key bindings for the macro application.
    /// </summary>
    public class HotkeyManager
    {
        #region Events
        public event EventHandler<Keys> MacroKeyChanged;
        public event EventHandler<Keys> SwitchKeyChanged;
        public event EventHandler<bool> KeySettingStateChanged;
        public event EventHandler<string> DebugInfoUpdated;
        #endregion

        #region Fields
        private Keys macroKey = Keys.Capital;  // Default to Caps Lock
        private Keys switchKey = Keys.Q;       // Default to Q
        private ToggleType toggleType = ToggleType.Keyboard;
        private bool isSettingMacroKey = false;
        private bool isSettingSwitchKey = false;
        private readonly MacroManager macroManager;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current macro toggle key.
        /// </summary>
        public Keys MacroKey => macroKey;

        /// <summary>
        /// Gets the current mode switch key.
        /// </summary>
        public Keys SwitchKey => switchKey;

        /// <summary>
        /// Gets the current toggle type.
        /// </summary>
        public ToggleType ToggleType => toggleType;

        /// <summary>
        /// Gets whether the manager is currently waiting for a macro key input.
        /// </summary>
        public bool IsSettingMacroKey => isSettingMacroKey;

        /// <summary>
        /// Gets whether the manager is currently waiting for a switch key input.
        /// </summary>
        public bool IsSettingSwitchKey => isSettingSwitchKey;
        #endregion

        #region Constructor
        public HotkeyManager(MacroManager macroManager)
        {
            this.macroManager = macroManager ?? throw new ArgumentNullException(nameof(macroManager));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets the macro toggle key.
        /// </summary>
        /// <param name="key">The key to set.</param>
        /// <param name="type">The type of toggle.</param>
        public void SetMacroKey(Keys key, ToggleType type)
        {
            if (key == Keys.None)
            {
                DebugInfoUpdated?.Invoke(this, $"Warning: Ignoring attempt to set MacroKey to None");
                return;
            }

            // Prevent using reserved system keys
            if (key == Keys.LWin || key == Keys.RWin || key == Keys.Apps || key == Keys.Sleep)
            {
                DebugInfoUpdated?.Invoke(this, $"Warning: Ignoring attempt to set MacroKey to reserved system key: {key}");
                return;
            }

            // Prevent both keys being the same
            if (key == switchKey && ConvertToggleTypeToInputType(type) == ConvertToggleTypeToInputType(toggleType))
            {
                DebugInfoUpdated?.Invoke(this, $"Warning: MacroKey and SwitchKey cannot be the same");
                return;
            }

            macroKey = key;
            toggleType = type;
            MacroKeyChanged?.Invoke(this, key);
            DebugInfoUpdated?.Invoke(this, $"Set macro key to {key} with toggle type {type}");
            SaveSettings();
        }

        /// <summary>
        /// Sets the mode switch key.
        /// </summary>
        /// <param name="key">The key to set.</param>
        public void SetSwitchKey(Keys key)
        {
            if (key == Keys.None)
            {
                DebugInfoUpdated?.Invoke(this, $"Warning: Ignoring attempt to set SwitchKey to None");
                return;
            }

            // Prevent using reserved system keys
            if (key == Keys.LWin || key == Keys.RWin || key == Keys.Apps || key == Keys.Sleep)
            {
                DebugInfoUpdated?.Invoke(this, $"Warning: Ignoring attempt to set SwitchKey to reserved system key: {key}");
                return;
            }

            // Prevent both keys being the same
            if (key == macroKey && toggleType == ToggleType.Keyboard)
            {
                DebugInfoUpdated?.Invoke(this, $"Warning: SwitchKey and MacroKey cannot be the same");
                return;
            }

            switchKey = key;
            SwitchKeyChanged?.Invoke(this, key);
            DebugInfoUpdated?.Invoke(this, $"Set switch key to {key}");
            SaveSettings();
        }

        /// <summary>
        /// Sets the mode switch key with type information.
        /// </summary>
        /// <param name="key">The key to set.</param>
        /// <param name="type">The toggle type (keyboard or mouse).</param>
        public void SetSwitchKey(Keys key, ToggleType type)
        {
            if (key == Keys.None)
            {
                DebugInfoUpdated?.Invoke(this, $"Warning: Ignoring attempt to set SwitchKey to None");
                return;
            }

            // Prevent using reserved system keys
            if (key == Keys.LWin || key == Keys.RWin || key == Keys.Apps || key == Keys.Sleep)
            {
                DebugInfoUpdated?.Invoke(this, $"Warning: Ignoring attempt to set SwitchKey to reserved system key: {key}");
                return;
            }

            // Prevent both keys being the same
            if (key == macroKey && ConvertToggleTypeToInputType(type) == ConvertToggleTypeToInputType(toggleType))
            {
                DebugInfoUpdated?.Invoke(this, $"Warning: SwitchKey and MacroKey cannot be the same");
                return;
            }

            switchKey = key;
            SwitchKeyChanged?.Invoke(this, key);
            DebugInfoUpdated?.Invoke(this, $"Set switch key to {key} with toggle type {type}");
            SaveSettings(type);
        }

        /// <summary>
        /// Loads hotkey settings from the settings manager.
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                var settings = ConfigurationManager.Instance.CurrentSettings;
                if (settings?.HotkeySettings == null)
                {
                    DebugInfoUpdated?.Invoke(this, "Failed to load hotkey settings: Settings are null");
                    ResetToDefaults();
                    return;
                }

                if (settings.HotkeySettings.MacroKey == null || settings.HotkeySettings.SwitchKey == null)
                {
                    DebugInfoUpdated?.Invoke(this, "Failed to load hotkey settings: MacroKey or SwitchKey is null");
                    ResetToDefaults();
                    return;
                }

                // Validate keys
                if (settings.HotkeySettings.MacroKey.Key == Keys.None)
                {
                    DebugInfoUpdated?.Invoke(this, "Invalid MacroKey value: None");
                    settings.HotkeySettings.MacroKey.Key = Keys.Capital;
                    settings.HotkeySettings.MacroKey.Type = InputType.Keyboard;
                }

                if (settings.HotkeySettings.SwitchKey.Key == Keys.None)
                {
                    DebugInfoUpdated?.Invoke(this, "Invalid SwitchKey value: None");
                    settings.HotkeySettings.SwitchKey.Key = Keys.Q;
                    settings.HotkeySettings.SwitchKey.Type = InputType.Keyboard;
                }

                // Prevent reserved system keys
                if (settings.HotkeySettings.MacroKey.Key == Keys.LWin || 
                    settings.HotkeySettings.MacroKey.Key == Keys.RWin || 
                    settings.HotkeySettings.MacroKey.Key == Keys.Apps || 
                    settings.HotkeySettings.MacroKey.Key == Keys.Sleep)
                {
                    DebugInfoUpdated?.Invoke(this, $"Reserved system key detected for MacroKey: {settings.HotkeySettings.MacroKey.Key}");
                    settings.HotkeySettings.MacroKey.Key = Keys.Capital;
                    settings.HotkeySettings.MacroKey.Type = InputType.Keyboard;
                }

                if (settings.HotkeySettings.SwitchKey.Key == Keys.LWin || 
                    settings.HotkeySettings.SwitchKey.Key == Keys.RWin || 
                    settings.HotkeySettings.SwitchKey.Key == Keys.Apps || 
                    settings.HotkeySettings.SwitchKey.Key == Keys.Sleep)
                {
                    DebugInfoUpdated?.Invoke(this, $"Reserved system key detected for SwitchKey: {settings.HotkeySettings.SwitchKey.Key}");
                    settings.HotkeySettings.SwitchKey.Key = Keys.Q;
                    settings.HotkeySettings.SwitchKey.Type = InputType.Keyboard;
                }

                // Prevent keys from being the same
                if (settings.HotkeySettings.MacroKey.Key == settings.HotkeySettings.SwitchKey.Key &&
                    settings.HotkeySettings.MacroKey.Type == settings.HotkeySettings.SwitchKey.Type)
                {
                    DebugInfoUpdated?.Invoke(this, "MacroKey and SwitchKey are the same, resetting SwitchKey");
                    settings.HotkeySettings.SwitchKey.Key = Keys.Q;
                    settings.HotkeySettings.SwitchKey.Type = InputType.Keyboard;
                }

                macroKey = settings.HotkeySettings.MacroKey.Key;
                switchKey = settings.HotkeySettings.SwitchKey.Key;
                toggleType = ConvertInputTypeToToggleType(settings.HotkeySettings.MacroKey.Type);
                
                DebugInfoUpdated?.Invoke(this, $"Hotkey settings loaded - MacroKey: {macroKey}, SwitchKey: {switchKey}");
            }
            catch (Exception ex)
            {
                DebugInfoUpdated?.Invoke(this, $"Error loading hotkey settings: {ex.Message}");
                ResetToDefaults();
            }
        }

        /// <summary>
        /// Saves current hotkey settings to the settings manager.
        /// </summary>
        private void SaveSettings()
        {
            SaveSettings(null);
        }
        
        /// <summary>
        /// Saves current hotkey settings to the settings manager with optional switch key type.
        /// </summary>
        private void SaveSettings(ToggleType? switchKeyType = null)
        {
            var settings = ConfigurationManager.Instance.CurrentSettings;
            if (settings != null)
            {
                settings.HotkeySettings.MacroKey.Key = macroKey;
                settings.HotkeySettings.SwitchKey.Key = switchKey;
                settings.HotkeySettings.MacroKey.Type = ConvertToggleTypeToInputType(toggleType);
                
                // Update SwitchKey type based on provided parameter or existing value
                if (switchKeyType.HasValue)
                {
                    settings.HotkeySettings.SwitchKey.Type = ConvertToggleTypeToInputType(switchKeyType.Value);
                }
                
                ConfigurationManager.Instance.SaveSettings();
            }
        }

        /// <summary>
        /// Converts InputType to ToggleType
        /// </summary>
        private ToggleType ConvertInputTypeToToggleType(Models.InputType inputType)
        {
            return inputType == Models.InputType.Keyboard ? ToggleType.Keyboard : ToggleType.MouseMiddle;
        }

        /// <summary>
        /// Converts ToggleType to InputType
        /// </summary>
        private Models.InputType ConvertToggleTypeToInputType(ToggleType toggleType)
        {
            return toggleType == ToggleType.Keyboard ? Models.InputType.Keyboard : Models.InputType.Mouse;
        }

        /// <summary>
        /// Resets hotkeys to their default values.
        /// </summary>
        public void ResetToDefaults()
        {
            SetMacroKey(Keys.Capital, ToggleType.Keyboard);
            SetSwitchKey(Keys.Q);
        }

        /// <summary>
        /// Starts listening for a new macro key.
        /// </summary>
        public void StartSettingMacroKey()
        {
            isSettingMacroKey = true;
            isSettingSwitchKey = false;
            KeySettingStateChanged?.Invoke(this, true);
            DebugInfoUpdated?.Invoke(this, "Waiting for new macro key...");
        }

        /// <summary>
        /// Starts listening for a new switch key.
        /// </summary>
        public void StartSettingSwitchKey()
        {
            isSettingSwitchKey = true;
            isSettingMacroKey = false;
            KeySettingStateChanged?.Invoke(this, true);
            DebugInfoUpdated?.Invoke(this, "Waiting for new switch key...");
        }

        /// <summary>
        /// Handles keyboard input events.
        /// </summary>
        public void HandleKeyDown(Keys virtualKeyCode)
        {
            if (isSettingMacroKey)
            {
                isSettingMacroKey = false;
                KeySettingStateChanged?.Invoke(this, false);
                SetMacroKey(virtualKeyCode, ToggleType.Keyboard);
                DebugInfoUpdated?.Invoke(this, $"Set macro key to {virtualKeyCode}");
            }
            else if (isSettingSwitchKey)
            {
                isSettingSwitchKey = false;
                KeySettingStateChanged?.Invoke(this, false);
                SetSwitchKey(virtualKeyCode);
                DebugInfoUpdated?.Invoke(this, $"Set switch key to {virtualKeyCode}");
            }
            else if (virtualKeyCode == macroKey && toggleType == ToggleType.Keyboard)
            {
                macroManager.ToggleMacro();
            }
            else if (virtualKeyCode == switchKey)
            {
                macroManager.SwitchMode();
            }
        }

        /// <summary>
        /// Handles mouse button input events.
        /// </summary>
        public void HandleMouseButton(MouseButtons button)
        {
            Keys key = MouseButtonToKeys(button);
            ToggleType type = MouseButtonToToggleType(button);

            if (isSettingMacroKey)
            {
                isSettingMacroKey = false;
                KeySettingStateChanged?.Invoke(this, false);
                SetMacroKey(key, type);
                DebugInfoUpdated?.Invoke(this, $"Set macro key to {button}");
            }
            else if (isSettingSwitchKey)
            {
                isSettingSwitchKey = false;
                KeySettingStateChanged?.Invoke(this, false);
                SetSwitchKey(key, type);
                DebugInfoUpdated?.Invoke(this, $"Set switch key to {button}");
            }
            else if (key == macroKey)
            {
                macroManager.ToggleMacro();
            }
            else if (key == switchKey)
            {
                macroManager.SwitchMode();
            }
        }

        /// <summary>
        /// Cancels any ongoing key setting operation.
        /// </summary>
        public void CancelKeySetting()
        {
            if (isSettingMacroKey || isSettingSwitchKey)
            {
                isSettingMacroKey = false;
                isSettingSwitchKey = false;
                KeySettingStateChanged?.Invoke(this, false);
                DebugInfoUpdated?.Invoke(this, "Key setting cancelled");
            }
        }

        #region Private Methods
        private Keys MouseButtonToKeys(MouseButtons button)
        {
            return button switch
            {
                MouseButtons.Middle => Keys.MButton,
                MouseButtons.XButton1 => Keys.XButton1,
                MouseButtons.XButton2 => Keys.XButton2,
                _ => Keys.None
            };
        }

        private ToggleType MouseButtonToToggleType(MouseButtons button)
        {
            return button switch
            {
                MouseButtons.Middle => ToggleType.MouseMiddle,
                MouseButtons.XButton1 => ToggleType.MouseX1,
                MouseButtons.XButton2 => ToggleType.MouseX2,
                _ => ToggleType.Keyboard
            };
        }
        #endregion
        #endregion
    }
} 