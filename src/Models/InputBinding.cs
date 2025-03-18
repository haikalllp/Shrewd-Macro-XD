using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NotesAndTasks.Models
{
    /// <summary>
    /// Enumeration of input types
    /// </summary>
    public enum InputType
    {
        /// <summary>
        /// Keyboard input
        /// </summary>
        Keyboard,
        
        /// <summary>
        /// Mouse input
        /// </summary>
        Mouse
    }

    /// <summary>
    /// Represents a keyboard or mouse binding for a command
    /// </summary>
    public class InputBinding : INotifyPropertyChanged
    {
        private Keys _key;
        private InputType _type;
        private string _displayName;

        /// <summary>
        /// Gets or sets the key code
        /// </summary>
        public Keys Key
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    DisplayName = _key.ToString();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the input type (keyboard or mouse)
        /// </summary>
        public InputType Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the display name for the key
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the InputBinding class
        /// </summary>
        public InputBinding()
        {
            _key = Keys.None;
            _type = InputType.Keyboard;
            _displayName = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the InputBinding class with specified key and type
        /// </summary>
        /// <param name="key">The key code</param>
        /// <param name="type">The input type</param>
        public InputBinding(Keys key, InputType type)
        {
            _key = key;
            _type = type;
            _displayName = key.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 