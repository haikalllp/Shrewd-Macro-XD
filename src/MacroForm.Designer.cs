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
            this.settingsPanel = new System.Windows.Forms.Panel();
            this.chkMinimizeToTray = new System.Windows.Forms.CheckBox();
            this.btnSetKey = new System.Windows.Forms.Button();
            this.lblCurrentKey = new System.Windows.Forms.Label();
            this.debugPanel = new System.Windows.Forms.Panel();
            this.debugLabel = new System.Windows.Forms.TextBox();
            this.btnToggleDebug = new System.Windows.Forms.Button();
            this.chkJitterEnabled = new System.Windows.Forms.CheckBox();
            this.strengthPanel1 = new System.Windows.Forms.Panel();
            this.lblRecoilStrength = new System.Windows.Forms.Label();
            this.trackBarRecoil = new System.Windows.Forms.TrackBar();
            this.strengthPanel2 = new System.Windows.Forms.Panel();
            this.lblJitterStrength = new System.Windows.Forms.Label();
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
            this.mainPanel.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.mainPanel.Controls.Add(this.debugPanel);
            this.mainPanel.Controls.Add(this.btnToggleDebug);
            this.mainPanel.Controls.Add(this.chkJitterEnabled);
            this.mainPanel.Controls.Add(this.strengthPanel1);
            this.mainPanel.Controls.Add(this.strengthPanel2);
            this.mainPanel.Controls.Add(this.settingsPanel);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Padding = new System.Windows.Forms.Padding(12);
            this.mainPanel.Size = new System.Drawing.Size(384, 461);
            this.mainPanel.TabIndex = 0;
            // 
            // settingsPanel
            // 
            this.settingsPanel.Controls.Add(this.lblCurrentKey);
            this.settingsPanel.Controls.Add(this.btnSetKey);
            this.settingsPanel.Controls.Add(this.chkMinimizeToTray);
            this.settingsPanel.Location = new System.Drawing.Point(12, 12);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(360, 74);
            this.settingsPanel.TabIndex = 11;
            // 
            // chkMinimizeToTray
            // 
            this.chkMinimizeToTray.AutoSize = true;
            this.chkMinimizeToTray.ForeColor = System.Drawing.Color.LightGray;
            this.chkMinimizeToTray.Location = new System.Drawing.Point(240, 12);
            this.chkMinimizeToTray.Name = "chkMinimizeToTray";
            this.chkMinimizeToTray.Size = new System.Drawing.Size(120, 24);
            this.chkMinimizeToTray.TabIndex = 6;
            this.chkMinimizeToTray.Text = "Minimize to Tray";
            // 
            // btnSetKey
            // 
            this.btnSetKey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetKey.ForeColor = System.Drawing.Color.LightGray;
            this.btnSetKey.Location = new System.Drawing.Point(12, 40);
            this.btnSetKey.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
            this.btnSetKey.Name = "btnSetKey";
            this.btnSetKey.Size = new System.Drawing.Size(360, 30);
            this.btnSetKey.TabIndex = 1;
            this.btnSetKey.Text = "Set Toggle Key";
            // 
            // lblCurrentKey
            // 
            this.lblCurrentKey.AutoSize = true;
            this.lblCurrentKey.ForeColor = System.Drawing.Color.LightGray;
            this.lblCurrentKey.Location = new System.Drawing.Point(12, 12);
            this.lblCurrentKey.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.lblCurrentKey.Name = "lblCurrentKey";
            this.lblCurrentKey.Size = new System.Drawing.Size(120, 20);
            this.lblCurrentKey.TabIndex = 0;
            this.lblCurrentKey.Text = "Current Key: Capital";
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
            this.debugPanel.Location = new System.Drawing.Point(12, 246);
            this.debugPanel.Name = "debugPanel";
            this.debugPanel.Padding = new System.Windows.Forms.Padding(8);
            this.debugPanel.Size = new System.Drawing.Size(360, 203);
            this.debugPanel.TabIndex = 7;
            this.debugPanel.Visible = false;
            // 
            // debugLabel
            // 
            this.debugLabel.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            this.debugLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.debugLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.debugLabel.ForeColor = System.Drawing.Color.LightGray;
            this.debugLabel.Location = new System.Drawing.Point(8, 8);
            this.debugLabel.Multiline = true;
            this.debugLabel.Name = "debugLabel";
            this.debugLabel.ReadOnly = true;
            this.debugLabel.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.debugLabel.Size = new System.Drawing.Size(340, 183);
            this.debugLabel.TabIndex = 0;
            this.debugLabel.WordWrap = true;
            // 
            // btnToggleDebug
            // 
            this.btnToggleDebug.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleDebug.ForeColor = System.Drawing.Color.LightGray;
            this.btnToggleDebug.Location = new System.Drawing.Point(12, 206);
            this.btnToggleDebug.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
            this.btnToggleDebug.Name = "btnToggleDebug";
            this.btnToggleDebug.Size = new System.Drawing.Size(360, 30);
            this.btnToggleDebug.TabIndex = 5;
            this.btnToggleDebug.Text = "Show Debug Info";
            // 
            // chkJitterEnabled
            // 
            this.chkJitterEnabled.AutoSize = true;
            this.chkJitterEnabled.ForeColor = System.Drawing.Color.LightGray;
            this.chkJitterEnabled.Location = new System.Drawing.Point(12, 166);
            this.chkJitterEnabled.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
            this.chkJitterEnabled.Name = "chkJitterEnabled";
            this.chkJitterEnabled.Size = new System.Drawing.Size(150, 24);
            this.chkJitterEnabled.TabIndex = 4;
            this.chkJitterEnabled.Text = "Enable Jitter Mode";
            // 
            // strengthPanel1
            // 
            this.strengthPanel1.Controls.Add(this.lblRecoilStrength);
            this.strengthPanel1.Controls.Add(this.trackBarRecoil);
            this.strengthPanel1.Location = new System.Drawing.Point(12, 86);
            this.strengthPanel1.Name = "strengthPanel1";
            this.strengthPanel1.Size = new System.Drawing.Size(360, 70);
            this.strengthPanel1.TabIndex = 9;
            // 
            // lblRecoilStrength
            // 
            this.lblRecoilStrength.AutoSize = true;
            this.lblRecoilStrength.ForeColor = System.Drawing.Color.LightGray;
            this.lblRecoilStrength.Location = new System.Drawing.Point(0, 0);
            this.lblRecoilStrength.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblRecoilStrength.Name = "lblRecoilStrength";
            this.lblRecoilStrength.Size = new System.Drawing.Size(120, 20);
            this.lblRecoilStrength.TabIndex = 7;
            this.lblRecoilStrength.Text = "Recoil Strength: 3";
            // 
            // trackBarRecoil
            // 
            this.trackBarRecoil.Location = new System.Drawing.Point(0, 24);
            this.trackBarRecoil.Margin = new System.Windows.Forms.Padding(0);
            this.trackBarRecoil.Maximum = 20;
            this.trackBarRecoil.Minimum = 1;
            this.trackBarRecoil.Name = "trackBarRecoil";
            this.trackBarRecoil.Size = new System.Drawing.Size(360, 45);
            this.trackBarRecoil.TabIndex = 8;
            this.trackBarRecoil.Value = 3;
            // 
            // strengthPanel2
            // 
            this.strengthPanel2.Controls.Add(this.lblJitterStrength);
            this.strengthPanel2.Controls.Add(this.trackBarJitter);
            this.strengthPanel2.Location = new System.Drawing.Point(12, 86);
            this.strengthPanel2.Name = "strengthPanel2";
            this.strengthPanel2.Size = new System.Drawing.Size(360, 70);
            this.strengthPanel2.TabIndex = 10;
            // 
            // lblJitterStrength
            // 
            this.lblJitterStrength.AutoSize = true;
            this.lblJitterStrength.ForeColor = System.Drawing.Color.LightGray;
            this.lblJitterStrength.Location = new System.Drawing.Point(0, 0);
            this.lblJitterStrength.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblJitterStrength.Name = "lblJitterStrength";
            this.lblJitterStrength.Size = new System.Drawing.Size(120, 20);
            this.lblJitterStrength.TabIndex = 2;
            this.lblJitterStrength.Text = "Jitter Strength: 3";
            // 
            // trackBarJitter
            // 
            this.trackBarJitter.Location = new System.Drawing.Point(0, 24);
            this.trackBarJitter.Margin = new System.Windows.Forms.Padding(0);
            this.trackBarJitter.Maximum = 20;
            this.trackBarJitter.Minimum = 1;
            this.trackBarJitter.Name = "trackBarJitter";
            this.trackBarJitter.Size = new System.Drawing.Size(360, 45);
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

        }

        #endregion

        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Button btnSetKey;
        private System.Windows.Forms.Label lblCurrentKey;
        private System.Windows.Forms.TrackBar trackBarJitter;
        private System.Windows.Forms.Label lblJitterStrength;
        private System.Windows.Forms.TrackBar trackBarRecoil;
        private System.Windows.Forms.Label lblRecoilStrength;
        private System.Windows.Forms.Button btnToggleDebug;
        private System.Windows.Forms.Panel debugPanel;
        private System.Windows.Forms.TextBox debugLabel;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip trayContextMenu;
        private System.Windows.Forms.ToolStripMenuItem showWindowMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.CheckBox chkMinimizeToTray;
        private System.Windows.Forms.CheckBox chkJitterEnabled;
        private System.Windows.Forms.Panel strengthPanel1;
        private System.Windows.Forms.Panel strengthPanel2;
        private System.Windows.Forms.Panel settingsPanel;
    }
}
