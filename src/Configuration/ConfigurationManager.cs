using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using Newtonsoft.Json;
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

        private static readonly string ConfigPath = Path.Combine(AppDataPath, "config.json");
        private static readonly string BackupPath = Path.Combine(AppDataPath, "Backups");
        private static readonly int MaxBackupCount = 5;

        private readonly ReaderWriterLockSlim _configLock = new ReaderWriterLockSlim();
        private readonly JsonSerializerSettings _jsonSettings;
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
            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
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
                    var loadedSettings = JsonConvert.DeserializeObject<AppSettings>(jsonContent, _jsonSettings);

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
                throw new ConfigurationException("Failed to load configuration", ex);
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
                    throw new ConfigurationException(validationArgs.Message ?? "Invalid configuration state");
                }

                CreateBackup();

                string jsonContent = JsonConvert.SerializeObject(_currentSettings, _jsonSettings);
                File.WriteAllText(ConfigPath, jsonContent);

                OnConfigurationChanged("All", _currentSettings, _currentSettings);
            }
            catch (Exception ex)
            {
                throw new ConfigurationException("Failed to save configuration", ex);
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
                string backupFile = Path.Combine(BackupPath, $"config_{timestamp}.json");

                File.Copy(ConfigPath, backupFile, true);
                CleanupOldBackups();

                var args = new ConfigurationBackupEventArgs(backupFile, true);
                ConfigurationBackupCompleted?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                var args = new ConfigurationBackupEventArgs(null, false, ex.Message);
                ConfigurationBackupCompleted?.Invoke(this, args);
                throw;
            }
        }

        /// <summary>
        /// Removes old backup files exceeding the maximum count
        /// </summary>
        private void CleanupOldBackups()
        {
            var backupFiles = Directory.GetFiles(BackupPath, "config_*.json")
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
            return new AppSettings();
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
                if (!IsValidHotkey(settings.HotkeySettings.MacroKey) ||
                    !IsValidHotkey(settings.HotkeySettings.SwitchKey) ||
                    settings.HotkeySettings.MacroKey == settings.HotkeySettings.SwitchKey)
                {
                    return false;
                }

                // Validate UISettings
                if (settings.UISettings.WindowSize.Width <= 0 || settings.UISettings.WindowSize.Height <= 0)
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

        /// <summary>
        /// Validates a hotkey value
        /// </summary>
        private bool IsValidHotkey(Keys key)
        {
            return key != Keys.None && key != Keys.LButton && key != Keys.RButton;
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

    /// <summary>
    /// Exception thrown when configuration operations fail
    /// </summary>
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message) : base(message) { }
        public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }
} 