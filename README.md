# Notes & Tasks (Mouse Macro)

A professional Windows Forms application for advanced mouse input management, featuring recoil compensation and jitter pattern generation. IMPORTANT NOTE: For professional system integration the application presents itself as "Notes&Tasks" and "NotesAndTasks" 😊.

## Features

- **Mouse Input Management**
  - Jitter Effect: Jitter mouse movement patterns strength (1-20)
  - Recoil Reduction: Vertical mouse movement compensation strength (1-20)
  - Multiple Operation Modes: Toggle or Always-On for each effect
  - Adjustable Effect Strengths: Fine-tune the intensity of each effect

- **Modern UI**
  - Dark Theme: Easy on the eyes during extended use
  - System Tray Integration: Minimize to tray for discrete operation
  - Real-time Status Display: Active effects and modes shown in window title
  - Customizable Window Position and Size: Remembers your preferences

#### Macro Engine
- **Recoil Reduction System**
  - Three-tier strength scaling:
    - Tier 1 (1-6): Linear scaling with logarithmic base
    - Tier 2 (7-16): Enhanced scaling with 1.2x multiplier
    - Tier 3 (17-20): Exponential scaling with dynamic boost
  - Real-time strength adjustment (1-20)
  - Optimized movement patterns
  - Default strength: 1
  - Advanced vertical compensation
  - Dynamic pattern adjustment

- **Jitter System**
  - Complex 24-point movement pattern
  - Dynamic strength scaling (1-20)
  - Pattern cycling with smooth transitions
  - Default strength: 3
  - Optimized for performance
  - Pattern customization options

- **Mode Switching**
  - Intelligent mode toggling
  - Support for keyboard and mouse buttons (Mouse3-5)
  - Always mode options with validation
  - Real-time mode state tracking
  - Visual state indicators

#### Configuration System
- **Configuration Models**
  - `AppSettings`: Root configuration class containing all settings
  - `MacroSettings`: Jitter and recoil reduction settings
  - `UISettings`: Window and interface preferences
  - `HotkeySettings`: Key bindings and hotkey configuration
  - Legacy configuration files for backward compatibility
  - settings save on loaded from and to JSON
  
- **Configuration Manager**
  - Thread-safe operations with ReaderWriterLockSlim
  - Standard location in %AppData%/NotesAndTasks
  - Automatic backups with versioning
  - Real-time validation with error recovery
  - Event notifications for changes, validation, and backups
  - Support for both new models and legacy configuration
  
- **Validation System**
  - Data annotations for property validation
  - Cross-property validation
  - Pre-save validation checks
  - Error reporting with context
  - Recovery mechanisms
  - Backward compatibility validation

- **Event System**
  - Configuration change tracking
  - Validation event handling
  - Backup completion notifications
  - Error event propagation
  - State change notifications
  - Legacy event support

#### UI System
- Modern dark theme interface
- Real-time status indicators
- Professional window management
- System tray integration
- DPI-aware scaling
- Single instance enforcement
- Clean system integration

#### Event System
- Centralized event management
- Type-safe event registration
- Automatic resource cleanup
- Comprehensive error handling:
  - Exception tracking
  - State recovery
  - Debug logging
- Categories:
  - UI events
  - Configuration events
  - Macro events
  - System events

## Requirements

### Hardware
- Windows 10/11 compatible PC
- DirectX compatible display
- Mouse with standard buttons
- Keyboard for hotkey support

### Software
- Windows 10/11 (64-bit)
- .NET 6.0 Runtime
- Administrator privileges (required for input simulation)
- DirectX 9.0c or later
- Standard mouse and keyboard

## Installation

