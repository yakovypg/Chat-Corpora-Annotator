using ChatCorporaAnnotator.Models.Indexing;
using System;

namespace ChatCorporaAnnotator.Infrastructure.Exceptions.Indexing
{
    internal class ProjectException : ApplicationException
    {
        public IProject? Project { get; }

        public ProjectException(string? message = null, IProject? project = null, Exception? innerException = null)
            : base(message, innerException)
        {
            Project = project;
        }
    }
}
