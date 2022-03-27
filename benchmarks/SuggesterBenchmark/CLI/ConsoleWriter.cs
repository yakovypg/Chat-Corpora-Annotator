using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

namespace SuggesterBenchmark.CLI
{
    internal static class ConsoleWriter
    {
        public static void PrintDividingLine(int length = 20)
        {
            Console.WriteLine(new string('-', length));
        }

        public static void PrintCaption(string caption, ConsoleColor color = ConsoleColor.White)
        {
            PrintDividingLine(caption.Length + 1);
            PrintMessage(caption + '\n', color);
            PrintDividingLine(caption.Length + 1);
        }

        public static void PrintMessage(string message, ConsoleColor color = ConsoleColor.White)
        {
            var oldColor = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = oldColor;
        }

        public static void PrintBenchmarkSummary(Summary? summary, IConfig? config)
        {
            if (summary == null || config == null)
                return;
            
            var logger = ConsoleLogger.Default;
            var analyser = config.GetAnalysers().First();
            var conclusions = analyser.Analyse(summary).ToList();

            MarkdownExporter.Console.ExportToLog(summary, logger);
            ConclusionHelper.Print(logger, conclusions);

            Console.WriteLine();
        }
    }
}
