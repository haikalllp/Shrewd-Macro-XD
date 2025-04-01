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
        /// <exception cref="ArgumentNullException">Thrown if control or manager is null.</exception>
        public static T RegisterEvents<T>(this T control, EventHandlerManager manager) where T : Control
        {
            try
            {
                if (control == null)
                {
                    throw new ArgumentNullException(nameof(control), "Control cannot be null when registering events");
                }

                if (manager == null)
                {
                    throw new ArgumentNullException(nameof(manager), "EventHandlerManager cannot be null when registering events");
                }

                manager.RegisterControlEvents(control);
                System.Diagnostics.Debug.WriteLine($"Successfully registered events for {typeof(T).Name}");
                return control;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RegisterEvents: {ex.Message}");
                // Re-throw to allow callers to handle the exception
                throw;
            }
        }

        /// <summary>
        /// Unregisters all event handlers for a control from the event handler manager.
        /// </summary>
        /// <param name="control">The control to unregister events for.</param>
        /// <param name="manager">The event handler manager.</param>
        /// <returns>The control for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if control or manager is null.</exception>
        public static T UnregisterEvents<T>(this T control, EventHandlerManager manager) where T : Control
        {
            try
            {
                if (control == null)
                {
                    throw new ArgumentNullException(nameof(control), "Control cannot be null when unregistering events");
                }

                if (manager == null)
                {
                    throw new ArgumentNullException(nameof(manager), "EventHandlerManager cannot be null when unregistering events");
                }

                manager.UnregisterControlEvents(control);
                System.Diagnostics.Debug.WriteLine($"Successfully unregistered events for {typeof(T).Name}");
                return control;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UnregisterEvents: {ex.Message}");
                // Re-throw to allow callers to handle the exception
                throw;
            }
        }

        /// <summary>
        /// Registers a specific event handler with tracking.
        /// </summary>
        /// <param name="control">The control to register the event for.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="handler">The event handler delegate.</param>
        /// <param name="manager">The event handler manager.</param>
        /// <returns>The control for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if eventName is empty.</exception>
        public static T RegisterEventHandler<T>(this T control, string eventName, Delegate handler, EventHandlerManager manager) where T : Control
        {
            try
            {
                if (control == null)
                {
                    throw new ArgumentNullException(nameof(control), "Control cannot be null when registering event handler");
                }

                if (string.IsNullOrEmpty(eventName))
                {
                    throw new ArgumentNullException(nameof(eventName), "Event name cannot be null or empty");
                }

                if (handler == null)
                {
                    throw new ArgumentNullException(nameof(handler), "Event handler delegate cannot be null");
                }

                if (manager == null)
                {
                    throw new ArgumentNullException(nameof(manager), "EventHandlerManager cannot be null when registering event handler");
                }

                // Add the event handler to the manager's tracking
                var registerMethod = typeof(EventHandlerManager)
                    .GetMethod("RegisterEventHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (registerMethod == null)
                {
                    throw new InvalidOperationException("RegisterEventHandler method not found in EventHandlerManager");
                }

                registerMethod.Invoke(manager, new object[] { $"{control.Name}_{eventName}", handler });
                System.Diagnostics.Debug.WriteLine($"Successfully registered event handler for {control.Name}_{eventName}");
                
                return control;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RegisterEventHandler: {ex.Message}");
                // Re-throw to allow callers to handle the exception
                throw;
            }
        }

        /// <summary>
        /// Registers common event handlers for a TrackBar control.
        /// </summary>
        /// <param name="trackBar">The trackbar to register events for.</param>
        /// <param name="manager">The event handler manager.</param>
        /// <param name="onValueChanged">Optional callback for value changes.</param>
        /// <returns>The trackbar for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if trackBar or manager is null.</exception>
        public static TrackBar RegisterTrackBarEvents(this TrackBar trackBar, EventHandlerManager manager, Action<int> onValueChanged = null)
        {
            try
            {
                if (trackBar == null)
                {
                    throw new ArgumentNullException(nameof(trackBar), "TrackBar cannot be null when registering events");
                }

                if (manager == null)
                {
                    throw new ArgumentNullException(nameof(manager), "EventHandlerManager cannot be null when registering events");
                }

                if (onValueChanged != null)
                {
                    EventHandler valueChangedHandler = (s, e) => 
                    {
                        try
                        {
                            onValueChanged(trackBar.Value);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in TrackBar ValueChanged handler: {ex.Message}");
                        }
                    };
                    
                    trackBar.RegisterEventHandler("ValueChanged", valueChangedHandler, manager);
                    trackBar.ValueChanged += valueChangedHandler;
                    System.Diagnostics.Debug.WriteLine($"Registered ValueChanged handler for TrackBar: {trackBar.Name}");
                }
                return trackBar;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RegisterTrackBarEvents: {ex.Message}");
                // Re-throw to allow callers to handle the exception
                throw;
            }
        }

        /// <summary>
        /// Registers common event handlers for a CheckBox control.
        /// </summary>
        /// <param name="checkBox">The checkbox to register events for.</param>
        /// <param name="manager">The event handler manager.</param>
        /// <param name="onCheckedChanged">Optional callback for checked state changes.</param>
        /// <returns>The checkbox for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if checkBox or manager is null.</exception>
        public static CheckBox RegisterCheckBoxEvents(this CheckBox checkBox, EventHandlerManager manager, Action<bool> onCheckedChanged = null)
        {
            try
            {
                if (checkBox == null)
                {
                    throw new ArgumentNullException(nameof(checkBox), "CheckBox cannot be null when registering events");
                }

                if (manager == null)
                {
                    throw new ArgumentNullException(nameof(manager), "EventHandlerManager cannot be null when registering events");
                }

                if (onCheckedChanged != null)
                {
                    EventHandler checkedChangedHandler = (s, e) => 
                    {
                        try
                        {
                            onCheckedChanged(checkBox.Checked);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in CheckBox CheckedChanged handler: {ex.Message}");
                        }
                    };
                    
                    checkBox.RegisterEventHandler("CheckedChanged", checkedChangedHandler, manager);
                    checkBox.CheckedChanged += checkedChangedHandler;
                    System.Diagnostics.Debug.WriteLine($"Registered CheckedChanged handler for CheckBox: {checkBox.Name}");
                }
                return checkBox;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RegisterCheckBoxEvents: {ex.Message}");
                // Re-throw to allow callers to handle the exception
                throw;
            }
        }

        /// <summary>
        /// Registers common event handlers for a Button control.
        /// </summary>
        /// <param name="button">The button to register events for.</param>
        /// <param name="manager">The event handler manager.</param>
        /// <param name="onClick">Optional callback for click events.</param>
        /// <returns>The button for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if button or manager is null.</exception>
        public static Button RegisterButtonEvents(this Button button, EventHandlerManager manager, Action onClick = null)
        {
            try
            {
                if (button == null)
                {
                    throw new ArgumentNullException(nameof(button), "Button cannot be null when registering events");
                }

                if (manager == null)
                {
                    throw new ArgumentNullException(nameof(manager), "EventHandlerManager cannot be null when registering events");
                }

                if (onClick != null)
                {
                    EventHandler clickHandler = (s, e) => 
                    {
                        try
                        {
                            onClick();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in Button Click handler: {ex.Message}");
                        }
                    };
                    
                    button.RegisterEventHandler("Click", clickHandler, manager);
                    button.Click += clickHandler;
                    System.Diagnostics.Debug.WriteLine($"Registered Click handler for Button: {button.Name}");
                }
                return button;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RegisterButtonEvents: {ex.Message}");
                // Re-throw to allow callers to handle the exception
                throw;
            }
        }

        /// <summary>
        /// Registers common event handlers for a TextBox control.
        /// </summary>
        /// <param name="textBox">The textbox to register events for.</param>
        /// <param name="manager">The event handler manager.</param>
        /// <param name="onTextChanged">Optional callback for text changes.</param>
        /// <param name="onKeyDown">Optional callback for key down events.</param>
        /// <returns>The textbox for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if textBox or manager is null.</exception>
        public static TextBox RegisterTextBoxEvents(this TextBox textBox, EventHandlerManager manager, 
            Action<string> onTextChanged = null,
            Action<KeyEventArgs> onKeyDown = null)
        {
            try
            {
                if (textBox == null)
                {
                    throw new ArgumentNullException(nameof(textBox), "TextBox cannot be null when registering events");
                }

                if (manager == null)
                {
                    throw new ArgumentNullException(nameof(manager), "EventHandlerManager cannot be null when registering events");
                }

                if (onTextChanged != null)
                {
                    EventHandler textChangedHandler = (s, e) => 
                    {
                        try
                        {
                            onTextChanged(textBox.Text);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in TextBox TextChanged handler: {ex.Message}");
                        }
                    };
                    
                    textBox.RegisterEventHandler("TextChanged", textChangedHandler, manager);
                    textBox.TextChanged += textChangedHandler;
                    System.Diagnostics.Debug.WriteLine($"Registered TextChanged handler for TextBox: {textBox.Name}");
                }

                if (onKeyDown != null)
                {
                    KeyEventHandler keyDownHandler = (s, e) => 
                    {
                        try
                        {
                            onKeyDown(e);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in TextBox KeyDown handler: {ex.Message}");
                        }
                    };
                    
                    textBox.RegisterEventHandler("KeyDown", keyDownHandler, manager);
                    textBox.KeyDown += keyDownHandler;
                    System.Diagnostics.Debug.WriteLine($"Registered KeyDown handler for TextBox: {textBox.Name}");
                }

                return textBox;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RegisterTextBoxEvents: {ex.Message}");
                // Re-throw to allow callers to handle the exception
                throw;
            }
        }
    }
} 