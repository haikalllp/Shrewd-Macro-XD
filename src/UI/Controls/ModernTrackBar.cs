using System.Drawing.Drawing2D;
using System.ComponentModel;
using NotesAndTasks.UI.Utilities;

namespace NotesAndTasks
{
    /// <summary>
    /// A modern-styled track bar control with customizable colors and smooth interaction.
    /// </summary>
    public class ModernTrackBar : TrackBar
    {
        private Color tickColor = ColorPalette.AccentPrimary;
        private Color trackColor = ColorPalette.AccentSecondary;
        private Color thumbColor = ColorPalette.ForegroundLight;
        private bool isDragging = false;

        /// <summary>
        /// Gets or sets the color of the tick marks on the track bar.
        /// </summary>
        /// <value>The tick mark color. Default is ColorPalette.AccentPrimary.</value>
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

        /// <summary>
        /// Gets or sets the color of the track bar's main track.
        /// </summary>
        /// <value>The track color. Default is ColorPalette.AccentSecondary.</value>
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

        /// <summary>
        /// Gets or sets the color of the track bar's thumb (slider).
        /// </summary>
        /// <value>The thumb color. Default is ColorPalette.ForegroundLight.</value>
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

        /// <summary>
        /// Initializes a new instance of the ModernTrackBar class with default styling.
        /// </summary>
        public ModernTrackBar()
        {
            SetStyle(ControlStyles.UserPaint | 
                    ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.OptimizedDoubleBuffer, true);

            ValueChanged += (s, e) => Invalidate();
        }

        /// <summary>
        /// Handles the mouse down event to start tracking thumb movement.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                UpdateValue(e.X);
            }
            base.OnMouseDown(e);
        }

        /// <summary>
        /// Handles the mouse move event to update the track bar value during drag operations.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isDragging)
            {
                UpdateValue(e.X);
            }
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Handles the mouse up event to end thumb movement tracking.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
            base.OnMouseUp(e);
        }

        /// <summary>
        /// Updates the track bar value based on the mouse position.
        /// </summary>
        /// <param name="mouseX">The X-coordinate of the mouse position.</param>
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

        /// <summary>
        /// Raises the Paint event to render the track bar with custom styling.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
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