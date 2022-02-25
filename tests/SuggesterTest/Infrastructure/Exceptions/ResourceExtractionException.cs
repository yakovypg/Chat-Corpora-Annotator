using System;

namespace SuggesterTest.Infrastructure.Exceptions
{
    public class ResourceExtractionException : ApplicationException
    {
        public const string DEFAULT_EXCEPTION_MESSAGE = "Failed to extract file from resources.";
        
        public object? ResourceFile { get; }

        public ResourceExtractionException(object? resourceFile, string message = DEFAULT_EXCEPTION_MESSAGE,
            Exception? innerException = null) : base(message, innerException)
        {
            ResourceFile = resourceFile;
        }
    }
}
