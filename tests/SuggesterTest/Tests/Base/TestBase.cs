using IndexEngine.Data.Paths;
using IndexEngine.Indexes;
using IndexEngine.Search;
using SuggesterTest.Infrastructure.Exceptions;
using System.IO;
using System.IO.Compression;

namespace SuggesterTest.Tests.Base
{
    public class TestBase
    {
        private const string USER_DICTS_NAME = "user_dicts";
        private const string USER_DICTS_FILE_NAME = $"{USER_DICTS_NAME}.txt";

        private const string TEST_PROJECT_NAME = "test_suggester_1";
        private const string TEST_PROJECT_FILE_NAME = $"{TEST_PROJECT_NAME}.cca";
        private const string TEST_PROJECT_ZIP_FILE_NAME = $"{TEST_PROJECT_NAME}.zip";

        private const string EXTRACTED_DATA_FOLDER_PATH = "TestData";

        public string WorkingDirectory { get; private set; } = string.Empty;
        public string ProjectFolderPath { get; private set; } = string.Empty;

        public string ProjectFilePath { get; private set; } = string.Empty;
        public string ProjectZipFilePath { get; private set; } = string.Empty;
        public string UserDictsFilePath { get; private set; } = string.Empty;

        private static int CreatedTestsCount = 0;

        public TestBase()
        {
            SetPaths();

            if (CreatedTestsCount++ == 0)
            {
                DeleteOldTests();
                ExtractTestProject();
            }

            ProjectInfo.LoadProject(ProjectFilePath);
            LuceneService.OpenIndex();
            UserDictsIndex.GetInstance().ImportIndex(UserDictsFilePath);
        }

        private void ExtractTestProject()
        {
            object? projectResObj = Properties.Resources.ResourceManager.GetObject(TEST_PROJECT_NAME);
            object? userDictsResObj = Properties.Resources.ResourceManager.GetObject(USER_DICTS_NAME);

            if (projectResObj is not byte[] zipProjectBytes)
                throw new ResourceExtractionException(projectResObj);

            if (userDictsResObj is not string userDictsData)
                throw new ResourceExtractionException(userDictsResObj);

            Directory.CreateDirectory(WorkingDirectory);

            File.WriteAllBytes(ProjectZipFilePath, zipProjectBytes);
            File.WriteAllText(UserDictsFilePath, userDictsData);

            ZipFile.ExtractToDirectory(ProjectZipFilePath, WorkingDirectory);

            if (!File.Exists(ProjectFilePath))
                throw new ResourceExtractionException(projectResObj);

            if (!File.Exists(UserDictsFilePath))
                throw new ResourceExtractionException(userDictsResObj);
        }

        private void SetPaths()
        {
            WorkingDirectory = GetWorkingDirectory();

            UserDictsFilePath = Path.Combine(WorkingDirectory, USER_DICTS_FILE_NAME);
            ProjectFolderPath = Path.Combine(WorkingDirectory, TEST_PROJECT_NAME);
            ProjectZipFilePath = Path.Combine(WorkingDirectory, TEST_PROJECT_ZIP_FILE_NAME);
            ProjectFilePath = Path.Combine(ProjectFolderPath, TEST_PROJECT_FILE_NAME);
        }

        private static string GetWorkingDirectory()
        {
            string name = TEST_PROJECT_NAME;
            string workingDirectory = Path.Combine(EXTRACTED_DATA_FOLDER_PATH, name);

            return workingDirectory;
        }

        private static void DeleteOldTests()
        {
            if (!Directory.Exists(EXTRACTED_DATA_FOLDER_PATH))
                return;

            var dirInfo = new DirectoryInfo(EXTRACTED_DATA_FOLDER_PATH);
            var oldTests = dirInfo.GetDirectories();

            for (int i = 0; i < oldTests.Length; ++i)
            {
                try
                {
                    new DirectoryInfo(oldTests[i].FullName).Delete(true);
                }
                catch { }
            }
        }
    }
}
