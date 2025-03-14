using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;
using System.Linq;

namespace NotesAndTasks.Configuration
{
    /// <summary>
    /// Manages application configuration, providing thread-safe access to settings
    /// and handling configuration persistence.
    /// </summary>
    public class ConfigurationManager
    {
        private static readonly Lazy<ConfigurationManager> instance = 
            new Lazy<ConfigurationManager>(() => new ConfigurationManager(), LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly ReaderWriterLockSlim configLock = new ReaderWriterLockSlim();
        private readonly string configFilePath;
        private AppConfiguration currentConfig;
        private readonly JsonSerializerOptions jsonOptions;

        // Add these event declarations after the existing fields
        public event ConfigurationChangedEventHandler ConfigurationChanged;
        public event ConfigurationValidationEventHandler ConfigurationValidating;
        public event ConfigurationBackupEventHandler ConfigurationBackupCompleted;

        /// <summary>
        /// Gets the singleton instance of the ConfigurationManager.
        /// </summary>
        public static ConfigurationManager Instance => instance.Value;

        /// <summary>
        /// Gets the current configuration. Thread-safe access to configuration values.
        /// </summary>
        public AppConfiguration CurrentConfig
        {
            get
            {
                configLock.EnterReadLock();
                try
                {
                    return currentConfig.Clone();
                }
                finally
                {
                    configLock.ExitReadLock();
                }
            }
        }

        private ConfigurationManager()
        {
            configFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NotesAndTasks",
                "config.json"
            );

            jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            InitializeConfiguration();
        }

        /// <summary>
        /// Initializes the configuration system, creating default config if none exists.
        /// </summary>
        private void InitializeConfiguration()
        {
            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));

