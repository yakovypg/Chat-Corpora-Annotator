using System.Windows.Media;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal interface ITag
    {
        string Header { get; set; }

        Brush BackgroundBrush { get; }
        System.Drawing.Color BackgroundColor { get; set; }
    }
}