### Download Latest Release
1. Download the latest `Notes&Tasks.exe` from the [releases page](https://github.com/haikalllp/Shrewd-Macro-XD/releases)
2. Extract the archive to your desired location
3. Run `Notes&Tasks.exe` with administrator privileges

## Usage Guide

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
1. **Starting the Macro**
   - Press the Macro Toggle Key to enable/disable
   - Status shown in window title and system tray

2. **Switching Modes**
   - Press Mode Switch Key to cycle between:
     - Jitter mode
     - Recoil reduction mode
     - Combined mode

3. **Quick Settings**
   - Use trackbars for real-time strength adjustment
   - Toggle "Always On" for persistent effects
   - Minimize to tray for discrete operation

### Usage Scenarios

#### 1. Dynamic Mode Switching
```
1. Press Toggle Key → Macro ON
2. Press Q to switch between modes
3. Hold LMB + RMB → Current mode activates
4. Release buttons → Effect stops
5. Press Q again → Switch to other mode
```

#### 2. Always Jitter Mode
```
1. Enable "Always Jitter Mode" checkbox
2. Press Toggle Key → Macro ON
3. Hold LMB + RMB → Jitter pattern active
4. Release buttons → Jitter stops
5. Q key has no effect (locked to jitter)
```

#### 3. Always Recoil Reduction Mode
```
1. Enable "Always Recoil Reduction Mode"
2. Press Toggle Key → Macro ON
3. Hold LMB + RMB → Recoil reduction active
4. Release buttons → Effect stops
5. Q key has no effect (locked to recoil reduction)
```

#### 4. Strength Optimization
```
1. Start with default strengths:
   - Recoil Reduction: 1
   - Jitter: 3
2. Test each mode
3. Adjust strength per mode as needed
4. Settings persist between mode switches
```

### Important Notes
- Effects ONLY activate when BOTH buttons are held
- Mode switch key (Q) works in real-time
- Always mode prevents accidental switching
- Each mode maintains its own strength setting
- Visual indicators show current active mode
- Window title reflects current state

### Advanced Features
1. **Custom Patterns**
   - Jitter patterns optimized for different scenarios
   - Recoil reduction with dynamic scaling
   - Combined mode synchronization

2. **Performance Optimization**
   - Timer-based execution for consistent timing
   - Resource-efficient hook implementation
   - Automatic cleanup on exit

## Development

### Environment Setup
1. **Required Tools**
   - Visual Studio 2022 or later
   - .NET 6.0 SDK
   - Git for version control

2. **Getting Started**
   ```powershell
   git clone https://github.com/haikalllp/Shrewd-Macro-XD.git
   cd MouseMacro
   ```

### Build From Source Options

#### Option 1: Using build.bat
1. Double-click `build.bat` in the project root
2. Script handles:
   - Admin rights elevation
   - NuGet package restoration
   - Debug and Release builds
   - Output directory creation

#### Option 2: Using Visual Studio
1. Open `MouseMacro.sln`
2. Select build configuration (Debug/Release)
3. Build solution (F6)

#### Option 3: Using Command Line
```powershell
# Debug build
dotnet build -c Debug

# Release build
dotnet build -c Release
```

### Output Locations
- Release: `bin/Release/net6.0-windows/Notes&Tasks.exe`
- Debug: `bin/Debug/net6.0-windows/Notes&Tasks.exe`

### Configuration Files
- Settings: `%AppData%/NotesAndTasks/config.json`
- Backups: `%AppData%/NotesAndTasks/Backups/`

### Testing
1. **Unit Tests**
   ```powershell
   dotnet test
   ```
   - Validates core functionality
   - Tests hook implementation
   - Verifies configuration management

2. **Manual Testing**
   - UI responsiveness
   - Feature functionality
   - Resource usage
   - Error handling

### Project Structure
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

## Troubleshooting

### Common Issues

1. **Application Won't Start**
   - Run as administrator
   - Check for existing instance
   - Verify .NET Runtime installation
   - Check Task Manager for hung instances
   - Verify file permissions

2. **Performance Issues**
   - Use Release build
   - Check system resources
   - Adjust timer intervals
   - Monitor debug output
   - Close resource-intensive applications

3. **Configuration Problems**
   - Delete corrupted config file
   - Check file permissions
   - Verify JSON format
   - Reset to defaults
   - Check backup configurations

### Error Handling
- **Validation Errors**
  - Automatic recovery
  - Default value fallback
  - User notification
  - State preservation

- **Runtime Errors**
  - Exception handling
  - Resource cleanup
  - State recovery
  - Debug logging
  - User feedback

## Contributing
1. Fork the repository
2. Create feature branch
3. Commit changes
4. Submit pull request

## License

This project is licensed under the [MIT License](LICENSE) - see the [LICENSE](LICENSE) file for details.

The MIT License is a permissive license that allows you to:
- ✅ Use the software commercially
- ✅ Modify the software
- ✅ Distribute the software
- ✅ Use the software privately
- ✅ Sublicense the software

Under the following conditions:
- ℹ️ License and copyright notice must be included
- ⚠️ Software is provided "as is", without warranty

## Acknowledgments

- Windows Forms (.NET 6.0)
- Windows API (user32.dll)
- .NET Community
