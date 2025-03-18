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
        private bool isTransitioningMode = false;
        #endregion

        #region Properties
        public bool IsEnabled { get; private set; }
        public bool IsJitterEnabled { get; private set; }
        public bool IsAlwaysJitterMode { get; private set; }
        public bool IsAlwaysRecoilReductionMode { get; private set; }

        /// <summary>
        /// Gets the current jitter strength value.
        /// </summary>
        public int JitterStrength => jitterStrength;

        /// <summary>
        /// Gets the current recoil reduction strength value.
        /// </summary>
        public int RecoilReductionStrength => recoilReductionStrength;
        #endregion

        #region Constructor
        public MacroManager()
        {
            try
            {
                inputSimulator = new InputSimulator();
                jitterManager = new JitterManager(inputSimulator);
                recoilManager = new RecoilReductionManager(inputSimulator);

                // Subscribe to state change events
                jitterManager.StateChanged += OnEffectStateChanged;
                recoilManager.StateChanged += OnEffectStateChanged;
            }
            catch (Exception)
            {
                // Clean up if initialization fails
                Dispose(true);
                throw;
            }
        }
        #endregion

        #region Public Methods
        public void HandleMouseButton(MouseButtons button, bool isDown)
        {
            ThrowIfDisposed();

            lock (lockObject)
            {
                try
                {
                    bool stateChanged = false;
                    if (button == MouseButtons.Left && leftButtonDown != isDown)
                    {
                        leftButtonDown = isDown;
                        stateChanged = true;
                    }
                    else if (button == MouseButtons.Right && rightButtonDown != isDown)
                    {
                        rightButtonDown = isDown;
                        stateChanged = true;
                    }

                    if (stateChanged)
                    {
                        CheckMacroState();
                    }
                }
                catch (Exception)
                {
                    // Reset button states on error
                    leftButtonDown = false;
                    rightButtonDown = false;
                    StopAllEffects();
                    throw;
                }
            }
        }

        public void ToggleMacro()
        {
            ThrowIfDisposed();

            lock (lockObject)
            {
                try
                {
                    IsEnabled = !IsEnabled;
                    MacroStateChanged?.Invoke(this, IsEnabled);
                    
                    if (!IsEnabled)
                    {
                        // Ensure cleanup when disabling
                        leftButtonDown = false;
                        rightButtonDown = false;
                    }
                    
                    CheckMacroState();
                }
                catch (Exception)
                {
                    // Reset state on error
                    IsEnabled = false;
                    StopAllEffects();
                    throw;
                }
            }
        }

        public void SwitchMode()
        {
            ThrowIfDisposed();

            lock (lockObject)
            {
                if (isTransitioningMode) return;
                
                try
                {
                    isTransitioningMode = true;
                    
                    if (!IsAlwaysJitterMode && !IsAlwaysRecoilReductionMode)
                    {
                        bool previousState = IsJitterEnabled;
                        IsJitterEnabled = !IsJitterEnabled;
                        
                        // Only trigger events and state check if the state actually changed
                        if (previousState != IsJitterEnabled)
                        {
                            ModeChanged?.Invoke(this, IsJitterEnabled);
                            CheckMacroState();
                        }
                    }
                }
                finally
                {
                    isTransitioningMode = false;
                }
            }
        }

        public void SetJitterStrength(int strength)
        {
            ThrowIfDisposed();

            if (strength < 1 || strength > 20)
                throw new ArgumentOutOfRangeException(nameof(strength), "Strength must be between 1 and 20.");

            lock (lockObject)
            {
                jitterStrength = strength;
                jitterManager.SetStrength(strength);
            }
        }

        public void SetRecoilReductionStrength(int strength)
        {
            ThrowIfDisposed();

            if (strength < 1 || strength > 20)
                throw new ArgumentOutOfRangeException(nameof(strength), "Strength must be between 1 and 20.");

            lock (lockObject)
            {
                recoilReductionStrength = strength;
                recoilManager.SetStrength(strength);
            }
        }

        public void SetAlwaysJitterMode(bool enabled)
        {
            ThrowIfDisposed();

            lock (lockObject)
            {
                if (isTransitioningMode) return;
                
                try
                {
                    isTransitioningMode = true;
                    
                    bool stateChanged = false;
                    
                    if (enabled && IsAlwaysRecoilReductionMode)
                    {
                        IsAlwaysRecoilReductionMode = false;
                        stateChanged = true;
                    }
                    
                    if (IsAlwaysJitterMode != enabled)
                    {
                        IsAlwaysJitterMode = enabled;
                        stateChanged = true;
                    }
                    
                    if (enabled && !IsJitterEnabled)
                    {
                        IsJitterEnabled = true;
                        stateChanged = true;
                    }
                    
                    if (stateChanged)
                    {
                        ModeChanged?.Invoke(this, IsJitterEnabled);
                        CheckMacroState();
                    }
                }
                finally
                {
                    isTransitioningMode = false;
                }
            }
        }

        public void SetAlwaysRecoilReductionMode(bool enabled)
        {
            ThrowIfDisposed();

            lock (lockObject)
            {
                if (isTransitioningMode) return;
                
                try
                {
                    isTransitioningMode = true;
                    
                    bool stateChanged = false;
                    
                    if (enabled && IsAlwaysJitterMode)
                    {
                        IsAlwaysJitterMode = false;
                        stateChanged = true;
                    }
                    
                    if (IsAlwaysRecoilReductionMode != enabled)
                    {
                        IsAlwaysRecoilReductionMode = enabled;
                        stateChanged = true;
                    }
                    
                    if (enabled && IsJitterEnabled)
                    {
                        IsJitterEnabled = false;
                        stateChanged = true;
                    }
                    
                    if (stateChanged)
                    {
                        ModeChanged?.Invoke(this, IsJitterEnabled);
                        CheckMacroState();
                    }
                }
                finally
                {
                    isTransitioningMode = false;
                }
            }
        }
        #endregion

        #region Private Methods
        private void ThrowIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(MacroManager));
        }

        private void OnEffectStateChanged(object sender, bool active)
        {
            lock (lockObject)
            {
                CheckMacroState();
            }
        }

        private void StopAllEffects()
        {
            // Ensure we're in a locked context
            if (!Monitor.IsEntered(lockObject))
                throw new InvalidOperationException("StopAllEffects must be called within a lock");

            try
            {
                if (jitterManager.IsActive)
                {
                    JitterStopped?.Invoke(this, EventArgs.Empty);
                    jitterManager.Stop();
                }
                if (recoilManager.IsActive)
                {
                    RecoilReductionStopped?.Invoke(this, EventArgs.Empty);
                    recoilManager.Stop();
                }
            }
            finally
            {
                isActive = false;
            }
        }

        private void CheckMacroState()
        {
            // Ensure we're in a locked context
            if (!Monitor.IsEntered(lockObject))
                throw new InvalidOperationException("CheckMacroState must be called within a lock");

            try
            {
                bool shouldActivate = IsEnabled && leftButtonDown && rightButtonDown;

                // Always stop effects if we shouldn't be active
                if (!shouldActivate)
                {
                    StopAllEffects();
                    return;
                }

                // Determine which mode should be active
                bool shouldUseJitter = IsAlwaysJitterMode || (!IsAlwaysRecoilReductionMode && IsJitterEnabled);

                // If we're already active but in the wrong mode, stop all effects first
                if (isActive && (jitterManager.IsActive != shouldUseJitter || recoilManager.IsActive == shouldUseJitter))
                {
                    StopAllEffects();
                }

                // Start the appropriate effect if we're not active
                if (!isActive)
                {
                    try
                    {
                        if (shouldUseJitter)
                        {
                            JitterStarted?.Invoke(this, EventArgs.Empty);
                            jitterManager.Start();
                        }
                        else
                        {
                            RecoilReductionStarted?.Invoke(this, EventArgs.Empty);
                            recoilManager.Start();
                        }
                        isActive = true;
                    }
                    catch (Exception)
                    {
                        StopAllEffects();
                        throw;
                    }
                }
            }
            catch (Exception)
            {
                // Reset to a safe state on any error
                isActive = false;
                leftButtonDown = false;
                rightButtonDown = false;
                throw;
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
                    lock (lockObject)
                    {
                        try
                        {
                            // Stop all effects first
                            StopAllEffects();

                            // Unsubscribe from events
                            if (jitterManager != null)
                            {
                                jitterManager.StateChanged -= OnEffectStateChanged;
                                jitterManager.Dispose();
                            }
                            if (recoilManager != null)
                            {
                                recoilManager.StateChanged -= OnEffectStateChanged;
                                recoilManager.Dispose();
                            }
                            inputSimulator?.Dispose();
                        }
                        catch
                        {
                            // Swallow exceptions in dispose
                        }
                    }
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