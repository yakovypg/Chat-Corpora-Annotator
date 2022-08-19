namespace IndexEngine.Parsers
{
    public class CultureNotSupportedException : ApplicationException
    {
        public string CultureCode { get; set; }

        public CultureNotSupportedException(string cultureCode, string? message = null, Exception? innerException = null) :
            base(message ?? $"Culture {cultureCode} not supported.", innerException)
        {
            CultureCode = cultureCode;
        }
    }
}
