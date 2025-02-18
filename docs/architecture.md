# Mouse Macro Application Architecture

## Introduction
This document provides an overview of the architecture for the Mouse Macro Application. The application is designed to allow users to create and manage mouse macros, with a modern dark-themed UI to change keybinds and adjust settings.

## Project Structure
```
MouseMacro/
├── assets/                  # Application assets
│   └── logo.ico            # Application icon
├── docs/                    # Documentation
│   ├── architecture.md      # This architecture document
│   └── LuaScript.lua       # Original Lua implementation (reference)
├── src/                    # Source code
│   ├── MacroForm.cs        # Main form implementation
│   ├── MacroForm.Designer.cs # Form designer code
│   └── Program.cs          # Application entry point and instance management
├── MouseMacro.csproj       # Project file
└── app.manifest           # Application manifest for admin privileges
```

## Components

### 1. User Interface (UI)
- **Framework**: Windows Forms (WinForms)
- **Theme**: Modern dark theme with a clean, minimalist design
- **Description**: A modern and intuitive interface for users to manage macro settings and view debug information
- **Elements**:
  - **Toggle Key Display**: Shows the current toggle key in bold text with light gray color
  - **Set Key Button**: Flat-style button with hover effects for setting a new toggle key
  - **Jitter Strength Slider**: Modern trackbar for adjusting jitter strength (1-20)
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
- **Description**: Handles the core functionality of toggling the macro, checking key states, and implementing jitter logic when both LMB and RMB are held.

#### 2.1 Instance Management
- **Class**: `Program`
- **Features**:
  - Global mutex to ensure single instance
  - Friendly message when attempting to run multiple instances
  - Proper cleanup on application exit

#### 2.2 Keybind Handling
- **Class**: `MacroForm`
- **Methods**:
  - `void KeyboardHookCallback(...)`: Handles global keyboard input for toggle key
  - `void btnSetKey_Click(...)`: Initiates the key binding process

#### 2.3 Jitter Logic
- **Methods**:
  - `void OnJitterTimer(object state)`: Implements the jitter pattern with strength adjustment
  - `void CheckJitterState()`: Manages jitter state based on mouse button states
- **Features**:
  - Real-time jitter strength adjustment through TrackBar
  - Smooth jitter pattern implementation
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

## Setup and Configuration

1. **Development Environment**: 
   - Visual Studio or Visual Studio Code
   - .NET 6.0 SDK

2. **Build and Run**:
   ```bash
   dotnet build --configuration Release
   ```
   The executable will be generated at:
   `bin/Release/net6.0-windows/MouseMacro.exe`

3. **Administrator Privileges**:
   - Required for global keyboard/mouse hooks
   - Configured in app.manifest

## User Guide

1. **Running the Application**:
   - Double-click MouseMacro.exe in the bin/Release/net6.0-windows folder
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
