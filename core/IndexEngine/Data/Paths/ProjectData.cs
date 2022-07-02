using IndexEngine.Containers;
using System.Drawing;

namespace IndexEngine.Data.Paths
{
    public class ProjectData
    {
        public int LineCount { get; set; }

        public List<string> SelectedFields { get; set; }
        public HashSet<string> UserKeys { get; set; }
        public Dictionary<string, Color> UserColors { get; set; }
        public HashSet<ActiveDate> ActiveDates { get; set; }
        public Dictionary<DateTime, int> MessagesPerDay { get; set; }

        public ProjectData()
        {
            SelectedFields = new List<string>();
            UserKeys = new HashSet<string>();
            UserColors = new Dictionary<string, Color>();
            ActiveDates = new HashSet<ActiveDate>();
            MessagesPerDay = new Dictionary<DateTime, int>();
        }
    }
}
