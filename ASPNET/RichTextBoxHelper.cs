using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Common
{
    public static class RichTextBoxHelper
    {
    }

    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }

        /// <summary>
        /// Moves the caret to end of the text. Then scrolls to the caret.
        /// </summary>
        /// <param name="box"></param>
        public static void ScrollToEnd(this RichTextBox box)
        {
            //Scroll to end
            box.SelectionStart = box.Text.Length;
            box.ScrollToCaret();
        }
    }
}
