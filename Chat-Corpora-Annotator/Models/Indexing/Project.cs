﻿using ChatCorporaAnnotator.Infrastructure.Exceptions;
using System;
using System.IO;

namespace ChatCorporaAnnotator.Models.Indexing
{
    internal class Project : IProject
    {
        public string Name { get; }
        public string FilePath { get; }
        public string WorkingDirectory { get; private set; }

        public Project(string filePath, bool initializeImmediately = false)
        {
            FilePath = filePath;
            Name = Path.GetFileNameWithoutExtension(FilePath);

            if (initializeImmediately)
                Initialize();
        }

        public void Initialize()
        {
            string projectFolderPath = null;

            try
            {
                string userProfileFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                string ccaFolderPath = Path.Combine(userProfileFolderPath, "CCA");
                projectFolderPath = Path.Combine(ccaFolderPath, Name);

                if (!Directory.Exists(ccaFolderPath))
                    Directory.CreateDirectory(ccaFolderPath);

                Directory.CreateDirectory(projectFolderPath);

                WorkingDirectory = projectFolderPath;
            }
            catch (Exception ex)
            {
                var folderEx = new ProjectFolderNotCreatedException("Failed to create the project folder.", projectFolderPath, ex);
                var projectEx = new ProjectNotCreatedException("Failed to create the project.", this, folderEx);

                throw projectEx;
            }
        }

        public void Delete()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);

            if (Directory.Exists(WorkingDirectory))
                Directory.Delete(WorkingDirectory);
        }

        public bool TryInitialize()
        {
            try
            {
                Initialize();
                return true;
            }
            catch { return false; }
        }

        public bool TryDelete()
        {
            try
            {
                Delete();
                return true;
            }
            catch { return false; }
        }
    }
}
