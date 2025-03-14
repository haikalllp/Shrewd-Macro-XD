using System;
using System.Drawing;

namespace NotesAndTasks.Configuration
{
    /// <summary>
    /// Root configuration class that contains all application settings.
    /// </summary>
    public class AppConfiguration : ICloneable
    {
        public JitterConfiguration JitterSettings { get; set; }
        public RecoilConfiguration RecoilSettings { get; set; }
        public HotkeyConfiguration HotkeySettings { get; set; }
        public UIConfiguration UISettings { get; set; }
        public BackupConfiguration BackupSettings { get; set; }

        public object Clone()
        {
            return new AppConfiguration
            {
                JitterSettings = (JitterConfiguration)JitterSettings?.Clone(),
                RecoilSettings = (RecoilConfiguration)RecoilSettings?.Clone(),
                HotkeySettings = (HotkeyConfiguration)HotkeySettings?.Clone(),
                UISettings = (UIConfiguration)UISettings?.Clone(),
                BackupSettings = (BackupConfiguration)BackupSettings?.Clone()
            };
        }
    }

    /// <summary>
    /// Configuration settings for the jitter functionality.
    /// </summary>
    public class JitterConfiguration : ICloneable
    {
        public int Strength { get; set; }
        public bool IsEnabled { get; set; }
        public bool AlwaysEnabled { get; set; }

        public object Clone()
        {
            return new JitterConfiguration
            {
                Strength = Strength,
                IsEnabled = IsEnabled,
                AlwaysEnabled = AlwaysEnabled
            };
        }
    }

    /// <summary>
    /// Configuration settings for the recoil reduction functionality.
    /// </summary>
    public class RecoilConfiguration : ICloneable
    {
        public int Strength { get; set; }
        public bool IsEnabled { get; set; }
        public bool AlwaysEnabled { get; set; }

        public object Clone()
        {
            return new RecoilConfiguration
            {
                Strength = Strength,
                IsEnabled = IsEnabled,
                AlwaysEnabled = AlwaysEnabled
            };
        }
    }

    /// <summary>
    /// Configuration settings for hotkeys.
    /// </summary>
    public class HotkeyConfiguration : ICloneable
    {
        public string MacroToggleKey { get; set; }
        public string ModeSwitchKey { get; set; }

        public object Clone()
        {
            return new HotkeyConfiguration
            {
                MacroToggleKey = MacroToggleKey,
                ModeSwitchKey = ModeSwitchKey
            };
        }
    }

    /// <summary>
    /// Configuration settings for the user interface.
    /// </summary>
    public class UIConfiguration : ICloneable
    {
        public bool MinimizeToTray { get; set; }
        public bool ShowDebugPanel { get; set; }
        public Point WindowPosition { get; set; }
        public Size WindowSize { get; set; }

        public object Clone()
        {
            return new UIConfiguration
            {
                MinimizeToTray = MinimizeToTray,
                ShowDebugPanel = ShowDebugPanel,
                WindowPosition = new Point(WindowPosition.X, WindowPosition.Y),
                WindowSize = new Size(WindowSize.Width, WindowSize.Height)
            };
        }
    }

    /// <summary>
    /// Configuration settings for backup functionality.
    /// </summary>
    public class BackupConfiguration : ICloneable
    {
        public bool AutoBackupEnabled { get; set; }
        public int BackupIntervalHours { get; set; }
        public int MaxBackupCount { get; set; }
        public string BackupDirectory { get; set; }

        public object Clone()
        {
            return new BackupConfiguration
            {
                AutoBackupEnabled = AutoBackupEnabled,
                BackupIntervalHours = BackupIntervalHours,
                MaxBackupCount = MaxBackupCount,
                BackupDirectory = BackupDirectory
            };
        }
    }
} 