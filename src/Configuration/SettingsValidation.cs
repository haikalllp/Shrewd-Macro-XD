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
            if (!IsValidHotkey(settings.MacroKey) || !IsValidHotkey(settings.SwitchKey))
                return false;

            // Check for hotkey conflicts
            if (settings.MacroKey == settings.SwitchKey)
                return false;

            return true;
        }
    }
} 