# Notes & Tasks (Mouse Macro)

A professional Windows Forms application for advanced mouse input management, featuring recoil compensation and jitter pattern generation. For professional system integration, the application presents itself as "Notes&Tasks" 😊.

## Features

- **Recoil Compensation**
  - Vertical movement compensation
  - Adjustable strength (1-20)
  - Real-time adjustments
  - Smooth pattern implementation

- **Jitter System**
  - Complex movement patterns
  - Independent strength control
  - Pattern cycling
  - Configurable activation

- **Professional Integration**
  - Clean system tray integration
  - Modern dark theme UI
  - Minimal resource usage
  - Single instance enforcement

## Requirements

### Hardware
- Windows 10/11 compatible PC
- DirectX compatible display
- Mouse with standard buttons
- Keyboard for hotkey support

### Software
- Windows 10/11 (64-bit)
- .NET 6.0 Runtime
- Administrator privileges
- DirectX 9.0c or later

### Optional
- Multi-button mouse for extended features
- High refresh rate display (recommended)
- SSD for faster startup (recommended)

## Installation

1. **Download**
   - Get the latest release from the releases page
   - Choose between Debug and Release builds

2. **Setup**
   - Extract the files to your preferred location
   - No installation required (portable application)
   - Run `NotesTasks.exe` with administrator privileges

## Building from Source

### Method 1: Using Build Script
1. Clone the repository
2. Double-click `build.bat`
   - Script automatically requests admin rights
   - Builds both Debug and Release configurations

### Method 2: Manual Build
1. Open command prompt
2. Navigate to project directory
3. Run commands:
   ```cmd
   # Debug build
   dotnet build -c Debug

   # Release build
   dotnet build -c Release
   ```

### Output Locations
- Debug: `bin/Debug/net6.0-windows/NotesTasks.exe`
- Release: `bin/Release/net6.0-windows/NotesTasks.exe`

## Usage Guide

### Basic Controls
1. **Toggle Key**
   - Click "Set Toggle Key" button
   - Press any key to set as toggle
   - Current toggle key shown in bold

2. **Strength Adjustment**
   - Use slider to set strength (1-20)
   - Changes apply immediately
   - Separate controls for recoil and jitter

3. **System Tray**
   - Optional minimize to tray
   - Double-click tray icon to restore
   - Right-click for context menu
   - Clean exit via tray menu

### Advanced Features

1. **Debug Panel**
   - Toggle with "Show Debug Info"
   - Real-time state monitoring
   - Performance metrics
   - Input state tracking

2. **Macro Activation**
   - Use toggle key for ON/OFF
   - Hold both LMB + RMB simultaneously for macro activation
   - You can toggle jiter mode
   - Window title shows current state

## Usage Scenarios

### Macro Activation Mechanics

1. **Basic Controls**
   - Toggle Key: Turns the entire macro system ON/OFF
   - Activation Trigger: Both LMB (Left Mouse Button) + RMB (Right Mouse Button) must be held simultaneously
   - Window Title: Shows current macro state (ON/OFF)

2. **Using Recoil Reducer**
   - First ensure macro is ON using toggle key
   - Hold both LMB + RMB simultaneously to activate
   - Recoil compensation starts immediately
   - Release either button to stop
   - Adjust strength (1-20) using the slider

3. **Using Jitter Mode**
   - Enable Jitter mode using the toggle
   - Ensure macro is ON using toggle key
   - Hold both LMB + RMB simultaneously to activate
   - Jitter pattern starts immediately
   - Release either button to stop
   - Adjust strength independently (1-20)

### Example Scenarios

1. **Recoil Only**
   ```
   1. Press Toggle Key → Macro ON
   2. Hold LMB + RMB together → Recoil compensation active
   3. Release either button → Recoil compensation stops
   4. Adjust strength as needed
   ```

2. **Recoil with Jitter**
   ```
   1. Press Toggle Key → Macro ON
   2. Enable Jitter mode
   3. Hold LMB + RMB together → Both effects active
   4. Release either button → All effects stop
   5. Adjust both strengths independently
   ```

3. **No Effects**
   ```
   - Macro OFF → No effects regardless of buttons
   - Macro ON but single button → No effects
   - Macro ON but buttons pressed separately → No effects
   ```

### Important Notes
- Effects ONLY activate when BOTH buttons are held simultaneously
- Releasing either button immediately stops all effects
- Toggle key provides quick way to disable all functionality
- Jitter can be toggled independently but requires same activation (LMB + RMB)
- Always check window title to confirm macro state

## Troubleshooting

### Common Issues

1. **"Another instance is running"**
   - Check Task Manager
   - End existing process if necessary
   - Restart application

2. **Admin Rights Required**
   - Run as administrator
   - Use build.bat for automatic elevation
   - Check app.manifest settings

3. **Performance Issues**
   - Switch to Release build
   - Check system resources
   - Adjust timer intervals
   - Monitor debug panel

## Development

### Environment Setup
1. Install Visual Studio 2022 or later
2. Install .NET 6.0 SDK
3. Clone repository
4. Open solution file

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
├── src/                    # Source code
│   ├── MacroForm.cs       # Main form implementation
│   ├── MacroForm.Designer.cs # Form designer code
│   └── Program.cs         # Application entry point
├── MouseMacro.csproj      # Project configuration
├── README.md              # Project documentation and setup guide
└── app.manifest           # Application manifest
```

## Contributing
1. Fork the repository
2. Create feature branch
3. Commit changes
4. Submit pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Windows Forms (.NET 6.0)
- Windows API (user32.dll)
- .NET Community