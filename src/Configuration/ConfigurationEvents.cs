using System;
using NotesAndTasks.Models;

namespace NotesAndTasks.Configuration
{
    /// <summary>
    /// Event arguments for when settings have changed
    /// </summary>
    public class SettingsChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the section of settings that changed
        /// </summary>
        public string Section { get; }

        /// <summary>
        /// Gets the previous settings
        /// </summary>
        public AppSettings PreviousSettings { get; }

        /// <summary>
        /// Gets the new settings
        /// </summary>
        public AppSettings NewSettings { get; }

        /// <summary>
        /// Initializes a new instance of the SettingsChangedEventArgs class
        /// </summary>
        /// <param name="section">The section that changed</param>
        /// <param name="previousSettings">The previous settings</param>
        /// <param name="newSettings">The new settings</param>
        public SettingsChangedEventArgs(string section, AppSettings previousSettings, AppSettings newSettings)
        {
            Section = section;
            PreviousSettings = previousSettings;
            NewSettings = newSettings;
        }
    }

    /// <summary>
    /// Event arguments for settings validation
    /// </summary>
    public class SettingsValidationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets whether the settings are valid
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// Gets or sets the validation error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets the settings being validated
        /// </summary>
        public AppSettings Settings { get; }

        /// <summary>
        /// Initializes a new instance of the SettingsValidationEventArgs class
        /// </summary>
        /// <param name="settings">The settings to validate</param>
        public SettingsValidationEventArgs(AppSettings settings)
        {
            Settings = settings;
        }
    }

    /// <summary>
    /// Event arguments for settings backup completion
    /// </summary>
    public class SettingsBackupEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the path to the backup file
        /// </summary>
        public string BackupPath { get; }

        /// <summary>
        /// Gets whether the backup was successful
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the error message if backup failed
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Initializes a new instance of the SettingsBackupEventArgs class
        /// </summary>
        /// <param name="backupPath">The path to the backup file</param>
        /// <param name="success">Whether the backup was successful</param>
        /// <param name="errorMessage">The error message if backup failed</param>
        public SettingsBackupEventArgs(string backupPath, bool success, string errorMessage = null)
        {
            BackupPath = backupPath;
            Success = success;
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// Delegate for settings change events
    /// </summary>
    public delegate void SettingsChangedEventHandler(object sender, SettingsChangedEventArgs e);

    /// <summary>
    /// Delegate for settings validation events
    /// </summary>
    public delegate void SettingsValidationEventHandler(object sender, SettingsValidationEventArgs e);

    /// <summary>
    /// Delegate for settings backup completion events
    /// </summary>
    public delegate void SettingsBackupEventHandler(object sender, SettingsBackupEventArgs e);
} 