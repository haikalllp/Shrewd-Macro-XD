# Mouse Macro Application Architecture

## Introduction
This document provides an overview of the Mouse Macro Application architecture. The application is designed to create 
and manage mouse macros, featuring a modern dark-themed UI for keybind configuration and settings management.
IMPORTANT NOTE: For professional system integration the application presents itself as "Notes&Tasks" and "NotesAndTasks" 😊.

## System Overview

### Identity & Integration
- **Process Name**: NotesTasks.exe
- **Display Name**: Notes&Tasks
- **Window Title**: Notes&Tasks - [Status]
- **System Tray**: Notes&Tasks with corresponding icon
- **Design Rationale**:
  - Professional appearance in enterprise environments
  - Discrete system integration
  - Consistent branding across interfaces

### System Requirements
1. **Hardware**
   - Windows 10/11 compatible PC
   - DirectX compatible display
   - Mouse with standard buttons
   - Keyboard for hotkey support
   - Optional: Multi-button mouse, high refresh rate display, SSD

2. **Software**
   - Windows 10/11 (64-bit)
   - .NET 6.0 Runtime
   - Administrator privileges
   - DirectX 9.0c or later

## Usage Guide

### Installation
1. Download the latest release from the releases page
2. Extract the archive to your desired location
3. Run NotesTasks.exe (no installation required)

### First-Time Setup
1. Launch the application
2. Configure hotkeys:
   - Set Macro Toggle Key (default: Caps Lock)
   - Set Mode Switch Key (default: Q)
3. Adjust feature settings:
   - Jitter strength (1-20)
   - Recoil reduction strength (1-20)
4. Choose toggle modes:
   - Hold: Features active while key held
   - Toggle: Features toggle on/off with key press

### Basic Operation
1. **Starting the Macro**:
   - Press the Macro Toggle Key to enable/disable
   - Status shown in window title and system tray

2. **Switching Modes**:
   - Press Mode Switch Key to cycle between:
     - Jitter mode
     - Recoil reduction mode

3. **Quick Settings**:
   - Use trackbars for real-time strength adjustment
   - Toggle "Always On" for persistent effects
   - Minimize to tray for discrete operation

### Advanced Features
1. **Custom Patterns**:
   - Jitter patterns optimized for different scenarios
   - Recoil reduction with dynamic scaling
   - Combined mode synchronization

2. **Performance Optimization**:
   - Timer-based execution for consistent timing
   - Resource-efficient hook implementation
   - Automatic cleanup on exit

## Build Process

### Development Environment Setup
1. **Required Tools**:
   - Visual Studio 2022 or later
   - .NET 6.0 SDK
   - Git for version control

2. **Getting Started**:
   ```powershell
   git clone https://github.com/yourusername/MouseMacro.git
   cd MouseMacro
   dotnet restore
   ```

### Build Configuration
1. **Debug Build**:
   ```powershell
   dotnet build -c Debug
   ```
   - Includes debug symbols
   - Enhanced logging
   - Development features enabled

2. **Release Build**:
   ```powershell
   dotnet build -c Release
   ```
   - Optimized for performance
   - Minimal logging
   - Production-ready

### Testing
1. **Unit Tests**:
   ```powershell
   dotnet test
   ```
   - Validates core functionality
   - Tests hook implementation
   - Verifies configuration management

2. **Manual Testing**:
   - UI responsiveness
   - Feature functionality
   - Resource usage
   - Error handling

## Core Components

### 1. Input System
#### Low-Level Hooks
```csharp
// KeyboardHook.cs
public class KeyboardHook : IDisposable
{
    private IntPtr hookID = IntPtr.Zero;
    private readonly HookProc hookProc;

    public event EventHandler<KeyEventArgs> KeyDown;
    public event EventHandler<KeyEventArgs> KeyUp;

    public void Start()
    {
        hookID = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, 
            GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
    }
}

// MouseHook.cs
public class MouseHook : IDisposable
{
    private IntPtr hookID = IntPtr.Zero;
    private readonly HookProc hookProc;

    public event EventHandler<MouseEventArgs> MouseDown;
    public event EventHandler<MouseEventArgs> MouseUp;
    public event EventHandler<MouseEventArgs> MouseMove;

    public void Start()
    {
        hookID = SetWindowsHookEx(WH_MOUSE_LL, hookProc, 
            GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
    }
}
```

