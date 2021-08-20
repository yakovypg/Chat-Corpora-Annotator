using ChatCorporaAnnotator.Data.Imaging;
using IndexEngine;
using IndexEngine.Paths;
using System;
using System.Windows.Media;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal class ChatMessage : IChatMessage
    {
        public DynamicMessage Source { get; }

        public Brush SenderColor
        {
            get
            {
                string senderKey = ProjectInfo.SenderFieldKey;

                if (!Source.Contents.TryGetValue(senderKey, out var user))
                    return Brushes.Black;

                string userKey = user.ToString();

                return ProjectInfo.Data.UserColors.TryGetValue(userKey, out var color)
                    ? new SolidColorBrush(ColorTransformer.ToWindowsColor(color))
                    : Brushes.Black;
            }
        }

        public ChatMessage(DynamicMessage source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public void AddSituation(ISituation situation)
        {
            if (situation == null)
                return;

            Source.AddSituation(situation.Header, situation.ID);
        }
    }
}
