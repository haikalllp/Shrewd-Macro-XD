using System;
using System.Threading;

namespace NotesAndTasks.Utilities
{
    /// <summary>
    /// Manages jitter pattern generation and application for mouse movement.
    /// This class handles the timing and pattern of jitter movements.
    /// </summary>
    public class JitterManager : IDisposable
    {
        private readonly InputSimulator inputSimulator;
        private readonly System.Threading.Timer jitterTimer;
        private readonly object lockObject = new object();
        private bool disposed = false;
        private bool isActive = false;
        private int currentStep = 0;
        private int currentStrength = 3; // Default strength

        /// <summary>
        /// Gets or sets the current jitter strength (1-20).
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
        /// Gets whether the jitter effect is currently active.
        /// </summary>
        public bool IsActive => isActive;

        /// <summary>
        /// Event raised when the jitter state changes.
        /// </summary>
        public event EventHandler<bool> StateChanged;

        /// <summary>
        /// The predefined jitter pattern for mouse movement.
        /// </summary>
        private static readonly (int dx, int dy)[] JitterPattern = new[]
        {
            (7, 7), (-7, -7), (0, 7), (7, 7), (-7, -7),
            (0, 6), (7, 7), (-7, -7), (0, 7), (7, 7),
            (-7, -7), (0, 6), (7, 7), (-7, -7), (0, 6),
            (7, 7), (-7, -7), (0, 7), (7, 7), (-7, -7),
            (0, 6), (7, 7), (-7, -7), (0, 6)
        };

        /// <summary>
        /// Initializes a new instance of the JitterManager class.
        /// </summary>
        /// <param name="simulator">The input simulator to use for mouse movement.</param>
        public JitterManager(InputSimulator simulator)
        {
            inputSimulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
            jitterTimer = new System.Threading.Timer(OnJitterTimer, null, Timeout.Infinite, 10);
        }

        /// <summary>
        /// Starts the jitter effect.
        /// </summary>
        public void Start()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(JitterManager));

            lock (lockObject)
            {
                if (!isActive)
                {
                    isActive = true;
                    currentStep = 0;
                    jitterTimer.Change(0, 10);
                    StateChanged?.Invoke(this, true);
                }
            }
        }

        /// <summary>
        /// Stops the jitter effect.
        /// </summary>
        public void Stop()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(JitterManager));

            lock (lockObject)
            {
                if (isActive)
                {
                    isActive = false;
                    jitterTimer.Change(Timeout.Infinite, 10);
                    StateChanged?.Invoke(this, false);
                }
            }
        }

        /// <summary>
        /// Timer callback that applies the jitter pattern.
        /// </summary>
        private void OnJitterTimer(object state)
        {
            if (!isActive || disposed)
                return;

            try
            {
                lock (lockObject)
                {
                    if (currentStep >= JitterPattern.Length)
                        currentStep = 0;

                    var pattern = JitterPattern[currentStep];
                    if (!inputSimulator.SimulateJitterMovement(pattern, currentStrength))
                    {
                        // If movement fails, stop the jitter
                        Stop();
                        return;
                    }

                    currentStep = (currentStep + 1) % JitterPattern.Length;
                }
            }
            catch
            {
                // If any error occurs, stop the jitter
                Stop();
            }
        }

        /// <summary>
        /// Disposes of resources used by the JitterManager.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                Stop();
                jitterTimer.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
} 