#### Input Simulation
```csharp
// InputSimulator.cs
public class InputSimulator
{
    public void MoveMouse(int dx, int dy)
    {
        var input = new INPUT
        {
            type = INPUT_MOUSE,
            u = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    dx = dx,
                    dy = dy,
                    mouseData = 0,
                    dwFlags = MOUSEEVENTF_MOVE,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
    }
}
```

### 2. Macro Engine

#### Recoil Reduction System
```csharp
// RecoilReductionManager.cs
public class RecoilReductionManager
{
    private readonly Timer timer;
    private readonly InputSimulator inputSimulator;

    public int Strength { get; set; } = 1;
    public bool IsEnabled { get; private set; }

    private void OnTimerTick(object sender, EventArgs e)
    {
        if (IsEnabled && IsActive)
        {
            int scaledStrength = CalculateScaledStrength(Strength);
            inputSimulator.MoveMouse(0, scaledStrength);
        }
    }

    private int CalculateScaledStrength(int strength)
    {
        if (strength <= 6) return strength;                    // Tier 1
        if (strength <= 16) return 6 + (strength - 6) * 2;    // Tier 2
        return 20 + (strength - 13) * 3;                      // Tier 3
    }
}
```

#### Jitter System
```csharp
// JitterManager.cs
public class JitterManager
{
    private readonly Timer timer;
    private readonly InputSimulator inputSimulator;
    private int currentPatternIndex;

    private readonly (int dx, int dy)[] pattern = {
        (0, 6), (7, 7), (-7, -7), (7, -7), (-7, 7),
        (0, -6), (-6, 0), (6, 0), (5, 5), (-5, -5)
    };

    private void OnTimerTick(object sender, EventArgs e)
    {
        if (IsEnabled && IsActive)
        {
            var (dx, dy) = pattern[currentPatternIndex];
            inputSimulator.MoveMouse(
                dx * Strength / 10,
                dy * Strength / 10
            );
            currentPatternIndex = (currentPatternIndex + 1) % pattern.Length;
        }
    }
}
```

### 3. Configuration System

#### Configuration Models
```csharp
// AppSettings.cs
public class AppSettings : INotifyPropertyChanged
{
    private readonly MacroSettings _macroSettings;
    private readonly UISettings _uiSettings;
    private readonly HotkeySettings _hotkeySettings;

    public MacroSettings MacroSettings => _macroSettings;
    public UISettings UISettings => _uiSettings;
    public HotkeySettings HotkeySettings => _hotkeySettings;

    public event PropertyChangedEventHandler PropertyChanged;
}

// MacroSettings.cs
public class MacroSettings : INotifyPropertyChanged
{
    [Range(1, 20)]
    public int JitterStrength { get; set; } = 3;
    [Range(1, 20)]
    public int RecoilReductionStrength { get; set; } = 1;
    public bool JitterEnabled { get; set; }
    public bool RecoilReductionEnabled { get; set; }
    public bool AlwaysJitterMode { get; set; }
    public bool AlwaysRecoilReductionMode { get; set; }
}

// UISettings.cs
public class UISettings : INotifyPropertyChanged
{
    public bool MinimizeToTray { get; set; }
    public bool ShowDebugPanel { get; set; }
    public bool ShowStatusInTitle { get; set; }
    public bool ShowTrayNotifications { get; set; }
    public Point WindowPosition { get; set; }
    public Size WindowSize { get; set; }
}

// HotkeySettings.cs
public class HotkeySettings : INotifyPropertyChanged
{
    public Keys MacroKey { get; set; } = Keys.Capital;
    public Keys SwitchKey { get; set; } = Keys.Q;
}

// Legacy configuration support
public class AppConfiguration
{
    // Existing configuration properties maintained for compatibility
}
```

