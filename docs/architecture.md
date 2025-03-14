# Mouse Macro Application Architecture

## Introduction
This document provides an overview of the Mouse Macro Application architecture. The application is designed to create and manage mouse macros, featuring a modern dark-themed UI for keybind configuration and settings management.IMPORTANT For professional system integration the application presents itself as "Notes&Tasks" and "NotesAndTasks" 😊.

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
├── src/                    # Source code
│   ├── MacroForm.cs       # Main form implementation
│   ├── MacroForm.Designer.cs # Form designer code
│   ├── ModernButton.cs    # Custom button control
│   ├── ModernTrackBar.cs  # Custom trackbar control
│   └── Program.cs         # Application entry point
├── MouseMacro.csproj      # Project configuration
├── README.md              # Project documentation and setup guide
└── app.manifest           # Application manifest
```

## Core Components

### 1. Input System
- **Low-Level Hooks**
  - Keyboard Hook (WH_KEYBOARD_LL)
  - Mouse Hook (WH_MOUSE_LL)
  - Global event capture
  
- **Input Simulation**
  - SendInput API implementation
  - Physical mouse movement simulation
  - Game compatibility optimizations

- **Toggle System**
  - Keyboard key support
  - Mouse button support (Mouse3-5)
  - State tracking via GetAsyncKeyState

### 2. Macro Engine
- **Recoil Reduction System**
  - Vertical compensation
  - Three-tier strength distribution:
    - Tier 1 (1-6): Linear scaling
    - Tier 2 (7-16): Enhanced scaling
    - Tier 3 (17-20): Maximum impact
  - Dynamic strength adjustment
  - Default strength: 1
  
- **Jitter System**
  - Complex movement patterns
  - Strength control (1-20)
  - Optional activation
  - Default strength: 3
  
- **Mode Switching**
  - Toggle between Jitter and Recoil Reduction modes
  - Default switch key: Q
  - Always mode options (locks to either Jitter or Recoil Reduction mode at all times)
  - Independent mode states

- **Timer Management**
  - Dual timer implementation
  - Independent operation
  - Performance optimized

### 3. User Interface
- **Main Window**
  - Modern dark theme with consistent color scheme
  - Responsive layout with fluid transitions
  - DPI scaling support
  - Professional visual design
  
- **Custom Controls**
  - ModernButton
    - Customizable hover and click effects
    - Smooth color transitions
    - Consistent styling with theme
    - Professional rounded corners
  - ModernTrackBar
    - Custom slider design
    - Visual value feedback
    - Smooth drag operations
    - Theme-consistent appearance
  
- **Standard Controls**
  - Macro Toggle key display/configuration (Default: Capital)
  - Macro Switch key configuration (Default: Q)
  - Strength sliders (1-20)
  - Mode toggles and indicators
  - Always mode checkboxes
  - Debug panel (collapsible)
  
- **System Tray**
  - Minimize to tray support
  - Context menu
  - Status indication

### 4. Process Management
- **Instance Control**
  - Global mutex implementation
  - Single instance enforcement
  - Clean termination handling
  
- **Security**
  - UAC integration
  - Privilege management
  - Secure API access

### 5. Settings Configuration System
- **Configuration Manager (`ConfigurationManager.cs`)**
  - Singleton pattern implementation
  - Thread-safe operations using `ReaderWriterLockSlim`
  - JSON-based configuration storage
  - Automatic configuration backup
  - Event-driven configuration changes
  - Validation system

#### 5.1 Configuration Components
```csharp
// Root configuration
public class AppConfiguration : ICloneable
{
    public JitterConfiguration JitterSettings { get; set; }
    public RecoilConfiguration RecoilSettings { get; set; }
    public HotkeyConfiguration HotkeySettings { get; set; }
    public UIConfiguration UISettings { get; set; }
    public BackupConfiguration BackupSettings { get; set; }
}

// Feature-specific configurations
public class JitterConfiguration : ICloneable
public class RecoilConfiguration : ICloneable
public class HotkeyConfiguration : ICloneable
public class UIConfiguration : ICloneable
public class BackupConfiguration : ICloneable
```

#### 5.2 Configuration Events
```csharp
// Event Arguments
public class ConfigurationChangedEventArgs : EventArgs
public class ConfigurationValidationEventArgs : EventArgs
public class ConfigurationBackupEventArgs : EventArgs

