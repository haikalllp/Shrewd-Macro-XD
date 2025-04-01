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
                    // Validate input - only handle left or right mouse buttons
                    if (button != MouseButtons.Left && button != MouseButtons.Right)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ignoring unhandled mouse button: {button}");
                        return;
                    }

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
                catch (Exception ex)
                {
                    // Reset button states on error
                    System.Diagnostics.Debug.WriteLine($"Error in HandleMouseButton: {ex.Message}");
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
                    System.Diagnostics.Debug.WriteLine($"Macro toggled: {(IsEnabled ? "enabled" : "disabled")}");
                    MacroStateChanged?.Invoke(this, IsEnabled);
                    
                    if (!IsEnabled)
                    {
                        // Ensure cleanup when disabling
                        leftButtonDown = false;
                        rightButtonDown = false;
                    }
                    
                    CheckMacroState();
                }
                catch (Exception ex)
                {
                    // Reset state on error
                    System.Diagnostics.Debug.WriteLine($"Error in ToggleMacro: {ex.Message}");
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
                // Prevent concurrent mode changes
                if (isTransitioningMode)
                {
                    System.Diagnostics.Debug.WriteLine("Mode switch ignored: another mode transition is in progress");
                    return;
                }
                
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
                            System.Diagnostics.Debug.WriteLine($"Mode switched to: {(IsJitterEnabled ? "Jitter" : "Recoil Reduction")}");
                            ModeChanged?.Invoke(this, IsJitterEnabled);
                            CheckMacroState();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Mode switch had no effect: mode was already in target state");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Mode switch ignored: in always-on mode");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in SwitchMode: {ex.Message}");
                    throw;
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

            // Validate input with better error message and handling
            if (strength < 1 || strength > 20)
            {
                string errorMsg = $"Invalid jitter strength: {strength}. Must be between 1 and 20.";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                throw new ArgumentOutOfRangeException(nameof(strength), strength, errorMsg);
            }

            lock (lockObject)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Setting jitter strength to: {strength}");
                    jitterStrength = strength;
                    
                    if (jitterManager == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Warning: JitterManager is null, cannot set strength");
                        return;
                    }
                    
                    jitterManager.SetStrength(strength);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in SetJitterStrength: {ex.Message}");
                    throw;
                }
            }
        }

        public void SetRecoilReductionStrength(int strength)
        {
            ThrowIfDisposed();

            // Validate input with better error message and handling
            if (strength < 1 || strength > 20)
            {
                string errorMsg = $"Invalid recoil reduction strength: {strength}. Must be between 1 and 20.";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                throw new ArgumentOutOfRangeException(nameof(strength), strength, errorMsg);
            }

            lock (lockObject)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Setting recoil reduction strength to: {strength}");
                    recoilReductionStrength = strength;
                    
                    if (recoilManager == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Warning: RecoilManager is null, cannot set strength");
                        return;
                    }
                    
                    recoilManager.SetStrength(strength);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in SetRecoilReductionStrength: {ex.Message}");
                    throw;
                }
            }
        }

        public void SetAlwaysJitterMode(bool enabled)
        {
            ThrowIfDisposed();

            lock (lockObject)
            {
                // Prevent concurrent mode changes
                if (isTransitioningMode)
                {
                    System.Diagnostics.Debug.WriteLine("SetAlwaysJitterMode ignored: another mode transition is in progress");
                    return;
                }
                
                try
                {
                    isTransitioningMode = true;
                    bool stateChanged = false;

                    // First handle disabling if requested
                    if (!enabled && IsAlwaysJitterMode)
                    {
                        System.Diagnostics.Debug.WriteLine("Disabling Always Jitter mode");
                        IsAlwaysJitterMode = false;
                        stateChanged = true;
                    }
                    // Only allow enabling if the other mode is not enabled
                    else if (enabled && !IsAlwaysJitterMode && !IsAlwaysRecoilReductionMode)
                    {
                        System.Diagnostics.Debug.WriteLine("Enabling Always Jitter mode");
                        IsAlwaysJitterMode = true;
                        IsJitterEnabled = true;
                        stateChanged = true;
                    }
                    else if (enabled && IsAlwaysJitterMode)
                    {
                        System.Diagnostics.Debug.WriteLine("Always Jitter mode is already enabled");
                    }
                    else if (enabled && IsAlwaysRecoilReductionMode)
                    {
                        System.Diagnostics.Debug.WriteLine("Cannot enable Always Jitter mode: Always Recoil Reduction mode is active");
                    }

                    if (stateChanged)
                    {
                        ModeChanged?.Invoke(this, IsJitterEnabled);
                        CheckMacroState();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in SetAlwaysJitterMode: {ex.Message}");
                    throw;
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
                // Prevent concurrent mode changes
                if (isTransitioningMode)
                {
                    System.Diagnostics.Debug.WriteLine("SetAlwaysRecoilReductionMode ignored: another mode transition is in progress");
                    return;
                }
                
                try
                {
                    isTransitioningMode = true;
                    bool stateChanged = false;

                    // First handle disabling if requested
                    if (!enabled && IsAlwaysRecoilReductionMode)
                    {
                        System.Diagnostics.Debug.WriteLine("Disabling Always Recoil Reduction mode");
                        IsAlwaysRecoilReductionMode = false;
                        stateChanged = true;
                    }
                    // Only allow enabling if the other mode is not enabled
                    else if (enabled && !IsAlwaysRecoilReductionMode && !IsAlwaysJitterMode)
                    {
                        System.Diagnostics.Debug.WriteLine("Enabling Always Recoil Reduction mode");
                        IsAlwaysRecoilReductionMode = true;
                        IsJitterEnabled = false;
                        stateChanged = true;
                    }
                    else if (enabled && IsAlwaysRecoilReductionMode)
                    {
                        System.Diagnostics.Debug.WriteLine("Always Recoil Reduction mode is already enabled");
                    }
                    else if (enabled && IsAlwaysJitterMode)
                    {
                        System.Diagnostics.Debug.WriteLine("Cannot enable Always Recoil Reduction mode: Always Jitter mode is active");
                    }

                    if (stateChanged)
                    {
                        ModeChanged?.Invoke(this, IsJitterEnabled);
                        CheckMacroState();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in SetAlwaysRecoilReductionMode: {ex.Message}");
                    throw;
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
            {
                System.Diagnostics.Debug.WriteLine("Warning: StopAllEffects called without obtaining lock first");
                throw new InvalidOperationException("StopAllEffects must be called within a lock");
            }

            try
            {
                if (jitterManager == null || recoilManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("Warning: Jitter or Recoil manager is null in StopAllEffects");
                    return;
                }
                
                if (jitterManager.IsActive)
                {
                    System.Diagnostics.Debug.WriteLine("Stopping jitter effect");
                    JitterStopped?.Invoke(this, EventArgs.Empty);
                    jitterManager.Stop();
                }
                if (recoilManager.IsActive)
                {
                    System.Diagnostics.Debug.WriteLine("Stopping recoil reduction effect");
                    RecoilReductionStopped?.Invoke(this, EventArgs.Empty);
                    recoilManager.Stop();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in StopAllEffects: {ex.Message}");
                // Continue with cleanup even if errors occur
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
            {
                System.Diagnostics.Debug.WriteLine("Warning: CheckMacroState called without obtaining lock first");
                throw new InvalidOperationException("CheckMacroState must be called within a lock");
            }

            try
            {
                // Validate managers exist
                if (jitterManager == null || recoilManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Manager instances are null in CheckMacroState");
                    return;
                }
                
                bool shouldActivate = IsEnabled && leftButtonDown && rightButtonDown;
                System.Diagnostics.Debug.WriteLine($"CheckMacroState: Should activate={shouldActivate}, " +
                    $"IsEnabled={IsEnabled}, leftButtonDown={leftButtonDown}, rightButtonDown={rightButtonDown}");

                // Always stop effects if we shouldn't be active
                if (!shouldActivate)
                {
                    StopAllEffects();
                    return;
                }

                // Determine which mode should be active
                bool shouldUseJitter = IsAlwaysJitterMode || (!IsAlwaysRecoilReductionMode && IsJitterEnabled);
                System.Diagnostics.Debug.WriteLine($"CheckMacroState: Should use jitter={shouldUseJitter}, " +
                    $"IsAlwaysJitterMode={IsAlwaysJitterMode}, IsAlwaysRecoilReductionMode={IsAlwaysRecoilReductionMode}, " +
                    $"IsJitterEnabled={IsJitterEnabled}");

                // If we're already active but in the wrong mode, stop all effects first
                if (isActive && (jitterManager.IsActive != shouldUseJitter || recoilManager.IsActive == shouldUseJitter))
                {
                    System.Diagnostics.Debug.WriteLine("Mode mismatch detected, stopping all effects");
                    StopAllEffects();
                }

                // Start the appropriate effect if we're not active
                if (!isActive)
                {
                    try
                    {
                        if (shouldUseJitter)
                        {
                            System.Diagnostics.Debug.WriteLine("Starting jitter effect");
                            JitterStarted?.Invoke(this, EventArgs.Empty);
                            jitterManager.Start();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Starting recoil reduction effect");
                            RecoilReductionStarted?.Invoke(this, EventArgs.Empty);
                            recoilManager.Start();
                        }
                        isActive = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error starting effect: {ex.Message}");
                        StopAllEffects();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                // Reset to a safe state on any error
                System.Diagnostics.Debug.WriteLine($"Error in CheckMacroState: {ex.Message}");
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