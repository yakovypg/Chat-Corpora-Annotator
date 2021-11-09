using CSharpTest.Net.Collections;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace IndexEngine.Data.Paths
{
    public class ProjectData
    {
        public BTreeDictionary<DateTime, int> MessagesPerDay { get; set; } = new BTreeDictionary<DateTime, int>();
        public HashSet<string> UserKeys { get; set; } = new HashSet<string>();
        public Dictionary<string, Color> UserColors { get; set; } = new Dictionary<string, Color>();
        public List<string> SelectedFields { get; set; } = new List<string>();
        public int LineCount { get; set; }
    }
}
