using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace NotesAndTasks
{
    public class ModernButton : Button
    {
        private int borderRadius = 10;
        private Color borderColor = Color.FromArgb(250, 91, 101);
        private Color hoverBackColor = Color.FromArgb(214, 37, 106);
        private bool isHovered = false;

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

        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.FromArgb(30, 1, 62);
            ForeColor = Color.White;
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