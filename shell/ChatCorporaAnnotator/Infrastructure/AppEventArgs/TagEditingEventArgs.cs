using ChatCorporaAnnotator.Infrastructure.Enums;

namespace ChatCorporaAnnotator.Infrastructure.AppEventArgs
{
    internal class TagEditingEventArgs : TaggerEventArgs
    {
        public string NewHeader { get; }
        public TagEditType EditType { get; }

        public TagEditingEventArgs(string newHeader)
        {
            NewHeader = newHeader;
            EditType = TagEditType.ChangeHeader;
        }
    }

    internal delegate void TagEditingEventHandler(object sender, TagEditingEventArgs args);
}
