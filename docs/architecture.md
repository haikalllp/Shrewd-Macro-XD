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
    private readonly NativeMethods.LowLevelHookProc hookCallback;
    private bool disposed;

    public event EventHandler<KeyboardHookEventArgs> KeyDown;
    public event EventHandler<KeyboardHookEventArgs> KeyUp;

    public void Start()
    {
        if (hookID == IntPtr.Zero)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            SetHook(NativeMethods.GetModuleHandle(curModule.ModuleName));
        }
    }
}

// MouseHook.cs
public class MouseHook : IDisposable
{
    private IntPtr hookID = IntPtr.Zero;
    private readonly NativeMethods.LowLevelHookProc hookProc;
    private bool disposed;

    public event EventHandler<MouseHookEventArgs> MouseDown;
    public event EventHandler<MouseHookEventArgs> MouseUp;

    public void Start()
    {
        if (hookID == IntPtr.Zero)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            SetHook(NativeMethods.GetModuleHandle(curModule.ModuleName));
        }
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

#### Base Effect Manager
```csharp
// MacroEffectBase.cs
public abstract class MacroEffectBase : IDisposable
{
    protected readonly InputSimulator InputSimulator;
    protected bool Disposed = false;
    protected int EffectStrength;
    protected bool IsEffectActive = false;
    protected System.Threading.Timer Timer;

    public event EventHandler<bool> StateChanged;
    public bool IsActive => IsEffectActive;
    public int Strength { get; protected set; }

    protected abstract void OnTimerTick(object state);
}
```

#### Recoil Reduction System
```csharp
// RecoilReductionManager.cs
public class RecoilReductionManager : MacroEffectBase
{
    public RecoilReductionManager(InputSimulator inputSimulator) : base(inputSimulator, 1)
    {
    }

    protected override void OnTimerTick(object state)
    {
        if (!IsActive) return;
        try
        {
            InputSimulator.SimulateRecoilReduction(Strength);
        }
        catch (Exception)
        {
            Stop();
        }
    }
}
```

#### Jitter System
```csharp
// JitterManager.cs
public class JitterManager : MacroEffectBase
{
    private int currentStep = 0;
    private readonly (int dx, int dy)[] jitterPattern;

    public JitterManager(InputSimulator inputSimulator) : base(inputSimulator, 3)
    {
    }

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
```

### 3. Configuration System

#### Configuration Models
```csharp
// AppSettings.cs
public class AppSettings : INotifyPropertyChanged
{
    public MacroSettings MacroSettings { get; }
    public UISettings UISettings { get; }
    public HotkeySettings HotkeySettings { get; }
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
    public Point WindowPosition { get; set; }
    public Size WindowSize { get; set; }
}

// HotkeySettings.cs
public class HotkeySettings : INotifyPropertyChanged
{
    public InputBinding MacroKey { get; set; }
    public InputBinding SwitchKey { get; set; }
}
```

#### Configuration Management
```csharp
// ConfigurationManager.cs
public class ConfigurationManager : IDisposable
{
    private static readonly string AppDirectory = Path.GetDirectoryName(Application.ExecutablePath);
    private static readonly string SettingsFilePath = Path.Combine(AppDirectory, "settings.json");
    private static readonly string SettingsBackupDirectoryPath = Path.Combine(AppDirectory, "Backups");
    private static readonly int MaxBackupCount = 5;

    private readonly ReaderWriterLockSlim _configLock = new ReaderWriterLockSlim();
    private readonly JsonSerializerOptions _jsonOptions;
    private AppSettings _currentSettings;

    public event EventHandler<SettingsChangedEventArgs> SettingsChanged;
    public event EventHandler<SettingsValidationEventArgs> SettingsValidating;
    public event EventHandler<SettingsBackupEventArgs> SettingsBackupCompleted;

    public void LoadSettings()
    {
        _configLock.EnterWriteLock();
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                LoadSettingsFromPath(SettingsFilePath);
            }
            else
            {
                _currentSettings = CreateDefaultSettings();
                SaveSettings();
            }
        }
        finally
        {
            _configLock.ExitWriteLock();
        }
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
    private readonly KeyboardHook keyboardHook;
    private readonly MouseHook mouseHook;
    private readonly MacroManager macroManager;
    private readonly HotkeyManager hotkeyManager;
    private readonly UIManager uiManager;
    private readonly ToolTip toolTip;

    public MacroForm()
    {
        InitializeComponent();
        
        // Initialize managers and hooks
        macroManager = new MacroManager();
        hotkeyManager = new HotkeyManager(macroManager);
        keyboardHook = new KeyboardHook();
        mouseHook = new MouseHook();
        
        // Initialize UI Manager
        uiManager = new UIManager(
            this, macroManager, hotkeyManager,
            debugLabel, lblJitterActive, lblRecoilReductionActive,
            lblCurrentKeyValue, lblMacroSwitchKeyValue,
            lblJitterStrengthValue, lblRecoilReductionStrengthValue,
            notifyIcon, toolTip
        );
    }
}
```

#### UI Manager
```csharp
// UIManager.cs
public class UIManager : IDisposable
{
    private readonly Form form;
    private readonly MacroManager macroManager;
    private readonly HotkeyManager hotkeyManager;
    private readonly TextBox debugLabel;
    private readonly Label lblJitterActive;
    private readonly Label lblRecoilReductionActive;
    private readonly NotifyIcon notifyIcon;
    private readonly ToolTip toolTip;

    public void UpdateTitle()
    {
        string jitterMode = macroManager.IsAlwaysJitterMode ? "Always Jitter" :
            (macroManager.IsJitterEnabled ? "Jitter" : "Jitter (OFF)");

        string recoilMode = macroManager.IsAlwaysRecoilReductionMode ? "Always Recoil Reduction" :
            (macroManager.IsJitterEnabled ? "Recoil Reduction (OFF)" : "Recoil Reduction");

        form.Text = $"Notes&Tasks [{(macroManager.IsEnabled ? "ON" : "OFF")}] - {jitterMode} / {recoilMode} Mode";
    }
}
```

### 5. Event System

#### Event Categories
1. **UI Events**
   - Form state changes
   - Control value updates
   - Window management events

2. **Configuration Events**
   - Settings changes
   - Validation events
   - Backup completion events

3. **Macro Events**
   - State changes
   - Mode switches
   - Effect start/stop events

### 6. Performance Optimization

#### Resource Management
```csharp
protected virtual void Dispose(bool disposing)
{
    if (!disposed)
    {
        if (disposing)
        {
            Stop();
            Timer?.Dispose();
        }
        disposed = true;
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
│   │   └── UIManager.cs      # UI manager
│   ├── Configuration.resx
│   ├── Configuration/        # Configuration management
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
│   └── Program.cs
├── tests/                   # Unit tests
├── docs/                    # Documentation
├── app.manifest            # Application manifest
├── MouseMacro.csproj       # Project configuration
└── README.md               # Project documentation
```