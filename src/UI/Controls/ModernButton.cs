using System.Drawing.Drawing2D;
using System.ComponentModel;
using NotesAndTasks.UI.Utilities;

namespace NotesAndTasks
{
    /// <summary>
    /// A modern-styled button control with rounded corners, custom colors, and hover effects.
    /// </summary>
    public class ModernButton : Button
    {
        private int borderRadius = 10;
        private Color borderColor = ColorPalette.AccentPrimary;
        private Color hoverBackColor = ColorPalette.AccentSecondary;
        private bool isHovered = false;

        /// <summary>
        /// Gets or sets the radius of the button's rounded corners.
        /// </summary>
        /// <value>The border radius in pixels. Default value is 10.</value>
        [Category("Modern Button")]
        public int BorderRadius
        {
            get => borderRadius;
            set
            {
                borderRadius = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the color of the button's border.
        /// </summary>
        /// <value>The border color. Default is ColorPalette.AccentPrimary.</value>
        [Category("Modern Button")]
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the background color when the mouse hovers over the button.
        /// </summary>
        /// <value>The hover background color. Default is ColorPalette.AccentSecondary.</value>
        [Category("Modern Button")]
        public Color HoverBackColor
        {
            get => hoverBackColor;
            set
            {
                hoverBackColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Initializes a new instance of the ModernButton class with default styling.
        /// </summary>
        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = ColorPalette.BackgroundDark;
            ForeColor = ColorPalette.ForegroundLight;
            Font = new Font("JetBrains Mono", 10F, FontStyle.Regular, GraphicsUnit.Point);
            Size = new Size(150, 40);
            Cursor = Cursors.Hand;

            MouseEnter += (s, e) =>
            {
                isHovered = true;
                Invalidate();
            };

            MouseLeave += (s, e) =>
            {
                isHovered = false;
                Invalidate();
            };
        }

        /// <summary>
        /// Raises the Paint event to render the button with custom styling.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            var graphicsPath = new GraphicsPath();
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = borderRadius;

            // Create rounded rectangle path
            graphicsPath.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            graphicsPath.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            graphicsPath.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            graphicsPath.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            graphicsPath.CloseFigure();

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Fill background
            using (var brush = new SolidBrush(isHovered ? hoverBackColor : BackColor))
            {
                e.Graphics.FillPath(brush, graphicsPath);
            }

            // Draw border
            using (var pen = new Pen(borderColor, 1))
            {
                e.Graphics.DrawPath(pen, graphicsPath);
            }

            // Draw text
            var textRect = new Rectangle(0, 0, Width, Height);
            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            graphicsPath.Dispose();
        }
    }
}