namespace IndexEngine.Parsers
{
    public class DateTimeFormatException : ApplicationException
    {
        public string SourceString { get; }
        public string CorrectedSourceString { get; }

        public DateTimeFormatException(string sourceString, string correctedSourceString, string? message = null, Exception? innerException = null) :
            base(message ?? $"The source string '{sourceString}' is not in a valid format.", innerException)
        {
            SourceString = sourceString;
            CorrectedSourceString = correctedSourceString;
        }
    }
}
