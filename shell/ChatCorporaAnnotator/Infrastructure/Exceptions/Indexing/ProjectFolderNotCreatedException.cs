using System;

namespace ChatCorporaAnnotator.Infrastructure.Exceptions.Indexing
{
    internal class ProjectFolderNotCreatedException : ApplicationException
    {
        public string? FolderPath { get; }

        public ProjectFolderNotCreatedException(string? message = null, string? folderPath = null, Exception? innerException = null)
            : base(message, innerException)
        {
            FolderPath = folderPath;
        }
    }
}
