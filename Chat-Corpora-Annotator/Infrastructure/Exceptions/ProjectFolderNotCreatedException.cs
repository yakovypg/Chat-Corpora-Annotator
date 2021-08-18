using System;

namespace ChatCorporaAnnotator.Infrastructure.Exceptions
{
    internal class ProjectFolderNotCreatedException : ApplicationException
    {
        public string FolderPath { get; }

        public ProjectFolderNotCreatedException(string message, string folderPath = null, Exception innerException = null)
            : base(message, innerException)
        {
            FolderPath = folderPath;
        }
    }
}
