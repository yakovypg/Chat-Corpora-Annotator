using FontAwesome5;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Models.History
{
    internal class RecentProject : IRecentProject
    {
        public static ImageAwesome DefaultIcon => new ImageAwesome() { Icon = EFontAwesomeIcon.Regular_File };

        public string Name { get; }
        public string Path { get; }
        public object Icon { get; set; }

        public RecentProject(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
            Icon = DefaultIcon;
        }

        public RecentProject(string name, string path)
        {
            Name = name;
            Path = path;
            Icon = DefaultIcon;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is RecentProject other &&
                   Name == other.Name &&
                   Path == other.Path;
        }

        public override int GetHashCode()
        {
            int hashCode = 2130088252;

            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);

            return hashCode;
        }
    }
}
