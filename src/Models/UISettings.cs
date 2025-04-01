using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace NotesAndTasks.Models
{
    /// <summary>
    /// Configuration settings for UI functionality and appearance
    /// </summary>
    public class UISettings : INotifyPropertyChanged
    {
        private bool _minimizeToTray = false;
        private bool _showDebugPanel;
        private Point _windowPosition = new Point(100, 100);
        private Size _windowSize = new Size(800, 600);
        private bool _showStatusInTitle = true;
        private bool _showTrayNotifications = true;

        /// <summary>
        /// Gets or sets whether the application should minimize to tray when closing
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
        /// Gets or sets whether to show the debug information panel
        /// </summary>
        public bool ShowDebugPanel
        {
            get => _showDebugPanel;
            set
            {
                if (_showDebugPanel != value)
                {
                    _showDebugPanel = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to show status information in the window title
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
        /// Gets or sets whether to show system tray notifications
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

        /// <summary>
        /// Gets or sets the main window position
        /// </summary>
        public Point WindowPosition
        {
            get => _windowPosition;
            set
            {
                if (_windowPosition != value)
                {
                    _windowPosition = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the main window size
        /// </summary>
        public Size WindowSize
        {
            get => _windowSize;
            set
            {
                if (_windowSize != value)
                {
                    _windowSize = value;
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