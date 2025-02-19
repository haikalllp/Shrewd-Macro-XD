namespace NotesTasks
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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MacroForm));
            this.mainPanel = new System.Windows.Forms.Panel();
            this.lblJitterActive = new System.Windows.Forms.Label();
            this.lblRecoilActive = new System.Windows.Forms.Label();
            this.chkAlwaysRecoil = new System.Windows.Forms.CheckBox();
            this.chkAlwaysJitter = new System.Windows.Forms.CheckBox();
            this.btnSetMacroSwitch = new System.Windows.Forms.Button();
            this.lblMacroSwitchKeyValue = new System.Windows.Forms.Label();
            this.lblMacroSwitchKeyPrefix = new System.Windows.Forms.Label();
            this.settingsPanel = new System.Windows.Forms.Panel();
            this.lblCurrentKeyPrefix = new System.Windows.Forms.Label();
            this.lblCurrentKeyValue = new System.Windows.Forms.Label();
            this.chkMinimizeToTray = new System.Windows.Forms.CheckBox();
            this.btnSetKey = new System.Windows.Forms.Button();
            this.debugPanel = new System.Windows.Forms.Panel();
            this.debugLabel = new System.Windows.Forms.TextBox();
            this.btnToggleDebug = new System.Windows.Forms.Button();
            this.strengthPanel1 = new System.Windows.Forms.Panel();
            this.lblRecoilStrengthPrefix = new System.Windows.Forms.Label();
            this.lblRecoilStrengthValue = new System.Windows.Forms.Label();
            this.trackBarRecoil = new System.Windows.Forms.TrackBar();
            this.strengthPanel2 = new System.Windows.Forms.Panel();
            this.lblJitterStrengthPrefix = new System.Windows.Forms.Label();
            this.lblJitterStrengthValue = new System.Windows.Forms.Label();
            this.trackBarJitter = new System.Windows.Forms.TrackBar();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showWindowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trayContextMenu.SuspendLayout();
            this.mainPanel.SuspendLayout();
            this.settingsPanel.SuspendLayout();
            this.debugPanel.SuspendLayout();
            this.strengthPanel1.SuspendLayout();
            this.strengthPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarJitter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarRecoil)).BeginInit();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainPanel.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.mainPanel.Controls.Add(this.lblJitterActive);
            this.mainPanel.Controls.Add(this.lblRecoilActive);
            this.mainPanel.Controls.Add(this.chkAlwaysRecoil);
            this.mainPanel.Controls.Add(this.chkAlwaysJitter);
            this.mainPanel.Controls.Add(this.btnSetMacroSwitch);
            this.mainPanel.Controls.Add(this.lblMacroSwitchKeyValue);
            this.mainPanel.Controls.Add(this.lblMacroSwitchKeyPrefix);
            this.mainPanel.Controls.Add(this.debugPanel);
            this.mainPanel.Controls.Add(this.btnToggleDebug);
            this.mainPanel.Controls.Add(this.strengthPanel1);
            this.mainPanel.Controls.Add(this.strengthPanel2);
            this.mainPanel.Controls.Add(this.settingsPanel);
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Padding = new System.Windows.Forms.Padding(16);
            this.mainPanel.Size = new System.Drawing.Size(384, 461);
            this.mainPanel.TabIndex = 0;

            // Adjust strengthPanel1 location
            this.strengthPanel1.Location = new System.Drawing.Point(16, 200);

            // Adjust strengthPanel2 location
            this.strengthPanel2.Location = new System.Drawing.Point(16, 280);

            // Adjust debug button location
            this.btnToggleDebug.Location = new System.Drawing.Point(16, 380);

            // 
            // settingsPanel
            // 
            this.settingsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.settingsPanel.Controls.Add(this.lblCurrentKeyValue);
            this.settingsPanel.Controls.Add(this.lblCurrentKeyPrefix);
            this.settingsPanel.Controls.Add(this.btnSetKey);
            this.settingsPanel.Controls.Add(this.chkMinimizeToTray);
            this.settingsPanel.Location = new System.Drawing.Point(16, 16);
            this.settingsPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(352, 80);
            this.settingsPanel.TabIndex = 11;
            // 
            // lblCurrentKeyPrefix
            // 
            this.lblCurrentKeyPrefix.AutoSize = true;
            this.lblCurrentKeyPrefix.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblCurrentKeyPrefix.ForeColor = System.Drawing.Color.White;
            this.lblCurrentKeyPrefix.Location = new System.Drawing.Point(0, 12);
            this.lblCurrentKeyPrefix.Margin = new System.Windows.Forms.Padding(0);
            this.lblCurrentKeyPrefix.Name = "lblCurrentKeyPrefix";
            this.lblCurrentKeyPrefix.Size = new System.Drawing.Size(95, 20);
            this.lblCurrentKeyPrefix.TabIndex = 0;
            this.lblCurrentKeyPrefix.Text = "Current Key: ";
            // 
            // lblCurrentKeyValue
            // 
            this.lblCurrentKeyValue.AutoSize = true;
            this.lblCurrentKeyValue.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCurrentKeyValue.ForeColor = System.Drawing.Color.White;
            this.lblCurrentKeyValue.Location = new System.Drawing.Point(95, 12);
            this.lblCurrentKeyValue.Margin = new System.Windows.Forms.Padding(0);
            this.lblCurrentKeyValue.Name = "lblCurrentKeyValue";
            this.lblCurrentKeyValue.Size = new System.Drawing.Size(60, 20);
            this.lblCurrentKeyValue.TabIndex = 1;
            this.lblCurrentKeyValue.Text = "Capital";
            // 
            // chkMinimizeToTray
            // 
            this.chkMinimizeToTray.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkMinimizeToTray.AutoSize = true;
            this.chkMinimizeToTray.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkMinimizeToTray.ForeColor = System.Drawing.Color.White;
            this.chkMinimizeToTray.Location = new System.Drawing.Point(232, 12);
            this.chkMinimizeToTray.Name = "chkMinimizeToTray";
            this.chkMinimizeToTray.Size = new System.Drawing.Size(120, 24);
            this.chkMinimizeToTray.TabIndex = 6;
            this.chkMinimizeToTray.Text = "Minimize to Tray";
            // 
            // btnSetKey
            // 
            this.btnSetKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSetKey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetKey.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnSetKey.ForeColor = System.Drawing.Color.White;
            this.btnSetKey.Location = new System.Drawing.Point(0, 40);
            this.btnSetKey.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
            this.btnSetKey.Name = "btnSetKey";
            this.btnSetKey.Size = new System.Drawing.Size(352, 40);
            this.btnSetKey.TabIndex = 1;
            this.btnSetKey.Text = "Set Toggle Key";
            // 
            // debugPanel
            // 
            this.debugPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.debugPanel.AutoScroll = true;
            this.debugPanel.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            this.debugPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.debugPanel.Controls.Add(this.debugLabel);
            this.debugPanel.Location = new System.Drawing.Point(16, 420);
            this.debugPanel.Name = "debugPanel";
            this.debugPanel.Padding = new System.Windows.Forms.Padding(8);
            this.debugPanel.Size = new System.Drawing.Size(352, 21);
            this.debugPanel.TabIndex = 7;
            this.debugPanel.Visible = false;
            // 
            // debugLabel
            // 
            this.debugLabel.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            this.debugLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.debugLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.debugLabel.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.debugLabel.ForeColor = System.Drawing.Color.White;
            this.debugLabel.Location = new System.Drawing.Point(8, 8);
            this.debugLabel.Multiline = true;
            this.debugLabel.Name = "debugLabel";
            this.debugLabel.ReadOnly = true;
            this.debugLabel.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.debugLabel.Size = new System.Drawing.Size(334, 5);
            this.debugLabel.TabIndex = 0;
            this.debugLabel.WordWrap = true;
            // 
            // btnToggleDebug
            // 
            this.btnToggleDebug.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleDebug.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleDebug.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnToggleDebug.ForeColor = System.Drawing.Color.White;
            this.btnToggleDebug.Location = new System.Drawing.Point(16, 380);
            this.btnToggleDebug.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
            this.btnToggleDebug.Name = "btnToggleDebug";
            this.btnToggleDebug.Size = new System.Drawing.Size(352, 40);
            this.btnToggleDebug.TabIndex = 5;
            this.btnToggleDebug.Text = "Show Debug Info";
            // 
            // strengthPanel1
            // 
            this.strengthPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.strengthPanel1.Controls.Add(this.lblRecoilStrengthValue);
            this.strengthPanel1.Controls.Add(this.lblRecoilStrengthPrefix);
            this.strengthPanel1.Controls.Add(this.trackBarRecoil);
            this.strengthPanel1.Location = new System.Drawing.Point(16, 200);
            this.strengthPanel1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
            this.strengthPanel1.Name = "strengthPanel1";
            this.strengthPanel1.Size = new System.Drawing.Size(352, 80);
            this.strengthPanel1.TabIndex = 9;
            // 
            // lblRecoilStrengthPrefix
            // 
            this.lblRecoilStrengthPrefix.AutoSize = true;
            this.lblRecoilStrengthPrefix.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblRecoilStrengthPrefix.ForeColor = System.Drawing.Color.White;
            this.lblRecoilStrengthPrefix.Location = new System.Drawing.Point(0, 0);
            this.lblRecoilStrengthPrefix.Margin = new System.Windows.Forms.Padding(0);
            this.lblRecoilStrengthPrefix.Name = "lblRecoilStrengthPrefix";
            this.lblRecoilStrengthPrefix.Size = new System.Drawing.Size(120, 20);
            this.lblRecoilStrengthPrefix.TabIndex = 0;
            this.lblRecoilStrengthPrefix.Text = "Recoil Strength: ";
            // 
            // lblRecoilStrengthValue
            // 
            this.lblRecoilStrengthValue.AutoSize = true;
            this.lblRecoilStrengthValue.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblRecoilStrengthValue.ForeColor = System.Drawing.Color.White;
            this.lblRecoilStrengthValue.Location = new System.Drawing.Point(120, 0);
            this.lblRecoilStrengthValue.Margin = new System.Windows.Forms.Padding(0);
            this.lblRecoilStrengthValue.Name = "lblRecoilStrengthValue";
            this.lblRecoilStrengthValue.Size = new System.Drawing.Size(17, 20);
            this.lblRecoilStrengthValue.TabIndex = 1;
            this.lblRecoilStrengthValue.Text = "3";
            // 
            // trackBarRecoil
            // 
            this.trackBarRecoil.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarRecoil.Location = new System.Drawing.Point(0, 24);
            this.trackBarRecoil.Margin = new System.Windows.Forms.Padding(0);
            this.trackBarRecoil.Maximum = 20;
            this.trackBarRecoil.Minimum = 1;
            this.trackBarRecoil.Name = "trackBarRecoil";
            this.trackBarRecoil.Size = new System.Drawing.Size(352, 45);
            this.trackBarRecoil.TabIndex = 8;
            this.trackBarRecoil.Value = 3;
            // 
            // strengthPanel2
            // 
            this.strengthPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.strengthPanel2.Controls.Add(this.lblJitterStrengthValue);
            this.strengthPanel2.Controls.Add(this.lblJitterStrengthPrefix);
            this.strengthPanel2.Controls.Add(this.trackBarJitter);
            this.strengthPanel2.Location = new System.Drawing.Point(16, 280);
            this.strengthPanel2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
            this.strengthPanel2.Name = "strengthPanel2";
            this.strengthPanel2.Size = new System.Drawing.Size(352, 80);
            this.strengthPanel2.TabIndex = 10;
            // 
            // lblJitterStrengthPrefix
            // 
            this.lblJitterStrengthPrefix.AutoSize = true;
            this.lblJitterStrengthPrefix.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblJitterStrengthPrefix.ForeColor = System.Drawing.Color.White;
            this.lblJitterStrengthPrefix.Location = new System.Drawing.Point(0, 0);
            this.lblJitterStrengthPrefix.Margin = new System.Windows.Forms.Padding(0);
            this.lblJitterStrengthPrefix.Name = "lblJitterStrengthPrefix";
            this.lblJitterStrengthPrefix.Size = new System.Drawing.Size(110, 20);
            this.lblJitterStrengthPrefix.TabIndex = 0;
            this.lblJitterStrengthPrefix.Text = "Jitter Strength: ";
            // 
            // lblJitterStrengthValue
            // 
            this.lblJitterStrengthValue.AutoSize = true;
            this.lblJitterStrengthValue.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblJitterStrengthValue.ForeColor = System.Drawing.Color.White;
            this.lblJitterStrengthValue.Location = new System.Drawing.Point(110, 0);
            this.lblJitterStrengthValue.Margin = new System.Windows.Forms.Padding(0);
            this.lblJitterStrengthValue.Name = "lblJitterStrengthValue";
            this.lblJitterStrengthValue.Size = new System.Drawing.Size(17, 20);
            this.lblJitterStrengthValue.TabIndex = 1;
            this.lblJitterStrengthValue.Text = "3";
            // 
            // trackBarJitter
            // 
            this.trackBarJitter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarJitter.Location = new System.Drawing.Point(0, 24);
            this.trackBarJitter.Margin = new System.Windows.Forms.Padding(0);
            this.trackBarJitter.Maximum = 20;
            this.trackBarJitter.Minimum = 1;
            this.trackBarJitter.Name = "trackBarJitter";
            this.trackBarJitter.Size = new System.Drawing.Size(352, 45);
            this.trackBarJitter.TabIndex = 3;
            this.trackBarJitter.Value = 3;
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.trayContextMenu;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Mouse Macro";
            this.notifyIcon.Visible = true;
            // 
            // trayContextMenu
            // 
            this.trayContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showWindowMenuItem,
            this.exitMenuItem});
            this.trayContextMenu.Name = "trayContextMenu";
            this.trayContextMenu.Size = new System.Drawing.Size(153, 70);
            // 
            // showWindowMenuItem
            // 
            this.showWindowMenuItem.Name = "showWindowMenuItem";
            this.showWindowMenuItem.Size = new System.Drawing.Size(152, 22);
            this.showWindowMenuItem.Text = "Show Window";
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitMenuItem.Text = "Exit";
            // 
            // MacroForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.ClientSize = new System.Drawing.Size(384, 461);
            this.Controls.Add(this.mainPanel);
            this.MinimumSize = new System.Drawing.Size(400, 500);
            this.Name = "MacroForm";
            this.Text = "Notes&Tasks";
            this.trayContextMenu.ResumeLayout(false);
            this.mainPanel.ResumeLayout(false);
            this.settingsPanel.ResumeLayout(false);
            this.settingsPanel.PerformLayout();
            this.debugPanel.ResumeLayout(false);
            this.debugPanel.PerformLayout();
            this.strengthPanel1.ResumeLayout(false);
            this.strengthPanel1.PerformLayout();
            this.strengthPanel2.ResumeLayout(false);
            this.strengthPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarJitter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarRecoil)).EndInit();
            this.ResumeLayout(false);

            // Add Switch Macro Mode controls
            this.lblMacroSwitchKeyPrefix = new System.Windows.Forms.Label();
            this.lblMacroSwitchKeyValue = new System.Windows.Forms.Label();
            this.btnSetMacroSwitch = new System.Windows.Forms.Button();
            
            // Add Always Mode checkboxes
            this.chkAlwaysJitter = new System.Windows.Forms.CheckBox();
            this.chkAlwaysRecoil = new System.Windows.Forms.CheckBox();
            
            // Add Active state labels
            this.lblRecoilActive = new System.Windows.Forms.Label();
            this.lblJitterActive = new System.Windows.Forms.Label();

            // Configure Switch Macro Mode controls
            this.lblMacroSwitchKeyPrefix.AutoSize = true;
            this.lblMacroSwitchKeyPrefix.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblMacroSwitchKeyPrefix.ForeColor = System.Drawing.Color.White;
            this.lblMacroSwitchKeyPrefix.Location = new System.Drawing.Point(16, 112);
            this.lblMacroSwitchKeyPrefix.Name = "lblMacroSwitchKeyPrefix";
            this.lblMacroSwitchKeyPrefix.Size = new System.Drawing.Size(130, 20);
            this.lblMacroSwitchKeyPrefix.Text = "Switch Macro Mode:";

            this.lblMacroSwitchKeyValue.AutoSize = true;
            this.lblMacroSwitchKeyValue.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblMacroSwitchKeyValue.ForeColor = System.Drawing.Color.White;
            this.lblMacroSwitchKeyValue.Location = new System.Drawing.Point(150, 112);
            this.lblMacroSwitchKeyValue.Name = "lblMacroSwitchKeyValue";
            this.lblMacroSwitchKeyValue.Size = new System.Drawing.Size(20, 20);
            this.lblMacroSwitchKeyValue.Text = "Q";

            this.btnSetMacroSwitch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSetMacroSwitch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetMacroSwitch.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnSetMacroSwitch.ForeColor = System.Drawing.Color.White;
            this.btnSetMacroSwitch.Location = new System.Drawing.Point(16, 140);
            this.btnSetMacroSwitch.Name = "btnSetMacroSwitch";
            this.btnSetMacroSwitch.Size = new System.Drawing.Size(352, 40);
            this.btnSetMacroSwitch.Text = "Set Switch Key";

            // Configure Always Mode checkboxes
            this.chkAlwaysJitter.AutoSize = true;
            this.chkAlwaysJitter.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkAlwaysJitter.ForeColor = System.Drawing.Color.White;
            this.chkAlwaysJitter.Location = new System.Drawing.Point(16, 340);
            this.chkAlwaysJitter.Name = "chkAlwaysJitter";
            this.chkAlwaysJitter.Size = new System.Drawing.Size(150, 24);
            this.chkAlwaysJitter.Text = "Always Jitter Mode";

            this.chkAlwaysRecoil.AutoSize = true;
            this.chkAlwaysRecoil.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkAlwaysRecoil.ForeColor = System.Drawing.Color.White;
            this.chkAlwaysRecoil.Location = new System.Drawing.Point(180, 340);
            this.chkAlwaysRecoil.Name = "chkAlwaysRecoil";
            this.chkAlwaysRecoil.Size = new System.Drawing.Size(150, 24);
            this.chkAlwaysRecoil.Text = "Always Recoil Mode";

            // Configure Active state labels
            this.lblRecoilActive.AutoSize = true;
            this.lblRecoilActive.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblRecoilActive.ForeColor = System.Drawing.Color.LightGreen;
            this.lblRecoilActive.Location = new System.Drawing.Point(250, 200);
            this.lblRecoilActive.Name = "lblRecoilActive";
            this.lblRecoilActive.Size = new System.Drawing.Size(60, 20);
            this.lblRecoilActive.Text = "";

            this.lblJitterActive.AutoSize = true;
            this.lblJitterActive.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblJitterActive.ForeColor = System.Drawing.Color.LightGreen;
            this.lblJitterActive.Location = new System.Drawing.Point(250, 280);
            this.lblJitterActive.Name = "lblJitterActive";
            this.lblJitterActive.Size = new System.Drawing.Size(60, 20);
            this.lblJitterActive.Text = "";

            // Add new controls to the form
            this.mainPanel.Controls.Add(this.lblMacroSwitchKeyPrefix);
            this.mainPanel.Controls.Add(this.lblMacroSwitchKeyValue);
            this.mainPanel.Controls.Add(this.btnSetMacroSwitch);
            this.mainPanel.Controls.Add(this.chkAlwaysJitter);
            this.mainPanel.Controls.Add(this.chkAlwaysRecoil);
            this.mainPanel.Controls.Add(this.lblRecoilActive);
            this.mainPanel.Controls.Add(this.lblJitterActive);
        }

        #endregion

        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Button btnSetKey;
        private System.Windows.Forms.Label lblCurrentKeyPrefix;
        private System.Windows.Forms.Label lblCurrentKeyValue;
        private System.Windows.Forms.TrackBar trackBarJitter;
        private System.Windows.Forms.Label lblJitterStrengthPrefix;
        private System.Windows.Forms.Label lblJitterStrengthValue;
        private System.Windows.Forms.TrackBar trackBarRecoil;
        private System.Windows.Forms.Label lblRecoilStrengthPrefix;
        private System.Windows.Forms.Label lblRecoilStrengthValue;
        private System.Windows.Forms.Button btnToggleDebug;
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
        private System.Windows.Forms.Button btnSetMacroSwitch;
        private System.Windows.Forms.Label lblMacroSwitchKeyPrefix;
        private System.Windows.Forms.Label lblMacroSwitchKeyValue;
        private System.Windows.Forms.CheckBox chkAlwaysJitter;
        private System.Windows.Forms.CheckBox chkAlwaysRecoil;
        private System.Windows.Forms.Label lblRecoilActive;
        private System.Windows.Forms.Label lblJitterActive;
    }
}
