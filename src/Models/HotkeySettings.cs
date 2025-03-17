using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NotesAndTasks.Models
{
    /// <summary>
    /// Configuration settings for hotkeys and key bindings
    /// </summary>
    public class HotkeySettings : INotifyPropertyChanged
    {
        private Keys _macroKey = Keys.Capital;  // Default to Caps Lock
        private Keys _switchKey = Keys.Q;       // Default to Q

        /// <summary>
        /// Gets or sets the key for toggling macro functionality
        /// </summary>
        public Keys MacroKey
        {
            get => _macroKey;
            set
            {
                if (_macroKey != value)
                {
                    _macroKey = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the key for switching macro modes
        /// </summary>
        public Keys SwitchKey
        {
            get => _switchKey;
            set
            {
                if (_switchKey != value)
                {
                    _switchKey = value;
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