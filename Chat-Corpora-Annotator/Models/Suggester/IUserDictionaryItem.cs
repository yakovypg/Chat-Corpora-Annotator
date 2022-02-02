using System.Collections.Generic;
using System.ComponentModel;

namespace ChatCorporaAnnotator.Models.Suggester
{
    internal interface IUserDictionaryItem : INotifyPropertyChanged
    {
        string Name { get; set; }
        string Content { get; }
        IReadOnlyList<string> Words { get; }

        bool CanAddWordToContent(string word);
        bool AddWordToContent(string word);
        bool RemoveWordFromContent(string word);
    }
}
