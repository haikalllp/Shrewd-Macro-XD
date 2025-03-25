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
        private MacroSettings _macroSettings;
        private UISettings _uiSettings;
        private HotkeySettings _hotkeySettings;

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
        /// Gets or sets the macro-related settings including jitter and recoil reduction
        /// </summary>
        [JsonInclude]
        public MacroSettings MacroSettings
        { 
            get => _macroSettings;
            set
            {
                if (_macroSettings != value)
                {
                    _macroSettings = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the UI-related settings
        /// </summary>
        [JsonInclude]
        public UISettings UISettings
        { 
            get => _uiSettings;
            set
            {
                if (_uiSettings != value)
                {
                    _uiSettings = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the hotkey configuration settings
        /// </summary>
        [JsonInclude]
        public HotkeySettings HotkeySettings
        { 
            get => _hotkeySettings;
            set
            {
                if (_hotkeySettings != value)
                {
                    _hotkeySettings = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 