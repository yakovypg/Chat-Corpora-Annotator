using Edu.Stanford.Nlp.Pipeline;

namespace CoreNLPEngine.Extensions
{
    internal static class ParseTreeExtensions
    {
        public static List<ParseTree> GetLeaves(this ParseTree tree)
        {
            if (tree.Child == null || tree.Child.Count == 0)
                return new List<ParseTree>() { tree };

            var leaves = new List<ParseTree>();

            foreach (var child in tree.Child)
            {
                List<ParseTree> childLeaves = child.GetLeaves();
                leaves.AddRange(childLeaves);
            }

            return leaves;
        }
    }
}
