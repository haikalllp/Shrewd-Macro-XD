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
            if (strength < minValue || strength > maxValue)
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    strength,
                    $"Strength value must be between {minValue} and {maxValue}."
                );
            }
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
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be empty or whitespace.", paramName);
            }
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
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
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
            if (handle == IntPtr.Zero)
            {
                throw new ArgumentException("Handle cannot be zero.", paramName);
            }
        }

        /// <summary>
        /// Validates that a hook code is valid for processing.
        /// </summary>
        /// <param name="nCode">The hook code to validate.</param>
        /// <returns>True if the hook code should be processed, false if it should be passed to the next hook.</returns>
        public static bool ValidateHookCode(int nCode)
        {
            return nCode >= 0;
        }
    }
} 