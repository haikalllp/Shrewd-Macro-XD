# Mouse Macro Application

A Windows Forms application built with C# (.NET 6.0) designed for creating and managing mouse macros with customizable jitter effects. This application features a modern, dark-themed UI, support for configuring a toggle key, debug information, system tray integration, and single-instance enforcement.

## Features

- **Modern UI**: A sleek, dark-themed interface that provides a clear view of macro settings and debug information.
- **Toggle Key Configuration**: Easily set a toggle key to activate/deactivate the macro. The active toggle key is displayed clearly on the main window.
- **Jitter Logic**: Introduces jitter effects for mouse macros with adjustable strength via a trackbar.
- **Debug Panel**: A collapsible panel that shows real-time debug information, including status updates and error messages.
- **System Tray Integration**: Option to minimize the application to the system tray instead of exiting. The tray icon includes a context menu for restoring the window or completely exiting the application.
- **Window Resizing**: The main window is resizable, and controls will adjust accordingly to fit the new size, ensuring a flexible user experience.
- **Single Instance Enforcement**: Utilizes a global mutex to ensure that only one instance of the application runs at a time.

## Project Structure

```
MouseMacro/
├── assets/                  # Application assets (e.g., logo.ico)
├── docs/                    # Documentation
│   ├── architecture.md      # Detailed architecture documentation
│   └── LuaScript.lua        # Original Lua implementation (reference)
├── src/                     # Source code
│   ├── MacroForm.cs         # Main form logic and UI behavior
│   ├── MacroForm.Designer.cs# Form designer code
│   └── Program.cs           # Application entry point with single instance enforcement
├── MouseMacro.csproj        # Project file with build configuration
└── README.md                # This file
```

## Installation and Build

### Prerequisites

- Windows OS (e.g., Windows 10)
- .NET 6.0 SDK
- Visual Studio or Visual Studio Code (recommended for editing)

### Build Instructions

1. Open a terminal in the project root directory (`e:\CODING\Projects\CODE\Macro`).
2. Run the following commands to clean and build the project:

   ```bash
   dotnet clean
   dotnet build --configuration Release
   ```

3. The executable will be generated at:
   `bin/Release/net6.0-windows/MouseMacro.exe`

## Usage

1. **Launching the Application**
   - Double-click the `MouseMacro.exe` or use the provided shortcut in the project root `Macro Jitter Apex`.
   - The application window displays the current toggle key, jitter strength, and provides buttons to set keys and toggle debug mode.

2. **Configuring the Toggle Key**
   - Click the "Set Toggle Key" button and press any key to assign a new toggle key for activating/deactivating the macro.
   - The selected key will be displayed as "Current Toggle Key: <Key>".

3. **Adjusting Jitter
   - Use the trackbar to adjust the jitter strength on the fly. The current value is displayed above the slider.

4. **Debug Mode**
   - Toggle the debug panel by clicking "Show Debug Info". This panel provides real-time updates and error messages.

5. **System Tray Integration and Exit Handling**
   - Check the "Minimize to System Tray" option to enable tray behavior. When enabled, closing the window minimizes the application to the system tray instead of terminating it.
   - To restore the application, double-click the tray icon or choose "Show Window" from its context menu.
   - To completely exit the application, click "Exit" from the tray context menu.

6. **Single Instance Enforcement**
   - The application prevents multiple instances by using a global mutex. If another instance is detected, an informative message is displayed.

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
