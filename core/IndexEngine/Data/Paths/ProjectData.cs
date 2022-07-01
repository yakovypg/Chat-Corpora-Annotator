using CSharpTest.Net.Collections;
using IndexEngine.Containers;
using System.Drawing;

namespace IndexEngine.Data.Paths
{
    public class ProjectData
    {
        public BTreeDictionary<DateTime, int> MessagesPerDay { get; set; } = new BTreeDictionary<DateTime, int>();
        public HashSet<ActiveDate> ActiveDates { get; set; } = new HashSet<ActiveDate>();
        public HashSet<string> UserKeys { get; set; } = new HashSet<string>();
        public Dictionary<string, Color> UserColors { get; set; } = new Dictionary<string, Color>();
        public List<string> SelectedFields { get; set; } = new List<string>();
        public int LineCount { get; set; }
    }
}
