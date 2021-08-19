using System.Collections.Generic;

namespace ChatCorporaAnnotator.Models.Indexing.Comparers
{
    internal class FileColumnEqualityComparer : IEqualityComparer<FileColumn>
    {
        public bool Equals(FileColumn x, FileColumn y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            return x.Header == y.Header;
        }

        public int GetHashCode(FileColumn obj)
        {
            return obj.GetHashCode();
        }
    }
}
