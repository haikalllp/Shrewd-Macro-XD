using System;
using NotesAndTasks.Models;

namespace NotesAndTasks.Configuration
{
    /// <summary>
    /// Event arguments for configuration change events.
    /// </summary>
    public class ConfigurationChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the configuration section that was changed.
        /// </summary>
        public string Section { get; }

        /// <summary>
        /// Gets the previous configuration state.
        /// </summary>
        public AppSettings PreviousConfig { get; }

        /// <summary>
        /// Gets the new configuration state.
        /// </summary>
        public AppSettings NewConfig { get; }

        public ConfigurationChangedEventArgs(string section, AppSettings previousConfig, AppSettings newConfig)
        {
            Section = section;
            PreviousConfig = previousConfig;
            NewConfig = newConfig;
        }
    }

    /// <summary>
    /// Event arguments for configuration validation events.
    /// </summary>
    public class ConfigurationValidationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets whether the configuration is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the validation message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets the configuration being validated.
        /// </summary>
        public AppSettings Configuration { get; }

        public ConfigurationValidationEventArgs(AppSettings configuration)
        {
            Configuration = configuration;
            IsValid = true;
        }
    }

    /// <summary>
    /// Event arguments for configuration backup events.
    /// </summary>
    public class ConfigurationBackupEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the backup file path.
        /// </summary>
        public string BackupPath { get; }

        /// <summary>
        /// Gets whether the backup was successful.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets any error message if the backup failed.
        /// </summary>
        public string ErrorMessage { get; }

        public ConfigurationBackupEventArgs(string backupPath, bool success, string errorMessage = null)
        {
            BackupPath = backupPath;
            Success = success;
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// Delegate for configuration change events.
    /// </summary>
    public delegate void ConfigurationChangedEventHandler(object sender, ConfigurationChangedEventArgs e);

    /// <summary>
    /// Delegate for configuration validation events.
    /// </summary>
    public delegate void ConfigurationValidationEventHandler(object sender, ConfigurationValidationEventArgs e);

    /// <summary>
    /// Delegate for configuration backup events.
    /// </summary>
    public delegate void ConfigurationBackupEventHandler(object sender, ConfigurationBackupEventArgs e);
} 