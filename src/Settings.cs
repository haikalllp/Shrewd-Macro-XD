using System;

namespace NotesTasks
{
    public class Settings
    {
        // Jitter settings
        public int JitterStrength { get; set; } = 3;
        public bool JitterEnabled { get; set; } = false;  
        public bool AlwaysJitterMode { get; set; } = false;

        // Recoil reduction settings
        public int RecoilReductionStrength { get; set; } = 1;
        public bool RecoilReductionEnabled { get; set; } = false;
        public bool AlwaysRecoilReductionMode { get; set; } = false;

        // Key bindings
        public string MacroToggleKey { get; set; } = "Capital";
        public string ModeSwitchKey { get; set; } = "Q";
        
        // UI preferences
        public bool MinimizeToTray { get; set; } = false;
        public bool StartMinimized { get; set; } = false;
    }
}
