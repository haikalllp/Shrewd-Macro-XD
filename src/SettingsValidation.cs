using System;
using System.Windows.Forms;

namespace NotesAndTasks
{
    /// <summary>
    /// Provides validation methods for application settings.
    /// Ensures settings values are within valid ranges and constraints.
    /// </summary>
    public static class SettingsValidation
    {
        /// <summary>
        /// Validates all settings in a settings object.
        /// </summary>
        /// <param name="settings">The settings object to validate.</param>
        /// <param name="minStrength">Minimum allowed strength value.</param>
        /// <param name="maxStrength">Maximum allowed strength value.</param>
        /// <returns>True if all settings are valid, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when settings is null.</exception>
        public static bool ValidateSettings(Settings settings, int minStrength, int maxStrength)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            try
            {
                // Validate strength values
                if (!ValidateStrengthValue(settings.JitterStrength, minStrength, maxStrength, "Jitter strength"))
                    return false;

                if (!ValidateStrengthValue(settings.RecoilReductionStrength, minStrength, maxStrength, "Recoil reduction strength"))
                    return false;

                // Validate mode states
                if (!ValidateModeStates(settings))
                    return false;

                // Validate hotkeys
                if (!ValidateHotkeys(settings))
                    return false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Validates a strength value is within the allowed range.
        /// </summary>
        /// <param name="value">The strength value to validate.</param>
        /// <param name="min">Minimum allowed value.</param>
        /// <param name="max">Maximum allowed value.</param>
        /// <param name="paramName">Name of the parameter being validated.</param>
        /// <returns>True if the value is valid, false otherwise.</returns>
        private static bool ValidateStrengthValue(int value, int min, int max, string paramName)
        {
            if (value < min || value > max)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid {paramName}: {value} (must be between {min} and {max})");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates the mode states are consistent.
        /// </summary>
        /// <param name="settings">The settings object containing mode states.</param>
        /// <returns>True if mode states are valid, false otherwise.</returns>
        private static bool ValidateModeStates(Settings settings)
        {
            // Cannot have both always modes enabled
            if (settings.AlwaysJitterMode && settings.AlwaysRecoilReductionMode)
            {
                System.Diagnostics.Debug.WriteLine("Invalid mode state: Both always modes cannot be enabled");
                return false;
            }

            // If always jitter is enabled, jitter should be enabled and recoil reduction disabled
            if (settings.AlwaysJitterMode && (!settings.JitterEnabled || settings.RecoilReductionEnabled))
            {
                System.Diagnostics.Debug.WriteLine("Invalid mode state: Inconsistent always jitter mode");
                return false;
            }

            // If always recoil reduction is enabled, recoil reduction should be enabled and jitter disabled
            if (settings.AlwaysRecoilReductionMode && (!settings.RecoilReductionEnabled || settings.JitterEnabled))
            {
                System.Diagnostics.Debug.WriteLine("Invalid mode state: Inconsistent always recoil reduction mode");
                return false;
            }

            // In normal mode, one and only one mode should be enabled
            if (!settings.AlwaysJitterMode && !settings.AlwaysRecoilReductionMode)
            {
                if (settings.JitterEnabled == settings.RecoilReductionEnabled)
                {
                    System.Diagnostics.Debug.WriteLine("Invalid mode state: Both modes cannot be in the same state");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates hotkey settings.
        /// </summary>
        /// <param name="settings">The settings object containing hotkey configurations.</param>
        /// <returns>True if hotkeys are valid, false otherwise.</returns>
        private static bool ValidateHotkeys(Settings settings)
        {
            // Validate macro toggle key
            if (string.IsNullOrEmpty(settings.MacroToggleKey))
            {
                System.Diagnostics.Debug.WriteLine("Invalid macro toggle key: Key cannot be empty");
                return false;
            }

            // Validate mode switch key
            if (string.IsNullOrEmpty(settings.ModeSwitchKey))
            {
                System.Diagnostics.Debug.WriteLine("Invalid mode switch key: Key cannot be empty");
                return false;
            }

            // Validate that the keys are different
            if (settings.MacroToggleKey == settings.ModeSwitchKey)
            {
                System.Diagnostics.Debug.WriteLine("Invalid hotkeys: Toggle and switch keys cannot be the same");
                return false;
            }

            // Validate that the keys are valid enum values
            try
            {
                Keys toggleKey = (Keys)Enum.Parse(typeof(Keys), settings.MacroToggleKey);
                Keys switchKey = (Keys)Enum.Parse(typeof(Keys), settings.ModeSwitchKey);

                // Validate that the keys are supported
                if (!IsValidHotkey(toggleKey) || !IsValidHotkey(switchKey))
                {
                    System.Diagnostics.Debug.WriteLine("Invalid hotkey: Unsupported key");
                    return false;
                }
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Invalid hotkey: Failed to parse key value");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if a key is valid for use as a hotkey.
        /// </summary>
        /// <param name="key">The key to validate.</param>
        /// <returns>True if the key is valid for use as a hotkey, false otherwise.</returns>
        private static bool IsValidHotkey(Keys key)
        {
            // List of invalid keys that shouldn't be used as hotkeys
            Keys[] invalidKeys = {
                Keys.None,
                Keys.LButton,
                Keys.RButton,
                Keys.Cancel,
                Keys.LineFeed,
                Keys.Clear,
                Keys.Return,
                Keys.Enter,
                Keys.ShiftKey,
                Keys.ControlKey,
                Keys.Menu,
                Keys.Pause,
                Keys.KeyCode,
                Keys.Modifiers,
                Keys.Alt,
                Keys.Control,
                Keys.Shift,
                Keys.Apps
            };

            return !Array.Exists(invalidKeys, k => k == key);
        }
    }
} 