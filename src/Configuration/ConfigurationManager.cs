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
                System.Diagnostics.Debug.WriteLine("Starting LoadSettings process...");
                
                // Try to load settings from file
                if (File.Exists(SettingsFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Settings file found at: {SettingsFilePath}");
                    
                    string jsonContent = File.ReadAllText(SettingsFilePath);
                    System.Diagnostics.Debug.WriteLine($"Settings JSON: {jsonContent}");
                    
                    _currentSettings = null; // Force new instance
                    LoadSettingsFromPath(SettingsFilePath);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Settings file not found, creating defaults");
                    // If no settings exist, create defaults
                    _currentSettings = CreateDefaultSettings();
                    SaveSettings(); // Save default settings
                }

                // Validate settings after loading
                if (_currentSettings == null)
                {
                    System.Diagnostics.Debug.WriteLine("_currentSettings is null after loading, creating defaults");
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
                // Only proceed if the file exists
                if (!File.Exists(path))
                {
                    System.Diagnostics.Debug.WriteLine($"Settings file not found at: {path}");
                    return;
                }

                string jsonContent = File.ReadAllText(path);
                System.Diagnostics.Debug.WriteLine($"Loading settings from: {path}");
                
                // Create a fresh AppSettings instance - don't reuse existing
                var loadedSettings = JsonSerializer.Deserialize<AppSettings>(jsonContent, _jsonOptions);
                
                if (loadedSettings != null)
                {
                    System.Diagnostics.Debug.WriteLine("Settings deserialized successfully");
                    
                    // Debug loaded values
                    System.Diagnostics.Debug.WriteLine($"Loaded JitterStrength: {loadedSettings.MacroSettings.JitterStrength}");
                    System.Diagnostics.Debug.WriteLine($"Loaded RecoilReductionStrength: {loadedSettings.MacroSettings.RecoilReductionStrength}");
                    System.Diagnostics.Debug.WriteLine($"Loaded MinimizeToTray: {loadedSettings.UISettings.MinimizeToTray}");
                    System.Diagnostics.Debug.WriteLine($"Loaded MacroKey: {loadedSettings.HotkeySettings.MacroKey.Key}");
                    System.Diagnostics.Debug.WriteLine($"Loaded SwitchKey: {loadedSettings.HotkeySettings.SwitchKey.Key}");
                    
                    // Validate settings
                    if (!ValidateSettings(loadedSettings))
                    {
                        System.Diagnostics.Debug.WriteLine("Settings validation failed");
                        if (_currentSettings == null)
                        {
                            _currentSettings = CreateDefaultSettings();
                            SaveSettings();
                        }
                        return;
                    }
                    
                    // Store previous settings for change notification if needed
                    var previousSettings = _currentSettings;
                    
                    // Simply use the loaded settings directly
                    _currentSettings = loadedSettings;
                    
                    System.Diagnostics.Debug.WriteLine("Settings loaded and applied successfully");
                    
                    // Notify about settings changes
                    if (previousSettings != null)
                    {
                        OnSettingsChanged("All", previousSettings, _currentSettings);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed to deserialize settings file");
                    if (_currentSettings == null)
                    {
                        _currentSettings = CreateDefaultSettings();
                        SaveSettings();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                if (_currentSettings == null)
                {
                    _currentSettings = CreateDefaultSettings();
                    SaveSettings();
                }
            }
        }

        /// <summary>
        /// Copies settings from one instance to another
        /// </summary>
        private void CopySettings(AppSettings source, AppSettings target)
        {
            // Copy MacroSettings values
            target.MacroSettings.JitterStrength = source.MacroSettings.JitterStrength;
            target.MacroSettings.RecoilReductionStrength = source.MacroSettings.RecoilReductionStrength;
            target.MacroSettings.JitterEnabled = source.MacroSettings.JitterEnabled;
            target.MacroSettings.RecoilReductionEnabled = source.MacroSettings.RecoilReductionEnabled;
            target.MacroSettings.AlwaysJitterMode = source.MacroSettings.AlwaysJitterMode;
            target.MacroSettings.AlwaysRecoilReductionMode = source.MacroSettings.AlwaysRecoilReductionMode;

            // Copy UISettings values
            target.UISettings.MinimizeToTray = source.UISettings.MinimizeToTray;
            target.UISettings.ShowDebugPanel = source.UISettings.ShowDebugPanel;
            target.UISettings.ShowStatusInTitle = source.UISettings.ShowStatusInTitle;
            target.UISettings.ShowTrayNotifications = source.UISettings.ShowTrayNotifications;
            target.UISettings.WindowPosition = source.UISettings.WindowPosition;
            target.UISettings.WindowSize = source.UISettings.WindowSize;

            // Copy HotkeySettings values
            target.HotkeySettings.MacroKey.Key = source.HotkeySettings.MacroKey.Key;
            target.HotkeySettings.MacroKey.Type = source.HotkeySettings.MacroKey.Type;
            target.HotkeySettings.MacroKey.DisplayName = source.HotkeySettings.MacroKey.DisplayName;
            target.HotkeySettings.SwitchKey.Key = source.HotkeySettings.SwitchKey.Key;
            target.HotkeySettings.SwitchKey.Type = source.HotkeySettings.SwitchKey.Type;
            target.HotkeySettings.SwitchKey.DisplayName = source.HotkeySettings.SwitchKey.DisplayName;
        }

        /// <summary>
        /// Saves the current settings to disk and creates a backup
        /// </summary>
        public void SaveSettings()
        {
            _configLock.EnterWriteLock();
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting SaveSettings process...");
                
                if (_currentSettings == null)
                {
                    System.Diagnostics.Debug.WriteLine("_currentSettings is null, creating default settings");
                    _currentSettings = CreateDefaultSettings();
                }

                // Validate before saving
                if (!ValidateSettings(_currentSettings))
                {
                    System.Diagnostics.Debug.WriteLine("Settings validation failed, cannot save");
                    return;
                }

                CreateSettingsBackup();

                string jsonContent = JsonSerializer.Serialize(_currentSettings, _jsonOptions);
                System.Diagnostics.Debug.WriteLine($"Saving settings: {jsonContent}");
                File.WriteAllText(SettingsFilePath, jsonContent);

                System.Diagnostics.Debug.WriteLine("Settings saved successfully");
                
                // Don't call OnSettingsChanged here, since we're just saving existing settings
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
            if (settings == null)
            {
                System.Diagnostics.Debug.WriteLine("Settings validation failed: Settings object is null");
                return false;
            }

            try
            {
                // Let subscribers validate the settings first
                var validationArgs = new SettingsValidationEventArgs(settings);
                SettingsValidating?.Invoke(this, validationArgs);
                
                // If a subscriber has marked the settings as invalid, return false
                if (!validationArgs.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine($"Settings validation failed: {validationArgs.Message}");
                    return false;
                }

                // Validate MacroSettings existence
                if (settings.MacroSettings == null)
                {
                    System.Diagnostics.Debug.WriteLine("Settings validation failed: MacroSettings is null");
                    return false;
                }
                
                // Validate UISettings existence
                if (settings.UISettings == null)
                {
                    System.Diagnostics.Debug.WriteLine("Settings validation failed: UISettings is null");
                    return false;
                }
                
                // Validate HotkeySettings existence
                if (settings.HotkeySettings == null)
                {
                    System.Diagnostics.Debug.WriteLine("Settings validation failed: HotkeySettings is null");
                    return false;
                }

                // Validate MacroSettings values
                if (settings.MacroSettings.JitterStrength < 1 || settings.MacroSettings.JitterStrength > 20)
                {
                    System.Diagnostics.Debug.WriteLine($"Settings validation failed: Invalid JitterStrength value: {settings.MacroSettings.JitterStrength}");
                    return false;
                }
                
                if (settings.MacroSettings.RecoilReductionStrength < 1 || settings.MacroSettings.RecoilReductionStrength > 20)
                {
                    System.Diagnostics.Debug.WriteLine($"Settings validation failed: Invalid RecoilReductionStrength value: {settings.MacroSettings.RecoilReductionStrength}");
                    return false;
                }

                // Validate UISettings values
                if (settings.UISettings.WindowPosition.IsEmpty)
                {
                    System.Diagnostics.Debug.WriteLine("Settings validation failed: WindowPosition is empty");
                    return false;
                }
                
                if (settings.UISettings.WindowSize.IsEmpty || 
                    settings.UISettings.WindowSize.Width < 300 || 
                    settings.UISettings.WindowSize.Height < 200)
                {
                    System.Diagnostics.Debug.WriteLine($"Settings validation failed: Invalid WindowSize: {settings.UISettings.WindowSize}");
                    return false;
                }

                // Validate HotkeySettings values and nested objects
                if (settings.HotkeySettings.MacroKey == null)
                {
                    System.Diagnostics.Debug.WriteLine("Settings validation failed: MacroKey is null");
                    return false;
                }
                
                if (settings.HotkeySettings.SwitchKey == null)
                {
                    System.Diagnostics.Debug.WriteLine("Settings validation failed: SwitchKey is null");
                    return false;
                }
                
                if (settings.HotkeySettings.MacroKey.Key == Keys.None)
                {
                    System.Diagnostics.Debug.WriteLine("Settings validation failed: MacroKey.Key is None");
                    return false;
                }
                
                if (settings.HotkeySettings.SwitchKey.Key == Keys.None)
                {
                    System.Diagnostics.Debug.WriteLine("Settings validation failed: SwitchKey.Key is None");
                    return false;
                }

                // Validate that keys are not the same when they have the same input type
                if (settings.HotkeySettings.MacroKey.Key == settings.HotkeySettings.SwitchKey.Key &&
                    settings.HotkeySettings.MacroKey.Type == settings.HotkeySettings.SwitchKey.Type)
                {
                    System.Diagnostics.Debug.WriteLine("Settings validation failed: MacroKey and SwitchKey are the same");
                    return false;
                }
                
                // Validate DisplayName consistency
                if (string.IsNullOrEmpty(settings.HotkeySettings.MacroKey.DisplayName) ||
                    settings.HotkeySettings.MacroKey.DisplayName != settings.HotkeySettings.MacroKey.Key.ToString())
                {
                    System.Diagnostics.Debug.WriteLine("Settings validation failed: MacroKey DisplayName mismatch");
                    // Fix the display name rather than failing validation
                    settings.HotkeySettings.MacroKey.DisplayName = settings.HotkeySettings.MacroKey.Key.ToString();
                }
                
                if (string.IsNullOrEmpty(settings.HotkeySettings.SwitchKey.DisplayName) ||
                    settings.HotkeySettings.SwitchKey.DisplayName != settings.HotkeySettings.SwitchKey.Key.ToString())
                {
                    System.Diagnostics.Debug.WriteLine("Settings validation failed: SwitchKey DisplayName mismatch");
                    // Fix the display name rather than failing validation
                    settings.HotkeySettings.SwitchKey.DisplayName = settings.HotkeySettings.SwitchKey.Key.ToString();
                }

                // Validate MacroSettings combinations
                if (settings.MacroSettings.AlwaysJitterMode && settings.MacroSettings.AlwaysRecoilReductionMode)
                {
                    System.Diagnostics.Debug.WriteLine("Settings validation failed: Both AlwaysJitterMode and AlwaysRecoilReductionMode cannot be true simultaneously");
                    // Fix the conflict rather than failing validation
                    settings.MacroSettings.AlwaysJitterMode = true;
                    settings.MacroSettings.AlwaysRecoilReductionMode = false;
                }

                System.Diagnostics.Debug.WriteLine("Settings validation passed successfully");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception during settings validation: {ex.Message}");
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