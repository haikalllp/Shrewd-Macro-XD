using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NotesAndTasks.Models
{
    /// <summary>
    /// Configuration settings for UI functionality
    /// </summary>
    public class UISettings : INotifyPropertyChanged
    {
        private bool _minimizeToTray;
        private bool _startMinimized;
        private bool _showStatusInTitle = true;
        private bool _showTrayNotifications = true;

        /// <summary>
        /// Gets or sets whether the application should minimize to tray
        /// </summary>
        public bool MinimizeToTray
        {
            get => _minimizeToTray;
            set
            {
                if (_minimizeToTray != value)
                {
                    _minimizeToTray = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the application should start minimized
        /// </summary>
        public bool StartMinimized
        {
            get => _startMinimized;
            set
            {
                if (_startMinimized != value)
                {
                    _startMinimized = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to show status in window title
        /// </summary>
        public bool ShowStatusInTitle
        {
            get => _showStatusInTitle;
            set
            {
                if (_showStatusInTitle != value)
                {
                    _showStatusInTitle = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to show tray notifications
        /// </summary>
        public bool ShowTrayNotifications
        {
            get => _showTrayNotifications;
            set
            {
                if (_showTrayNotifications != value)
                {
                    _showTrayNotifications = value;
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