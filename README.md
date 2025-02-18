# Notes & Tasks Application

A Windows Forms application built with C# (.NET 6.0) designed for creating and managing mouse macros with customizable jitter effects. For system integration purposes, the application presents itself as "Notes&Tasks" to maintain a professional appearance.

## System Integration

- **Process Name**: `NotesTasks.exe`
- **Display Name**: Notes&Tasks
- **Window Title**: Notes&Tasks - [Status]
- **System Tray**: Appears as Notes&Tasks with corresponding icon
- **Task Manager**: Listed as Notes&Tasks

### Design Rationale
- Professional appearance in enterprise environments
- Discrete system integration
- Consistent branding across all system interfaces

## Features

- **Modern UI**: A sleek, dark-themed interface that provides a clear view of macro settings and debug information.
- **Toggle Key Configuration**: 
  - Easily set a toggle key to activate/deactivate the macro
  - Current toggle key displayed in bold text with light gray color
  - Flat-style "Set Key" button with hover effects
- **Jitter Logic**: 
  - Introduces jitter effects for mouse macros 
  - Adjustable strength via a modern trackbar (1-20)
  - Immediate effect on changes
  - Current strength value displayed above the slider
- **Debug Panel**: 
  - Collapsible panel showing real-time debug information
  - Includes event logging system
  - Performance metrics tracking
- **System Tray Integration**: 
  - Option to minimize to system tray instead of exiting
  - Tray icon context menu for window restoration or complete exit
- **Window Resizing**: Responsive design with adjustable controls
- **Single Instance Enforcement**: Utilizes a global mutex to ensure only one application instance runs

## Project Structure

```
MouseMacro/
├── assets/                  # Application resources
│   ├── logo.ico            # Application icon for Notes&Tasks branding
│   └── themes/             # UI theme resources and configurations
├── bin/                    # Compiled binaries and runtime files
│   └── Debug/             # Debug build output
│       └── net6.0-windows/
│           ├── NotesTasks.exe     # Main executable (Notes&Tasks branded)
│           ├── NotesTasks.dll     # Core application library
│           └── *.deps.json        # Runtime dependency configurations
├── docs/                   # Documentation
│   ├── architecture.md     # Detailed architecture documentation
│   ├── update.md           # Change log and updates
│   └── LuaScript.lua       # Original Lua implementation (reference)
├── src/                    # Source code
│   ├── MacroForm.cs        # Main form implementation
│   │   - Core macro logic
│   │   - Input handling and hooks
│   │   - UI event handlers
│   │   - System tray integration
│   ├── MacroForm.Designer.cs # Form designer code
│   │   - UI layout and controls
│   │   - Component initialization
│   └── Program.cs          # Application entry point with single instance enforcement
├── MouseMacro.csproj       # Project file with build configuration
└── README.md               # This file
```

## Installation and Build

### Prerequisites

- Windows OS (e.g., Windows 10)
- .NET 6.0 SDK
- Visual Studio or Visual Studio Code (recommended for editing)

### Build Instructions

1. Open a terminal in the project root directory
2. Run the following commands to clean and build the project:

   ```bash
   dotnet clean
   dotnet build
   ```

## Usage

1. **Setting Toggle Key**:
   - Click the "Set Key" button
   - Press the desired key to set as the toggle
   - Key is immediately saved and displayed

2. **Adjusting Jitter Strength**:
   - Use the slider to set jitter strength (1-20)
   - Changes take effect immediately
   - Current strength value is displayed above the slider

## Architecture and Design

For a detailed overview of the system design and the evolution of the application, please refer to the [architecture.md](docs/architecture.md) document in the docs folder. It explains the UI layout, component interactions, and the rationale behind key design decisions.

## Troubleshooting

- **Icon or Resource Issues**: Ensure that the `assets` folder containing `logo.ico` is present in the project directory.
- **Permissions**: Global hooks and system tray operations may require running the application with appropriate privileges.
- **Build/Run Errors**: Clean and rebuild the project if you encounter unexpected errors.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.

## Acknowledgements

Thanks to the development team for designing this intuitive and reliable mouse macro application.
