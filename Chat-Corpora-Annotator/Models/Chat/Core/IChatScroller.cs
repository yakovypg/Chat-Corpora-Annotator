using ChatCorporaAnnotator.Infrastructure.AppEventArgs;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal interface IChatScroller
    {
        void ScrollMessages(ChatScrollingEventArgs scrollArgs);
    }
}
