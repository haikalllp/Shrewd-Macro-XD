using System.Drawing;

namespace NotesAndTasks.UI.Utilities
{
    /// <summary>
    /// Provides a centralized collection of colors used across modern UI controls.
    /// </summary>
    public static class ColorPalette
    {
        /// <summary>
        /// Primary accent color used for borders and tick marks (Pink/Red).
        /// </summary>
        public static readonly Color AccentPrimary = Color.FromArgb(250, 91, 101);

        /// <summary>
        /// Secondary accent color used for hover states and track elements (Darker Pink).
        /// </summary>
        public static readonly Color AccentSecondary = Color.FromArgb(214, 37, 106);

        /// <summary>
        /// Default background color for controls (Dark Purple).
        /// </summary>
        public static readonly Color BackgroundDark = Color.FromArgb(30, 1, 62);

        /// <summary>
        /// Light color used for foreground elements (White).
        /// </summary>
        public static readonly Color ForegroundLight = Color.FromArgb(255, 255, 255);
    }
}