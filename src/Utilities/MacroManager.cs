using System;
using NotesAndTasks.Hooks;

namespace NotesAndTasks.Utilities
{
    /// <summary>
    /// Coordinates between hook and feature managers to provide macro functionality.
    /// This class handles the overall macro state and mode switching.
    /// </summary>
    public class MacroManager : IDisposable
    {
        private readonly InputSimulator inputSimulator;
        private readonly JitterManager jitterManager;
        private readonly RecoilReductionManager recoilManager;
        private readonly object lockObject = new object();
        private bool disposed = false;
        private bool isEnabled = false;
        private bool jitterEnabled = false;
        private bool alwaysJitterMode = false;
        private bool alwaysRecoilReductionMode = false;
        private bool leftButtonDown = false;
        private bool rightButtonDown = false;

        /// <summary>
        /// Gets whether the macro is currently enabled.
        /// </summary>
        public bool IsEnabled => isEnabled;

        /// <summary>
        /// Gets whether jitter mode is currently enabled.
        /// </summary>
        public bool IsJitterEnabled => jitterEnabled;

        /// <summary>
        /// Gets whether always jitter mode is enabled.
        /// </summary>
        public bool IsAlwaysJitterMode => alwaysJitterMode;

        /// <summary>
        /// Gets whether always recoil reduction mode is enabled.
        /// </summary>
        public bool IsAlwaysRecoilReductionMode => alwaysRecoilReductionMode;

        /// <summary>
        /// Event raised when the macro state changes.
        /// </summary>
        public event EventHandler<bool> MacroStateChanged;

        /// <summary>
        /// Event raised when the mode changes.
        /// </summary>
        public event EventHandler<bool> ModeChanged;

        /// <summary>
        /// Initializes a new instance of the MacroManager class.
        /// </summary>
        public MacroManager()
        {
            inputSimulator = new InputSimulator();
            jitterManager = new JitterManager(inputSimulator);
            recoilManager = new RecoilReductionManager(inputSimulator);

            // Subscribe to state change events
            jitterManager.StateChanged += (s, active) => CheckAndUpdateState();
            recoilManager.StateChanged += (s, active) => CheckAndUpdateState();
        }

        /// <summary>
        /// Toggles the macro on/off state.
        /// </summary>
        public void ToggleMacro()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MacroManager));

            lock (lockObject)
            {
                isEnabled = !isEnabled;
                CheckAndUpdateState();
                MacroStateChanged?.Invoke(this, isEnabled);
            }
        }

        /// <summary>
        /// Switches between jitter and recoil reduction modes.
        /// Only works when neither "Always" mode is enabled.
        /// </summary>
        public void SwitchMode()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MacroManager));

            lock (lockObject)
            {
                if (alwaysJitterMode || alwaysRecoilReductionMode)
                    return;

                jitterEnabled = !jitterEnabled;
                CheckAndUpdateState();
                ModeChanged?.Invoke(this, jitterEnabled);
            }
        }

        /// <summary>
        /// Sets the jitter strength.
        /// </summary>
        public void SetJitterStrength(int strength)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MacroManager));

            jitterManager.Strength = strength;
        }

        /// <summary>
        /// Sets the recoil reduction strength.
        /// </summary>
        public void SetRecoilReductionStrength(int strength)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MacroManager));

            recoilManager.Strength = strength;
        }

        /// <summary>
        /// Sets the always jitter mode state.
        /// </summary>
        public void SetAlwaysJitterMode(bool enabled)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MacroManager));

            lock (lockObject)
            {
                alwaysJitterMode = enabled;
                if (enabled)
                {
                    alwaysRecoilReductionMode = false;
                    jitterEnabled = true;
                }
                CheckAndUpdateState();
            }
        }

        /// <summary>
        /// Sets the always recoil reduction mode state.
        /// </summary>
        public void SetAlwaysRecoilReductionMode(bool enabled)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MacroManager));

            lock (lockObject)
            {
                alwaysRecoilReductionMode = enabled;
                if (enabled)
                {
                    alwaysJitterMode = false;
                    jitterEnabled = false;
                }
                CheckAndUpdateState();
            }
        }

        /// <summary>
        /// Handles mouse button state changes.
        /// </summary>
        public void HandleMouseButton(MouseButtons button, bool isDown)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MacroManager));

            lock (lockObject)
            {
                switch (button)
                {
                    case MouseButtons.Left:
                        leftButtonDown = isDown;
                        break;
                    case MouseButtons.Right:
                        rightButtonDown = isDown;
                        break;
                }
                CheckAndUpdateState();
            }
        }

        /// <summary>
        /// Checks the current state and updates managers accordingly.
        /// </summary>
        private void CheckAndUpdateState()
        {
            if (!isEnabled || !(leftButtonDown && rightButtonDown))
            {
                jitterManager.Stop();
                recoilManager.Stop();
                return;
            }

            bool useJitter = alwaysJitterMode || (!alwaysRecoilReductionMode && jitterEnabled);

            if (useJitter)
            {
                recoilManager.Stop();
                jitterManager.Start();
            }
            else
            {
                jitterManager.Stop();
                recoilManager.Start();
            }
        }

        /// <summary>
        /// Disposes of resources used by the MacroManager.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                jitterManager.Dispose();
                recoilManager.Dispose();
                inputSimulator.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
} 