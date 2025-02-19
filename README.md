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

- **Modern UI**: 
  - Sleek, dark-themed interface
  - Split label system with bold values
  - Thread-safe updates and cross-thread synchronization
  - Consistent Segoe UI font styling
- **Toggle Key Configuration**: 
  - Easy key configuration with visual feedback
  - Current key displayed with bold styling
  - Thread-safe key state updates
- **Dual Macro Modes**:
  - Default Recoil Reducer
    - Constant vertical compensation
    - Primary strength slider (1-20)
    - Always accessible
  - Toggle Jitter Mode
    - Complex movement patterns
    - Independent strength control
    - Optional activation
- **Debug System**: 
  - Enhanced logging with standardized format
  - Real-time strength and key updates
  - Thread-safe message queue
  - Automatic UI synchronization
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
- **Default Mode (Recoil Reducer)**:
  - Active when macro is ON and both LMB + RMB held
  - Provides constant vertical compensation
  - Strength adjustable from 1-20
  - Always available when macro is ON

- **Optional Mode (Jitter)**:
  - Must be explicitly enabled via toggle
  - Uses separate strength settings
  - Independent activation from recoil reducer
  - Can be disabled when not needed

#### Detailed Behavior
1. **Toggle Key**
   - Controls overall macro ON/OFF state
   - Affects both recoil and jitter (if enabled)
   - Current state shown in window title

2. **Recoil Reducer**
   - Primary macro functionality
   - Activated by holding LMB + RMB
   - Strength controlled by dedicated slider
   - Takes effect immediately when macro is ON

3. **Jitter Mode**
   - Secondary, optional functionality
   - Can be toggled on/off independently
   - Separate strength control (1-20)
   - Only active when enabled and macro is ON

### Example Scenarios

- **Scenario 1**: Macro OFF → No effects active
- **Scenario 2**: Macro ON, Recoil Only → Vertical compensation when LMB + RMB held
- **Scenario 3**: Macro ON, Jitter Enabled → Both effects when LMB + RMB held
- **Scenario 4**: Macro ON, Single Button → No effects

### Tips
- Always check the application window to confirm the current macro state
- The toggle key provides a quick way to enable/disable the macro
- Experiment with different jitter strengths to find your preferred setting

## Architecture and Design

For a detailed overview of the system design and the evolution of the application, please refer to the [architecture.md](docs/architecture.md) document in the docs folder. It explains the UI layout, component interactions, and the rationale behind key design decisions.

## Troubleshooting

- **SDK Version Mismatch**: Ensure you have exactly .NET SDK 6.0.428 installed
- **Build Errors**: 
  - Verify all dependencies are restored
  - Check that you're using a compatible Windows version
  - Confirm Visual Studio or .NET development tools are installed
- **Icon or Resource Issues**: Ensure that the `assets` folder containing `logo.ico` is present in the project directory.
- **Permissions**: Global hooks and system tray operations may require running the application with appropriate privileges.
- **Build/Run Errors**: Clean and rebuild the project if you encounter unexpected errors.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.

## Acknowledgements

Thanks to the development team for designing this intuitive and reliable mouse macro application.