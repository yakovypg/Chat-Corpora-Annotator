using ChatCorporaAnnotator.Models.Indexing;
using System;

namespace ChatCorporaAnnotator.Infrastructure.Exceptions.Indexing
{
    internal class ProjectNotCreatedException : ProjectException
    {
        public ProjectNotCreatedException(string message = null, IProject project = null, Exception innerException = null)
            : base(message, project, innerException)
        {
        }
    }
}
