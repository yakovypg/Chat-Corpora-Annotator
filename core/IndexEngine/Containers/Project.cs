namespace IndexEngine.Containers
{
    public class Project
    {
        public const char SEPARATOR = '=';

        public string Name { get; }
        public string ConfigFilePath { get; }
        public string WorkingDirectory { get; }

        public Dictionary<string, string> Paths { get; }

        public Project(string filePath)
        {
            string[] data = File.ReadAllLines(filePath)
                .Where(t => !string.IsNullOrEmpty(t) && !string.IsNullOrWhiteSpace(t))
                .ToArray();

            Paths = data.ToDictionary(t => t.Remove(t.IndexOf(SEPARATOR)),
                                      t => t.Substring(t.IndexOf(SEPARATOR) + 1));

            ConfigFilePath = filePath;
            Name = Path.GetFileNameWithoutExtension(filePath);
            WorkingDirectory = Directory.GetParent(filePath).FullName;
        }

        public static Project Create(string filePath)
        {
            File.WriteAllText(filePath, "Info=info");
            return new Project(filePath);
        }
    }
}
