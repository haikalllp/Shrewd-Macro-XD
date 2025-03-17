using System;
using System.Windows.Forms;
using NotesAndTasks.Configuration;

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
            macroKey = key;
            toggleType = type;
            MacroKeyChanged?.Invoke(this, key);
            SaveSettings();
        }

        /// <summary>
        /// Sets the mode switch key.
        /// </summary>
        /// <param name="key">The key to set.</param>
        public void SetSwitchKey(Keys key)
        {
            switchKey = key;
            SwitchKeyChanged?.Invoke(this, key);
            SaveSettings();
        }

        /// <summary>
        /// Loads hotkey settings from the settings manager.
        /// </summary>
        public void LoadSettings()
        {
            var settings = SettingsManager.CurrentSettings;
            if (settings != null)
            {
                macroKey = settings.MacroKey;
                switchKey = settings.SwitchKey;
                toggleType = settings.ToggleType;
            }
        }

        /// <summary>
        /// Saves current hotkey settings to the settings manager.
        /// </summary>
        private void SaveSettings()
        {
            var settings = SettingsManager.CurrentSettings;
            if (settings != null)
            {
                settings.MacroKey = macroKey;
                settings.SwitchKey = switchKey;
                settings.ToggleType = toggleType;
                SettingsManager.SaveSettings();
            }
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
                SetSwitchKey(key);
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