#### Configuration Management
```csharp
// ConfigurationManager.cs
public class ConfigurationManager : IDisposable
{
    private static readonly string AppDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NotesAndTasks"
    );
    private static readonly string ConfigPath = Path.Combine(AppDataPath, "config.json");
    private static readonly string BackupPath = Path.Combine(AppDataPath, "Backups");
    private static readonly int MaxBackupCount = 5;

    private readonly ReaderWriterLockSlim _configLock = new ReaderWriterLockSlim();
    private AppSettings _currentSettings;

    public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;
    public event EventHandler<ConfigurationValidationEventArgs> ConfigurationValidating;
    public event EventHandler<ConfigurationBackupEventArgs> ConfigurationBackupCompleted;

    public AppSettings CurrentSettings
    {
        get
        {
            _configLock.EnterReadLock();
            try { return _currentSettings; }
            finally { _configLock.ExitReadLock(); }
        }
    }

    public void LoadConfiguration()
    {
        // Thread-safe configuration loading with validation and legacy support
    }

    public void SaveConfiguration()
    {
        // Thread-safe configuration saving with backup and legacy format support
    }

    private void CreateBackup()
    {
        // Create timestamped backup with rotation
    }
}
```

#### Configuration Events
```csharp
// ConfigurationEvents.cs
public class ConfigurationChangedEventArgs : EventArgs
{
    public string Section { get; }
    public AppSettings PreviousConfig { get; }
    public AppSettings NewConfig { get; }
}

public class ConfigurationValidationEventArgs : EventArgs
{
    public bool IsValid { get; set; }
    public string Message { get; set; }
    public AppSettings Configuration { get; }
}

public class ConfigurationBackupEventArgs : EventArgs
{
    public string BackupPath { get; }
    public bool Success { get; }
    public string ErrorMessage { get; }
}
```

### 4. UI System

#### Main Form
```csharp
// MacroForm.cs
public partial class MacroForm : Form
{
    private readonly UIManager uiManager;
    private readonly MacroManager macroManager;
    private readonly HotkeyManager hotkeyManager;

    public MacroForm()
    {
        InitializeComponent();
        uiManager = new UIManager(this);
        macroManager = new MacroManager();
        hotkeyManager = new HotkeyManager();

        InitializeEventHandlers();
        LoadSettings();
    }

    private void InitializeEventHandlers()
    {
        hotkeyManager.KeySettingStateChanged += OnKeySettingStateChanged;
        macroManager.ModeChanged += OnModeChanged;
        macroManager.StateChanged += OnStateChanged;
    }
}
```

#### UI Manager
```csharp
// UIManager.cs
public class UIManager
{
    private readonly MacroForm form;
    private readonly Dictionary<string, Control> controls;

    public void UpdateModeLabels(bool jitterEnabled, bool recoilEnabled)
    {
        if (controls.TryGetValue("lblJitterActive", out var jitterLabel))
            jitterLabel.Text = jitterEnabled ? "[Active]" : "";

        if (controls.TryGetValue("lblRecoilActive", out var recoilLabel))
            recoilLabel.Text = recoilEnabled ? "[Active]" : "";
    }

    public void UpdateWindowTitle(bool macroEnabled)
    {
        form.Text = $"Notes&Tasks - {(macroEnabled ? "ON" : "OFF")}";
    }
}
```

### 5. Event System

#### Event Categories
1. **UI Events**
```csharp
// Event handler registration in MacroForm
btnSetMacroKey.Click += (s, e) => hotkeyManager.StartSettingMacroKey();
btnSetSwitchKey.Click += (s, e) => hotkeyManager.StartSettingSwitchKey();
trackBarJitter.ValueChanged += (s, e) => macroManager.SetJitterStrength(trackBarJitter.Value);
```

