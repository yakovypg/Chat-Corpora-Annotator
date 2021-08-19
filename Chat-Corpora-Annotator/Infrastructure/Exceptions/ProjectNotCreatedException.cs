using ChatCorporaAnnotator.Models.Indexing;
using System;

namespace ChatCorporaAnnotator.Infrastructure.Exceptions
{
    internal class ProjectNotCreatedException : ApplicationException
    {
        public Project Project { get; }

        public ProjectNotCreatedException(string message = null, Project project = null, Exception innerException = null)
            : base (message, innerException)
        {
            Project = project;
        }
    }
}
