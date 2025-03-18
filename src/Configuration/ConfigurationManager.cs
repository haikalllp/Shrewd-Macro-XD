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

        // Legacy configuration path for compatibility
        private static readonly string LegacyConfigPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "macro_config.json"
        );

        private readonly ReaderWriterLockSlim _configLock = new ReaderWriterLockSlim();
        private readonly JsonSerializerOptions _jsonOptions;
        private AppSettings _currentSettings;
        private bool _disposed;

        // Singleton instance
        private static ConfigurationManager _instance;
        private static readonly object _instanceLock = new object();

        public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;
        public event EventHandler<ConfigurationValidationEventArgs> ConfigurationValidating;
        public event EventHandler<ConfigurationBackupEventArgs> ConfigurationBackupCompleted;

        /// <summary>
        /// Gets the singleton instance of the ConfigurationManager
        /// </summary>
        public static ConfigurationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigurationManager();
                        }
                    }
                }
                return _instance;
            }
        }

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
        private ConfigurationManager()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            // Register custom converters
            _jsonOptions.Converters.Add(new InputBindingConverter());

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
                // Try new configuration path first
                if (File.Exists(ConfigPath))
                {
                    LoadConfigurationFromPath(ConfigPath);
                }
                // If new configuration doesn't exist, try to load from legacy path
                else if (File.Exists(LegacyConfigPath))
                {
                    MigrateLegacyConfiguration();
                }
                // If no configuration exists, create defaults
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
        /// Loads configuration from the specified file path
        /// </summary>
        private void LoadConfigurationFromPath(string path)
        {
            string jsonContent = File.ReadAllText(path);
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

        /// <summary>
        /// Migrates settings from the legacy configuration file to the new format
        /// </summary>
        private void MigrateLegacyConfiguration()
        {
            try
            {
                string jsonContent = File.ReadAllText(LegacyConfigPath);
                var legacySettings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(jsonContent);

                if (legacySettings != null)
                {
                    // Create new configuration from legacy settings
                    var settings = new AppSettings();
                    
                    // Macro settings
                    settings.MacroSettings.JitterStrength = legacySettings.JitterStrength;
                    settings.MacroSettings.RecoilReductionStrength = legacySettings.RecoilReductionStrength;
                    settings.MacroSettings.AlwaysJitterMode = legacySettings.AlwaysJitterMode;
                    settings.MacroSettings.AlwaysRecoilReductionMode = legacySettings.AlwaysRecoilReductionMode;
                    settings.MacroSettings.JitterEnabled = legacySettings.JitterEnabled;
                    settings.MacroSettings.RecoilReductionEnabled = legacySettings.RecoilReductionEnabled;
                    
                    // UI settings
                    settings.UISettings.MinimizeToTray = legacySettings.MinimizeToTray;
                    
                    // Hotkey settings
                    settings.HotkeySettings.MacroKey = new InputBinding(legacySettings.MacroKey, 
                        legacySettings.ToggleType == Utilities.ToggleType.Keyboard ? InputType.Keyboard : InputType.Mouse);
                    settings.HotkeySettings.SwitchKey = new InputBinding(legacySettings.SwitchKey, InputType.Keyboard);

                    _currentSettings = settings;
                    SaveConfiguration();
                    
                    // Log the migration
                    System.Diagnostics.Debug.WriteLine("Migrated settings from legacy format to new format");
                }
                else
                {
                    _currentSettings = CreateDefaultConfiguration();
                    SaveConfiguration();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to migrate legacy configuration: {ex.Message}");
                _currentSettings = CreateDefaultConfiguration();
                SaveConfiguration();
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
            
            // Set default values
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