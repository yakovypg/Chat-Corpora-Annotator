using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ChatCorporaAnnotator.Models.Suggester
{
    internal class UserDictionaryItem : IUserDictionaryItem
    {
        private readonly List<string> _words;

        public string Name { get; set; }

        public string Content => string.Join(", ", _words);
        public IReadOnlyList<string> Words => _words;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public UserDictionaryItem(string name, params string[] content)
        {
            Name = name;
            _words = new List<string>();

            if (content != null)
            {
                _words.AddRange(content
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Distinct()
                    .ToArray());
            }
        }

        public bool CanAddWordToContent(string word)
        {
            return !_words.Contains(word);
        }

        public bool AddWordToContent(string word)
        {
            if (!CanAddWordToContent(word))
                return false;

            _words.Add(word);
            OnPropertyChanged(nameof(Content));

            return true;
        }

        public bool RemoveWordFromContent(string word)
        {
            var result = _words.Remove(word);

            if (result)
                OnPropertyChanged(nameof(Content));

            return result;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
