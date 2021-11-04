using System.Collections.Generic;

namespace ChatCorporaAnnotator.Models.Serialization
{
    internal interface ISituationData
    {
        int Id { get; set; }
        string Header { get; }
        List<int> Messages { get; }
    }
}
