namespace ChatCorporaAnnotator.Models.Indexing
{
    internal interface IProject
    {
        string Name { get; }
        string CsvFilePath { get; }
        string ConfigFilePath { get; }
        string WorkingDirectory { get; }

        ProjectInformation GetInfo();

        void Initialize();
        void Delete();

        bool TryInitialize();
        bool TryDelete();
    }
}
