using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NotesAndTasks.Models
{
    /// <summary>
    /// Root configuration class that contains all application settings
    /// </summary>
    public class AppSettings : INotifyPropertyChanged
    {
        private MacroSettings _macroSettings = new();
        private UISettings _uiSettings = new();
        private HotkeySettings _hotkeySettings = new();

        /// <summary>
        /// Gets or sets the macro-specific settings
        /// </summary>
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