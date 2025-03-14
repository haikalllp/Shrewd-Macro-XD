using System;
using System.Windows.Forms;

namespace NotesAndTasks.Configuration
{
    public static class SettingsValidation
    {
        public static bool IsValidHotkey(Keys key)
        {
            // Validate that the key is a valid hotkey
            return key != Keys.None && 
                   key != Keys.LButton && 
                   key != Keys.RButton;
        }

        public static bool ValidateSettings(Settings settings, int minStrength, int maxStrength)
        {
            if (settings == null) return false;

            // Validate strength values
            if (settings.JitterStrength < minStrength || settings.JitterStrength > maxStrength ||
                settings.RecoilReductionStrength < minStrength || settings.RecoilReductionStrength > maxStrength)
                return false;

            // Validate hotkeys
            if (string.IsNullOrEmpty(settings.MacroToggleKey) || string.IsNullOrEmpty(settings.ModeSwitchKey))
                return false;

            try
            {
                var toggleKey = (Keys)Enum.Parse(typeof(Keys), settings.MacroToggleKey);
                var switchKey = (Keys)Enum.Parse(typeof(Keys), settings.ModeSwitchKey);

                if (!IsValidHotkey(toggleKey) || !IsValidHotkey(switchKey))
                    return false;

                if (toggleKey == switchKey)
                    return false;
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
} 