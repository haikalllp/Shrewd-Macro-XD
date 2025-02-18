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

#### .NET SDK 6.0.428
This project requires a specific version of the .NET SDK. Follow these steps to download and install:

1. **Download .NET SDK 6.0.428**
   - **Direct Download Link**: [.NET SDK 6.0.428 (Windows x64)](https://dotnet.microsoft.com/download/dotnet/6.0)
   - Navigate to the download page and select:
     - **Build apps - SDK**
     - **Windows x64** installer
   
2. **Installation Options**:
   a. **Recommended: Web Installer**
      - Automatically downloads the correct SDK version
      - Handles dependencies and system requirements
   
   b. **Offline Installer**
      - Useful for systems without direct internet access
      - Download the full installer from the Microsoft .NET website

3. **Verify Installation**
   After installation, open a new PowerShell or Command Prompt and run:
   ```powershell
   dotnet --version
   ```
   Confirm the output shows `6.0.428`

### Build Instructions

1. **Clone the Repository**
   ```powershell
   git clone https://github.com/haikalllp/MouseMacroApex
   cd MouseMacroApex
   ```

2. **Restore Dependencies**
   ```powershell
   dotnet restore
   ```

3. **Build the Project**
   ```powershell
   # Clean previous builds (optional)
   dotnet clean
   
   # Build the project
   dotnet build --configuration Release
   ```

4. **Run the Application**
   ```powershell
   # Navigate to the release directory
   cd bin\Release\net6.0-windows
   
   # Run the executable
   # Remember executeable are disguised as 'NotesTasks.exe'
   .\NotesTasks.exe
   ```

### Troubleshooting

- **SDK Version Mismatch**: Ensure you have exactly .NET SDK 6.0.428 installed
- **Build Errors**: 
  - Verify all dependencies are restored
  - Check that you're using a compatible Windows version
  - Confirm Visual Studio or .NET development tools are installed

### Development Environment

**Recommended Tools**:
- Visual Studio 2022 (Community Edition is free)
- Visual Studio Code with C# extension
- JetBrains Rider

**Optional but Recommended**:
- Install Windows Desktop Development workload in Visual Studio
- Ensure you have the latest Windows SDK

## Usage

### Macro Activation and Toggle

#### Macro Mechanics
- **Macro Activation**: The jitter macro is only active when **both Left Mouse Button (LMB) and Right Mouse Button (RMB) are held simultaneously**
- **Toggle State**: You can turn the macro on/off using the configured toggle key
  - When macro is ON: Jitter will trigger when LMB and RMB are held
  - When macro is OFF: No jitter will occur, even if mouse buttons are held

#### Detailed Behavior
1. **Toggle Key**
   - Press the configured toggle key to switch between ON and OFF states
   - The current state is displayed in the application window
   - Default state is OFF when the application starts

2. **Jitter Activation**
   - Hold down both LMB and RMB at the same time
   - Jitter effect will only apply when:
     a) Macro is in ON state
     b) Both LMB and RMB are held
   - Releasing either mouse button will immediately stop the jitter

3. **Jitter Strength**
   - Adjust the jitter strength using the slider in the main window
   - Strength ranges from 1 (minimal jitter) to 20 (maximum jitter)
   - Changes to strength take effect immediately

### Example Scenarios

- **Scenario 1**: Macro OFF, LMB + RMB held → No jitter
- **Scenario 2**: Macro ON, LMB + RMB held → Jitter active
- **Scenario 3**: Macro ON, only LMB held → No jitter
- **Scenario 4**: Macro ON, only RMB held → No jitter

### Tips
- Always check the application window to confirm the current macro state
- The toggle key provides a quick way to enable/disable the macro
- Experiment with different jitter strengths to find your preferred setting

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
