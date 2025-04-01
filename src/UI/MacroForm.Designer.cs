using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NotesAndTasks.Hooks;

namespace NotesAndTasks
{
    partial class MacroForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unsubscribe from events
                if (chkAlwaysJitter != null)
                {
                    chkAlwaysJitter.CheckedChanged -= alwaysJitterCheckedChanged;
                }
                if (chkAlwaysRecoilReduction != null)
                {
                    chkAlwaysRecoilReduction.CheckedChanged -= alwaysRecoilReductionCheckedChanged;
                }

                // Dispose other resources
                if (components != null)
                {
                    components.Dispose();
                }

                // Dispose managers
                if (macroManager != null)
                {
                    macroManager.Dispose();
                }

                // Dispose hooks
                if (keyboardHook != null)
                {
                    keyboardHook.Stop();
                    keyboardHook.Dispose();
                }

                if (mouseHook != null)
                {
                    mouseHook.Stop();
                    mouseHook.Dispose();
                }

                // Dispose UI resources
                if (notifyIcon != null)
                {
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();
                }

                if (toolTip != null)
                {
                    toolTip.Dispose();
                }

                if (uiManager != null)
                {
                    uiManager.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            mainPanel = new Panel();
            debugPanel = new Panel();
            debugLabel = new TextBox();
            btnToggleDebug = new ModernButton();
            strengthPanel1 = new Panel();
            lblRecoilReductionStrengthValue = new Label();
            lblRecoilReductionStrengthPrefix = new Label();
            trackBarRecoilReduction = new ModernTrackBar();
            lblRecoilReductionActive = new Label();
            strengthPanel2 = new Panel();
            lblJitterStrengthValue = new Label();
            lblJitterStrengthPrefix = new Label();
            trackBarJitter = new ModernTrackBar();
            lblJitterActive = new Label();
            settingsPanel = new Panel();
            lblCurrentKeyValue = new Label();
            lblCurrentKeyPrefix = new Label();
            btnSetKey = new ModernButton();
            chkMinimizeToTray = new CheckBox();
            lblMacroSwitchKeyValue = new Label();
            btnSetMacroSwitch = new ModernButton();
            lblMacroSwitchKeyPrefix = new Label();
            chkAlwaysJitter = new CheckBox();
            chkAlwaysRecoilReduction = new CheckBox();
            notifyIcon = new NotifyIcon(components);
            trayContextMenu = new ContextMenuStrip(components);
            showWindowMenuItem = new ToolStripMenuItem();
            exitMenuItem = new ToolStripMenuItem();

            mainPanel.SuspendLayout();
            debugPanel.SuspendLayout();
            strengthPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarRecoilReduction).BeginInit();
            strengthPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarJitter).BeginInit();
            settingsPanel.SuspendLayout();
            trayContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.FromArgb(16, 2, 31);
            mainPanel.Controls.Add(debugPanel);
            mainPanel.Controls.Add(btnToggleDebug);
            mainPanel.Controls.Add(strengthPanel1);
            mainPanel.Controls.Add(strengthPanel2);
            mainPanel.Controls.Add(settingsPanel);
            mainPanel.Controls.Add(chkAlwaysJitter);
            mainPanel.Controls.Add(chkAlwaysRecoilReduction);
            mainPanel.Location = new Point(0, 0);
            mainPanel.Margin = new Padding(3, 2, 3, 2);
            mainPanel.Name = "mainPanel";
            mainPanel.Padding = new Padding(14, 12, 14, 12);
            mainPanel.Size = new Size(477, 676);
            mainPanel.TabIndex = 0;
            // 
            // debugPanel
            // 
            debugPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            debugPanel.AutoScroll = true;
            debugPanel.BackColor = Color.FromArgb(214, 37, 106);
            debugPanel.BorderStyle = BorderStyle.FixedSingle;
            debugPanel.Controls.Add(debugLabel);
            debugPanel.Location = new Point(20, 541);
            debugPanel.Margin = new Padding(3, 2, 3, 2);
            debugPanel.Name = "debugPanel";
            debugPanel.Padding = new Padding(7, 6, 7, 6);
            debugPanel.Size = new Size(438, 124);
            debugPanel.TabIndex = 7;
            debugPanel.Visible = false;
            // 
            // debugLabel
            // 
            debugLabel.BackColor = Color.FromArgb(16, 2, 31);
            debugLabel.BorderStyle = BorderStyle.None;
            debugLabel.Dock = DockStyle.Fill;
            debugLabel.Font = new Font("JetBrains Mono", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            debugLabel.ForeColor = Color.White;
            debugLabel.Location = new Point(7, 6);
            debugLabel.Margin = new Padding(3, 2, 3, 2);
            debugLabel.Multiline = true;
            debugLabel.Name = "debugLabel";
            debugLabel.ReadOnly = true;
            debugLabel.ScrollBars = ScrollBars.Vertical;
            debugLabel.Size = new Size(422, 110);
            debugLabel.TabIndex = 0;
            // 
            // btnToggleDebug
            // 
            btnToggleDebug.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnToggleDebug.BackColor = Color.FromArgb(30, 1, 62);
            btnToggleDebug.BorderColor = Color.FromArgb(250, 91, 101);
            btnToggleDebug.BorderRadius = 10;
            btnToggleDebug.FlatStyle = FlatStyle.Flat;
            btnToggleDebug.Font = new Font("JetBrains Mono", 10F, FontStyle.Regular, GraphicsUnit.Point);
            btnToggleDebug.ForeColor = Color.White;
            btnToggleDebug.HoverBackColor = Color.FromArgb(214, 37, 106);
            btnToggleDebug.Location = new Point(21, 495);
            btnToggleDebug.Margin = new Padding(0, 0, 0, 12);
            btnToggleDebug.Name = "btnToggleDebug";
            btnToggleDebug.Size = new Size(437, 35);
            btnToggleDebug.TabIndex = 5;
            btnToggleDebug.Text = "Show Debug Info";
            btnToggleDebug.UseVisualStyleBackColor = false;
            // 
            // strengthPanel1
            // 
            strengthPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            strengthPanel1.Controls.Add(lblRecoilReductionStrengthValue);
            strengthPanel1.Controls.Add(lblRecoilReductionStrengthPrefix);
            strengthPanel1.Controls.Add(trackBarRecoilReduction);
            strengthPanel1.Controls.Add(lblRecoilReductionActive);
            strengthPanel1.Location = new Point(20, 225);
            strengthPanel1.Margin = new Padding(0, 0, 0, 12);
            strengthPanel1.Name = "strengthPanel1";
            strengthPanel1.Size = new Size(438, 84);
            strengthPanel1.TabIndex = 9;
            // 
            // lblRecoilReductionStrengthValue
            // 
            lblRecoilReductionStrengthValue.AutoSize = true;
            lblRecoilReductionStrengthValue.Font = new Font("JetBrains Mono", 10F, FontStyle.Bold, GraphicsUnit.Point);
            lblRecoilReductionStrengthValue.ForeColor = Color.White;
            lblRecoilReductionStrengthValue.Location = new Point(212, 6);
            lblRecoilReductionStrengthValue.Margin = new Padding(0);
            lblRecoilReductionStrengthValue.Name = "lblRecoilReductionStrengthValue";
            lblRecoilReductionStrengthValue.Size = new Size(16, 18);
            lblRecoilReductionStrengthValue.TabIndex = 1;
            lblRecoilReductionStrengthValue.Text = "1";
            // 
            // lblRecoilReductionStrengthPrefix
            // 
            lblRecoilReductionStrengthPrefix.AutoSize = true;
            lblRecoilReductionStrengthPrefix.Font = new Font("JetBrains Mono", 10.2F, FontStyle.Regular, GraphicsUnit.Point);
            lblRecoilReductionStrengthPrefix.ForeColor = Color.White;
            lblRecoilReductionStrengthPrefix.Location = new Point(0, 6);
            lblRecoilReductionStrengthPrefix.Margin = new Padding(0);
            lblRecoilReductionStrengthPrefix.Name = "lblRecoilReductionStrengthPrefix";
            lblRecoilReductionStrengthPrefix.Size = new Size(216, 18);
            lblRecoilReductionStrengthPrefix.TabIndex = 0;
            lblRecoilReductionStrengthPrefix.Text = "Recoil Reduction Strength:";
            // 
            // trackBarRecoilReduction
            // 
            trackBarRecoilReduction.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trackBarRecoilReduction.Location = new Point(0, 37);
            trackBarRecoilReduction.Margin = new Padding(0);
            trackBarRecoilReduction.Maximum = 20;
            trackBarRecoilReduction.Minimum = 1;
            trackBarRecoilReduction.Name = "trackBarRecoilReduction";
            trackBarRecoilReduction.Size = new Size(438, 45);
            trackBarRecoilReduction.TabIndex = 8;
            trackBarRecoilReduction.ThumbColor = Color.FromArgb(255, 255, 255);
            trackBarRecoilReduction.TickColor = Color.FromArgb(250, 91, 101);
            trackBarRecoilReduction.TrackColor = Color.FromArgb(214, 37, 106);
            trackBarRecoilReduction.Value = 1;
            // 
            // lblRecoilReductionActive (RecoilReduction Active Label)
            // 
            lblRecoilReductionActive.AutoSize = true;
            lblRecoilReductionActive.Font = new Font("JetBrains Mono", 10.2F, FontStyle.Regular, GraphicsUnit.Point);
            lblRecoilReductionActive.ForeColor = Color.FromArgb(250, 91, 101);
            lblRecoilReductionActive.Location = new Point(350, 5);
            lblRecoilReductionActive.Name = "lblRecoilReductionActive";
            lblRecoilReductionActive.Size = new Size(0, 20);
            lblRecoilReductionActive.TabIndex = 1;
            lblRecoilReductionActive.TextAlign = ContentAlignment.MiddleRight;
            // 
            // strengthPanel2
            // 
            strengthPanel2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            strengthPanel2.Controls.Add(lblJitterStrengthValue);
            strengthPanel2.Controls.Add(lblJitterStrengthPrefix);
            strengthPanel2.Controls.Add(trackBarJitter);
            strengthPanel2.Controls.Add(lblJitterActive);
            strengthPanel2.Location = new Point(20, 331);
            strengthPanel2.Margin = new Padding(0, 0, 0, 12);
            strengthPanel2.Name = "strengthPanel2";
            strengthPanel2.Size = new Size(438, 81);
            strengthPanel2.TabIndex = 10;
            // 
            // lblJitterStrengthValue
            // 
            lblJitterStrengthValue.AutoSize = true;
            lblJitterStrengthValue.Font = new Font("JetBrains Mono", 10F, FontStyle.Bold, GraphicsUnit.Point);
            lblJitterStrengthValue.ForeColor = Color.White;
            lblJitterStrengthValue.Location = new Point(132, 6);
            lblJitterStrengthValue.Margin = new Padding(0);
            lblJitterStrengthValue.Name = "lblJitterStrengthValue";
            lblJitterStrengthValue.Size = new Size(16, 18);
            lblJitterStrengthValue.TabIndex = 1;
            lblJitterStrengthValue.Text = "3";
            // 
            // lblJitterStrengthPrefix
            // 
            lblJitterStrengthPrefix.AutoSize = true;
            lblJitterStrengthPrefix.Font = new Font("JetBrains Mono", 10.2F, FontStyle.Regular, GraphicsUnit.Point);
            lblJitterStrengthPrefix.ForeColor = Color.White;
            lblJitterStrengthPrefix.Location = new Point(0, 6);
            lblJitterStrengthPrefix.Margin = new Padding(0);
            lblJitterStrengthPrefix.Name = "lblJitterStrengthPrefix";
            lblJitterStrengthPrefix.Size = new Size(144, 18);
            lblJitterStrengthPrefix.TabIndex = 0;
            lblJitterStrengthPrefix.Text = "Jitter Strength: ";
            // 
            // trackBarJitter
            // 
            trackBarJitter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trackBarJitter.Location = new Point(0, 34);
            trackBarJitter.Margin = new Padding(0);
            trackBarJitter.Maximum = 20;
            trackBarJitter.Minimum = 1;
            trackBarJitter.Name = "trackBarJitter";
            trackBarJitter.Size = new Size(438, 45);
            trackBarJitter.TabIndex = 3;
            trackBarJitter.ThumbColor = Color.FromArgb(255, 255, 255);
            trackBarJitter.TickColor = Color.FromArgb(250, 91, 101);
            trackBarJitter.TrackColor = Color.FromArgb(214, 37, 106);
            trackBarJitter.Value = 3;
            // 
            // lblJitterActive (Jitter Active Label)
            // 
            lblJitterActive.AutoSize = true;
            lblJitterActive.Font = new Font("JetBrains Mono", 10.2F, FontStyle.Regular, GraphicsUnit.Point);
            lblJitterActive.ForeColor = Color.FromArgb(250, 91, 101);
            lblJitterActive.Location = new Point(350, 5);
            lblJitterActive.Name = "lblJitterActive";
            lblJitterActive.Size = new Size(0, 20);
            lblJitterActive.TabIndex = 0;
            lblJitterActive.TextAlign = ContentAlignment.MiddleRight;
            // 
            // settingsPanel
            // 
            settingsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            settingsPanel.Controls.Add(lblCurrentKeyValue);
            settingsPanel.Controls.Add(lblCurrentKeyPrefix);
            settingsPanel.Controls.Add(btnSetKey);
            settingsPanel.Controls.Add(chkMinimizeToTray);
            settingsPanel.Controls.Add(lblMacroSwitchKeyValue);
            settingsPanel.Controls.Add(btnSetMacroSwitch);
            settingsPanel.Controls.Add(lblMacroSwitchKeyPrefix);
            settingsPanel.Location = new Point(20, 25);
            settingsPanel.Margin = new Padding(0, 0, 0, 12);
            settingsPanel.Name = "settingsPanel";
            settingsPanel.Size = new Size(438, 179);
            settingsPanel.TabIndex = 11;
            // 
            // lblCurrentKeyValue
            // 
            lblCurrentKeyValue.AutoSize = true;
            lblCurrentKeyValue.Font = new Font("JetBrains Mono", 10.2F, FontStyle.Bold, GraphicsUnit.Point);
            lblCurrentKeyValue.ForeColor = Color.White;
            lblCurrentKeyValue.Location = new Point(141, 22);
            lblCurrentKeyValue.Margin = new Padding(0);
            lblCurrentKeyValue.Name = "lblCurrentKeyValue";
            lblCurrentKeyValue.Size = new Size(64, 18);
            lblCurrentKeyValue.TabIndex = 1;
            lblCurrentKeyValue.Text = "Capital";
            // 
            // lblCurrentKeyPrefix
            // 
            lblCurrentKeyPrefix.AutoSize = true;
            lblCurrentKeyPrefix.Font = new Font("JetBrains Mono", 10.2F, FontStyle.Regular, GraphicsUnit.Point);
            lblCurrentKeyPrefix.ForeColor = Color.White;
            lblCurrentKeyPrefix.Location = new Point(0, 21);
            lblCurrentKeyPrefix.Margin = new Padding(0);
            lblCurrentKeyPrefix.Name = "lblCurrentKeyPrefix";
            lblCurrentKeyPrefix.Size = new Size(144, 18);
            lblCurrentKeyPrefix.TabIndex = 0;
            lblCurrentKeyPrefix.Text = "Macro Toggle Key:";
            // 
            // btnSetKey
            // 
            btnSetKey.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnSetKey.BackColor = Color.FromArgb(30, 1, 62);
            btnSetKey.BorderColor = Color.FromArgb(250, 91, 101);
            btnSetKey.BorderRadius = 10;
            btnSetKey.FlatStyle = FlatStyle.Flat;
            btnSetKey.Font = new Font("JetBrains Mono", 10F, FontStyle.Regular, GraphicsUnit.Point);
            btnSetKey.ForeColor = Color.White;
            btnSetKey.HoverBackColor = Color.FromArgb(214, 37, 106);
            btnSetKey.Location = new Point(1, 49);
            btnSetKey.Margin = new Padding(0, 0, 0, 12);
            btnSetKey.Name = "btnSetKey";
            btnSetKey.Size = new Size(437, 35);
            btnSetKey.TabIndex = 1;
            btnSetKey.Text = "Set Toggle Key";
            btnSetKey.UseVisualStyleBackColor = false;
            // 
            // chkMinimizeToTray
            // 
            chkMinimizeToTray.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chkMinimizeToTray.AutoSize = true;
            chkMinimizeToTray.Font = new Font("JetBrains Mono", 10F, FontStyle.Regular, GraphicsUnit.Point);
            chkMinimizeToTray.ForeColor = Color.White;
            chkMinimizeToTray.Location = new Point(280, 3);
            chkMinimizeToTray.Margin = new Padding(3, 2, 3, 2);
            chkMinimizeToTray.Name = "chkMinimizeToTray";
            chkMinimizeToTray.Size = new Size(155, 22);
            chkMinimizeToTray.TabIndex = 6;
            chkMinimizeToTray.Text = "Minimize to Tray";
            // 
            // lblMacroSwitchKeyValue
            // 
            lblMacroSwitchKeyValue.AutoSize = true;
            lblMacroSwitchKeyValue.Font = new Font("JetBrains Mono", 10.2F, FontStyle.Bold, GraphicsUnit.Point);
            lblMacroSwitchKeyValue.ForeColor = Color.White;
            lblMacroSwitchKeyValue.Location = new Point(148, 105);
            lblMacroSwitchKeyValue.Name = "lblMacroSwitchKeyValue";
            lblMacroSwitchKeyValue.Size = new Size(20, 19);
            lblMacroSwitchKeyValue.TabIndex = 5;
            lblMacroSwitchKeyValue.Text = "Q";
            // 
            // btnSetMacroSwitch
            // 
            btnSetMacroSwitch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnSetMacroSwitch.BackColor = Color.FromArgb(30, 1, 62);
            btnSetMacroSwitch.BorderColor = Color.FromArgb(250, 91, 101);
            btnSetMacroSwitch.BorderRadius = 10;
            btnSetMacroSwitch.FlatStyle = FlatStyle.Flat;
            btnSetMacroSwitch.Font = new Font("JetBrains Mono", 10F, FontStyle.Regular, GraphicsUnit.Point);
            btnSetMacroSwitch.ForeColor = Color.White;
            btnSetMacroSwitch.HoverBackColor = Color.FromArgb(214, 37, 106);
            btnSetMacroSwitch.Location = new Point(1, 133);
            btnSetMacroSwitch.Margin = new Padding(0, 0, 0, 12);
            btnSetMacroSwitch.Name = "btnSetMacroSwitch";
            btnSetMacroSwitch.Size = new Size(437, 35);
            btnSetMacroSwitch.TabIndex = 4;
            btnSetMacroSwitch.Text = "Set Switch Key";
            btnSetMacroSwitch.UseVisualStyleBackColor = false;
            // 
            // lblMacroSwitchKeyPrefix
            // 
            lblMacroSwitchKeyPrefix.AutoSize = true;
            lblMacroSwitchKeyPrefix.Font = new Font("JetBrains Mono", 10.2F, FontStyle.Regular, GraphicsUnit.Point);
            lblMacroSwitchKeyPrefix.ForeColor = Color.White;
            lblMacroSwitchKeyPrefix.Location = new Point(0, 106);
            lblMacroSwitchKeyPrefix.Name = "lblMacroSwitchKeyPrefix";
            lblMacroSwitchKeyPrefix.Size = new Size(152, 18);
            lblMacroSwitchKeyPrefix.TabIndex = 6;
            lblMacroSwitchKeyPrefix.Text = "Switch Macro Mode:";
            // 
            // chkAlwaysJitter
            // 
            chkAlwaysJitter.AutoSize = true;
            chkAlwaysJitter.Font = new Font("JetBrains Mono", 10.2F, FontStyle.Regular, GraphicsUnit.Point);
            chkAlwaysJitter.ForeColor = Color.White;
            chkAlwaysJitter.Location = new Point(24, 459);
            chkAlwaysJitter.Margin = new Padding(3, 2, 3, 2);
            chkAlwaysJitter.Name = "chkAlwaysJitter";
            chkAlwaysJitter.Size = new Size(171, 22);
            chkAlwaysJitter.TabIndex = 3;
            chkAlwaysJitter.Text = "Always Jitter Mode";
            // 
            // chkAlwaysRecoilReduction
            // 
            chkAlwaysRecoilReduction.AutoSize = true;
            chkAlwaysRecoilReduction.Font = new Font("JetBrains Mono", 10.2F, FontStyle.Regular, GraphicsUnit.Point);
            chkAlwaysRecoilReduction.ForeColor = Color.White;
            chkAlwaysRecoilReduction.Location = new Point(24, 429);
            chkAlwaysRecoilReduction.Margin = new Padding(3, 2, 3, 2);
            chkAlwaysRecoilReduction.Name = "chkAlwaysRecoilReduction";
            chkAlwaysRecoilReduction.Size = new Size(251, 22);
            chkAlwaysRecoilReduction.TabIndex = 2;
            chkAlwaysRecoilReduction.Text = "Always Recoil Reduction Mode";
            // 
            // notifyIcon
            // 
            notifyIcon.ContextMenuStrip = trayContextMenu;
            notifyIcon.Text = "Notes&Tasks";
            notifyIcon.Visible = false;
            // 
            // trayContextMenu
            // 
            trayContextMenu.Items.AddRange(new ToolStripItem[] {
                showWindowMenuItem,
                new ToolStripSeparator(),
                exitMenuItem
            });
            trayContextMenu.Name = "trayContextMenu";
            trayContextMenu.ShowImageMargin = false;
            trayContextMenu.Size = new Size(151, 48);
            // 
            // showWindowMenuItem
            // 
            showWindowMenuItem.Name = "showWindowMenuItem";
            showWindowMenuItem.Size = new Size(150, 22);
            showWindowMenuItem.Text = "Open";
            showWindowMenuItem.Font = new Font(showWindowMenuItem.Font, FontStyle.Bold);
            // 
            // exitMenuItem
            // 
            exitMenuItem.Name = "exitMenuItem";
            exitMenuItem.Size = new Size(150, 22);
            exitMenuItem.Text = "Exit";
            // 
            // MacroForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(477, 676);
            Controls.Add(mainPanel);
            Margin = new Padding(3, 2, 3, 2);
            MinimumSize = new Size(493, 422);
            Name = "MacroForm";
            Text = "NotesAndTasks";
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            debugPanel.ResumeLayout(false);
            debugPanel.PerformLayout();
            strengthPanel1.ResumeLayout(false);
            strengthPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarRecoilReduction).EndInit();
            strengthPanel2.ResumeLayout(false);
            strengthPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarJitter).EndInit();
            settingsPanel.ResumeLayout(false);
            settingsPanel.PerformLayout();
            trayContextMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Panel mainPanel;
        private ModernButton btnSetKey;
        private System.Windows.Forms.Label lblCurrentKeyPrefix;
        private System.Windows.Forms.Label lblCurrentKeyValue;
        private ModernTrackBar trackBarJitter;
        private System.Windows.Forms.Label lblJitterStrengthPrefix;
        private System.Windows.Forms.Label lblJitterStrengthValue;
        private ModernTrackBar trackBarRecoilReduction;
        private System.Windows.Forms.Label lblRecoilReductionStrengthPrefix;
        private System.Windows.Forms.Label lblRecoilReductionStrengthValue;
        private ModernButton btnToggleDebug;
        private System.Windows.Forms.Panel debugPanel;
        private System.Windows.Forms.TextBox debugLabel;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip trayContextMenu;
        private System.Windows.Forms.ToolStripMenuItem showWindowMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.Panel settingsPanel;
        private System.Windows.Forms.Panel strengthPanel1;
        private System.Windows.Forms.Panel strengthPanel2;
        private System.Windows.Forms.CheckBox chkMinimizeToTray;
        private ModernButton btnSetMacroSwitch;
        private System.Windows.Forms.Label lblMacroSwitchKeyPrefix;
        private System.Windows.Forms.Label lblMacroSwitchKeyValue;
        private System.Windows.Forms.CheckBox chkAlwaysJitter;
        private System.Windows.Forms.CheckBox chkAlwaysRecoilReduction;
        private System.Windows.Forms.Label lblRecoilReductionActive;
        private System.Windows.Forms.Label lblJitterActive;
    }
}
