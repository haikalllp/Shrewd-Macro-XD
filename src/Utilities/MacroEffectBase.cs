using System;
using System.Windows.Forms;

namespace NotesAndTasks.Utilities
{
    /// <summary>
    /// Base class for macro effect managers like jitter and recoil reduction.
    /// Provides common functionality for handling strength, activation state, and lifecycle.
    /// </summary>
    public abstract class MacroEffectBase : IDisposable
    {
        protected readonly InputSimulator InputSimulator;
        protected bool Disposed = false;
        protected int EffectStrength;
        protected bool IsEffectActive = false;
        protected System.Threading.Timer Timer;

        /// <summary>
        /// Event raised when the effect state changes.
        /// </summary>
        public event EventHandler<bool> StateChanged;

        /// <summary>
        /// Gets whether the effect is currently active.
        /// </summary>
        public bool IsActive => IsEffectActive;

        /// <summary>
        /// Gets or sets the current effect strength (1-20).
        /// </summary>
        public int Strength
        {
            get => EffectStrength;
            protected set
            {
                if (value < 1 || value > 20)
                    throw new ArgumentOutOfRangeException(nameof(value), "Strength must be between 1 and 20.");
                EffectStrength = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the MacroEffectBase class.
        /// </summary>
        /// <param name="inputSimulator">The input simulator to use for mouse movement.</param>
        /// <param name="defaultStrength">The default strength value.</param>
        protected MacroEffectBase(InputSimulator inputSimulator, int defaultStrength)
        {
            InputSimulator = inputSimulator ?? throw new ArgumentNullException(nameof(inputSimulator));
            EffectStrength = defaultStrength;
            Timer = new System.Threading.Timer(OnTimerTick, null, System.Threading.Timeout.Infinite, 10);
        }

        /// <summary>
        /// Sets the effect strength value (1-20).
        /// </summary>
        /// <param name="value">The strength value to set.</param>
        public void SetStrength(int value)
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);
            Strength = value;
        }

        /// <summary>
        /// Starts the effect.
        /// </summary>
        public void Start()
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (!IsEffectActive)
            {
                IsEffectActive = true;
                Timer.Change(0, 10);
                StateChanged?.Invoke(this, true);
            }
        }

        /// <summary>
        /// Stops the effect.
        /// </summary>
        public void Stop()
        {
            if (Disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (IsEffectActive)
            {
                IsEffectActive = false;
                Timer.Change(System.Threading.Timeout.Infinite, 10);
                StateChanged?.Invoke(this, false);
            }
        }

        /// <summary>
        /// Timer callback that applies the effect.
        /// Must be implemented by derived classes.
        /// </summary>
        protected abstract void OnTimerTick(object state);

        /// <summary>
        /// Disposes of resources used by the manager.
        /// </summary>
        public void Dispose()
        {
            if (!Disposed)
            {
                Stop();
                Timer.Dispose();
                Disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
} 