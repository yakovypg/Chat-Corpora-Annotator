namespace ChatCorporaAnnotator.Models.Indexing
{
    internal class ProjectInformation : IProjectInformation
    {
        public string Name { get; }
        public string WorkingDirectory { get; }

        public ProjectInformation(string name, string workingDirectory)
        {
            Name = name;
            WorkingDirectory = workingDirectory;
        }
    }
}
