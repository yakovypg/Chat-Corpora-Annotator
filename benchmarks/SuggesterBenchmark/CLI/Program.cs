using SuggesterBenchmark.Benchmarks.Config;
using System.IO;

namespace SuggesterBenchmark.CLI
{
    public class Program
    {
        private static void Main()
        {
            bool end = false;

            while (!end)
            {
                bool isDataReceived = GetData(out string[] projectFilePaths, out string[] benchmarks, out string[] parameters)
                    && projectFilePaths.Length > 0
                    && benchmarks.Length > 0;

                if (isDataReceived)
                {
                    foreach (string project in projectFilePaths)
                    {
                        string caption = $"Project {project} is being processed";

                        Console.WriteLine();
                        ConsoleWriter.PrintCaption(caption);

                        ProcessProject(project, benchmarks, parameters);

                        Console.WriteLine();
                        ConsoleWriter.PrintDividingLine(caption.Length + 1);
                    }
                }
                else
                {
                    ConsoleWriter.PrintMessage("Data not received.\n\n", ConsoleColor.DarkRed);
                }

                Console.Write("Start over (y/n): ");
                string? answer = Console.ReadLine()?.ToLower();

                end = answer != "y";

                Console.WriteLine();
            }
        }

        private static void ProcessProject(string projectFilePath, string[] benchmarks, string[] parameters)
        {
            BenchmarksConfig.SetPaths(projectFilePath, "user_dicts.txt", true);

            BenchmarkLauncher.ApplyParameters(parameters);
            BenchmarkLauncher.StartBenchmarking(benchmarks);
        }

        private static bool GetData(out string[] projectFilePaths, out string[] benchmarks, out string[] parameters)
        {
            benchmarks = Array.Empty<string>();
            parameters = Array.Empty<string>();

            Console.Write("Project file paths: ");
            string? projectFileLine = Console.ReadLine();

            Console.Write("Benchmarks: ");
            string? benchmarksLine = Console.ReadLine();

            Console.Write("Parameters: ");
            string? parametersLine = Console.ReadLine();

            projectFilePaths = InputParser.ParseProjectPaths(projectFileLine);
            benchmarks = InputParser.ParseBenchmarks(benchmarksLine);
            parameters = InputParser.ParseParameters(parametersLine);

            string[] unknownProjects = projectFilePaths
                .Where(t => !File.Exists(t))
                .ToArray();

            string[] unknownBenchmarks = benchmarks
                .Where(t => !BenchmarkLauncher.SupportedBenchmarks.Contains(t))
                .ToArray();

            string[] unknownParameters = parameters
                .Where(t => !BenchmarkLauncher.SupportedParameters.Contains(t))
                .ToArray();

            Console.WriteLine();

            if (projectFilePaths.Length == 0)
            {
                ConsoleWriter.PrintMessage("Error: no projects\n", ConsoleColor.DarkRed);
                return false;
            }
            if (benchmarks.Length == 0)
            {
                ConsoleWriter.PrintMessage("Error: no benchmarks\n", ConsoleColor.DarkRed);
                return false;
            }

            if (unknownProjects.Length > 0)
            {
                foreach (var p in unknownProjects)
                    ConsoleWriter.PrintMessage($"Project {p} not found\n", ConsoleColor.DarkRed);
            }
            if (unknownBenchmarks.Length > 0)
            {
                foreach (var b in unknownBenchmarks)
                    ConsoleWriter.PrintMessage($"Benchmark {b} not found\n", ConsoleColor.DarkRed);
            }
            if (unknownParameters.Length > 0)
            {
                foreach (var p in unknownParameters)
                    ConsoleWriter.PrintMessage($"Parameter {p} not found\n", ConsoleColor.DarkRed);
            }
            if (unknownProjects.Length > 0 || unknownBenchmarks.Length > 0 || unknownParameters.Length > 0)
            {
                Console.Write("\nIgnore (y/n): ");
                string? answer = Console.ReadLine()?.ToLower();

                if (answer != "y")
                    return false;
            }

            projectFilePaths = projectFilePaths
                .Where(t => File.Exists(t))
                .ToArray();

            benchmarks = benchmarks
                .Where(t => BenchmarkLauncher.SupportedBenchmarks.Contains(t))
                .ToArray();

            parameters = parameters
                .Where(t => BenchmarkLauncher.SupportedParameters.Contains(t))
                .ToArray();

            return true;
        }
    }
}