// Event Handlers
public delegate void ConfigurationChangedEventHandler(object sender, ConfigurationChangedEventArgs e);
public delegate void ConfigurationValidationEventHandler(object sender, ConfigurationValidationEventArgs e);
public delegate void ConfigurationBackupEventHandler(object sender, ConfigurationBackupEventArgs e);
```

#### 5.3 Configuration Validation
```csharp
public static class Validation
{
    // Core validation methods
    public static void ValidateStrength(int strength, int minValue, int maxValue, string paramName)
    public static void ValidateStringNotNullOrEmpty(string value, string paramName)
    public static void ValidateNotNull<T>(T value, string paramName)
    public static void ValidateHandle(IntPtr handle, string paramName)
    public static bool ValidateHookCode(int nCode)
}

public static class SettingsValidation
{
    // Settings-specific validation
    public static bool ValidateSettings(Settings settings, int minStrength, int maxStrength)
    public static bool IsValidHotkey(Keys key)
    public static bool ValidateStrengthValue(int strength, int min, int max)
    public static bool ValidateModeStates(bool jitterEnabled, bool recoilEnabled)
}
```

#### Validation Features
1. **Input Validation**
   - Parameter validation
   - Range checking
   - Null checks
   - Empty string prevention
   
2. **Settings Validation**
   - Configuration integrity
   - Hotkey validation
   - Mode state validation
   - Strength range validation
   
3. **Handle Validation**
   - Windows handle validation
   - Hook code validation
   - Resource validation

4. **Recovery Mechanisms**
   - Default value fallback
   - State preservation
   - Error reporting
   - Automatic recovery

### 5.4 Error Handling

#### 1. Validation Errors
```csharp
try
{
    Validation.ValidateStrength(strength, min, max, paramName);
}
catch (ArgumentOutOfRangeException ex)
{
    // Reset to default value
    strength = defaultValue;
    UpdateDebugInfo($"Reset to default strength: {ex.Message}");
}
```

#### 2. Runtime Errors
```csharp
try
{
    // Critical operation
}
catch (Exception ex)
{
    UpdateDebugInfo($"Error: {ex.Message}");
    // Cleanup and recovery
    CleanupResources();
    RestoreState();
}
```

#### 3. Resource Management
```csharp
protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        // Cleanup resources
        jitterTimer?.Dispose();
        keyboardHookID = IntPtr.Zero;
        mouseHookID = IntPtr.Zero;
    }
    base.Dispose(disposing);
}
```

#### 4. Debug System
```csharp
private void UpdateDebugInfo(string message)
{
    if (debugLabel.InvokeRequired)
    {
        debugLabel.Invoke(new Action(() => UpdateDebugInfo(message)));
        return;
    }

    string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
    string newLine = $"[{timestamp}] {message}";
    
    // Keep last 100 lines
    var lines = debugLabel.Lines.ToList();
    lines.Add(newLine);
    if (lines.Count > 100)
        lines.RemoveAt(0);
        
    debugLabel.Lines = lines.ToArray();
    debugLabel.SelectionStart = debugLabel.TextLength;
    debugLabel.ScrollToCaret();
}
```

### 6. Event Handler System

#### 6.1 Event Handler Manager
```csharp
public class EventHandlerManager : IDisposable
{
    private readonly Dictionary<string, List<Delegate>> eventHandlers;
    private readonly ConfigurationManager configManager;
    
    // Event registration methods
    public void RegisterControlEvents(Control control)
    public void UnregisterControlEvents(Control control)
    private void RegisterEventHandler(string eventName, Delegate handler)
    
    // Configuration event handlers
    private void OnConfigurationChanged(object sender, ConfigurationChangedEventArgs e)
    private void OnConfigurationValidating(object sender, ConfigurationValidationEventArgs e)
    private void OnConfigurationBackupCompleted(object sender, ConfigurationBackupEventArgs e)
}
```

#### 6.2 Event Handler Extensions
```csharp
public static class EventHandlerExtensions
{
    // Generic registration
    public static T RegisterEvents<T>(this T control, EventHandlerManager manager)
    public static T UnregisterEvents<T>(this T control, EventHandlerManager manager)
    