2. **Configuration Events**
```csharp
// Settings change notification
public event EventHandler<SettingsChangedEventArgs> SettingsChanged;

protected virtual void OnSettingsChanged(Settings newSettings)
{
    SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(newSettings));
}
```

3. **Macro Events**
```csharp
// MacroManager.cs
public event EventHandler<MacroStateEventArgs> StateChanged;
public event EventHandler<MacroModeEventArgs> ModeChanged;

private void OnStateChanged(bool enabled)
{
    StateChanged?.Invoke(this, new MacroStateEventArgs(enabled));
}
```

### 6. Performance Optimization

#### Timer Management
```csharp
// Base timer configuration
private void InitializeTimers()
{
    recoilTimer = new System.Windows.Forms.Timer
    {
        Interval = 16,  // ~60Hz
        Enabled = false
    };

    jitterTimer = new System.Windows.Forms.Timer
    {
        Interval = 25,  // 40Hz
        Enabled = false
    };
}
```

#### Resource Management
```csharp
public void Dispose()
{
    Dispose(true);
    GC.SuppressFinalize(this);
}

protected virtual void Dispose(bool disposing)
{
    if (disposing)
    {
        keyboardHook?.Dispose();
        mouseHook?.Dispose();
        recoilTimer?.Dispose();
        jitterTimer?.Dispose();
    }
}
```

## Build Configurations

### Debug Build
- Location: `bin/Debug/net6.0-windows/`
- Debug symbols enabled
- Logging enabled
- Development features active

### Release Build
- Location: `bin/Release/net6.0-windows/`
- Optimized performance
- Production ready
- Minimal logging

## Project Structure

### Directory Organization
```
MouseMacro/
├── assets/                  # Application resources
│   ├── logo.ico            # Application icon
├── bin/                    # Compiled binaries
│   ├── Debug/             # Debug build output
│   └── Release/           # Release build output
├── docs/                   # Documentation
├── obj/                    # Intermediate build files
│   ├── Debug/             # Debug build intermediates
│   │   └── net6.0-windows/
│   │       ├── ref/       # Assembly reference files
│   │       ├── refint/    # Reference interface files
│   │       └── *.cache    # Build cache files
│   └── Release/           # Release build intermediates
├── src/
│   ├── UI/                    # User interface components
│   │   ├── Controls/         # Custom UI controls
│   │   │   ├── ModernButton.cs
│   │   │   └── ModernTrackBar.cs
│   │   ├── MacroForm.cs
│   │   ├── MacroForm.Designer.cs
│   │   ├── MacroForm.resx
│   │   ├── Resources.Designer.cs
│   │   └── Resources.resx
│   │   └── UIManager.cs      # UI state management
│   ├── Configuration/        # Configuration management
│   │   ├── AppConfiguration.cs
│   │   ├── ConfigurationEvents.cs
│   │   ├── ConfigurationManager.cs
│   │   ├── EventHandlerExtensions.cs
│   │   ├── EventHandlerManager.cs
│   │   ├── Settings.cs
│   │   ├── SettingsManager.cs
│   │   ├── SettingsValidation.cs
│   │   └── Validation.cs
│   ├── Hooks/               # System hooks
│   │   ├── KeyboardHook.cs
│   │   ├── MouseHook.cs
│   │   ├── NativeMethods.cs
│   │   └── WinMessages.cs
│   ├── Models/              # Data models
│   │   ├── AppSettings.cs
│   │   ├── MacroSettings.cs
│   │   ├── UISettings.cs
│   │   └── HotkeySettings.cs
│   ├── Utilities/           # Core functionality
│   │   ├── InputSimulator.cs
│   │   ├── JitterManager.cs
│   │   ├── MacroManager.cs
│   │   └── RecoilReductionManager.cs
│   └── Program.cs          # Application entry point
├── tests/                  # Unit tests (planned)
├── MouseMacro.csproj      # Project configuration
├── README.md              # Project documentation and setup guide
└── app.manifest           # Application manifest
```
