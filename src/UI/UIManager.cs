using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NotesAndTasks.Configuration;
using NotesAndTasks.Utilities;

namespace NotesAndTasks.UI
{
    /// <summary>
    /// Manages UI-specific operations and state updates.
    /// </summary>
    public class UIManager : IDisposable
    {
        #region Fields
        private readonly Form form;
        private readonly MacroManager macroManager;
        private readonly HotkeyManager hotkeyManager;
        private readonly TextBox debugLabel;
        private readonly Label lblJitterActive;
        private readonly Label lblRecoilReductionActive;
        private readonly Label lblCurrentKeyValue;
        private readonly Label lblMacroSwitchKeyValue;
        private readonly Label lblJitterStrengthValue;
        private readonly Label lblRecoilReductionStrengthValue;
        private readonly NotifyIcon notifyIcon;
        private readonly ToolTip toolTip;
        private bool disposed = false;
        #endregion

        #region Constructor
        public UIManager(
            Form form,
            MacroManager macroManager,
            HotkeyManager hotkeyManager,
            TextBox debugLabel,
            Label lblJitterActive,
            Label lblRecoilReductionActive,
            Label lblCurrentKeyValue,
            Label lblMacroSwitchKeyValue,
            Label lblJitterStrengthValue,
            Label lblRecoilReductionStrengthValue,
            NotifyIcon notifyIcon,
            ToolTip toolTip)
        {
            this.form = form ?? throw new ArgumentNullException(nameof(form));
            this.macroManager = macroManager ?? throw new ArgumentNullException(nameof(macroManager));
            this.hotkeyManager = hotkeyManager ?? throw new ArgumentNullException(nameof(hotkeyManager));
            this.debugLabel = debugLabel ?? throw new ArgumentNullException(nameof(debugLabel));
            this.lblJitterActive = lblJitterActive ?? throw new ArgumentNullException(nameof(lblJitterActive));
            this.lblRecoilReductionActive = lblRecoilReductionActive ?? throw new ArgumentNullException(nameof(lblRecoilReductionActive));
            this.lblCurrentKeyValue = lblCurrentKeyValue ?? throw new ArgumentNullException(nameof(lblCurrentKeyValue));
            this.lblMacroSwitchKeyValue = lblMacroSwitchKeyValue ?? throw new ArgumentNullException(nameof(lblMacroSwitchKeyValue));
            this.lblJitterStrengthValue = lblJitterStrengthValue ?? throw new ArgumentNullException(nameof(lblJitterStrengthValue));
            this.lblRecoilReductionStrengthValue = lblRecoilReductionStrengthValue ?? throw new ArgumentNullException(nameof(lblRecoilReductionStrengthValue));
            this.notifyIcon = notifyIcon ?? throw new ArgumentNullException(nameof(notifyIcon));
            this.toolTip = toolTip ?? throw new ArgumentNullException(nameof(toolTip));

            InitializeTooltips();
            InitializeIcon();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the form title to reflect current macro state and mode.
        /// </summary>
        public void UpdateTitle()
        {
            try
            {
                if (form == null || macroManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Cannot update title - form or macroManager is null");
                    return;
                }

                string jitterMode = macroManager.IsAlwaysJitterMode ? "Always Jitter" :
                    (macroManager.IsJitterEnabled ? "Jitter" : "Jitter (OFF)");

                string recoilMode = macroManager.IsAlwaysRecoilReductionMode ? "Always Recoil Reduction" :
                    (macroManager.IsJitterEnabled ? "Recoil Reduction (OFF)" : "Recoil Reduction");

                form.Text = $"Notes&Tasks [{(macroManager.IsEnabled ? "ON" : "OFF")}] - {jitterMode} / {recoilMode} Mode";
                UpdateModeLabels();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateTitle: {ex.Message}");
                // Use a safe default title if something goes wrong
                try
                {
                    form.Text = "Notes&Tasks";
                }
                catch
                {
                    // Ignore further errors
                }
            }
        }

        /// <summary>
        /// Updates the displayed current key binding for macro toggle.
        /// </summary>
        /// <param name="key">The key name to display.</param>
        public void UpdateCurrentKey(string key)
        {
            try
            {
                if (lblCurrentKeyValue == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Cannot update current key - label is null");
                    return;
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    System.Diagnostics.Debug.WriteLine("Warning: Empty key name provided to UpdateCurrentKey");
                    key = "None";
                }

                // Ensure we're on the UI thread
                if (lblCurrentKeyValue.InvokeRequired)
                {
                    lblCurrentKeyValue.Invoke(new Action<string>(UpdateCurrentKey), key);
                    return;
                }

                lblCurrentKeyValue.Text = key;
                UpdateDebugInfo($"Current key updated to: {key}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateCurrentKey: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the displayed key binding for mode switching.
        /// </summary>
        /// <param name="key">The key name to display.</param>
        public void UpdateSwitchKey(string key)
        {
            try
            {
                if (lblMacroSwitchKeyValue == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Cannot update switch key - label is null");
                    return;
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    System.Diagnostics.Debug.WriteLine("Warning: Empty key name provided to UpdateSwitchKey");
                    key = "None";
                }

                // Ensure we're on the UI thread
                if (lblMacroSwitchKeyValue.InvokeRequired)
                {
                    lblMacroSwitchKeyValue.Invoke(new Action<string>(UpdateSwitchKey), key);
                    return;
                }

                lblMacroSwitchKeyValue.Text = key;
                UpdateDebugInfo($"Switch key updated to: {key}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateSwitchKey: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the displayed jitter strength value.
        /// Thread-safe method that can be called from any thread.
        /// </summary>
        /// <param name="strength">The strength value to display (1-20).</param>
        public void UpdateJitterStrength(int strength)
        {
            try
            {
                if (lblJitterStrengthValue == null || form == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Cannot update jitter strength - UI elements are null");
                    return;
                }

                // Validate strength within acceptable range
                if (strength < 1 || strength > 20)
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Jitter strength value {strength} is outside valid range (1-20)");
                    strength = Math.Clamp(strength, 1, 20);
                }

                // Ensure we're on the UI thread
                if (form.InvokeRequired)
                {
                    form.Invoke(new Action<int>(UpdateJitterStrength), strength);
                    return;
                }

                lblJitterStrengthValue.Text = strength.ToString();
                UpdateDebugInfo($"Jitter strength updated to: {strength}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating jitter strength: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the displayed recoil reduction strength value.
        /// Thread-safe method that can be called from any thread.
        /// </summary>
        /// <param name="strength">The strength value to display (1-20).</param>
        public void UpdateRecoilReductionStrength(int strength)
        {
            try
            {
                if (lblRecoilReductionStrengthValue == null || form == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Cannot update recoil reduction strength - UI elements are null");
                    return;
                }

                // Validate strength within acceptable range
                if (strength < 1 || strength > 20)
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Recoil reduction strength value {strength} is outside valid range (1-20)");
                    strength = Math.Clamp(strength, 1, 20);
                }

                // Ensure we're on the UI thread
                if (form.InvokeRequired)
                {
                    form.Invoke(new Action<int>(UpdateRecoilReductionStrength), strength);
                    return;
                }

                lblRecoilReductionStrengthValue.Text = strength.ToString();
                UpdateDebugInfo($"Recoil reduction strength updated to: {strength}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating recoil reduction strength: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the displayed macro switch key value.
        /// Thread-safe method that can be called from any thread.
        /// </summary>
        /// <param name="key">The key name to display.</param>
        public void UpdateMacroSwitchKey(string key)
        {
            try
            {
                if (lblMacroSwitchKeyValue == null || form == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Cannot update macro switch key - UI elements are null");
                    return;
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    System.Diagnostics.Debug.WriteLine("Warning: Empty key name provided to UpdateMacroSwitchKey");
                    key = "None";
                }

                // Ensure we're on the UI thread
                if (form.InvokeRequired)
                {
                    form.Invoke(new Action<string>(UpdateMacroSwitchKey), key);
                    return;
                }

                lblMacroSwitchKeyValue.Text = key;
                UpdateDebugInfo($"Macro switch key updated to: {key}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateMacroSwitchKey: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the active mode labels in the UI.
        /// Thread-safe method that can be called from any thread.
        /// </summary>
        public void UpdateModeLabels()
        {
            try
            {
                if (form == null || macroManager == null || 
                    lblRecoilReductionActive == null || lblJitterActive == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Cannot update mode labels - UI elements are null");
                    return;
                }

                // Ensure we're on the UI thread
                if (form.InvokeRequired)
                {
                    form.Invoke(new Action(UpdateModeLabels));
                    return;
                }

                lblRecoilReductionActive.Text = (!macroManager.IsJitterEnabled && macroManager.IsEnabled) ? "[Active]" : "";
                lblJitterActive.Text = (macroManager.IsJitterEnabled && macroManager.IsEnabled) ? "[Active]" : "";
                
                System.Diagnostics.Debug.WriteLine($"Mode labels updated - Jitter: {(macroManager.IsJitterEnabled ? "active" : "inactive")}, " +
                    $"Recoil: {(!macroManager.IsJitterEnabled ? "active" : "inactive")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateModeLabels: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows and activates the main window when restored from system tray.
        /// </summary>
        public void ShowWindow()
        {
            try
            {
                if (form == null || notifyIcon == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Cannot show window - form or notifyIcon is null");
                    return;
                }

                form.Show();
                form.WindowState = FormWindowState.Normal;
                form.Activate();
                notifyIcon.Visible = false;
                UpdateDebugInfo("Application restored from system tray");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ShowWindow: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a debug message to the debug panel with timestamp.
        /// Thread-safe method that can be called from any thread.
        /// </summary>
        /// <param name="message">The debug message to display.</param>
        public void UpdateDebugInfo(string message)
        {
            try
            {
                if (debugLabel == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Debug message (not shown in UI): {message}");
                    return;
                }

                if (string.IsNullOrEmpty(message))
                {
                    return; // Skip empty messages
                }

                // Ensure we're on the UI thread
                if (debugLabel.InvokeRequired)
                {
                    debugLabel.Invoke(new Action(() => UpdateDebugInfo(message)));
                    return;
                }

                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                string newLine = $"[{timestamp}] {message}";

                // Keep last 100 lines of debug info
                var lines = new List<string>(debugLabel.Lines ?? Array.Empty<string>());
                lines.Add(newLine);
                if (lines.Count > 100)
                {
                    lines.RemoveAt(0);
                }
                
                debugLabel.Lines = lines.ToArray();

                // Auto-scroll to bottom
                debugLabel.SelectionStart = debugLabel.TextLength;
                debugLabel.ScrollToCaret();
            }
            catch (Exception ex)
            {
                // Last resort logging if debug panel fails
                System.Diagnostics.Debug.WriteLine($"Error in UpdateDebugInfo: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Original message: {message}");
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes tooltips for various UI controls to provide user guidance.
        /// </summary>
        private void InitializeTooltips()
        {
            try
            {
                if (form == null || toolTip == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Cannot initialize tooltips - form or toolTip is null");
                    return;
                }

                // Dictionary of control names and their tooltip text for safer access
                var tooltips = new Dictionary<string, string>
                {
                    { "chkAlwaysJitter", "Always keep Jitter enabled" },
                    { "trackBarJitter", "Adjust Jitter strength" },
                    { "chkAlwaysRecoilReduction", "Always keep Recoil Reduction enabled" },
                    { "trackBarRecoilReduction", "Adjust Recoil Reduction strength" },
                    { "chkMinimizeToTray", "Minimize to system tray when closing" }
                };

                foreach (var tooltip in tooltips)
                {
                    var control = form.Controls.Find(tooltip.Key, true).FirstOrDefault();
                    if (control != null)
                    {
                        toolTip.SetToolTip(control, tooltip.Value);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Control '{tooltip.Key}' not found for tooltip");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in InitializeTooltips: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes the application icon for both the main window and system tray.
        /// </summary>
        private void InitializeIcon()
        {
            try
            {
                if (form == null || notifyIcon == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Cannot initialize icon - form or notifyIcon is null");
                    return;
                }

                string appPath = Application.ExecutablePath;
                if (string.IsNullOrEmpty(appPath) || !System.IO.File.Exists(appPath))
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Application executable path is invalid: {appPath}");
                    return;
                }

                using var icon = Icon.ExtractAssociatedIcon(appPath);
                if (icon != null)
                {
                    form.Icon = (Icon)icon.Clone();
                    notifyIcon.Icon = (Icon)icon.Clone();
                    System.Diagnostics.Debug.WriteLine("Application icon initialized successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Warning: Failed to extract icon from executable");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading icon: {ex.Message}");
            }
        }
        #endregion

        #region IDisposable Implementation
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    notifyIcon?.Dispose();
                    toolTip?.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
} 