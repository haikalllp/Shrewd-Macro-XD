namespace MouseMacro
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
            this.debugPanel = new System.Windows.Forms.Panel();
            this.debugLabel = new System.Windows.Forms.Label();
            this.btnToggleDebug = new System.Windows.Forms.Button();
            this.lblJitterStrength = new System.Windows.Forms.Label();
            this.trackBarJitter = new System.Windows.Forms.TrackBar();
            this.lblCurrentKey = new System.Windows.Forms.Label();
            this.btnSetKey = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showWindowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trayContextMenu.SuspendLayout();
            this.mainPanel.SuspendLayout();
            this.settingsPanel.SuspendLayout();
            this.debugPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarJitter)).BeginInit();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.mainPanel.Controls.Add(this.settingsPanel);
            this.mainPanel.Controls.Add(this.debugPanel);
            this.mainPanel.Controls.Add(this.btnToggleDebug);
            this.mainPanel.Controls.Add(this.lblJitterStrength);
            this.mainPanel.Controls.Add(this.trackBarJitter);
            this.mainPanel.Controls.Add(this.lblCurrentKey);
            this.mainPanel.Controls.Add(this.btnSetKey);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Padding = new System.Windows.Forms.Padding(20);
            this.mainPanel.Size = new System.Drawing.Size(400, 350);
            this.mainPanel.TabIndex = 0;
            // 
            // settingsPanel
            // 
            this.settingsPanel.Controls.Add(this.chkMinimizeToTray);
            this.settingsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.settingsPanel.Location = new System.Drawing.Point(20, 250);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(360, 30);
            this.settingsPanel.TabIndex = 6;
            // 
            // chkMinimizeToTray
            // 
            this.chkMinimizeToTray.AutoSize = true;
            this.chkMinimizeToTray.ForeColor = System.Drawing.Color.LightGray;
            this.chkMinimizeToTray.Location = new System.Drawing.Point(0, 5);
            this.chkMinimizeToTray.Name = "chkMinimizeToTray";
            this.chkMinimizeToTray.Size = new System.Drawing.Size(180, 19);
            this.chkMinimizeToTray.TabIndex = 0;
            this.chkMinimizeToTray.Text = "Minimize to System Tray";
            this.chkMinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // debugPanel
            // 
            this.debugPanel.Controls.Add(this.debugLabel);
            this.debugPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.debugPanel.Location = new System.Drawing.Point(20, 280);
            this.debugPanel.Name = "debugPanel";
            this.debugPanel.Size = new System.Drawing.Size(360, 50);
            this.debugPanel.TabIndex = 5;
            this.debugPanel.Visible = false;
            // 
            // debugLabel
            // 
            this.debugLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.debugLabel.ForeColor = System.Drawing.Color.LightGray;
            this.debugLabel.Location = new System.Drawing.Point(0, 0);
            this.debugLabel.Name = "debugLabel";
            this.debugLabel.Size = new System.Drawing.Size(360, 50);
            this.debugLabel.TabIndex = 0;
            this.debugLabel.Text = "Debug Info";
            // 
            // btnToggleDebug
            // 
            this.btnToggleDebug.BackColor = System.Drawing.Color.FromArgb(45, 45, 45);
            this.btnToggleDebug.FlatAppearance.BorderSize = 0;
            this.btnToggleDebug.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleDebug.ForeColor = System.Drawing.Color.LightGray;
            this.btnToggleDebug.Location = new System.Drawing.Point(20, 160);
            this.btnToggleDebug.Name = "btnToggleDebug";
            this.btnToggleDebug.Size = new System.Drawing.Size(360, 30);
            this.btnToggleDebug.TabIndex = 4;
            this.btnToggleDebug.Text = "Show Debug Info";
            this.btnToggleDebug.UseVisualStyleBackColor = false;
            // 
            // lblJitterStrength
            // 
            this.lblJitterStrength.AutoSize = true;
            this.lblJitterStrength.ForeColor = System.Drawing.Color.LightGray;
            this.lblJitterStrength.Location = new System.Drawing.Point(20, 100);
            this.lblJitterStrength.Name = "lblJitterStrength";
            this.lblJitterStrength.Size = new System.Drawing.Size(89, 15);
            this.lblJitterStrength.TabIndex = 3;
            this.lblJitterStrength.Text = "Jitter Strength: 3";
            // 
            // trackBarJitter
            // 
            this.trackBarJitter.Location = new System.Drawing.Point(20, 120);
            this.trackBarJitter.Maximum = 20;
            this.trackBarJitter.Minimum = 1;
            this.trackBarJitter.Name = "trackBarJitter";
            this.trackBarJitter.Size = new System.Drawing.Size(360, 45);
            this.trackBarJitter.TabIndex = 2;
            this.trackBarJitter.Value = 3;
            // 
            // lblCurrentKey
            // 
            this.lblCurrentKey.AutoSize = true;
            this.lblCurrentKey.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCurrentKey.ForeColor = System.Drawing.Color.LightGray;
            this.lblCurrentKey.Location = new System.Drawing.Point(20, 20);
            this.lblCurrentKey.Name = "lblCurrentKey";
            this.lblCurrentKey.Size = new System.Drawing.Size(156, 21);
            this.lblCurrentKey.TabIndex = 1;
            this.lblCurrentKey.Text = "Current Toggle Key: CAPS";
            // 
            // btnSetKey
            // 
            this.btnSetKey.BackColor = System.Drawing.Color.FromArgb(45, 45, 45);
            this.btnSetKey.FlatAppearance.BorderSize = 0;
            this.btnSetKey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetKey.ForeColor = System.Drawing.Color.LightGray;
            this.btnSetKey.Location = new System.Drawing.Point(20, 50);
            this.btnSetKey.Name = "btnSetKey";
            this.btnSetKey.Size = new System.Drawing.Size(360, 40);
            this.btnSetKey.TabIndex = 0;
            this.btnSetKey.Text = "Set Toggle Key";
            this.btnSetKey.UseVisualStyleBackColor = false;
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.ClientSize = new System.Drawing.Size(400, 350);
            this.Controls.Add(this.mainPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(400, 350);
            this.Name = "MacroForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mouse Macro";
            this.trayContextMenu.ResumeLayout(false);
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
            this.settingsPanel.ResumeLayout(false);
            this.settingsPanel.PerformLayout();
            this.debugPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarJitter)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Panel debugPanel;
        private System.Windows.Forms.Label debugLabel;
        private System.Windows.Forms.Button btnToggleDebug;
        private System.Windows.Forms.Label lblJitterStrength;
        private System.Windows.Forms.TrackBar trackBarJitter;
        private System.Windows.Forms.Label lblCurrentKey;
        private System.Windows.Forms.Button btnSetKey;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip trayContextMenu;
        private System.Windows.Forms.ToolStripMenuItem showWindowMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.Panel settingsPanel;
        private System.Windows.Forms.CheckBox chkMinimizeToTray;
    }
}
