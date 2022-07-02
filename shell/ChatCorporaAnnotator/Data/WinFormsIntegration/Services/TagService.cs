using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Services
{
    public interface ITagService
    {
        bool TagsetSet { get; set; }
        List<int> TaggedIds { get; set; }
        Dictionary<int, string> SituationContainer { get; set; }
        string ProjectTagset { get; set; }
        void EditTagset(string name, string keys, int op);

        void UnloadData();
    }

    public class TagService : ITagService
    {
        public bool TagsetSet { get; set; } = false;

        public void UnloadData()
        {
            TagsetSet = false;
            TaggedIds.Clear();
            SituationContainer.Clear();
        }
        public Dictionary<int, string> SituationContainer { get; set; } = new Dictionary<int, string>();
        public string ProjectTagset { get; set; }
        public List<int> TaggedIds { get; set; } = new List<int>();

        public void EditTagset(string name, string tag, int op)
        {
        }
    }
}
