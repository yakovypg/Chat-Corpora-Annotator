using ChatCorporaAnnotator.Data.Imaging;
using System.Windows.Media;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal class ChatUser : IChatUser
    {
        public string Name { get; set; }

        public System.Drawing.Color BackgroundColor { get; set; }
        public Brush BackgroundBrush => new SolidColorBrush(ColorTransformer.ToWindowsColor(BackgroundColor));

        public ChatUser(string name) : this(name, System.Drawing.Color.Transparent)
        {
        }

        public ChatUser(string name, System.Drawing.Color color)
        {
            Name = name;
            BackgroundColor = color;
        }
    }
}