                if (File.Exists(configFilePath))
                {
                    LoadConfiguration();
                }
                else
                {
                    currentConfig = CreateDefaultConfiguration();
                    SaveConfiguration();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing configuration: {ex.Message}");
                currentConfig = CreateDefaultConfiguration();
            }
        }

        /// <summary>
        /// Creates a default configuration with preset values.
        /// </summary>
        private AppConfiguration CreateDefaultConfiguration()
        {
            return new AppConfiguration
            {
                JitterSettings = new JitterConfiguration
                {
                    Strength = 3,
                    IsEnabled = false,
                    AlwaysEnabled = false
                },
                RecoilSettings = new RecoilConfiguration
                {
                    Strength = 1,
                    IsEnabled = true,
                    AlwaysEnabled = false
                },
                HotkeySettings = new HotkeyConfiguration
                {
                    MacroToggleKey = Keys.Capital.ToString(),
                    ModeSwitchKey = Keys.Q.ToString()
                },
                UISettings = new UIConfiguration
                {
                    MinimizeToTray = false,
                    ShowDebugPanel = false,
                    WindowPosition = new System.Drawing.Point(100, 100),
                    WindowSize = new System.Drawing.Size(800, 600)
                },
                BackupSettings = new BackupConfiguration
                {
                    AutoBackupEnabled = true,
                    BackupIntervalHours = 24,
                    MaxBackupCount = 5,
                    BackupDirectory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "NotesAndTasks",
                        "Backups"
                    )
                }
            };
        }

        /// <summary>
        /// Updates the configuration with new values and saves to disk.
        /// </summary>
        /// <param name="newConfig">The new configuration to apply.</param>
        /// <param name="section">The section being updated.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        public bool UpdateConfiguration(AppConfiguration newConfig, string section = "General")
        {
            if (newConfig == null)
                throw new ArgumentNullException(nameof(newConfig));

            configLock.EnterWriteLock();
            try
            {
                // Raise validation event
                var validationArgs = new ConfigurationValidationEventArgs(newConfig);
                OnConfigurationValidating(validationArgs);

                if (!validationArgs.IsValid || !ValidateConfiguration(newConfig))
                {
                    System.Diagnostics.Debug.WriteLine($"Configuration validation failed: {validationArgs.Message}");
                    return false;
                }

                var previousConfig = currentConfig?.Clone() as AppConfiguration;
                currentConfig = newConfig.Clone() as AppConfiguration;
                SaveConfiguration();

                // Raise change event
                OnConfigurationChanged(new ConfigurationChangedEventArgs(section, previousConfig, currentConfig));
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating configuration: {ex.Message}");
                return false;
            }
            finally
            {
                configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Validates the configuration values.
        /// </summary>
        /// <param name="config">The configuration to validate.</param>
        /// <returns>True if the configuration is valid, false otherwise.</returns>
        private bool ValidateConfiguration(AppConfiguration config)
        {
            if (config == null) return false;

            try
            {
                // Validate Jitter settings
                if (config.JitterSettings.Strength < 1 || config.JitterSettings.Strength > 20)
                    return false;

                // Validate Recoil settings
                if (config.RecoilSettings.Strength < 1 || config.RecoilSettings.Strength > 20)
                    return false;

                // Validate hotkeys
                if (string.IsNullOrEmpty(config.HotkeySettings.MacroToggleKey) ||
                    string.IsNullOrEmpty(config.HotkeySettings.ModeSwitchKey))
                    return false;

                // Validate backup settings
                if (config.BackupSettings.AutoBackupEnabled)
                {
                    if (config.BackupSettings.BackupIntervalHours < 1 ||
                        config.BackupSettings.MaxBackupCount < 1 ||
                        string.IsNullOrEmpty(config.BackupSettings.BackupDirectory))
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Configuration validation error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads the configuration from disk.
        /// </summary>
        private void LoadConfiguration()
        {
            configLock.EnterWriteLock();
            try
            {
                string jsonContent = File.ReadAllText(configFilePath);
                var loadedConfig = JsonSerializer.Deserialize<AppConfiguration>(jsonContent, jsonOptions);

                if (ValidateConfiguration(loadedConfig))
                {
                    currentConfig = loadedConfig;
                }
                else
                {
                    currentConfig = CreateDefaultConfiguration();
                    SaveConfiguration();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex.Message}");
                currentConfig = CreateDefaultConfiguration();
            }
            finally
            {
                configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Saves the current configuration to disk.
        /// </summary>
        private void SaveConfiguration()
        {
            try
            {
                string jsonContent = JsonSerializer.Serialize(currentConfig, jsonOptions);
                File.WriteAllText(configFilePath, jsonContent);

                if (currentConfig.BackupSettings.AutoBackupEnabled)
                {
                    CreateConfigurationBackup();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a backup of the current configuration.
        /// </summary>
        private void CreateConfigurationBackup()
        {
            try
            {
                string backupDir = currentConfig.BackupSettings.BackupDirectory;
                Directory.CreateDirectory(backupDir);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupPath = Path.Combine(backupDir, $"config_backup_{timestamp}.json");

                File.Copy(configFilePath, backupPath, true);

                // Cleanup old backups
                var backupFiles = Directory.GetFiles(backupDir, "config_backup_*.json")
                    .OrderByDescending(f => f)
                    .Skip(currentConfig.BackupSettings.MaxBackupCount);

                foreach (var file in backupFiles)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error deleting old backup {file}: {ex.Message}");
                    }
                }

                // Raise backup completed event
                OnConfigurationBackupCompleted(new ConfigurationBackupEventArgs(backupPath, true));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating configuration backup: {ex.Message}");
                OnConfigurationBackupCompleted(new ConfigurationBackupEventArgs(null, false, ex.Message));
            }
        }

        /// <summary>
        /// Restores configuration from a backup file.
        /// </summary>
        /// <param name="backupFilePath">Path to the backup file to restore from.</param>
        /// <returns>True if restore was successful, false otherwise.</returns>
        public bool RestoreFromBackup(string backupFilePath)
        {
            if (!File.Exists(backupFilePath))
                return false;

            configLock.EnterWriteLock();
            try
            {
                string jsonContent = File.ReadAllText(backupFilePath);
                var restoredConfig = JsonSerializer.Deserialize<AppConfiguration>(jsonContent, jsonOptions);

                if (ValidateConfiguration(restoredConfig))
                {
                    currentConfig = restoredConfig;
                    SaveConfiguration();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring configuration: {ex.Message}");
                return false;
            }
            finally
            {
                configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets a list of available configuration backups.
        /// </summary>
        /// <returns>Array of backup file paths.</returns>
        public string[] GetAvailableBackups()
        {
            try
            {
                string backupDir = currentConfig.BackupSettings.BackupDirectory;
                if (!Directory.Exists(backupDir))
                    return Array.Empty<string>();

                return Directory.GetFiles(backupDir, "config_backup_*.json")
                    .OrderByDescending(f => f)
                    .ToArray();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting available backups: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Raises the ConfigurationChanged event.
        /// </summary>
        protected virtual void OnConfigurationChanged(ConfigurationChangedEventArgs e)
        {
            ConfigurationChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the ConfigurationValidating event.
        /// </summary>
        protected virtual void OnConfigurationValidating(ConfigurationValidationEventArgs e)
        {
            ConfigurationValidating?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the ConfigurationBackupCompleted event.
        /// </summary>
        protected virtual void OnConfigurationBackupCompleted(ConfigurationBackupEventArgs e)
        {
            ConfigurationBackupCompleted?.Invoke(this, e);
        }
    }
} 