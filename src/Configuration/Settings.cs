using System.Windows.Forms;
using NotesAndTasks.Utilities;

namespace NotesAndTasks.Configuration
{
    /// <summary>
    /// Represents the application settings including hotkeys, strengths, and modes.
    /// </summary>
    public class Settings
    {
        #region Properties
        /// <summary>
        /// Gets or sets the jitter strength value (1-20).
        /// </summary>
        public int JitterStrength { get; set; } = 3;

        /// <summary>
        /// Gets or sets the recoil reduction strength value (1-20).
        /// </summary>
        public int RecoilReductionStrength { get; set; } = 1;

        /// <summary>
        /// Gets or sets whether jitter mode is always enabled.
        /// </summary>
        public bool AlwaysJitterMode { get; set; } = false;

        /// <summary>
        /// Gets or sets whether recoil reduction mode is always enabled.
        /// </summary>
        public bool AlwaysRecoilReductionMode { get; set; } = false;

        /// <summary>
        /// Gets or sets whether jitter is enabled.
        /// </summary>
        public bool JitterEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets whether recoil reduction is enabled.
        /// </summary>
        public bool RecoilReductionEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to minimize to system tray when closing.
        /// </summary>
        public bool MinimizeToTray { get; set; } = true;

        /// <summary>
        /// Gets or sets the macro toggle key.
        /// </summary>
        public Keys MacroKey { get; set; } = Keys.Capital;

        /// <summary>
        /// Gets or sets the mode switch key.
        /// </summary>
        public Keys SwitchKey { get; set; } = Keys.Q;

        /// <summary>
        /// Gets or sets the toggle type for macro activation.
        /// </summary>
        public ToggleType ToggleType { get; set; } = ToggleType.Keyboard;
        #endregion

        #region Methods
        /// <summary>
        /// Creates a new instance of Settings with default values.
        /// </summary>
        public static Settings CreateDefault()
        {
            return new Settings();
        }

        /// <summary>
        /// Resets all settings to their default values.
        /// </summary>
        public void ResetToDefaults()
        {
            JitterStrength = 3;
            RecoilReductionStrength = 1;
            AlwaysJitterMode = false;
            AlwaysRecoilReductionMode = false;
            JitterEnabled = false;
            RecoilReductionEnabled = false;
            MinimizeToTray = true;
            MacroKey = Keys.Capital;
            SwitchKey = Keys.Q;
            ToggleType = ToggleType.Keyboard;
        }
        #endregion
    }
}