    // Control-specific registration
    public static TrackBar RegisterTrackBarEvents(...)
    public static CheckBox RegisterCheckBoxEvents(...)
    public static Button RegisterButtonEvents(...)
    public static TextBox RegisterTextBoxEvents(...)
}
```

#### 6.3 Event Categories
1. **Configuration Events**
   - Configuration changes
   - Validation events
   - Backup events

2. **Control Events**
   - TrackBar events (ValueChanged, Scroll)
   - CheckBox events (CheckedChanged)
   - Button events (Click, MouseDown, MouseUp)
   - TextBox events (TextChanged, KeyDown)

3. **System Events**
   - Window events
   - Application lifecycle events
   - Error events

#### 6.4 Event Handler Features
- Centralized event management
- Automatic cleanup on disposal
- Type-safe event registration
- Fluent API support
- Event tracking and logging
- Error handling and recovery

### 7. Project Structure Update
```
MouseMacro/
├── src/
│   ├── Configuration/           # Configuration system
│   │   ├── ConfigurationManager.cs
│   │   ├── AppConfiguration.cs
│   │   ├── ConfigurationEvents.cs
│   │   ├── EventHandlerManager.cs
│   │   └── EventHandlerExtensions.cs
│   ├── Controls/               # UI Controls
│   │   ├── ModernButton.cs
│   │   └── ModernTrackBar.cs
│   ├── MacroForm.cs           # Main form
│   ├── MacroForm.Designer.cs  # Form designer
│   └── Program.cs             # Entry point
```

## Macro Implementation Details

### 1. Core Macro Logic
- **Language**: C#
- **Primary Components**:
  - Recoil reduction system
  - Jitter pattern generation
  - State management
  - Timer-based execution

#### 1.1 Recoil Reduction System
- **Implementation**:
  ```csharp
  private void OnRecoilReductionTimer(object state)
  {
      if (IsRecoilReductionActive)
      {
          int strength = recoilReductionStrength.Value;
          int scaledStrength = CalculateScaledStrength(strength);
          SendInput.MoveMouse(0, scaledStrength);
      }
  }

  private int CalculateScaledStrength(int strength)
  {
      if (strength <= 6) // Tier 1
          return strength;
      else if (strength <= 13) // Tier 2
          return 6 + (strength - 6) * 2;
      else // Tier 3
          return 20 + (strength - 13) * 3;
  }
  ```
- **Features**:
  - Vertical movement compensation
  - Three-tier strength scaling system
  - Dynamic strength calculation
  - Optimized performance
  - Smooth movement patterns

#### 1.2 Jitter System
- **Pattern Definition**:
  ```csharp
  private readonly (int dx, int dy)[] jitterPattern = {
      (0, 6), (7, 7), (-7, -7), (7, -7), (-7, 7),
      (0, -6), (-6, 0), (6, 0), (5, 5), (-5, -5)
  };
  ```
- **Implementation**:
  ```csharp
  private void OnJitterTimer(object state)
  {
      if (IsJitterActive && currentPattern < jitterPattern.Length)
      {
          var (dx, dy) = jitterPattern[currentPattern];
          SendInput.MoveMouse(
              dx * jitterStrength.Value / 10,
              dy * jitterStrength.Value / 10
          );
          currentPattern = (currentPattern + 1) % jitterPattern.Length;
      }
  }
  ```
- **Features**:
  - Complex movement patterns
  - Pattern cycling
  - Strength scaling
  - Independent activation

### 2. State Management

#### 2.1 Activation States
```csharp
private bool IsRecoilReductionActive => 
    MacroEnabled && 
    (GetAsyncKeyState(VK_LBUTTON) < 0) && 
    (GetAsyncKeyState(VK_RBUTTON) < 0);

private bool IsJitterActive =>
    MacroEnabled && 
    (JitterEnabled || alwaysJitterMode) && 
    !alwaysRecoilReductionMode && 
    IsRecoilReductionActive;
```

#### 2.2 Mode Management
```csharp
private void ToggleMacroMode()
{
    // If either always mode is on, we can't switch modes
    if (alwaysJitterMode || alwaysRecoilReductionMode)
        return;

    // Toggle between jitter and recoil reduction modes
    jitterEnabled = !jitterEnabled;
    UpdateModeLabels();
}

private void UpdateModeLabels()
{
    lblJitterActive.Text = jitterEnabled || alwaysJitterMode ? "[Active]" : "";
    lblRecoilReductionActive.Text = !jitterEnabled || alwaysRecoilReductionMode ? "[Active]" : "";
}
```

#### 2.3 Toggle System
- **Keyboard Implementation**:
  ```csharp
  private IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
  {
      if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
      {
          var kb = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
          if (kb.vkCode == ToggleKey)
          {
              MacroEnabled = !MacroEnabled;
              UpdateWindowTitle();
          }
      }
      return CallNextHookEx(KeyboardHook, nCode, wParam, lParam);
  }
  ```

- **Mouse Implementation**:
  ```csharp
  private IntPtr MouseProc(int nCode, IntPtr wParam, IntPtr lParam)
  {
      if (nCode >= 0)
      {
          var mouseData = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
          if (wParam == (IntPtr)WM_XBUTTONDOWN)
          {
              int button = mouseData.mouseData >> 16;
              if (button == ToggleButton)
              {
                  MacroEnabled = !MacroEnabled;
                  UpdateWindowTitle();
              }
          }
      }
      return CallNextHookEx(MouseHook, nCode, wParam, lParam);
  }
  ```

### 3. Timer System

#### 3.1 Timer Configuration
```csharp
private readonly System.Windows.Forms.Timer recoilReductionTimer;
private readonly System.Windows.Forms.Timer jitterTimer;

