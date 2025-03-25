using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace NotesAndTasks.Models
{
    /// <summary>
    /// Root configuration class that contains all application settings
    /// </summary>
    public class AppSettings : INotifyPropertyChanged
    {
        private readonly MacroSettings _macroSettings;
        private readonly UISettings _uiSettings;
        private readonly HotkeySettings _hotkeySettings;

        /// <summary>
        /// Initializes a new instance of the AppSettings class with default settings
        /// </summary>
        public AppSettings()
        {
            _macroSettings = new MacroSettings();
            _uiSettings = new UISettings();
            _hotkeySettings = new HotkeySettings();
        }

        /// <summary>
        /// Gets the macro-related settings including jitter and recoil reduction
        /// </summary>
        [JsonInclude]
        public MacroSettings MacroSettings => _macroSettings;

        /// <summary>
        /// Gets the UI-related settings
        /// </summary>
        [JsonInclude]
        public UISettings UISettings => _uiSettings;

        /// <summary>
        /// Gets the hotkey configuration settings
        /// </summary>
        [JsonInclude]
        public HotkeySettings HotkeySettings => _hotkeySettings;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 