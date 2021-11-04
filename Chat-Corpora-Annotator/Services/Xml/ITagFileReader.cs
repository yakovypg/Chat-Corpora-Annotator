using ChatCorporaAnnotator.Models.Serialization;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Services.Xml
{
    internal interface ITagFileReader
    {
        void OpenReader(string path);
        void OpenReader();
        void CloseReader();

        SituationData ReadSituation();
        List<SituationData> ReadAllSituations();
    }
}
