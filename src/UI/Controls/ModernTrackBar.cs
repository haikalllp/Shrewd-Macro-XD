using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace NotesAndTasks
{
    public class ModernTrackBar : TrackBar
    {
        private Color tickColor = Color.FromArgb(250, 91, 101);
        private Color trackColor = Color.FromArgb(214, 37, 106);
        private Color thumbColor = Color.FromArgb(255,255,255);
        private bool isDragging = false;

        [Category("Modern TrackBar")]
        public Color TickColor
        {
            get => tickColor;
            set
            {
                tickColor = value;
                Invalidate();
            }
        }

        [Category("Modern TrackBar")]
        public Color TrackColor
        {
            get => trackColor;
            set
            {
                trackColor = value;
                Invalidate();
            }
        }

        [Category("Modern TrackBar")]
        public Color ThumbColor
        {
            get => thumbColor;
            set
            {
                thumbColor = value;
                Invalidate();
            }
        }

        public ModernTrackBar()
        {
            SetStyle(ControlStyles.UserPaint | 
                    ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.OptimizedDoubleBuffer, true);

            ValueChanged += (s, e) => Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                UpdateValue(e.X);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isDragging)
            {
                UpdateValue(e.X);
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
            base.OnMouseUp(e);
        }

        private void UpdateValue(int mouseX)
        {
            if (mouseX < 10) mouseX = 10;
            if (mouseX > Width - 10) mouseX = Width - 10;

            float valueRange = Maximum - Minimum;
            float pixelRange = Width - 20;
            float valuePerPixel = valueRange / pixelRange;

            int newValue = (int)((mouseX - 10) * valuePerPixel) + Minimum;
            if (newValue < Minimum) newValue = Minimum;
            if (newValue > Maximum) newValue = Maximum;

            Value = newValue;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(BackColor);

            var trackRect = new Rectangle(0, Height / 2 - 2, Width - 1, 4);
            using (var trackBrush = new SolidBrush(trackColor))
            {
                e.Graphics.FillRectangle(trackBrush, trackRect);
            }

            // Draw ticks
            if (TickStyle != TickStyle.None)
            {
                using (var tickPen = new Pen(tickColor, 1))
                {
                    int tickCount = Maximum - Minimum;
                    float tickSpacing = (Width - 20) / (float)tickCount;
                    for (int i = 0; i <= tickCount; i++)
                    {
                        int x = 10 + (int)(i * tickSpacing);
                        e.Graphics.DrawLine(tickPen, x, Height / 2 + 5, x, Height / 2 + 10);
                    }
                }
            }

            // Draw thumb
            float thumbPosition = (Value - Minimum) * (Width - 20) / (float)(Maximum - Minimum) + 10;
            var thumbRect = new Rectangle((int)thumbPosition - 6, Height / 2 - 6, 12, 12);
            using (var thumbBrush = new SolidBrush(thumbColor))
            {
                e.Graphics.FillEllipse(thumbBrush, thumbRect);
            }
        }
    }
}