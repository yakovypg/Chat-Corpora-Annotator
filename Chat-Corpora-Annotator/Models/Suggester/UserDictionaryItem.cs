namespace ChatCorporaAnnotator.Models.Suggester
{
    internal struct UserDictionaryItem : IUserDictionaryItem
    {
        public string Name { get; }
        public string Content { get; }

        public UserDictionaryItem(string name, string content)
        {
            Name = name;
            Content = content;
        }

        public UserDictionaryItem(string name, params string[] content)
        {
            Name = name;
            Content = string.Join(", ", content);
        }
    }
}