private void InitializeTimers()
{
    recoilReductionTimer = new System.Windows.Forms.Timer
    {
        Interval = 16,  // ~60Hz
        Enabled = true
    };
    recoilReductionTimer.Tick += OnRecoilReductionTimer;

    jitterTimer = new System.Windows.Forms.Timer
    {
        Interval = 25,  // 40Hz
        Enabled = true
    };
    jitterTimer.Tick += OnJitterTimer;
}
```

#### 3.2 Performance Optimization
- **Timer Intervals**:
  - Recoil Reduction: 16ms (60Hz) for smooth movement
  - Jitter: 25ms (40Hz) for pattern execution
  - Balanced for performance and responsiveness

- **Resource Management**:
  ```csharp
  protected override void Dispose(bool disposing)
  {
      if (disposing)
      {
          recoilReductionTimer?.Dispose();
          jitterTimer?.Dispose();
      }
      base.Dispose(disposing);
  }
  ```

### 4. Input Simulation

#### 4.1 SendInput Implementation
```csharp
public static class SendInput
{
    [DllImport("user32.dll")]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    public static void MoveMouse(int dx, int dy)
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

#### 4.2 Low-Level Hooks
```csharp
private const int WH_KEYBOARD_LL = 13;
private const int WH_MOUSE_LL = 14;

private IntPtr SetWindowsHookEx(int idHook, HookProc lpfn)
{
    using var curProcess = Process.GetCurrentProcess();
    using var curModule = curProcess.MainModule;
    return SetWindowsHookEx(idHook, lpfn, 
        GetModuleHandle(curModule.ModuleName), 0);
}
```

### 5. Debug System

#### 5.1 State Monitoring
```csharp
private void UpdateDebugInfo()
{
    if (debugTextBox.Visible)
    {
        var info = $"[{DateTime.Now:HH:mm:ss.fff}] " +
                   $"Macro: {(MacroEnabled ? "ON" : "OFF")} | " +
                   $"Recoil Reduction: {(IsRecoilReductionActive ? "Active" : "Inactive")} | " +
                   $"Jitter: {(IsJitterActive ? "Active" : "Inactive")} | " +
                   $"LMB: {(GetAsyncKeyState(VK_LBUTTON) < 0)} | " +
                   $"RMB: {(GetAsyncKeyState(VK_RBUTTON) < 0)}";
        
        debugTextBox.AppendText(info + Environment.NewLine);
    }
}
```

#### 5.2 Performance Monitoring
```csharp
private readonly Stopwatch perfTimer = new();
private void MeasurePerformance(Action action, string operation)
{
    if (debugTextBox.Visible)
    {
        perfTimer.Restart();
        action();
        perfTimer.Stop();
        
        UpdateDebugInfo($"{operation}: {perfTimer.ElapsedMilliseconds}ms");
    }
    else
    {
        action();
    }
}
```

## Build System

### Configurations
1. **Debug Build**
   - Location: `bin/Debug/net6.0-windows/`
   - Debug symbols and logging
   - Development features enabled
   
2. **Release Build**
   - Location: `bin/Release/net6.0-windows/`
   - Optimized performance
   - Production ready

### Build Process
- **Automated Script**: `build.bat`
  - Admin privilege elevation
  - Environment preparation
  - Dual configuration builds
  - Error handling

## Usage Guide

### Application Lifecycle
1. **Startup**
   - Admin rights elevation
   - Single instance check
   - System tray initialization

2. **Runtime**
   - Background operation
   - Resource optimization
   - State management
   
3. **Shutdown**
   - Clean termination
   - Resource cleanup
   - Settings preservation

### Troubleshooting
1. **Common Issues**
   - Instance conflicts
   - Admin privileges
   - Performance optimization
   - Resource management

2. **Debug Mode**
   - Real-time monitoring
   - Event logging
   - Performance metrics
   - Error tracking

## Performance Optimization
1. **Resource Management**
   - Efficient hook handling
   - Minimal GC impact
   - Handle management
   
2. **CPU Usage**
   - Timer optimization
   - Event throttling
   - Efficient state checks

## Security Considerations
1. **Process Protection**
   - Mutex implementation
   - Process name obfuscation
   
2. **Privilege Management**
   - Manifest-based elevation
   - Runtime privilege checks
   - Secure API access

## Maintenance
1. **Regular Updates**
   - Runtime compatibility
   - Security patches
   - Feature updates
   
2. **Code Maintenance**
   - Performance optimization
   - Security audits
   - Documentation updates
