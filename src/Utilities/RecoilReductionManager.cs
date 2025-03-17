using System;
using System.Windows.Forms;

namespace NotesAndTasks.Utilities
{
    /// <summary>
    /// Manages recoil reduction functionality for mouse movement.
    /// This class handles the timing and strength of vertical compensation.
    /// </summary>
    public class RecoilReductionManager : IDisposable
    {
        private readonly InputSimulator inputSimulator;
        private bool disposed = false;
        private int strength = 1;
        private bool isActive = false;
        private System.Threading.Timer timer;

        private const double BASE_RECOIL_STRENGTH = 0.75;
        private const double BASE_RECOIL_STRENGTH_2 = 2.0;
        private const double LOW_LEVEL_1_SPEED = 0.25;
        private const double LOW_LEVEL_2_SPEED = 0.5;
        private const double LOW_LEVEL_3_SPEED = 0.75;

        /// <summary>
        /// Gets or sets the current recoil reduction strength (1-20).
        /// </summary>
        public int Strength
        {
            get => strength;
            private set
            {
                if (value < 1 || value > 20)
                    throw new ArgumentOutOfRangeException(nameof(value), "Strength must be between 1 and 20.");
                strength = value;
            }
        }

        /// <summary>
        /// Sets the recoil reduction strength value (1-20).
        /// </summary>
        /// <param name="value">The strength value to set.</param>
        public void SetStrength(int value)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(RecoilReductionManager));
            Strength = value;
        }

        /// <summary>
        /// Gets whether the recoil reduction is currently active.
        /// </summary>
        public bool IsActive => isActive;

        /// <summary>
        /// Event raised when the recoil reduction state changes.
        /// </summary>
        public event EventHandler<bool> StateChanged;

        /// <summary>
        /// Initializes a new instance of the RecoilReductionManager class.
        /// </summary>
        /// <param name="inputSimulator">The input simulator to use for mouse movement.</param>
        public RecoilReductionManager(InputSimulator inputSimulator)
        {
            this.inputSimulator = inputSimulator ?? throw new ArgumentNullException(nameof(inputSimulator));
            timer = new System.Threading.Timer(OnTimer, null, System.Threading.Timeout.Infinite, 10);
        }

        /// <summary>
        /// Starts the recoil reduction effect.
        /// </summary>
        public void Start()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(RecoilReductionManager));

            if (!isActive)
            {
                isActive = true;
                timer.Change(0, 10);
                StateChanged?.Invoke(this, true);
            }
        }

        /// <summary>
        /// Stops the recoil reduction effect.
        /// </summary>
        public void Stop()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(RecoilReductionManager));

            if (isActive)
            {
                isActive = false;
                timer.Change(System.Threading.Timeout.Infinite, 10);
                StateChanged?.Invoke(this, false);
            }
        }

        /// <summary>
        /// Timer callback that applies the recoil reduction movement.
        /// </summary>
        private void OnTimer(object state)
        {
            if (!isActive) return;

            try
            {
                int dy;
                // Low level recoil reduction
                if (strength <= 6)
                {
                    // Lowest level recoil reduction
                    if (strength == 1)
                    {
                        dy = Math.Max(1, (int)Math.Round(BASE_RECOIL_STRENGTH * 0.3));
                    }
                    else
                    {
                        double logBase = 1.5;
                        dy = Math.Max(1, (int)Math.Round(BASE_RECOIL_STRENGTH * Math.Log(strength + 1, logBase)));
                    }
                }
                // Medium level recoil reduction
                else if (strength <= 16)
                {
                    dy = Math.Max(1, (int)Math.Round(BASE_RECOIL_STRENGTH * strength * 1.2));
                }
                // High level recoil reduction
                else
                {
                    double baseValue = BASE_RECOIL_STRENGTH * 20.0;
                    double scalingFactor = 1.19; // Reduced from 1.2 to further slow growth
                    double exponentialBoost = 1.1; // Reduced from 1.1 to further slow growth
                    dy = Math.Max(1, (int)Math.Round(
                        baseValue *
                        Math.Pow(scalingFactor, strength - 13) *
                        Math.Pow(exponentialBoost, strength - 13)
                    ));
                }

                inputSimulator.MoveMouse(0, dy);
            }
            catch (Exception)
            {
                Stop();
            }
        }

        /// <summary>
        /// Disposes of resources used by the RecoilReductionManager.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                Stop();
                timer.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}