# Notes & Tasks (Mouse Macro)

A professional Windows Forms application for advanced mouse input management, featuring recoil compensation and jitter pattern generation. For professional system integration, the application presents itself as "Notes&Tasks" 😊.

## Features

- **Recoil Reduction System**
  - Vertical movement compensation
  - Adjustable strength (1-20)
  - Default strength: 1
  - Real-time adjustments

- **Jitter System**
  - Complex movement patterns
  - Strength control (1-20)
  - Default strength: 3
  - Pattern cycling

- **Mode Switching**
  - Toggle between modes (Default: Q)
  - Support for mouse buttons 3-5 for key bindings
  - Always mode options
  - Active state indicators
  - Independent mode states

- **Professional Integration**
  - Clean system tray integration
  - Modern dark theme UI
  - Minimal resource usage
  - Single instance enforcement

- **Settings Saved with JSON**
  - Settings saved automatically into a JSON file
  - JSON file located at the executable directory
  - Settings automatically saved while running the Application
  - Settings automatically loaded upon starting the Application

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
1. **Macro Toggle Key (Macro ON/OFF)**
   - Click "Set Toggle Key" button
   - Press any key to set as macro toggle
   - Default: 'Capital' key
   - Supports keyboard and mouse buttons (Mouse3-5)
   - LMB/RMB reserved for activation

2. **Mode Switch Key**
   - Click "Set Switch Key" button
   - Press any key to set as switch toggle
   - Default: 'Q' key
   - Supports keyboard and mouse buttons (Mouse3-5)
   - LMB/RMB reserved for activation

3. **Strength Adjustment**
   - Use slider to set strength (1-20)
   - Changes apply immediately
   - Recoil Reduction default: 1
   - Jitter default: 3

4. **Always Mode Options**
   - Always Jitter Mode: Locks to jitter
   - Always Recoil Reduction Mode: Locks to recoil reduction
   - Prevents mode switching while active

5. **System Tray**
   - Optional minimize to tray
   - Double-click tray icon to restore
   - Right-click for context menu
   - Clean exit via tray menu

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

### Settings Configuration
- Settings stored in JSON format next to executable
- All preferences automatically saved
- Settings include:
  - Key bindings
  - Mode preferences
  - UI configuration
  - Strength values
- Invalid settings automatically reset to defaults

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
│   ├── ModernButton.cs    # Custom button control
│   ├── ModernTrackBar.cs  # Custom trackbar control
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