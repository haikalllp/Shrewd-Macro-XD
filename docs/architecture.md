# Mouse Macro Application Architecture

## Introduction
This document provides an overview of the architecture for the Mouse Macro Application. The application is designed to allow users to create and manage mouse macros, with a simple UI to change keybinds and adjust settings.

## Project Structure
```
MouseMacro/
├── docs/                    # Documentation
│   ├── architecture.md      # This architecture document
│   └── LuaScript.lua       # Original Lua implementation (reference)
├── src/                    # Source code
│   ├── MacroForm.cs        # Main form implementation
│   ├── MacroForm.Designer.cs # Form designer code
│   └── Program.cs          # Application entry point
├── MouseMacro.csproj       # Project file
└── app.manifest           # Application manifest for admin privileges
```

## Components

### 1. User Interface (UI)
- **Framework**: Windows Forms (WinForms)
- **Description**: A simple and intuitive interface for users to change keybinds for toggling the macro, and adjust the jitter strength.
- **Elements**:
  - **Toggle Key Display**: Shows the current toggle key in bold text
  - **Set Key Button**: Allows users to set a new toggle key by clicking and pressing the desired key
  - **Jitter Strength Slider**: For adjusting the jitter strength (1-20)
  - **Status Indication**: Window title shows macro state (ON/OFF)

### 2. Core Logic
- **Language**: C#
- **Description**: Handles the core functionality of toggling the macro, checking key states, and implementing jitter logic when both LMB and RMB are held.

#### 2.1 Keybind Handling
- **Classes**: `MacroForm`
- **Methods**:
  - `void OnToggleKeyPress(object sender, KeyEventArgs e)`: Handles key presses for both toggle activation and key binding
  - `void btnSetKey_Click(object sender, EventArgs e)`: Initiates the key binding process

#### 2.2 Jitter Logic
- **Methods**:
  - `void OnMouseMove(object sender, MouseEventArgs e)`: Checks if both LMB and RMB are held, and applies jitter if the macro is active
  - Real-time jitter strength adjustment through the TrackBar

### 3. Event Handling
- **Methods**:
  - `void MacroForm_Load(object sender, EventArgs e)`: Sets up the event handlers
  - `void MacroForm_FormClosing(object sender, FormClosingEventArgs e)`: Cleans up event handlers

## Setup and Configuration

1. **Development Environment**: 
   - Visual Studio or Visual Studio Code
   - .NET 6.0 SDK

2. **Build and Run**:
   ```bash
   dotnet build
   dotnet run
   ```

3. **Administrator Privileges**:
   - Required for global keyboard/mouse hooks
   - Configured in app.manifest

## User Guide

1. **Running the Application**:
   - Double-click the MouseMacro.exe in the bin/Debug/net6.0-windows folder
   - Or create a shortcut to the exe for easy access

2. **Setting the Toggle Key**:
   - Click the "Click to Set New Key" button
   - Press any key to set it as the new toggle key
   - The current toggle key is displayed in bold

3. **Adjusting Jitter Strength**:
   - Use the slider to set jitter strength (1-20)
   - Changes take effect immediately

4. **Using the Macro**:
   - Press the toggle key to turn the macro ON/OFF
   - Hold both LMB and RMB to activate the jitter effect
   - Window title shows current state (ON/OFF)

## Conclusion

This architecture provides a clear framework for a mouse macro application with an intuitive user interface. The application is designed with modular components to ensure maintainability and scalability.
