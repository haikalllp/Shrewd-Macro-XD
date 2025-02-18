using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MouseMacro
{
    public partial class MacroForm : Form
    {
        private bool isMacroOn = false;
        private Keys toggleKey = Keys.F6;
        private int jitterStrength = 10;
        private bool isSettingKey = false;
        private Label debugLabel;

        public MacroForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
            InitializeEventHandlers();
            UpdateDebugLabel();
        }

        private void InitializeCustomComponents()
        {
            // Form settings
            this.Text = "Mouse Macro";
            this.Size = new System.Drawing.Size(300, 250);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Labels
            Label lblToggleKeyTitle = new Label();
            lblToggleKeyTitle.Text = "Toggle Key:";
            lblToggleKeyTitle.Location = new System.Drawing.Point(20, 20);
            lblToggleKeyTitle.AutoSize = true;

            Label lblCurrentKey = new Label();
            lblCurrentKey.Text = toggleKey.ToString();
            lblCurrentKey.Location = new System.Drawing.Point(120, 20);
            lblCurrentKey.AutoSize = true;
            lblCurrentKey.Font = new System.Drawing.Font(lblCurrentKey.Font, System.Drawing.FontStyle.Bold);

            Button btnSetKey = new Button();
            btnSetKey.Text = "Click to Set New Key";
            btnSetKey.Location = new System.Drawing.Point(20, 50);
            btnSetKey.Size = new System.Drawing.Size(120, 30);
            btnSetKey.Click += btnSetKey_Click;

            Label lblJitterStrength = new Label();
            lblJitterStrength.Text = "Jitter Strength:";
            lblJitterStrength.Location = new System.Drawing.Point(20, 100);
            lblJitterStrength.AutoSize = true;

            // TrackBar for jitter strength
            TrackBar trackBarJitter = new TrackBar();
            trackBarJitter.Location = new System.Drawing.Point(120, 100);
            trackBarJitter.Size = new System.Drawing.Size(150, 45);
            trackBarJitter.Minimum = 1;
            trackBarJitter.Maximum = 20;
            trackBarJitter.Value = jitterStrength;
            trackBarJitter.TickFrequency = 1;
            trackBarJitter.ValueChanged += trackBarJitter_ValueChanged;

            // Add debug label at the bottom
            debugLabel = new Label
            {
                Dock = DockStyle.Bottom,
                AutoSize = false,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                Font = new Font("Consolas", 9)
            };

            // Add controls to the form
            this.Controls.Add(lblToggleKeyTitle);
            this.Controls.Add(lblCurrentKey);
            this.Controls.Add(btnSetKey);
            this.Controls.Add(lblJitterStrength);
            this.Controls.Add(trackBarJitter);
            this.Controls.Add(debugLabel);
        }

        private void UpdateDebugLabel()
        {
            debugLabel.Text = $"DEBUG:\r\nMacro: {(isMacroOn ? "ON" : "OFF")} | Toggle Key: {toggleKey} | Strength: {jitterStrength}\r\nMouse Buttons: {Control.MouseButtons}";
        }

        private void OnToggleKeyPress(object sender, KeyEventArgs e)
        {
            if (isSettingKey)
            {
                // Don't allow modifier keys alone as toggle keys
                if (e.KeyCode != Keys.ControlKey && e.KeyCode != Keys.ShiftKey && e.KeyCode != Keys.Menu)
                {
                    toggleKey = e.KeyCode;
                    ((Button)this.Controls.Find("btnSetKey", false)[0]).Text = $"Click to Set New Key ({toggleKey})";
                    isSettingKey = false;
                    e.Handled = true;
                    UpdateDebugLabel();
                    return;
                }
            }

            if (e.KeyCode == toggleKey)
            {
                isMacroOn = !isMacroOn;
                this.Text = $"Mouse Macro ({(isMacroOn ? "ON" : "OFF")})";
                UpdateDebugLabel();
                e.Handled = true;
            }
        }

        private void MacroForm_MouseMove(object sender, MouseEventArgs e)
        {
            // Check if macro is active and both left and right mouse buttons are pressed
            if(isMacroOn && ((Control.MouseButtons & MouseButtons.Left) != 0) && ((Control.MouseButtons & MouseButtons.Right) != 0))
            {
                // Apply jitter effect
                Random rnd = new Random();
                int dx = rnd.Next(-jitterStrength, jitterStrength + 1);
                int dy = rnd.Next(-jitterStrength, jitterStrength + 1);
                Point currentPos = Cursor.Position;
                Cursor.Position = new Point(currentPos.X + dx, currentPos.Y + dy);
                UpdateDebugLabel(); // Update debug info when jitter is applied
            }
        }

        private void trackBarJitter_ValueChanged(object sender, EventArgs e)
        {
            jitterStrength = ((TrackBar)sender).Value;
            UpdateDebugLabel();
        }

        private void btnSetKey_Click(object sender, EventArgs e)
        {
            isSettingKey = true;
            ((Button)sender).Text = "Press any key...";
            UpdateDebugLabel();
        }

        private void InitializeEventHandlers()
        {
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(OnToggleKeyPress);
            this.MouseMove += new MouseEventHandler(MacroForm_MouseMove);
        }

        private void MacroForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.KeyDown -= new KeyEventHandler(OnToggleKeyPress);
            this.MouseMove -= new MouseEventHandler(MacroForm_MouseMove);
        }
    }
}
