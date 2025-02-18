using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace MouseMacro
{
    public partial class MacroForm : Form
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;

        private delegate IntPtr LowLevelHookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelHookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        private IntPtr keyboardHookID = IntPtr.Zero;
        private IntPtr mouseHookID = IntPtr.Zero;
        private readonly LowLevelHookProc keyboardProc;
        private readonly LowLevelHookProc mouseProc;

        private bool isMacroOn = false;
        private Keys toggleKey = Keys.Capital;  
        private int jitterStrength = 3;  
        private bool isSettingKey = false;
        private Label debugLabel;
        private Button btnSetKey;
        private System.Threading.Timer jitterTimer;
        private bool isJittering = false;
        private int currentStep = 0;
        private readonly object lockObject = new object();
        private bool leftButtonDown = false;
        private bool rightButtonDown = false;

        private readonly (int dx, int dy)[] jitterPattern = new[]
        {
            (7, 7), (-7, -7), (0, 7), (7, 7), (-7, -7),
            (0, 6), (7, 7), (-7, -7), (0, 7), (7, 7),
            (-7, -7), (0, 6), (7, 7), (-7, -7), (0, 6),
            (7, 7), (-7, -7), (0, 7), (7, 7), (-7, -7),
            (0, 6), (7, 7), (-7, -7), (0, 6)
        };

        public MacroForm()
        {
            keyboardProc = KeyboardHookCallback;
            mouseProc = MouseHookCallback;
            
            InitializeComponent();
            InitializeCustomComponents();
            InitializeHooks();
            
            jitterTimer = new System.Threading.Timer(
                JitterCallback, 
                null, 
                System.Threading.Timeout.Infinite, 
                System.Threading.Timeout.Infinite
            );
            
            UpdateDebugLabel();
        }

        private void InitializeHooks()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                keyboardHookID = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardProc,
                    GetModuleHandle(curModule.ModuleName), 0);
                mouseHookID = SetWindowsHookEx(WH_MOUSE_LL, mouseProc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                
                switch ((int)wParam)
                {
                    case WM_LBUTTONDOWN:
                        leftButtonDown = true;
                        break;
                    case WM_LBUTTONUP:
                        leftButtonDown = false;
                        break;
                    case WM_RBUTTONDOWN:
                        rightButtonDown = true;
                        break;
                    case WM_RBUTTONUP:
                        rightButtonDown = false;
                        break;
                }

                if (isMacroOn && leftButtonDown && rightButtonDown && !isJittering)
                {
                    StartJitter();
                }
                else if (!leftButtonDown || !rightButtonDown)
                {
                    StopJitter();
                }

                this.BeginInvoke(new Action(UpdateDebugLabel));
            }
            return CallNextHookEx(mouseHookID, nCode, wParam, lParam);
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                
                if (isSettingKey)
                {
                    toggleKey = (Keys)vkCode;
                    this.BeginInvoke(new Action(() => {
                        btnSetKey.Text = $"Click to Set New Key ({toggleKey})";
                        isSettingKey = false;
                        UpdateDebugLabel();
                    }));
                    return (IntPtr)1;
                }
                
                if ((Keys)vkCode == toggleKey)
                {
                    isMacroOn = !isMacroOn;
                    this.BeginInvoke(new Action(() => {
                        this.Text = $"Mouse Macro ({(isMacroOn ? "ON" : "OFF")})";
                        UpdateDebugLabel();
                    }));
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(keyboardHookID, nCode, wParam, lParam);
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Mouse Macro";
            this.Size = new System.Drawing.Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(300, 200);

            Label lblToggleKeyTitle = new Label();
            lblToggleKeyTitle.Text = "Toggle Key:";
            lblToggleKeyTitle.Location = new System.Drawing.Point(20, 20);
            lblToggleKeyTitle.AutoSize = true;

            btnSetKey = new Button();
            btnSetKey.Text = $"Click to Set New Key ({toggleKey})";
            btnSetKey.Location = new System.Drawing.Point(20, 50);
            btnSetKey.Size = new System.Drawing.Size(160, 30);
            btnSetKey.Click += btnSetKey_Click;

            Label lblJitterStrength = new Label();
            lblJitterStrength.Text = "Jitter Strength:";
            lblJitterStrength.Location = new System.Drawing.Point(20, 100);
            lblJitterStrength.AutoSize = true;

            TrackBar trackBarJitter = new TrackBar();
            trackBarJitter.Location = new System.Drawing.Point(120, 100);
            trackBarJitter.Size = new System.Drawing.Size(150, 45);
            trackBarJitter.Minimum = 1;
            trackBarJitter.Maximum = 20;
            trackBarJitter.Value = jitterStrength;
            trackBarJitter.TickFrequency = 1;
            trackBarJitter.ValueChanged += trackBarJitter_ValueChanged;

            debugLabel = new Label
            {
                Dock = DockStyle.Bottom,
                AutoSize = false,
                Height = 80,
                TextAlign = ContentAlignment.MiddleLeft,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                Font = new Font("Consolas", 9)
            };

            this.Controls.Add(lblToggleKeyTitle);
            this.Controls.Add(btnSetKey);
            this.Controls.Add(lblJitterStrength);
            this.Controls.Add(trackBarJitter);
            this.Controls.Add(debugLabel);
        }

        private void UpdateDebugLabel()
        {
            if (this.IsDisposed) return;

            GetCursorPos(out POINT currentPos);
            var mouseState = "";
            if (leftButtonDown) mouseState += "LMB ";
            if (rightButtonDown) mouseState += "RMB ";
            if (string.IsNullOrEmpty(mouseState)) mouseState = "None";

            debugLabel.Text = $"DEBUG:\r\n" +
                            $"Macro: {(isMacroOn ? "ON" : "OFF")} | Key: {toggleKey} | Strength: {jitterStrength}\r\n" +
                            $"Mouse Buttons: {mouseState} | Cursor: {currentPos.X},{currentPos.Y}\r\n" +
                            $"Jittering: {isJittering} | Step: {currentStep}";
        }

        private void JitterCallback(object state)
        {
            if (!isMacroOn || !leftButtonDown || !rightButtonDown)
            {
                StopJitter();
                return;
            }

            lock (lockObject)
            {
                GetCursorPos(out POINT currentPos);
                var pattern = jitterPattern[currentStep];
                SetCursorPos(currentPos.X + pattern.dx, currentPos.Y + pattern.dy);
                currentStep = (currentStep + 1) % jitterPattern.Length;
                
                this.BeginInvoke(new Action(UpdateDebugLabel));
            }
        }

        private void StartJitter()
        {
            if (!isJittering)
            {
                isJittering = true;
                currentStep = 0;
                jitterTimer.Change(0, 10); 
            }
        }

        private void StopJitter()
        {
            if (isJittering)
            {
                isJittering = false;
                jitterTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                this.BeginInvoke(new Action(UpdateDebugLabel));
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
            btnSetKey.Text = "Press any key...";
            UpdateDebugLabel();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopJitter();
            jitterTimer.Dispose();
            UnhookWindowsHookEx(keyboardHookID);
            UnhookWindowsHookEx(mouseHookID);
            base.OnFormClosing(e);
        }
    }
}
