# Profile System Implementation Plan

This document outlines the detailed plan for implementing the Profile System feature in the "MouseMacro" (Notes & Tasks) application. The feature will allow users to save, load, update, and manage multiple configuration profiles, with a default profile that cannot be deleted or updated. Profiles will be stored as JSON files in a dedicated directory alongside the executable, and the UI will include a dropdown menu for profile management, adhering to the application's modern theme.

## Table of Contents

- [Profile System Implementation Plan](#profile-system-implementation-plan)
  - [Table of Contents](#table-of-contents)
  - [Update Configuration Storage](#update-configuration-storage)
  - [Modify the UI](#modify-the-ui)
  - [Implement Profile Management Logic](#implement-profile-management-logic)
  - [Update Existing Code](#update-existing-code)
  - [Testing](#testing)

---

## Update Configuration Storage

The current application saves its state settings to a single JSON file in the executable folder via the `ConfigurationManager`. This section details how to extend this to support multiple profiles stored in a dedicated "Profiles" directory.

- **Create Profiles Directory:**
  - In the `ConfigurationManager` constructor, check if a "Profiles" directory exists in the executable folder. If not, create it using `Directory.CreateDirectory`.

- **Save Profile:**
  - Add a `SaveProfile(string profileName)` method to `ConfigurationManager`.
  - Validate the profile name to ensure it’s not null or empty.
  - Check if a profile with the same name exists. If it does (and isn’t the default profile), prompt the user to confirm overwriting.
  - Serialize the current configuration to JSON and save it as `${profileName}.json` in the "Profiles" directory.

- **Load Profile:**
  - Add a `LoadProfile(string profileName)` method to `ConfigurationManager`.
  - Validate the profile name.
  - Check if `${profileName}.json` exists in the "Profiles" directory. If not, display an error message.
  - Deserialize the JSON file into an `AppConfiguration` object and update the current configuration.

- **Update Profile:**
  - Add an `UpdateProfile(string profileName)` method to `ConfigurationManager`.
  - Validate the profile name.
  - Check if the profile is the default profile. If so, prevent the update and show a message (e.g., "The default profile cannot be updated").
  - Serialize the current configuration to JSON and overwrite the existing `${profileName}.json` file.

- **Rename Profile:**
  - Add a `RenameProfile(string oldName, string newName)` method to `ConfigurationManager`.
  - Validate both old and new names.
  - Check if the old profile is the default profile or if the new name already exists. If either condition is true, prevent the rename and show a message.
  - Rename the file from `${oldName}.json` to `${newName}.json` in the "Profiles" directory.

- **Get Profile Names:**
  - Add a `GetProfileNames()` method to `ConfigurationManager`.
  - Scan the "Profiles" directory for `.json` files and return a list of profile names (without the `.json` extension).

- **Default Profile:**
  - Define a constant `DEFAULT_PROFILE_NAME = "Default"`.
  - In the `ConfigurationManager` initialization, check if `Default.json` exists in the "Profiles" directory. If not, create it with default settings.
  - Ensure methods that modify profiles (update, rename) block operations on the default profile.

---

## Modify the UI

The UI will be updated to include a dropdown menu and buttons for profile management, following the application’s modern theme (e.g., consistent fonts, colors, and styles).

- **Add Profile Selection Dropdown:**
  - Add a `ComboBox` named `cmbProfiles` to the `settingsPanel`.
  - Style it to match the modern theme (e.g., use `JetBrains Mono` font, current color scheme).
  - Populate it with available profiles using `configManager.GetProfileNames()` when the form loads or after profile operations.

- **Add Profile Management Buttons:**
  - Add `ModernButton`s for the following actions:
    - "Save New Profile" (`btnSaveProfile`)
    - "Load Profile" (`btnLoadProfile`)
    - "Update Profile" (`btnUpdateProfile`)
    - "Rename Profile" (`btnRenameProfile`)
  - Arrange these buttons logically within the `settingsPanel`, ensuring alignment with existing controls.

- **Display Current Profile:**
  - Add a label (e.g., `lblCurrentProfile`) to show the current profile name, formatted as "Current Profile: [profileName]".
  - If the current profile is the default, append "(Default)" or adjust button states (e.g., disable update/rename).

- **UI Theme:**
  - Use the existing modern theme for all new controls:
    - Font: `JetBrains Mono`
    - Colors: Match the current scheme (e.g., dark background, light text)
    - Styles: Use `ModernButton` for buttons and apply consistent padding/margins.

---

## Implement Profile Management Logic

This section covers the logic behind profile operations, triggered by UI interactions.

- **Save New Profile:**
  - On `btnSaveProfile.Click`:
    - Prompt the user for a profile name via a dialog or text input.
    - Call `configManager.SaveProfile(profileName)`.
    - If successful, refresh `cmbProfiles` and select the new profile.

- **Load Profile:**
  - On `btnLoadProfile.Click`:
    - Get the selected profile from `cmbProfiles`.
    - Call `configManager.LoadProfile(selectedProfile)`.
    - If successful, update the UI and application state with the loaded settings.

- **Update Profile:**
  - On `btnUpdateProfile.Click`:
    - Check if the current profile is not the default.
    - If allowed, call `configManager.UpdateProfile(currentProfile)`.
    - If the default profile is selected, show a message (e.g., "Cannot update the default profile").

- **Rename Profile:**
  - On `btnRenameProfile.Click`:
    - Prompt the user for a new name.
    - Call `configManager.RenameProfile(currentProfile, newName)`.
    - If successful, refresh `cmbProfiles` and select the renamed profile.
    - If the default profile is selected, show a message (e.g., "Cannot rename the default profile").

- **Handle Default Profile:**
  - When `Default` is selected in `cmbProfiles`:
    - Disable `btnUpdateProfile` and `btnRenameProfile`.
    - Show a message if the user attempts to modify it.

---

## Update Existing Code

Integrate the Profile System with the existing codebase to ensure seamless operation.

- **Load Last Used Profile:**
  - Add a `LastUsedProfile` property to `AppConfiguration`.
  - On application startup:
    - Load the last used profile from the main configuration file.
    - If none exists or it’s invalid, load the default profile.

- **Update Current Profile:**
  - When settings are modified (e.g., via sliders or checkboxes):
    - If the current profile isn’t the default, call `configManager.UpdateProfile(currentProfile)` automatically.
    - If it’s the default, prompt the user to save a new profile or discard changes.

- **Switch Profiles:**
  - On `cmbProfiles.SelectedIndexChanged`:
    - Call `configManager.LoadProfile(selectedProfile)`.
    - Apply the loaded settings to the application and update `lblCurrentProfile`.

---

## Testing

Thorough testing ensures the feature meets all requirements and handles edge cases.

- **Test Cases:**
  - **Save Profile:** Create a new profile and verify a `${profileName}.json` file appears in the "Profiles" directory.
  - **Load Profile:** Load different profiles and confirm settings (e.g., jitter strength, hotkeys) apply correctly.
  - **Update Profile:** Modify settings, update a profile, and check that the JSON file reflects the changes.
  - **Rename Profile:** Rename a profile and verify the file name changes in the "Profiles" directory.
  - **Default Profile Protection:** Attempt to update or rename the default profile and ensure it’s blocked with a message.
  - **UI Updates:** Confirm `cmbProfiles` and `lblCurrentProfile` refresh correctly after each operation.

- **Edge Cases:**
  - Save a profile with an existing name and verify overwrite confirmation.
  - Attempt to load a non-existent profile and check for error handling.
  - Switch between multiple profiles and ensure settings apply consistently.
  - Delete `Default.json` manually, restart the app, and confirm it’s recreated.

---

This plan ensures the Profile System meets all specified requirements:
- Saving and loading named profiles as JSON files in a "Profiles" directory.
- Updating and renaming profiles, with a protected default profile.
- A modern-themed UI with a dropdown menu for profile management.
- Integration with the existing configuration system.