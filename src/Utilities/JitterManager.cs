using System;
using System.Windows.Forms;

namespace NotesAndTasks.Utilities
{
    /// <summary>
    /// Manages jitter pattern generation and application for mouse movement.
    /// This class handles the timing and pattern of jitter movements.
    /// </summary>
    public class JitterManager : MacroEffectBase
    {
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
        /// Initializes a new instance of the JitterManager class.
        /// </summary>
        /// <param name="inputSimulator">The input simulator to use for mouse movement.</param>
        public JitterManager(InputSimulator inputSimulator) : base(inputSimulator, 3)
        {
        }

        /// <summary>
        /// Timer callback that applies the jitter pattern.
        /// </summary>
        protected override void OnTimerTick(object state)
        {
            if (!IsActive) return;

            try
            {
                var pattern = jitterPattern[currentStep];
                InputSimulator.SimulateJitterMovement(pattern, Strength);
                currentStep = (currentStep + 1) % jitterPattern.Length;
            }
            catch (Exception)
            {
                Stop();
            }
        }
    }
} 