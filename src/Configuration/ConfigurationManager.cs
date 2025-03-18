using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using NotesAndTasks.Models;

namespace NotesAndTasks.Configuration
{
    /// <summary>
    /// Manages application configuration including loading, saving, validation, and backup
    /// </summary>
    public class ConfigurationManager : IDisposable
    {
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NotesAndTasks"
        );

        private static readonly string ConfigPath = Path.Combine(AppDataPath, "settings.json");
        private static readonly string BackupPath = Path.Combine(AppDataPath, "Backups");
        private static readonly int MaxBackupCount = 5;

        private readonly ReaderWriterLockSlim _configLock = new ReaderWriterLockSlim();
        private readonly JsonSerializerOptions _jsonOptions;
        private AppSettings _currentSettings;
        private bool _disposed;

        public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;
        public event EventHandler<ConfigurationValidationEventArgs> ConfigurationValidating;
        public event EventHandler<ConfigurationBackupEventArgs> ConfigurationBackupCompleted;

        /// <summary>
        /// Gets the current application settings
        /// </summary>
        public AppSettings CurrentSettings
        {
            get
            {
                _configLock.EnterReadLock();
                try
                {
                    return _currentSettings;
                }
                finally
                {
                    _configLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the ConfigurationManager class
        /// </summary>
        public ConfigurationManager()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            InitializeDirectories();
            LoadConfiguration();
        }

        /// <summary>
        /// Loads the configuration from disk, creating default settings if none exist
        /// </summary>
        public void LoadConfiguration()
        {
            _configLock.EnterWriteLock();
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string jsonContent = File.ReadAllText(ConfigPath);
                    var loadedSettings = JsonSerializer.Deserialize<AppSettings>(jsonContent, _jsonOptions);

                    var validationArgs = new ConfigurationValidationEventArgs(loadedSettings);
                    ConfigurationValidating?.Invoke(this, validationArgs);

                    if (validationArgs.IsValid && ValidateConfiguration(loadedSettings))
                    {
                        var previousSettings = _currentSettings;
                        _currentSettings = loadedSettings;
                        OnConfigurationChanged("All", previousSettings, _currentSettings);
                    }
                    else
                    {
                        _currentSettings = CreateDefaultConfiguration();
                        SaveConfiguration(); // Save default configuration
                    }
                }
                else
                {
                    _currentSettings = CreateDefaultConfiguration();
                    SaveConfiguration(); // Save default configuration
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load configuration: {ex.Message}");
                if (_currentSettings == null)
                {
                    _currentSettings = CreateDefaultConfiguration();
                }
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Saves the current configuration to disk and creates a backup
        /// </summary>
        public void SaveConfiguration()
        {
            _configLock.EnterWriteLock();
            try
            {
                var validationArgs = new ConfigurationValidationEventArgs(_currentSettings);
                ConfigurationValidating?.Invoke(this, validationArgs);

                if (!validationArgs.IsValid || !ValidateConfiguration(_currentSettings))
                {
                    throw new Exception(validationArgs.Message ?? "Invalid configuration state");
                }

                CreateBackup();

                string jsonContent = JsonSerializer.Serialize(_currentSettings, _jsonOptions);
                File.WriteAllText(ConfigPath, jsonContent);

                OnConfigurationChanged("All", _currentSettings, _currentSettings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save configuration: {ex.Message}");
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        private void OnConfigurationChanged(string section, AppSettings previousConfig, AppSettings newConfig)
        {
            var args = new ConfigurationChangedEventArgs(section, previousConfig, newConfig);
            ConfigurationChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Creates a backup of the current configuration file
        /// </summary>
        private void CreateBackup()
        {
            if (!File.Exists(ConfigPath)) return;

            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFile = Path.Combine(BackupPath, $"settings_{timestamp}.json");

                File.Copy(ConfigPath, backupFile, true);
                CleanupOldBackups();

                var args = new ConfigurationBackupEventArgs(backupFile, true);
                ConfigurationBackupCompleted?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                var args = new ConfigurationBackupEventArgs(null, false, ex.Message);
                ConfigurationBackupCompleted?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Removes old backup files exceeding the maximum count
        /// </summary>
        private void CleanupOldBackups()
        {
            var backupFiles = Directory.GetFiles(BackupPath, "settings_*.json")
                                     .OrderByDescending(f => f)
                                     .Skip(MaxBackupCount);

            foreach (var file in backupFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    // Log but continue if deletion fails
                }
            }
        }

        /// <summary>
        /// Creates necessary directories for configuration storage
        /// </summary>
        private void InitializeDirectories()
        {
            Directory.CreateDirectory(AppDataPath);
            Directory.CreateDirectory(BackupPath);
        }

        /// <summary>
        /// Creates a default configuration with preset values
        /// </summary>
        private AppSettings CreateDefaultConfiguration()
        {
            var settings = new AppSettings();
            
            // Set default values as per KnownIssue.md
            settings.MacroSettings.JitterStrength = 3;
            settings.MacroSettings.RecoilReductionStrength = 1;
            settings.MacroSettings.AlwaysJitterMode = false;
            settings.MacroSettings.AlwaysRecoilReductionMode = false;
            settings.MacroSettings.JitterEnabled = false;
            settings.MacroSettings.RecoilReductionEnabled = false;
            settings.UISettings.MinimizeToTray = false;
            
            // Default hotkeys
            settings.HotkeySettings.MacroKey = new InputBinding(Keys.Capital, InputType.Keyboard);
            settings.HotkeySettings.SwitchKey = new InputBinding(Keys.Q, InputType.Keyboard);
            
            return settings;
        }

        /// <summary>
        /// Validates the configuration settings
        /// </summary>
        private bool ValidateConfiguration(AppSettings settings)
        {
            if (settings == null) return false;

            try
            {
                // Validate MacroSettings
                if (settings.MacroSettings.JitterStrength < 1 || settings.MacroSettings.JitterStrength > 20 ||
                    settings.MacroSettings.RecoilReductionStrength < 1 || settings.MacroSettings.RecoilReductionStrength > 20)
                {
                    return false;
                }

                // Validate HotkeySettings
                if (settings.HotkeySettings.MacroKey == null || settings.HotkeySettings.SwitchKey == null ||
                    settings.HotkeySettings.MacroKey.Key == Keys.None ||
                    settings.HotkeySettings.SwitchKey.Key == Keys.None)
                {
                    return false;
                }

                // Validate that keys are not the same
                if (settings.HotkeySettings.MacroKey.Key == settings.HotkeySettings.SwitchKey.Key &&
                    settings.HotkeySettings.MacroKey.Type == settings.HotkeySettings.SwitchKey.Type)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _configLock.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
} 