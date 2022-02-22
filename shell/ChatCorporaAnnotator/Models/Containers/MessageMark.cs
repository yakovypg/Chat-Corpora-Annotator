using ChatCorporaAnnotator.Infrastructure.Enums;

namespace ChatCorporaAnnotator.Models.Containers
{
    internal struct MessageMark : IMessageMark
    {
        public string TagName { get; }
        public MessageMarkType MarkType { get; }

        public string MarkTypePresenter
        {
            get
            {
                switch (MarkType)
                {
                    case MessageMarkType.Tagged: return "I";
                    case MessageMarkType.FirstInSituation: return "B";
                    case MessageMarkType.NotTagged: return "O";

                    default: return "*";
                }
            }
        }

        public MessageMark(MessageMarkType markType, string tagName = null)
        {
            MarkType = markType;
            TagName = tagName;
        }

        public override string ToString()
        {
            return MarkType == MessageMarkType.NotTagged || string.IsNullOrEmpty(TagName)
                ? MarkTypePresenter
                : $"{MarkTypePresenter}-{TagName}";
        }

        public static MessageMarkType GetMessageMarkType(bool isTagged, bool isFirstInSituation)
        {
            if (isFirstInSituation)
                return MessageMarkType.FirstInSituation;

            return isTagged
                ? MessageMarkType.Tagged
                : MessageMarkType.NotTagged;
        }
    }
}
