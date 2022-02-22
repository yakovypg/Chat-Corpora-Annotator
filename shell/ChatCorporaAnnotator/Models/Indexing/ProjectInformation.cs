using System.IO;

namespace ChatCorporaAnnotator.Models.Indexing
{
    internal class ProjectInformation : IProjectInformation
    {
        public string Name { get; }
        public string WorkingDirectory { get; }

        public string ConfigFilePath => Path.Combine(WorkingDirectory, Name + ".cca");

        public ProjectInformation(string name, string workingDirectory)
        {
            Name = name;
            WorkingDirectory = workingDirectory;
        }
    }
}
