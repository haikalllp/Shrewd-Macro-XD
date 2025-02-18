# Mouse Macro Update Log

## Mouse Button Toggle Support Update (2025-02-18)

### Overview
Added support for using mouse buttons (Mouse3, Mouse4, Mouse5) as toggle keys for the macro functionality. This enhancement allows users to use additional mouse buttons instead of being limited to keyboard keys only.

### Detailed Changes

#### 1. New Constants Added
- `WM_MBUTTONDOWN (0x0207)`: For middle mouse button detection
- `WM_XBUTTONDOWN (0x020B)`: For side mouse buttons detection
- `XBUTTON1 (0x0001)`: For Mouse4
- `XBUTTON2 (0x0002)`: For Mouse5

#### 2. New Toggle Type System
Added `ToggleType` enum to track the type of toggle being used:
```csharp
private enum ToggleType
{
    Keyboard,
    MouseMiddle,
    MouseX1,
    MouseX2
}
```

#### 3. Mouse Hook Enhancements
- Modified `MouseHookCallback` to handle middle click and X buttons
- Added safety checks to prevent LMB/RMB from being set as toggle keys
- Added helper methods:
  - `IsXButton1`: Checks if Mouse4 is pressed
  - `IsXButton2`: Checks if Mouse5 is pressed

#### 4. UI Updates
- Updated toggle key display to show mouse button names:
  - "Mouse3 (Middle)"
  - "Mouse4"
  - "Mouse5"
- Enhanced debug information to show mouse button toggle states

### User-Facing Changes
1. Users can now set the following as toggle keys:
   - Middle mouse button (Mouse3)
   - Forward side button (Mouse4)
   - Back side button (Mouse5)
   - Any keyboard key (maintained from previous version)

2. Left and Right mouse buttons remain reserved for macro activation
   - LMB + RMB combination activates the jitter
   - These buttons cannot be set as toggle keys

### Technical Implementation
- Uses Windows API's `mouseData` field to detect specific X buttons
- Implements bit manipulation to extract X button information
- Maintains separate toggle type tracking for proper event handling
- Preserves existing keyboard toggle functionality

### Notes
- Administrator privileges are still required for proper functionality
- System tray functionality remains unchanged
- Jitter strength adjustment works the same with all toggle types
