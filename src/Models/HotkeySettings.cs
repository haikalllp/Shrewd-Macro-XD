using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NotesAndTasks.Models
{
    /// <summary>
    /// Settings related to keyboard shortcuts and hotkeys
    /// </summary>
    public class HotkeySettings : INotifyPropertyChanged
    {
        private InputBinding _macroKey;
        private InputBinding _switchKey;

        /// <summary>
        /// Initializes a new instance of the HotkeySettings class with default values
        /// </summary>
        public HotkeySettings()
        {
            _macroKey = new InputBinding(Keys.Capital, InputType.Keyboard);
            _switchKey = new InputBinding(Keys.Q, InputType.Keyboard);
        }

        /// <summary>
        /// Gets or sets the key used to toggle the macro
        /// </summary>
        public InputBinding MacroKey
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
        /// Gets or sets the key used to switch between modes
        /// </summary>
        public InputBinding SwitchKey
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