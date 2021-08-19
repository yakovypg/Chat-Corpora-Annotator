using System;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal interface IMessageDate
    {
        DateTime Source { get; }

        string Presenter { get; }

        string ShortDatePresenter { get; }
        string LongDatePresenter { get; }

        string LongTimePresenter { get; }
        string ShortTimePresenter { get; }
    }
}
