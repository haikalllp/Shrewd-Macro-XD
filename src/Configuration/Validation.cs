using System;

namespace NotesAndTasks.Configuration
{
    /// <summary>
    /// Provides validation methods for input values throughout the application.
    /// </summary>
    internal static class Validation
    {
        /// <summary>
        /// Validates a strength value for jitter or recoil reduction.
        /// </summary>
        /// <param name="strength">The strength value to validate.</param>
        /// <param name="minValue">The minimum allowed value (inclusive).</param>
        /// <param name="maxValue">The maximum allowed value (inclusive).</param>
        /// <param name="paramName">The name of the parameter being validated.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the strength value is outside the valid range.</exception>
        public static void ValidateStrength(int strength, int minValue, int maxValue, string paramName)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                System.Diagnostics.Debug.WriteLine("Warning: Parameter name is null or empty in ValidateStrength");
                paramName = "strength";
            }

            if (strength < minValue || strength > maxValue)
            {
                string errorMessage = $"Strength value must be between {minValue} and {maxValue}.";
                System.Diagnostics.Debug.WriteLine($"Validation error: {errorMessage} Actual value: {strength}");
                throw new ArgumentOutOfRangeException(
                    paramName,
                    strength,
                    errorMessage
                );
            }
        }

        /// <summary>
        /// Safely validates a strength value and returns a valid value even if out of range.
        /// </summary>
        /// <param name="strength">The strength value to validate.</param>
        /// <param name="minValue">The minimum allowed value (inclusive).</param>
        /// <param name="maxValue">The maximum allowed value (inclusive).</param>
        /// <param name="paramName">The name of the parameter being validated (for logging).</param>
        /// <returns>The original value if in range, or a clamped value if out of range.</returns>
        public static int ValidateAndClampStrength(int strength, int minValue, int maxValue, string paramName)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                paramName = "strength";
            }

            if (strength < minValue)
            {
                System.Diagnostics.Debug.WriteLine($"Validation warning: {paramName} value {strength} is below minimum {minValue}, clamping");
                return minValue;
            }
            
            if (strength > maxValue)
            {
                System.Diagnostics.Debug.WriteLine($"Validation warning: {paramName} value {strength} is above maximum {maxValue}, clamping");
                return maxValue;
            }
            
            return strength;
        }

        /// <summary>
        /// Validates that a string parameter is not null or empty.
        /// </summary>
        /// <param name="value">The string value to validate.</param>
        /// <param name="paramName">The name of the parameter being validated.</param>
        /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the value is empty.</exception>
        public static void ValidateStringNotNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                System.Diagnostics.Debug.WriteLine("Warning: Parameter name is null or empty in ValidateStringNotNullOrEmpty");
                paramName = "string";
            }

            if (value == null)
            {
                System.Diagnostics.Debug.WriteLine($"Validation error: {paramName} is null");
                throw new ArgumentNullException(paramName);
            }
            
            if (string.IsNullOrWhiteSpace(value))
            {
                string errorMessage = "Value cannot be empty or whitespace.";
                System.Diagnostics.Debug.WriteLine($"Validation error: {paramName} - {errorMessage}");
                throw new ArgumentException(errorMessage, paramName);
            }
        }

        /// <summary>
        /// Returns a default string value if the input is null or empty.
        /// </summary>
        /// <param name="value">The string value to validate.</param>
        /// <param name="defaultValue">The default value to return if input is invalid.</param>
        /// <param name="paramName">The name of the parameter (for logging).</param>
        /// <returns>The original string if valid, or the default value if not.</returns>
        public static string GetStringOrDefault(string value, string defaultValue, string paramName)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                paramName = "string";
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                System.Diagnostics.Debug.WriteLine($"Validation warning: {paramName} is null or empty, using default value");
                return defaultValue;
            }
            
            return value;
        }

        /// <summary>
        /// Validates that a reference parameter is not null.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="paramName">The name of the parameter being validated.</param>
        /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
        public static void ValidateNotNull<T>(T value, string paramName) where T : class
        {
            if (string.IsNullOrEmpty(paramName))
            {
                System.Diagnostics.Debug.WriteLine("Warning: Parameter name is null or empty in ValidateNotNull");
                paramName = typeof(T).Name;
            }

            if (value == null)
            {
                string errorMessage = $"{paramName} cannot be null.";
                System.Diagnostics.Debug.WriteLine($"Validation error: {errorMessage}");
                throw new ArgumentNullException(paramName, errorMessage);
            }
        }

        /// <summary>
        /// Validates that a handle is not IntPtr.Zero.
        /// </summary>
        /// <param name="handle">The handle to validate.</param>
        /// <param name="paramName">The name of the parameter being validated.</param>
        /// <exception cref="ArgumentException">Thrown when the handle is IntPtr.Zero.</exception>
        public static void ValidateHandle(IntPtr handle, string paramName)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                System.Diagnostics.Debug.WriteLine("Warning: Parameter name is null or empty in ValidateHandle");
                paramName = "handle";
            }

            if (handle == IntPtr.Zero)
            {
                string errorMessage = "Handle cannot be zero.";
                System.Diagnostics.Debug.WriteLine($"Validation error: {errorMessage}");
                throw new ArgumentException(errorMessage, paramName);
            }
        }

        /// <summary>
        /// Validates that a hook code is valid for processing.
        /// </summary>
        /// <param name="nCode">The hook code to validate.</param>
        /// <returns>True if the hook code should be processed, false if it should be passed to the next hook.</returns>
        public static bool ValidateHookCode(int nCode)
        {
            if (nCode < 0)
            {
                System.Diagnostics.Debug.WriteLine($"Hook code {nCode} is negative, passing to next hook");
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Validates a Windows Forms key value for use as a hotkey.
        /// </summary>
        /// <param name="key">The key to validate.</param>
        /// <param name="paramName">The name of the parameter being validated.</param>
        /// <returns>True if the key is valid for a hotkey, false otherwise.</returns>
        public static bool ValidateHotkeyValue(System.Windows.Forms.Keys key, string paramName)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                paramName = "key";
            }

            // Check for None, which is an invalid hotkey
            if (key == System.Windows.Forms.Keys.None)
            {
                System.Diagnostics.Debug.WriteLine($"Validation error: {paramName} cannot be Keys.None");
                return false;
            }

            // Check for reserved system keys
            if (key == System.Windows.Forms.Keys.LWin || 
                key == System.Windows.Forms.Keys.RWin || 
                key == System.Windows.Forms.Keys.Apps || 
                key == System.Windows.Forms.Keys.Sleep)
            {
                System.Diagnostics.Debug.WriteLine($"Validation error: {paramName} cannot be a reserved system key ({key})");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a value is within a specified range.
        /// </summary>
        /// <typeparam name="T">The type of value to validate.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="minValue">The minimum valid value.</param>
        /// <param name="maxValue">The maximum valid value.</param>
        /// <param name="paramName">The name of the parameter being validated.</param>
        /// <returns>True if the value is within range, false otherwise.</returns>
        public static bool ValidateRange<T>(T value, T minValue, T maxValue, string paramName) where T : IComparable<T>
        {
            if (string.IsNullOrEmpty(paramName))
            {
                paramName = "value";
            }

            if (value.CompareTo(minValue) < 0)
            {
                System.Diagnostics.Debug.WriteLine($"Validation error: {paramName} value {value} is less than minimum value {minValue}");
                return false;
            }

            if (value.CompareTo(maxValue) > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Validation error: {paramName} value {value} is greater than maximum value {maxValue}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates and clamps a value to a specified range.
        /// </summary>
        /// <typeparam name="T">The type of value to validate and clamp.</typeparam>
        /// <param name="value">The value to validate and clamp.</param>
        /// <param name="minValue">The minimum valid value.</param>
        /// <param name="maxValue">The maximum valid value.</param>
        /// <param name="paramName">The name of the parameter being validated (for logging).</param>
        /// <returns>The original value if within range, or the nearest boundary value if outside range.</returns>
        public static T ClampToRange<T>(T value, T minValue, T maxValue, string paramName) where T : IComparable<T>
        {
            if (string.IsNullOrEmpty(paramName))
            {
                paramName = "value";
            }

            if (value.CompareTo(minValue) < 0)
            {
                System.Diagnostics.Debug.WriteLine($"Validation warning: {paramName} value {value} is less than minimum value {minValue}, clamping");
                return minValue;
            }

            if (value.CompareTo(maxValue) > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Validation warning: {paramName} value {value} is greater than maximum value {maxValue}, clamping");
                return maxValue;
            }

            return value;
        }
    }
} 