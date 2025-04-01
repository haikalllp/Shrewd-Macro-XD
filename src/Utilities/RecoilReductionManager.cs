using System;
using System.Windows.Forms;

namespace NotesAndTasks.Utilities
{
    /// <summary>
    /// Manages recoil reduction functionality for mouse movement.
    /// This class handles the timing and strength of vertical compensation.
    /// </summary>
    public class RecoilReductionManager : MacroEffectBase
    {
        /// <summary>
        /// Initializes a new instance of the RecoilReductionManager class.
        /// </summary>
        /// <param name="inputSimulator">The input simulator to use for mouse movement.</param>
        public RecoilReductionManager(InputSimulator inputSimulator) : base(inputSimulator, 1)
        {
        }

        /// <summary>
        /// Timer callback that applies the recoil reduction movement.
        /// </summary>
        protected override void OnTimerTick(object state)
        {
            if (!IsActive) return;

            try
            {
                // Use InputSimulator's SimulateRecoilReduction method to avoid duplication
                InputSimulator.SimulateRecoilReduction(Strength);
            }
            catch (Exception)
            {
                Stop();
            }
        }
    }
}