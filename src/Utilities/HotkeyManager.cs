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
        #endregion

        #region Fields
        private Keys macroKey = Keys.Capital;  // Default to Caps Lock
        private Keys switchKey = Keys.Q;       // Default to Q
        private ToggleType toggleType = ToggleType.Keyboard;
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
        #endregion
    }
} 