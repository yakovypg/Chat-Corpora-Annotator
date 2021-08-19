using System.Windows.Media;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal interface IChatUser
    {
        string Name { get; set; }

        Brush BackgroundBrush { get; }
        System.Drawing.Color BackgroundColor { get; set; }
    }
}
