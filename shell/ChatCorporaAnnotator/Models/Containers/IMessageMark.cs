using ChatCorporaAnnotator.Infrastructure.Enums;

namespace ChatCorporaAnnotator.Models.Containers
{
    internal interface IMessageMark
    {
        string TagName { get; }
        MessageMarkType MarkType { get; }
    }
}
