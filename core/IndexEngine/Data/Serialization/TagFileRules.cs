namespace IndexEngine.Data.Serialization
{
    public static class TagFileRules
    {
        public const string START_ELEMENT = "Corpus";
        public const string SITUATION_ELEMENT_NAME = "Situation";

        public const string MESSAGE_ELEMENT_NAME = "Message";
        public const string MESSAGE_TEXT_ELEMENT_NAME = "Text";
        public const string MESSAGE_USER_ELEMENT_NAME = "User";
        public const string MESSAGE_DATE_ELEMENT_NAME = "Date";

        public const string MESSAGE_ID_ATTRIBUTE = "id";
        public const string SITUATION_ID_ATTRIBUTE = "SId";
    }
}
