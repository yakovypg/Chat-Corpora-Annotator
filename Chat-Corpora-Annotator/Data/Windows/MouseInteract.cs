using System.Drawing;
using System.Windows.Input;

namespace ChatCorporaAnnotator.Data.Windows
{
    internal static class MouseInteract
    {
        public static bool IsLeftPressed => Mouse.LeftButton == MouseButtonState.Pressed;
        public static bool IsRightPressed => Mouse.RightButton == MouseButtonState.Pressed;

        public static void MoveCursor(int x, int y)
        {
            var cursourPos = new Point(x, y);
            System.Windows.Forms.Cursor.Position = cursourPos;
        }
    }
}
