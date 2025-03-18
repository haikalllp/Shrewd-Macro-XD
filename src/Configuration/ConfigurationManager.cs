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
    /// Manages application settings including loading, saving, validation, and backup
    /// </summary>
    public class ConfigurationManager : IDisposable
    {
        // Application directory for storing configuration
        private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
        
        // Settings file paths
        private static readonly string SettingsFilePath = Path.Combine(AppDirectory, "settings.json");
        private static readonly string SettingsBackupDirectoryPath = Path.Combine(AppDirectory, "Backups");
        private static readonly int MaxBackupCount = 5;

        // Legacy settings path for compatibility
        private static readonly string LegacySettingsFilePath = Path.Combine(AppDirectory, "macro_config.json");

        private readonly ReaderWriterLockSlim _configLock = new ReaderWriterLockSlim();
        private readonly JsonSerializerOptions _jsonOptions;
        private AppSettings _currentSettings;
        private bool _disposed;

        // Singleton instance
        private static ConfigurationManager _instance;
        private static readonly object _instanceLock = new object();

        public event EventHandler<SettingsChangedEventArgs> SettingsChanged;
        public event EventHandler<SettingsValidationEventArgs> SettingsValidating;
        public event EventHandler<SettingsBackupEventArgs> SettingsBackupCompleted;

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
            LoadSettings();
        }

        /// <summary>
        /// Loads the settings from disk, creating default settings if none exist
        /// </summary>
        public void LoadSettings()
        {
            _configLock.EnterWriteLock();
            try
            {
                // Try new settings path first
                if (File.Exists(SettingsFilePath))
                {
                    LoadSettingsFromPath(SettingsFilePath);
                }
                // If new settings don't exist, try to load from legacy path
                else if (File.Exists(LegacySettingsFilePath))
                {
                    MigrateLegacySettings();
                }
                // If no settings exist, create defaults
                else
                {
                    _currentSettings = CreateDefaultSettings();
                    SaveSettings(); // Save default settings
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
                if (_currentSettings == null)
                {
                    _currentSettings = CreateDefaultSettings();
                }
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Loads settings from the specified file path
        /// </summary>
        private void LoadSettingsFromPath(string path)
        {
            string jsonContent = File.ReadAllText(path);
            var loadedSettings = JsonSerializer.Deserialize<AppSettings>(jsonContent, _jsonOptions);

            var validationArgs = new SettingsValidationEventArgs(loadedSettings);
            SettingsValidating?.Invoke(this, validationArgs);

            if (validationArgs.IsValid && ValidateSettings(loadedSettings))
            {
                var previousSettings = _currentSettings;
                _currentSettings = loadedSettings;
                OnSettingsChanged("All", previousSettings, _currentSettings);
            }
            else
            {
                _currentSettings = CreateDefaultSettings();
                SaveSettings(); // Save default settings
            }
        }

        /// <summary>
        /// Migrates settings from the legacy settings file to the new format
        /// </summary>
        private void MigrateLegacySettings()
        {
            try
            {
                string jsonContent = File.ReadAllText(LegacySettingsFilePath);
                // Use dynamic to avoid direct dependency on the Settings class that no longer exists
                dynamic legacySettings = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonContent);

                if (legacySettings != null)
                {
                    // Create new settings from legacy settings
                    var settings = new AppSettings();
                    
                    try
                    {
                        // Macro settings
                        settings.MacroSettings.JitterStrength = legacySettings.JitterStrength;
                        settings.MacroSettings.RecoilReductionStrength = legacySettings.RecoilReductionStrength;
                        settings.MacroSettings.AlwaysJitterMode = legacySettings.AlwaysJitterMode;
                        settings.MacroSettings.AlwaysRecoilReductionMode = legacySettings.AlwaysRecoilReductionMode;
                        settings.MacroSettings.JitterEnabled = legacySettings.JitterEnabled;
                        settings.MacroSettings.RecoilReductionEnabled = legacySettings.RecoilReductionEnabled;
                        
                        // UI settings
                        settings.UISettings.MinimizeToTray = legacySettings.MinimizeToTray;
                        
                        // Hotkey settings - handle with care since we need to determine the type
                        var macroKeyValue = (int)legacySettings.MacroKey;
                        var switchKeyValue = (int)legacySettings.SwitchKey;
                        var toggleTypeValue = (int)legacySettings.ToggleType;
                        
                        // Set to keyboard by default (0 in the enum)
                        var inputType = InputType.Keyboard;
                        if (toggleTypeValue != 0) // Not keyboard type
                        {
                            inputType = InputType.Mouse;
                        }
                        
                        settings.HotkeySettings.MacroKey = new InputBinding((Keys)macroKeyValue, inputType);
                        settings.HotkeySettings.SwitchKey = new InputBinding((Keys)switchKeyValue, InputType.Keyboard);

                        _currentSettings = settings;
                        SaveSettings();
                        
                        // Log the migration
                        System.Diagnostics.Debug.WriteLine("Migrated settings from legacy format to new format");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error during settings migration: {ex.Message}");
                        _currentSettings = CreateDefaultSettings();
                        SaveSettings();
                    }
                }
                else
                {
                    _currentSettings = CreateDefaultSettings();
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to migrate legacy settings: {ex.Message}");
                _currentSettings = CreateDefaultSettings();
                SaveSettings();
            }
        }

        /// <summary>
        /// Saves the current settings to disk and creates a backup
        /// </summary>
        public void SaveSettings()
        {
            _configLock.EnterWriteLock();
            try
            {
                var validationArgs = new SettingsValidationEventArgs(_currentSettings);
                SettingsValidating?.Invoke(this, validationArgs);

                if (!validationArgs.IsValid || !ValidateSettings(_currentSettings))
                {
                    throw new Exception(validationArgs.Message ?? "Invalid settings state");
                }

                CreateSettingsBackup();

                string jsonContent = JsonSerializer.Serialize(_currentSettings, _jsonOptions);
                File.WriteAllText(SettingsFilePath, jsonContent);

                OnSettingsChanged("All", _currentSettings, _currentSettings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }

        private void OnSettingsChanged(string section, AppSettings previousSettings, AppSettings newSettings)
        {
            var args = new SettingsChangedEventArgs(section, previousSettings, newSettings);
            SettingsChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Creates a backup of the current settings file
        /// </summary>
        private void CreateSettingsBackup()
        {
            if (!File.Exists(SettingsFilePath)) return;

            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFile = Path.Combine(SettingsBackupDirectoryPath, $"settings_{timestamp}.json");

                File.Copy(SettingsFilePath, backupFile, true);
                CleanupOldBackups();

                var args = new SettingsBackupEventArgs(backupFile, true);
                SettingsBackupCompleted?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                var args = new SettingsBackupEventArgs(null, false, ex.Message);
                SettingsBackupCompleted?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Removes old backup files exceeding the maximum count
        /// </summary>
        private void CleanupOldBackups()
        {
            var backupFiles = Directory.GetFiles(SettingsBackupDirectoryPath, "settings_*.json")
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
        /// Creates necessary directories for settings storage
        /// </summary>
        private void InitializeDirectories()
        {
            Directory.CreateDirectory(SettingsBackupDirectoryPath);
        }

        /// <summary>
        /// Creates a default settings with preset values
        /// </summary>
        private AppSettings CreateDefaultSettings()
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
        /// Validates the settings
        /// </summary>
        private bool ValidateSettings(AppSettings settings)
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