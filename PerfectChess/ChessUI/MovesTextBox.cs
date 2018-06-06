using System.Drawing;
using System.Windows.Forms;

namespace PerfectChess
{
    class MovesTextBox : RichTextBox
    {
        public MovesTextBox() : base()
        {

        }

        /// <summary>
        /// Appends text with specified color.
        /// </summary>
        /// <param name="text">Text to append.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="scroll">Scroll to the last line.</param>
        /// <param name="refresh">Refresh the control.</param>
        public void AddText(string text, Color? textColor = null, bool scroll = true, bool refresh = true)
        {
            // Set selection
            this.SelectionStart = this.TextLength;
            this.SelectionLength = 0;

            // Save current color
            Color currentColor = this.SelectionColor;
            bool changeColor = textColor != null && textColor != currentColor;

            // Set the color, append the text and restore the color
            if (changeColor) this.SelectionColor = (Color)textColor;
            this.AppendText(text);
            if (changeColor) this.SelectionColor = currentColor;

            // Scroll to the last line
            if (scroll) this.Scroll();

            // Refresh the control
            if (refresh) this.Refresh();
        }

        /// <summary>
        /// Sets text with specified color.
        /// </summary>
        /// <param name="text">Text to set.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="scroll">Scroll to the last line.</param>
        /// <param name="refresh">Refresh the control.</param>
        public void SetText(string text, Color? textColor = null, bool scroll = true, bool refresh = true)
        {
            this.ResetText();
            this.AddText(text, textColor, scroll, refresh);
        }

        /// <summary>
        /// Scrolls to the last line.
        /// </summary>
        public void Scroll()
        {
            this.SelectionStart = this.TextLength;
            this.ScrollToCaret();
        }

        /// <summary>
        /// Temporarily saved text.
        /// </summary>
        public string SavedText;
    }
}
