# Mouse Macro Application Architecture

## Introduction
This document provides an overview of the architecture for the Mouse Macro Application. The application is designed to allow users to create and manage mouse macros, with a modern dark-themed UI to change keybinds and adjust settings. For system integration purposes, the application presents itself as "Notes&Tasks" to maintain a professional appearance.

## System Integration
- **Process Name**: NotesTasks.exe
- **Display Name**: Notes&Tasks
- **Window Title**: Notes&Tasks - [Status]
- **System Tray**: Appears as Notes&Tasks with corresponding icon
- **Task Manager**: Listed as Notes&Tasks
- **Design Rationale**: 
  - Professional appearance in enterprise environments
  - Discrete system integration
  - Consistent branding across all system interfaces

## Project Structure

### Directory Organization
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
├── obj/                    # Intermediate build files
│   └── Debug/             # Debug build intermediates
│       └── net6.0-windows/
│           ├── ref/              # Assembly reference files
│           ├── refint/           # Reference interface files
│           └── *.cache           # Build cache files
├── docs/                   # Documentation
│   ├── architecture.md     # This architecture document
│   ├── update.md          # Change log and updates
│   └── LuaScript.lua      # Original Lua implementation (reference)
├── src/                   # Source code
│   ├── MacroForm.cs       # Main form implementation
│   │   - Core macro logic
│   │   - Input handling and hooks
│   │   - UI event handlers
│   │   - System tray integration
│   ├── MacroForm.Designer.cs # Form designer code
│   │   - UI layout and controls
│   │   - Component initialization
│   │   - Event wire-up
│   └── Program.cs         # Application entry point
│       - Instance management
│       - Process lifecycle
│       - Error handling
├── MouseMacro.csproj      # Project configuration
└── app.manifest          # Application manifest for privileges
```

### Directory Details

#### 1. `/assets` Directory
- **Purpose**: Contains all static resources used by the application
- **Contents**:
  - Application icons and branding materials
  - UI theme definitions and resources
  - Any additional static assets needed at runtime
- **Usage**: Resources are embedded into the final executable
- **Maintenance**: Update when modifying application appearance

#### 2. `/bin` Directory
- **Purpose**: Contains compiled application binaries
- **Structure**:
  - Organized by build configuration (Debug/Release)
  - Contains platform-specific outputs
- **Key Files**:
  - `NotesTasks.exe`: Main application executable
  - `NotesTasks.dll`: Core application library
  - Configuration and dependency files
- **Note**: Files here should not be version controlled

#### 3. `/obj` Directory
- **Purpose**: Holds intermediate compilation files
- **Contents**:
  - Temporary build artifacts
  - Assembly references
  - Compiler-generated files
- **Management**:
  - Automatically managed by build system
  - Should be excluded from version control
  - Safe to clean for fresh builds

#### 4. `/src` Directory
- **Purpose**: Contains all source code files
- **Organization**:
  - Separated by functionality
  - Clear file naming conventions
- **Key Components**:
  - `MacroForm.cs`: Core application logic
  - `MacroForm.Designer.cs`: UI definitions
  - `Program.cs`: Application entry point
- **Development Guidelines**:
  - Follow consistent coding style
  - Maintain separation of concerns
  - Document complex logic


## Components

### 1. User Interface (UI)
- **Framework**: Windows Forms (WinForms)
- **Theme**: Modern dark theme with a clean, minimalist design
- **System Identity**:
  - Application presents itself as "Notes&Tasks"
  - Process name: NotesTasks.exe
  - Window title shows as "Notes&Tasks - [Status]"
  - System tray icon and context menu maintain Notes&Tasks branding
  - Task Manager entry appears as Notes&Tasks
- **Description**: A modern and intuitive interface for users to manage macro settings and view debug information
- **Elements**:
  - **Toggle Key Display**: Shows the current toggle key in bold text with light gray color
  - **Set Key Button**: Flat-style button with hover effects for setting a new toggle key
  - **Recoil Control**: Primary strength slider (1-20) with bold value display
  - **Jitter Panel**: Optional mode controls with separate strength slider
  - **Debug Panel**: Collapsible panel showing real-time debug information
  - **Settings Panel**: Contains application settings like minimize to tray option
  - **Status Indication**: Window title shows macro state (ON/OFF)
- **Colors**:
  - Background: Dark gray (#1E1E1E)
  - Buttons: Slightly lighter gray (#2D2D2D)
  - Text: Light gray
  - Accent: Matches application icon colors
- **Window Features**:
  - Resizable with minimum size constraints (400x350)
  - Controls automatically adjust to window size
  - System tray integration with context menu

### 2. Core Logic
- **Language**: C#
- **Description**: Handles the core functionality of macro modes, toggle states, and implementing both recoil reduction and jitter patterns.

#### 2.1 Macro Modes
- **Default Mode: Recoil Reducer**
  - Primary functionality for vertical recoil compensation
  - Always available when macro is ON
  - Activated by LMB + RMB combination
  - Independent strength control (1-20)
- **Optional Mode: Jitter**
  - Secondary functionality for complex movement patterns
  - Can be toggled independently
  - Uses separate strength control
  - Maintains its own state

#### 2.2 Toggle System
- **Supported Toggle Methods**:
  - Keyboard Keys: Any keyboard key can be used as a toggle
  - Mouse Buttons:
    - Middle Button (Mouse3)
    - Side Button 1 (Mouse4/XBUTTON1)
    - Side Button 2 (Mouse5/XBUTTON2)
  - Note: Left and Right mouse buttons are reserved for macro activation
- **Implementation**:
  - Uses Windows Low-Level Mouse and Keyboard Hooks
  - Tracks toggle type (Keyboard/Mouse) via ToggleType enum
  - Handles mouse button detection through mouseData field in MSLLHOOKSTRUCT

#### 2.3 Instance Management
- **Class**: `Program`
- **Features**:
  - Global mutex to ensure single instance
  - Friendly message when attempting to run multiple instances
  - Proper cleanup on application exit

#### 2.4 Macro Logic
- **Methods**:
  - `void OnRecoilTimer(object state)`: Implements vertical recoil compensation
  - `void OnJitterTimer(object state)`: Implements complex movement patterns
  - `void CheckMacroState()`: Manages both recoil and jitter states
- **Features**:
  - Independent strength controls for each mode
  - Real-time adjustments through TrackBars
  - Smooth pattern implementation
  - Debug information display

### 3. System Tray Integration
- **Features**:
  - Optional minimize to tray functionality
  - Tray icon with context menu
  - Double-click to restore window
  - Clean process termination
- **Methods**:
  - `void ShowWindow()`: Restores window from tray
  - `void CleanupAndExit()`: Handles proper application shutdown

### 4. Debug System
- **Features**:
  - Collapsible debug panel
  - Real-time status updates
  - Timestamp for each debug message
  - Mouse button states
  - Current jitter settings
- **Methods**:
  - `void UpdateDebugInfo(string info)`: Adds timestamped debug information
  - `void btnToggleDebug_Click`: Toggles debug panel visibility


## Technical Implementation

### 1. Input Handling System

#### 1.1 Windows Hooks
- **Low-Level Keyboard Hook**
  - Intercepts keyboard events globally
  - Processes toggle key activation
  - Handles key state tracking
  - Hook ID: `WH_KEYBOARD_LL (13)`

- **Low-Level Mouse Hook**
  - Captures mouse events system-wide
  - Processes mouse button states
  - Handles jitter activation
  - Hook ID: `WH_MOUSE_LL (14)`

#### 1.2 Mouse Input Simulation
- **SendInput API Implementation**
  - Uses Windows API for low-level input
  - Simulates physical mouse movements
  - Supports both relative and absolute positioning
  - Bypasses standard cursor movement for game compatibility

#### 1.3 Toggle System
- **Keyboard Toggles**
  - Any keyboard key can be bound
  - Uses virtual key codes
  - State tracked via GetAsyncKeyState

- **Mouse Button Toggles**
  - Middle Button (Mouse3): `WM_MBUTTONDOWN (0x0207)`
  - Side Button 1 (Mouse4): `WM_XBUTTONDOWN + XBUTTON1`
  - Side Button 2 (Mouse5): `WM_XBUTTONDOWN + XBUTTON2`
  - Button states extracted from mouseData field

### 2. Macro Implementation

#### 2.1 Pattern Generation
- **Recoil Reduction Pattern**
  - Constant vertical movement
  - Configurable strength (1-20)
  - Primary macro functionality
  - Always available when enabled

- **Jitter Pattern Array**
  ```csharp
  private readonly (int dx, int dy)[] jitterPattern = {
      (0, 6), (7, 7), (-7, -7), /* ... */
  };
  ```
  - Optional secondary functionality
  - Complex movement patterns
  - Independent strength control
  - Toggle activation

#### 2.2 Timer System
- **Dual Timer Implementation**
  - Recoil Timer: Constant interval for smooth compensation
  - Jitter Timer: Pattern-based movement
  - Both use Windows.Forms.Timer
  - Independent operation and control

#### 2.3 State Management
- **Activation States**
  - Overall Macro State (ON/OFF)
  - Recoil State (Primary)
  - Jitter State (Optional)
  - Mouse Button States (LMB, RMB)
  - Mode Selection State

### 3. UI Framework

#### 3.1 Form Components
- **Main Window**
  - Modern dark theme implementation
  - Responsive layout design
  - Automatic DPI scaling
  - Minimum size constraints

- **Control Panels**
  - Recoil strength control (1-20)
  - Jitter mode toggle
  - Independent jitter strength (1-20)
  - Bold value displays for both modes

- **Debug Panel**
  - Real-time state monitoring
  - Mode status tracking
  - Strength value logging
  - Performance metrics

### 4. Error Handling and Logging

#### 4.1 Exception Management
- **Structured Error Handling**
  - Hook initialization errors
  - Input simulation failures
  - UI thread exceptions
  - Resource cleanup errors

#### 4.2 Debug Logging
- **Logging System**
  - Timestamped entries
  - State change tracking
  - Error reporting
  - Performance monitoring

### 5. Performance Considerations

#### 5.1 Resource Management
- **Memory Optimization**
  - Efficient hook handling
  - Minimal GC impact
  - Resource cleanup
  - Handle management

#### 5.2 CPU Usage
- **Optimization Techniques**
  - Timer interval tuning
  - Event throttling
  - Efficient state checks
  - Minimal redraws

### 6. Security Features

#### 6.1 Process Protection
- **Single Instance**
  - Global mutex implementation
  - Process name obfuscation
  - Clean termination handling

#### 6.2 Privilege Management
- **UAC Integration**
  - Manifest-based elevation
  - Runtime privilege checks
  - Secure API access

### 7. Customization Support

#### 7.1 User Preferences
- **Configurable Elements**
  - Toggle key binding
  - Jitter strength (1-20)
  - Recoil reducer strength (1-20)
  - UI theme settings
  - Startup behavior

#### 7.2 Persistence
- **Settings Storage**
  - User preferences saved
  - Window position/state
  - Last used configuration
  - Automatic restoration


## Troubleshooting and Maintenance

### 1. Common Issues

#### 1.1 Input Detection
- **Symptom**: Toggle key not responding
  - Check administrative privileges
  - Verify hook installation
  - Confirm no conflicts with other applications

- **Symptom**: Mouse buttons not detected
  - Verify mouse driver installation
  - Check Windows input settings
  - Confirm button mapping

#### 1.2 Performance
- **Symptom**: High CPU usage
  - Adjust timer interval
  - Check for competing processes
  - Verify system resources

- **Symptom**: Delayed response
  - Check event queue
  - Verify hook chain
  - Monitor system load

### 2. Maintenance Tasks

#### 2.1 Regular Updates
- Check for .NET runtime updates
- Verify Windows compatibility
- Update documentation
- Review security requirements

#### 2.2 Code Maintenance
- Regular code reviews
- Performance optimization
- Security audits
- Feature updates


## Usage Scenarios

### 1. Gaming Integration
- Compatible with DirectInput games
- Works with various game engines
- Supports windowed and fullscreen modes
- Minimal performance impact

### 2. Professional Use
- Discrete system integration
- Professional appearance
- Stable operation
- Resource-efficient

### 3. Configuration
- Easy toggle key setup
- Intuitive strength adjustment
- Quick enable/disable
- Persistent settings

## Build and Deployment

### Build Process
1. **Development Build**
   - Output goes to `/bin/Debug/net6.0-windows/`
   - Debug symbols and logging enabled
   - Development-specific configurations active

2. **Release Build**
   - Output goes to `/bin/Release/net6.0-windows/`
   - Optimized for performance
   - Debug features disabled
   - Branded as Notes&Tasks in all system interfaces

### Deployment Considerations
1. **Required Files**
   - `NotesTasks.exe` - Main executable
   - `NotesTasks.dll` - Core library
   - Associated configuration files
   - Embedded resources (icons, themes)

2. **System Requirements**
   - Windows OS (tested on Windows 10/11)
   - .NET 6.0 Runtime
   - Administrative privileges for input simulation

3. **Installation**
   - No installation required (portable application)
   - Copy required files to desired location
   - Run executable with administrative privileges

## Setup and Configuration

1. **Development Environment**: 
   - Visual Studio or Visual Studio Code
   - .NET 6.0 SDK

2. **Build and Run**:
   ```bash
   dotnet build --configuration Release
   ```
   The executable will be generated at:
   `bin/Release/net6.0-windows/NotesTasks.exe`

3. **Administrator Privileges**:
   - Required for global keyboard/mouse hooks
   - Configured in app.manifest


## User Guide

1. **Running the Application**:
   - Double-click NotesTasks.exe in the bin/Release/net6.0-windows folder
   - Application icon will be visible in taskbar and window title
   - Only one instance can run at a time

2. **Setting the Toggle Key**:
   - Click "Set Toggle Key" button
   - Press any key to set it as the new toggle key
   - The current toggle key is displayed in bold at the top

3. **Adjusting Jitter Strength**:
   - Use the slider to set jitter strength (1-20)
   - Changes take effect immediately
   - Current strength value is displayed above the slider

4. **Using Debug Information**:
   - Click "Show Debug Info" button to toggle the debug panel
   - Debug panel shows real-time status updates
   - Each debug message includes a timestamp

5. **System Tray Features**:
   - Check "Minimize to System Tray" to enable tray functionality
   - When enabled, closing window minimizes to tray instead of exiting
   - Double-click tray icon or use context menu to restore window
   - Use "Exit" in tray menu to fully close the application

6. **Using the Macro**:
   - Press the toggle key to turn the macro ON/OFF
   - Hold both LMB and RMB to activate the jitter effect
   - Window title shows current state (ON/OFF)
   - Window can be resized as needed


## Conclusion

This architecture provides a framework for a modern, user-friendly mouse macro application. The dark-themed UI offers excellent usability while maintaining a professional appearance. The application is designed with modular components to ensure maintainability and scalability, with added debug capabilities for troubleshooting. The system tray integration and single-instance management provide a polished, professional user experience.

