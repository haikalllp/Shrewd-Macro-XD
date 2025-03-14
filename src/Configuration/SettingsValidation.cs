using System.Windows.Forms;

public static class SettingsValidation
{
    public static bool IsValidHotkey(Keys key)
    {
        // Validate that the key is a valid hotkey
        return key != Keys.None && 
               key != Keys.LButton && 
               key != Keys.RButton;
    }

    // ... existing code ...
} 