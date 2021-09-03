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
            Source.Contents[ProjectInfo.TextFieldKey] = Source.Id.ToString();
        }

        public void AddSituation(ISituation situation)
        {
            if (situation == null)
                return;

            Source.AddSituation(situation.Header, situation.ID);
        }

        public bool TryGetSender(out string sender)
        {
            if (!Source.Contents.TryGetValue(ProjectInfo.SenderFieldKey, out object senderObj))
            {
                sender = null;
                return false;
            }

            sender = senderObj?.ToString() ?? string.Empty;
            return true;
        }

        public bool TryGetSentDate(out DateTime sentDate)
        {
            if (!Source.Contents.TryGetValue(ProjectInfo.DateFieldKey, out object dateObj))
            {
                sentDate = DateTime.MinValue;
                return false;
            }

            try
            {
                sentDate = DateTime.Parse(dateObj.ToString());
                return true;
            }
            catch
            {
                sentDate = DateTime.MinValue;
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is ChatMessage other && Source.Id == other.Source.Id;
        }

        public override int GetHashCode()
        {
            return Source.Id;
        }
    }
}
