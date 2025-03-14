using System;
using System.Windows.Forms;

namespace NotesAndTasks.Configuration
{
    /// <summary>
    /// Provides extension methods for event handler registration.
    /// </summary>
    public static class EventHandlerExtensions
    {
        /// <summary>
        /// Registers all event handlers for a control with the event handler manager.
        /// </summary>
        /// <param name="control">The control to register events for.</param>
        /// <param name="manager">The event handler manager.</param>
        /// <returns>The control for method chaining.</returns>
        public static T RegisterEvents<T>(this T control, EventHandlerManager manager) where T : Control
        {
            manager.RegisterControlEvents(control);
            return control;
        }

        /// <summary>
        /// Unregisters all event handlers for a control from the event handler manager.
        /// </summary>
        /// <param name="control">The control to unregister events for.</param>
        /// <param name="manager">The event handler manager.</param>
        /// <returns>The control for method chaining.</returns>
        public static T UnregisterEvents<T>(this T control, EventHandlerManager manager) where T : Control
        {
            manager.UnregisterControlEvents(control);
            return control;
        }

        /// <summary>
        /// Registers a specific event handler with tracking.
        /// </summary>
        /// <param name="control">The control to register the event for.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="handler">The event handler delegate.</param>
        /// <param name="manager">The event handler manager.</param>
        /// <returns>The control for method chaining.</returns>
        public static T RegisterEventHandler<T>(this T control, string eventName, Delegate handler, EventHandlerManager manager) where T : Control
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            // Add the event handler to the manager's tracking
            typeof(EventHandlerManager)
                .GetMethod("RegisterEventHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(manager, new object[] { $"{control.Name}_{eventName}", handler });

            return control;
        }

        /// <summary>
        /// Registers common event handlers for a TrackBar control.
        /// </summary>
        public static TrackBar RegisterTrackBarEvents(this TrackBar trackBar, EventHandlerManager manager, Action<int> onValueChanged = null)
        {
            if (onValueChanged != null)
            {
                EventHandler valueChangedHandler = (s, e) => onValueChanged(trackBar.Value);
                trackBar.RegisterEventHandler("ValueChanged", valueChangedHandler, manager);
                trackBar.ValueChanged += valueChangedHandler;
            }
            return trackBar;
        }

        /// <summary>
        /// Registers common event handlers for a CheckBox control.
        /// </summary>
        public static CheckBox RegisterCheckBoxEvents(this CheckBox checkBox, EventHandlerManager manager, Action<bool> onCheckedChanged = null)
        {
            if (onCheckedChanged != null)
            {
                EventHandler checkedChangedHandler = (s, e) => onCheckedChanged(checkBox.Checked);
                checkBox.RegisterEventHandler("CheckedChanged", checkedChangedHandler, manager);
                checkBox.CheckedChanged += checkedChangedHandler;
            }
            return checkBox;
        }

        /// <summary>
        /// Registers common event handlers for a Button control.
        /// </summary>
        public static Button RegisterButtonEvents(this Button button, EventHandlerManager manager, Action onClick = null)
        {
            if (onClick != null)
            {
                EventHandler clickHandler = (s, e) => onClick();
                button.RegisterEventHandler("Click", clickHandler, manager);
                button.Click += clickHandler;
            }
            return button;
        }

        /// <summary>
        /// Registers common event handlers for a TextBox control.
        /// </summary>
        public static TextBox RegisterTextBoxEvents(this TextBox textBox, EventHandlerManager manager, 
            Action<string> onTextChanged = null,
            Action<KeyEventArgs> onKeyDown = null)
        {
            if (onTextChanged != null)
            {
                EventHandler textChangedHandler = (s, e) => onTextChanged(textBox.Text);
                textBox.RegisterEventHandler("TextChanged", textChangedHandler, manager);
                textBox.TextChanged += textChangedHandler;
            }

            if (onKeyDown != null)
            {
                KeyEventHandler keyDownHandler = (s, e) => onKeyDown(e);
                textBox.RegisterEventHandler("KeyDown", keyDownHandler, manager);
                textBox.KeyDown += keyDownHandler;
            }

            return textBox;
        }
    }
} 