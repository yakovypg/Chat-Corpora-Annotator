using ChatCorporaAnnotator.Models.Indexing;
using System;

namespace ChatCorporaAnnotator.Infrastructure.Exceptions.Indexing
{
    internal class ProjectIsInUseException : ProjectException
    {
        public ProjectIsInUseException(string message = null, IProject project = null, Exception innerException = null)
            : base(message, project, innerException)
        {
        }
    }
}
