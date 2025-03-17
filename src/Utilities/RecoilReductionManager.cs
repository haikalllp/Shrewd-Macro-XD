using System;
using System.Threading;

namespace NotesAndTasks.Utilities
{
    /// <summary>
    /// Manages recoil reduction functionality for mouse movement.
    /// This class handles the timing and strength of vertical compensation.
    /// </summary>
    public class RecoilReductionManager : IDisposable
    {
        private readonly InputSimulator inputSimulator;
        private readonly System.Threading.Timer recoilTimer;
        private readonly object lockObject = new object();
        private bool disposed = false;
        private bool isActive = false;
        private int currentStrength = 1; // Default strength

        /// <summary>
        /// Gets or sets the current recoil reduction strength (1-20).
        /// </summary>
        public int Strength
        {
            get => currentStrength;
            set
            {
                if (value < 1 || value > 20)
                    throw new ArgumentOutOfRangeException(nameof(value), "Strength must be between 1 and 20.");
                currentStrength = value;
            }
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
        /// <param name="simulator">The input simulator to use for mouse movement.</param>
        public RecoilReductionManager(InputSimulator simulator)
        {
            inputSimulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
            recoilTimer = new System.Threading.Timer(OnRecoilTimer, null, Timeout.Infinite, 10);
        }

        /// <summary>
        /// Starts the recoil reduction effect.
        /// </summary>
        public void Start()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(RecoilReductionManager));

            lock (lockObject)
            {
                if (!isActive)
                {
                    isActive = true;
                    recoilTimer.Change(0, 10);
                    StateChanged?.Invoke(this, true);
                }
            }
        }

        /// <summary>
        /// Stops the recoil reduction effect.
        /// </summary>
        public void Stop()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(RecoilReductionManager));

            lock (lockObject)
            {
                if (isActive)
                {
                    isActive = false;
                    recoilTimer.Change(Timeout.Infinite, 10);
                    StateChanged?.Invoke(this, false);
                }
            }
        }

        /// <summary>
        /// Timer callback that applies the recoil reduction movement.
        /// </summary>
        private void OnRecoilTimer(object state)
        {
            if (!isActive || disposed)
                return;

            try
            {
                lock (lockObject)
                {
                    if (!inputSimulator.SimulateRecoilReduction(currentStrength))
                    {
                        // If movement fails, stop the recoil reduction
                        Stop();
                    }
                }
            }
            catch
            {
                // If any error occurs, stop the recoil reduction
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
                recoilTimer.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
} 