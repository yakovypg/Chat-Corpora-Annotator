using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ChatCorporaAnnotator.Models.History
{
    internal static class RecentProjectParser
    {
        public const char SEPARATOR = '=';

        public static RecentProject Parse(string data)
        {
            int sepIndex = data.IndexOf(SEPARATOR);

            string name = data.Remove(sepIndex);
            string path = data.Substring(sepIndex + 1);

            return new RecentProject(name, path);
        }

        public static HashSet<RecentProject> Parse(string[] data)
        {
            var projects = new HashSet<RecentProject>();

            if (data == null)
                return projects;

            foreach (string item in data)
            {
                var project = Parse(item);
                projects.Add(project);
            }

            return projects;
        }

        public static HashSet<RecentProject> ParseFile(string path)
        {
            string[] data = File.ReadAllLines(path);
            return Parse(data);
        }

        public static bool TryParse(string data, out RecentProject project)
        {
            try
            {
                project = Parse(data);
                return true;
            }
            catch
            {
                project = null;
                return false;
            }
        }

        public static bool TryParse(string[] data, out HashSet<RecentProject> projects)
        {
            try
            {
                projects = Parse(data);
                return true;
            }
            catch
            {
                projects = null;
                return false;
            }
        }

        public static bool TryParseFile(string path, out HashSet<RecentProject> projects)
        {
            try
            {
                projects = ParseFile(path);
                return true;
            }
            catch
            {
                projects = null;
                return false;
            }
        }

        public static string Save(IRecentProject project)
        {
            return project.Name + SEPARATOR + project.Path;
        }

        public static string[] Save(IEnumerable<IRecentProject> projects)
        {
            string[] output = new string[projects?.Count() ?? 0];

            int index = 0;

            foreach (var project in projects)
                output[index++] = Save(project);

            return output;
        }

        public static void SaveToFile(string path, IEnumerable<IRecentProject> projects)
        {
            string[] lines = projects.Select(t => Save(t)).ToArray();
            File.WriteAllLines(path, lines);
        }

        public static bool TrySaveToFile(string path, IEnumerable<IRecentProject> projects)
        {
            try
            {
                SaveToFile(path, projects);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
