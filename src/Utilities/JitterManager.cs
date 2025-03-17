using System;
using System.Windows.Forms;

namespace NotesAndTasks.Utilities
{
    /// <summary>
    /// Manages jitter pattern generation and application for mouse movement.
    /// This class handles the timing and pattern of jitter movements.
    /// </summary>
    public class JitterManager : IDisposable
    {
        private readonly InputSimulator inputSimulator;
        private bool disposed = false;
        private int strength = 3;
        private bool isActive = false;
        private System.Threading.Timer timer;
        private int currentStep = 0;

        private readonly (int dx, int dy)[] jitterPattern = new[]
        {
            (7, 7), (-7, -7), (0, 7), (7, 7), (-7, -7),
            (0, 6), (7, 7), (-7, -7), (0, 7), (7, 7),
            (-7, -7), (0, 6), (7, 7), (-7, -7), (0, 6),
            (7, 7), (-7, -7), (0, 7), (7, 7), (-7, -7),
            (0, 6), (7, 7), (-7, -7), (0, 6)
        };

        /// <summary>
        /// Event raised when the jitter state changes.
        /// </summary>
        public event EventHandler<bool> StateChanged;

        /// <summary>
        /// Initializes a new instance of the JitterManager class.
        /// </summary>
        /// <param name="inputSimulator">The input simulator to use for mouse movement.</param>
        public JitterManager(InputSimulator inputSimulator)
        {
            this.inputSimulator = inputSimulator ?? throw new ArgumentNullException(nameof(inputSimulator));
            timer = new System.Threading.Timer(OnTimer, null, System.Threading.Timeout.Infinite, 10);
        }

        /// <summary>
        /// Gets or sets the current jitter strength (1-20).
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
        /// Sets the jitter strength value (1-20).
        /// </summary>
        /// <param name="value">The strength value to set.</param>
        public void SetStrength(int value)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(JitterManager));
            Strength = value;
        }

        /// <summary>
        /// Gets whether the jitter effect is currently active.
        /// </summary>
        public bool IsActive => isActive;

        /// <summary>
        /// Starts the jitter effect.
        /// </summary>
        public void Start()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(JitterManager));

            if (!isActive)
            {
                isActive = true;
                timer.Change(0, 10);
                StateChanged?.Invoke(this, true);
            }
        }

        /// <summary>
        /// Stops the jitter effect.
        /// </summary>
        public void Stop()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(JitterManager));

            if (isActive)
            {
                isActive = false;
                timer.Change(System.Threading.Timeout.Infinite, 10);
                StateChanged?.Invoke(this, false);
            }
        }

        /// <summary>
        /// Timer callback that applies the jitter pattern.
        /// </summary>
        private void OnTimer(object state)
        {
            if (!isActive) return;

            try
            {
                var pattern = jitterPattern[currentStep];
                int dx = (int)(pattern.dx * strength / 7);
                int dy = (int)(pattern.dy * strength / 7);

                inputSimulator.MoveMouse(dx, dy);
                currentStep = (currentStep + 1) % jitterPattern.Length;
            }
            catch (Exception)
            {
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
                timer.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
} 