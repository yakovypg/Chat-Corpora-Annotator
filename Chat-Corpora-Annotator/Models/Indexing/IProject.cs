﻿namespace ChatCorporaAnnotator.Models.Indexing
{
    internal interface IProject
    {
        string Name { get; }
        string FilePath { get; }
        string WorkingDirectory { get; }

        void Initialize();
        void Delete();

        bool TryInitialize();
        bool TryDelete();
    }
}