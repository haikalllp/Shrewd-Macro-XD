using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using NotesAndTasks.Models;
using System.Text.Json.Serialization;

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
        private static readonly int MaxBackupAgeDays = 7; // Keep backups for 7 days maximum

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
                            try
                            {
                                _instance = new ConfigurationManager();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Failed to create ConfigurationManager instance: {ex.Message}");
                                throw;
                            }
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
            try
            {
                _jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IncludeFields = true,
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = ReferenceHandler.Preserve,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                // Register custom converters
                _jsonOptions.Converters.Add(new InputBindingConverter());

                InitializeDirectories();
                LoadSettings();

                // If settings are still null after LoadSettings, create defaults
                if (_currentSettings == null)
                {
                    _currentSettings = CreateDefaultSettings();
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize ConfigurationManager: {ex.Message}");
                // Create default settings even if initialization fails
                _currentSettings = CreateDefaultSettings();
                try
                {
                    SaveSettings();
                }
                catch
                {
                    // Ignore save errors during initialization
                }
            }
        }

        /// <summary>
        /// Loads the settings from disk, creating default settings if none exist
        /// </summary>
        public void LoadSettings()
        {
            _configLock.EnterWriteLock();
            try
            {
                // Try to load settings from file
                if (File.Exists(SettingsFilePath))
                {
                    LoadSettingsFromPath(SettingsFilePath);
                }
                else
                {
                    // If no settings exist, create defaults
                    _currentSettings = CreateDefaultSettings();
                    SaveSettings(); // Save default settings
                }

                // Validate settings after loading
                if (_currentSettings == null || !ValidateSettings(_currentSettings))
                {
                    System.Diagnostics.Debug.WriteLine("Invalid settings detected after loading, creating defaults");
                    _currentSettings = CreateDefaultSettings();
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
                // Create default settings if loading fails
                _currentSettings = CreateDefaultSettings();
                try
                {
                    SaveSettings();
                }
                catch
                {
                    // Ignore save errors during load failure recovery
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
            try
            {
                string jsonContent = File.ReadAllText(path);
                
                // Create a new instance if _currentSettings is null
                if (_currentSettings == null)
                {
                    _currentSettings = new AppSettings();
                }

                // Deserialize directly into the existing instance
                var loadedSettings = JsonSerializer.Deserialize<AppSettings>(jsonContent, _jsonOptions);

                // Copy values from loaded settings to existing instance
                if (loadedSettings != null)
                {
                    // Copy MacroSettings values
                    _currentSettings.MacroSettings.JitterStrength = loadedSettings.MacroSettings.JitterStrength;
                    _currentSettings.MacroSettings.RecoilReductionStrength = loadedSettings.MacroSettings.RecoilReductionStrength;
                    _currentSettings.MacroSettings.JitterEnabled = loadedSettings.MacroSettings.JitterEnabled;
                    _currentSettings.MacroSettings.RecoilReductionEnabled = loadedSettings.MacroSettings.RecoilReductionEnabled;
                    _currentSettings.MacroSettings.AlwaysJitterMode = loadedSettings.MacroSettings.AlwaysJitterMode;
                    _currentSettings.MacroSettings.AlwaysRecoilReductionMode = loadedSettings.MacroSettings.AlwaysRecoilReductionMode;

                    // Copy UISettings values
                    _currentSettings.UISettings.MinimizeToTray = loadedSettings.UISettings.MinimizeToTray;
                    _currentSettings.UISettings.ShowDebugPanel = loadedSettings.UISettings.ShowDebugPanel;
                    _currentSettings.UISettings.ShowStatusInTitle = loadedSettings.UISettings.ShowStatusInTitle;
                    _currentSettings.UISettings.ShowTrayNotifications = loadedSettings.UISettings.ShowTrayNotifications;
                    _currentSettings.UISettings.WindowPosition = loadedSettings.UISettings.WindowPosition;
                    _currentSettings.UISettings.WindowSize = loadedSettings.UISettings.WindowSize;

                    // Copy HotkeySettings values
                    _currentSettings.HotkeySettings.MacroKey.Key = loadedSettings.HotkeySettings.MacroKey.Key;
                    _currentSettings.HotkeySettings.MacroKey.Type = loadedSettings.HotkeySettings.MacroKey.Type;
                    _currentSettings.HotkeySettings.MacroKey.DisplayName = loadedSettings.HotkeySettings.MacroKey.DisplayName;
                    _currentSettings.HotkeySettings.SwitchKey.Key = loadedSettings.HotkeySettings.SwitchKey.Key;
                    _currentSettings.HotkeySettings.SwitchKey.Type = loadedSettings.HotkeySettings.SwitchKey.Type;
                    _currentSettings.HotkeySettings.SwitchKey.DisplayName = loadedSettings.HotkeySettings.SwitchKey.DisplayName;
                }

                var validationArgs = new SettingsValidationEventArgs(_currentSettings);
                SettingsValidating?.Invoke(this, validationArgs);

                if (!validationArgs.IsValid || !ValidateSettings(_currentSettings))
                {
                    System.Diagnostics.Debug.WriteLine("Settings validation failed, creating defaults");
                    _currentSettings = CreateDefaultSettings();
                    SaveSettings();
                }
                else
                {
                    // Notify that settings have changed
                    OnSettingsChanged("All", _currentSettings, _currentSettings);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
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
        /// Removes old backup files exceeding the maximum count or age
        /// </summary>
        private void CleanupOldBackups()
        {
            try
            {
                var backupFiles = Directory.GetFiles(SettingsBackupDirectoryPath, "settings_*.json")
                                         .Select(f => new FileInfo(f))
                                         .OrderByDescending(f => f.LastWriteTime)
                                         .ToList();

                // Remove files exceeding count limit
                var filesToDeleteCount = backupFiles.Skip(MaxBackupCount).ToList();
                
                // Remove files exceeding age limit
                var cutoffDate = DateTime.Now.AddDays(-MaxBackupAgeDays);
                var filesToDeleteAge = backupFiles.Where(f => f.LastWriteTime < cutoffDate).ToList();
                
                // Combine both lists and remove duplicates
                var filesToDelete = filesToDeleteCount.Union(filesToDeleteAge);

                foreach (var file in filesToDelete)
                {
                    try
                    {
                        file.Delete();
                        System.Diagnostics.Debug.WriteLine($"Deleted old backup: {file.Name}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to delete backup {file.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to cleanup backups: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates necessary directories for settings storage
        /// </summary>
        private void InitializeDirectories()
        {
            try
            {
                // Create application directory if it doesn't exist
                if (!Directory.Exists(AppDirectory))
                {
                    Directory.CreateDirectory(AppDirectory);
                }

                // Create backup directory if it doesn't exist
                if (!Directory.Exists(SettingsBackupDirectoryPath))
                {
                    Directory.CreateDirectory(SettingsBackupDirectoryPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize directories: {ex.Message}");
                throw; // Rethrow to handle in constructor
            }
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
            settings.UISettings.ShowStatusInTitle = true;
            settings.UISettings.ShowTrayNotifications = true;
            settings.UISettings.WindowPosition = new System.Drawing.Point(100, 100);
            settings.UISettings.WindowSize = new System.Drawing.Size(800, 600);
            
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