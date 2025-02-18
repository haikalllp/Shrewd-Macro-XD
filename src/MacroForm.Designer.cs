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
            this.debugPanel = new System.Windows.Forms.Panel();
            this.debugLabel = new System.Windows.Forms.Label();
            this.btnToggleDebug = new System.Windows.Forms.Button();
            this.lblJitterStrength = new System.Windows.Forms.Label();
            this.trackBarJitter = new System.Windows.Forms.TrackBar();
            this.lblCurrentKey = new System.Windows.Forms.Label();
            this.btnSetKey = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarJitter)).BeginInit();
            this.mainPanel.SuspendLayout();
            this.debugPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
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
            this.mainPanel.Size = new System.Drawing.Size(400, 300);
            this.mainPanel.TabIndex = 0;
            // 
            // debugPanel
            // 
            this.debugPanel.Controls.Add(this.debugLabel);
            this.debugPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.debugPanel.Location = new System.Drawing.Point(20, 200);
            this.debugPanel.Name = "debugPanel";
            this.debugPanel.Size = new System.Drawing.Size(360, 80);
            this.debugPanel.TabIndex = 5;
            this.debugPanel.Visible = false;
            // 
            // debugLabel
            // 
            this.debugLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.debugLabel.ForeColor = System.Drawing.Color.LightGray;
            this.debugLabel.Location = new System.Drawing.Point(0, 0);
            this.debugLabel.Name = "debugLabel";
            this.debugLabel.Size = new System.Drawing.Size(360, 80);
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
            // MacroForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Controls.Add(this.mainPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MacroForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mouse Macro";
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
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
    }
}
