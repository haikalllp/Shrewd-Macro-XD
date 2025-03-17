using System;
using System.Windows.Forms;
using NotesAndTasks.Hooks;

namespace NotesAndTasks.Utilities
{
    /// <summary>
    /// Coordinates between hook and feature managers to provide macro functionality.
    /// This class handles the overall macro state and mode switching.
    /// </summary>
    public class MacroManager : IDisposable
    {
        #region Events
        public event EventHandler<bool> MacroStateChanged;
        public event EventHandler<bool> ModeChanged;
        public event EventHandler JitterStarted;
        public event EventHandler JitterStopped;
        public event EventHandler RecoilReductionStarted;
        public event EventHandler RecoilReductionStopped;
        #endregion

        #region Fields
        private readonly InputSimulator inputSimulator;
        private readonly JitterManager jitterManager;
        private readonly RecoilReductionManager recoilManager;
        private readonly object lockObject = new object();
        private bool disposed = false;
        private bool leftButtonDown = false;
        private bool rightButtonDown = false;
        private bool isActive = false;
        private int jitterStrength = 3;
        private int recoilReductionStrength = 1;
        #endregion

        #region Properties
        public bool IsEnabled { get; private set; }
        public bool IsJitterEnabled { get; private set; }
        public bool IsAlwaysJitterMode { get; private set; }
        public bool IsAlwaysRecoilReductionMode { get; private set; }
        #endregion

        #region Constructor
        public MacroManager()
        {
            inputSimulator = new InputSimulator();
            jitterManager = new JitterManager(inputSimulator);
            recoilManager = new RecoilReductionManager(inputSimulator);

            // Subscribe to state change events
            jitterManager.StateChanged += (s, active) => CheckMacroState();
            recoilManager.StateChanged += (s, active) => CheckMacroState();
        }
        #endregion

        #region Public Methods
        public void HandleMouseButton(MouseButtons button, bool isDown)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MacroManager));

            lock (lockObject)
            {
                if (button == MouseButtons.Left)
                {
                    leftButtonDown = isDown;
                }
                else if (button == MouseButtons.Right)
                {
                    rightButtonDown = isDown;
                }

                CheckMacroState();
            }
        }

        public void ToggleMacro()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MacroManager));

            lock (lockObject)
            {
                IsEnabled = !IsEnabled;
                MacroStateChanged?.Invoke(this, IsEnabled);
                CheckMacroState();
            }
        }

        public void SwitchMode()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MacroManager));

            lock (lockObject)
            {
                if (!IsAlwaysJitterMode && !IsAlwaysRecoilReductionMode)
                {
                    IsJitterEnabled = !IsJitterEnabled;
                    ModeChanged?.Invoke(this, IsJitterEnabled);
                    CheckMacroState();
                }
            }
        }

        public void SetJitterStrength(int strength)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MacroManager));

            if (strength >= 1 && strength <= 20)
            {
                jitterStrength = strength;
                jitterManager.SetStrength(strength);
            }
        }

        public void SetRecoilReductionStrength(int strength)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MacroManager));

            if (strength >= 1 && strength <= 20)
            {
                recoilReductionStrength = strength;
                recoilManager.SetStrength(strength);
            }
        }

        public void SetAlwaysJitterMode(bool enabled)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MacroManager));

            lock (lockObject)
            {
                if (enabled && IsAlwaysRecoilReductionMode)
                {
                    IsAlwaysRecoilReductionMode = false;
                }
                IsAlwaysJitterMode = enabled;
                if (enabled)
                {
                    IsJitterEnabled = true;
                }
                CheckMacroState();
                ModeChanged?.Invoke(this, IsJitterEnabled);
            }
        }

        public void SetAlwaysRecoilReductionMode(bool enabled)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MacroManager));

            lock (lockObject)
            {
                if (enabled && IsAlwaysJitterMode)
                {
                    IsAlwaysJitterMode = false;
                }
                IsAlwaysRecoilReductionMode = enabled;
                if (enabled)
                {
                    IsJitterEnabled = false;
                }
                CheckMacroState();
                ModeChanged?.Invoke(this, IsJitterEnabled);
            }
        }
        #endregion

        #region Private Methods
        private void CheckMacroState()
        {
            bool shouldActivate = IsEnabled && leftButtonDown && rightButtonDown;

            if (shouldActivate && !isActive)
            {
                isActive = true;
                if (IsAlwaysJitterMode || (!IsAlwaysRecoilReductionMode && IsJitterEnabled))
                {
                    JitterStarted?.Invoke(this, EventArgs.Empty);
                    jitterManager.Start();
                }
                else
                {
                    RecoilReductionStarted?.Invoke(this, EventArgs.Empty);
                    recoilManager.Start();
                }
            }
            else if (!shouldActivate && isActive)
            {
                isActive = false;
                if (IsAlwaysJitterMode || (!IsAlwaysRecoilReductionMode && IsJitterEnabled))
                {
                    JitterStopped?.Invoke(this, EventArgs.Empty);
                    jitterManager.Stop();
                }
                else
                {
                    RecoilReductionStopped?.Invoke(this, EventArgs.Empty);
                    recoilManager.Stop();
                }
            }
        }
        #endregion

        #region IDisposable Implementation
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    jitterManager.Dispose();
                    recoilManager.Dispose();
                    inputSimulator.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
} 