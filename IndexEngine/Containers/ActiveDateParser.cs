using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IndexingServices.Containers
{
    public static class ActiveDateParser
    {
        public static ActiveDate Parse(string data)
        {
            int sepIndex = data.IndexOf(" ");

            string idStr = data.Remove(sepIndex);
            string dateStr = data.Substring(sepIndex + 1);

            int id = int.Parse(idStr);
            DateTime date = DateTime.Parse(dateStr);

            return new ActiveDate(date, id);
        }

        public static HashSet<ActiveDate> ParseFile(string path)
        {
            string[] fileLines = File.ReadAllLines(path);
            var dates = new HashSet<ActiveDate>();

            foreach (string line in fileLines)
            {
                ActiveDate date = Parse(line);
                dates.Add(date);
            }

            return dates;
        }

        public static bool TryParse(string data, out ActiveDate date)
        {
            try
            {
                date = Parse(data);
                return true;
            }
            catch
            {
                date = null;
                return false;
            }
        }

        public static bool TryParseFile(string path, out HashSet<ActiveDate> dates)
        {
            try
            {
                dates = ParseFile(path);
                return true;
            }
            catch
            {
                dates = null;
                return false;
            }
        }

        public static string Save(ActiveDate date)
        {
            return date.MessageId + " " + date.Date.ToShortDateString();
        }

        public static void SaveToFile(string path, IEnumerable<ActiveDate> dates)
        {
            string[] lines = dates.Select(t => Save(t)).ToArray();
            File.WriteAllLines(path, lines);
        }

        public static bool TrySaveToFile(string path, IEnumerable<ActiveDate> dates)
        {
            try
            {
                SaveToFile(path, dates);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
