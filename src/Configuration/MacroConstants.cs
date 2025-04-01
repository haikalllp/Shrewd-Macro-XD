namespace NotesAndTasks.Configuration
{
    /// <summary>
    /// Contains constant values used for macro behavior configuration.
    /// </summary>
    public static class MacroConstants
    {
        /// <summary>
        /// Base strength value for recoil calculations.
        /// Used as a multiplier in basic recoil reduction calculations.
        /// </summary>
        public const double BASE_RECOIL_STRENGTH = 0.75;

        /// <summary>
        /// Secondary base strength value for enhanced recoil calculations.
        /// Used as a multiplier in advanced recoil reduction calculations.
        /// </summary>
        public const double BASE_RECOIL_STRENGTH_2 = 2.0;

        /// <summary>
        /// Speed multiplier for the lowest level of movement adjustment.
        /// Applied to movement calculations when minimal adjustment is needed.
        /// </summary>
        public const double LOW_LEVEL_1_SPEED = 0.25;

        /// <summary>
        /// Speed multiplier for the medium-low level of movement adjustment.
        /// Applied to movement calculations when moderate adjustment is needed.
        /// </summary>
        public const double LOW_LEVEL_2_SPEED = 0.5;

        /// <summary>
        /// Speed multiplier for the medium level of movement adjustment.
        /// Applied to movement calculations when significant adjustment is needed.
        /// </summary>
        public const double LOW_LEVEL_3_SPEED = 0.75;
    }
}