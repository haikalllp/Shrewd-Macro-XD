using System;
using System.IO;
using Newtonsoft.Json;

namespace NotesAndTasks
{
    public static class SettingsManager
    {
        private static readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "macro_config.json");
        public static Settings CurrentSettings { get; private set; }

        static SettingsManager()
        {
            try
            {
                LoadOrCreateSettings();
            }
            catch (Exception)
            {
                CurrentSettings = new Settings();
                try
                {
                    SaveSettings();
                }
                catch
                {
                    // If we can't save settings, at least we have defaults in memory
                }
            }
        }

        private static void LoadOrCreateSettings()
        {
            if (File.Exists(_configPath))
            {
                string jsonContent = File.ReadAllText(_configPath);
                CurrentSettings = JsonConvert.DeserializeObject<Settings>(jsonContent) ?? new Settings();
            }
            else
            {
                CurrentSettings = new Settings();
                SaveSettings();
            }
        }

        public static void SaveSettings()
        {
            try
            {
                string jsonContent = JsonConvert.SerializeObject(CurrentSettings, Formatting.Indented);
                File.WriteAllText(_configPath, jsonContent);
            }
            catch (Exception)
            {
                // Consider adding logging here if needed
            }
        }
    }
}
