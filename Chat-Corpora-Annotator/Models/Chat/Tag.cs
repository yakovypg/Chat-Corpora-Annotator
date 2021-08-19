using ChatCorporaAnnotator.Data.Imaging;
using System.Windows.Media;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal class Tag : ITag
    {
        public string Header { get; set; }

        public System.Drawing.Color BackgroundColor { get; set; }
        public Brush BackgroundBrush => new SolidColorBrush(ColorTransformer.ToWindowsColor(BackgroundColor));

        public Tag(string header) : this(header, System.Drawing.Color.Transparent)
        {
        }

        public Tag(string header, System.Drawing.Color color)
        {
            Header = header;
            BackgroundColor = color;
        }
    }
}
