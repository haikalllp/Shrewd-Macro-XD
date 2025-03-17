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
            string jitterMode = macroManager.IsAlwaysJitterMode ? "Always Jitter" :
                (macroManager.IsJitterEnabled ? "Jitter" : "Jitter (OFF)");

            string recoilMode = macroManager.IsAlwaysRecoilReductionMode ? "Always Recoil Reduction" :
                (macroManager.IsJitterEnabled ? "Recoil Reduction (OFF)" : "Recoil Reduction");

            form.Text = $"Notes&Tasks [{(macroManager.IsEnabled ? "ON" : "OFF")}] - {jitterMode} / {recoilMode} Mode";
            UpdateModeLabels();
        }

        /// <summary>
        /// Updates the displayed current key binding for macro toggle.
        /// </summary>
        /// <param name="key">The key name to display.</param>
        public void UpdateCurrentKey(string key)
        {
            if (lblCurrentKeyValue != null)
            {
                lblCurrentKeyValue.Text = key;
            }
        }

        /// <summary>
        /// Updates the displayed key binding for mode switching.
        /// </summary>
        /// <param name="key">The key name to display.</param>
        public void UpdateSwitchKey(string key)
        {
            if (lblMacroSwitchKeyValue != null)
            {
                lblMacroSwitchKeyValue.Text = key;
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
                UpdateDebugInfo($"Error updating jitter strength: {ex.Message}");
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
                UpdateDebugInfo($"Error updating recoil reduction strength: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the displayed macro switch key value.
        /// Thread-safe method that can be called from any thread.
        /// </summary>
        /// <param name="key">The key name to display.</param>
        public void UpdateMacroSwitchKey(string key)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new Action<string>(UpdateMacroSwitchKey), key);
                return;
            }
            lblMacroSwitchKeyValue.Text = key;
            UpdateDebugInfo($"Macro switch key updated to: {key}");
        }

        /// <summary>
        /// Updates the active mode labels in the UI.
        /// Thread-safe method that can be called from any thread.
        /// </summary>
        public void UpdateModeLabels()
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new Action(UpdateModeLabels));
                return;
            }

            lblRecoilReductionActive.Text = (!macroManager.IsJitterEnabled && macroManager.IsEnabled) ? "[Active]" : "";
            lblJitterActive.Text = (macroManager.IsJitterEnabled && macroManager.IsEnabled) ? "[Active]" : "";
        }

        /// <summary>
        /// Shows and activates the main window when restored from system tray.
        /// </summary>
        public void ShowWindow()
        {
            form.Show();
            form.WindowState = FormWindowState.Normal;
            form.Activate();
            notifyIcon.Visible = false;
            UpdateDebugInfo("Application restored from system tray");
        }

        /// <summary>
        /// Adds a debug message to the debug panel with timestamp.
        /// Thread-safe method that can be called from any thread.
        /// </summary>
        /// <param name="message">The debug message to display.</param>
        public void UpdateDebugInfo(string message)
        {
            if (debugLabel.InvokeRequired)
            {
                debugLabel.Invoke(new Action(() => UpdateDebugInfo(message)));
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string newLine = $"[{timestamp}] {message}";

            // Keep last 100 lines of debug info
            var lines = debugLabel.Lines.ToList();
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
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes tooltips for various UI controls to provide user guidance.
        /// </summary>
        private void InitializeTooltips()
        {
            toolTip.SetToolTip(form.Controls.Find("chkAlwaysJitter", true).FirstOrDefault(), "Always keep Jitter enabled");
            toolTip.SetToolTip(form.Controls.Find("trackBarJitter", true).FirstOrDefault(), "Adjust Jitter strength");
            toolTip.SetToolTip(form.Controls.Find("chkAlwaysRecoilReduction", true).FirstOrDefault(), "Always keep Recoil Reduction enabled");
            toolTip.SetToolTip(form.Controls.Find("trackBarRecoilReduction", true).FirstOrDefault(), "Adjust Recoil Reduction strength");
            toolTip.SetToolTip(form.Controls.Find("chkMinimizeToTray", true).FirstOrDefault(), "Minimize to system tray when closing");
        }

        /// <summary>
        /// Initializes the application icon for both the main window and system tray.
        /// </summary>
        private void InitializeIcon()
        {
            try
            {
                using var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (icon != null)
                {
                    form.Icon = (Icon)icon.Clone();
                    notifyIcon.Icon = (Icon)icon.Clone();
                }
            }
            catch (Exception ex)
            {
                UpdateDebugInfo($"Error loading icon: {ex.Message}");
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