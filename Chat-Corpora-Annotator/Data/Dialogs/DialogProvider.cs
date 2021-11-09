using System.Drawing;
using System.Windows.Forms;

namespace ChatCorporaAnnotator.Data.Dialogs
{
    internal static class DialogProvider
    {
        public static bool GetCcaFilePath(out string path)
        {
            int filterIndex = 0;
            string title = "Open cca file";
            string filter = "CCA files|*.cca";

            return GetFilePath(out path, title, filter, filterIndex);
        }

        public static bool GetCsvFilePath(out string path)
        {
            int filterIndex = 0;
            string title = "Open a separated-value file";
            string filter = "CSV files|*.csv|TSV files|*.tsv";

            return GetFilePath(out path, title, filter, filterIndex);
        }

        public static bool GetXmlFilePath(out string path)
        {
            int filterIndex = 0;
            string title = "Open xml file";
            string filter = "XML files|*.xml";

            return GetFilePath(out path, title, filter, filterIndex);
        }

        public static bool GetFilePath(out string path, string title = null, string filter = null, int filterIndex = 0)
        {
            var _openFileDialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DereferenceLinks = true,
                RestoreDirectory = true,

                FilterIndex = filterIndex,
                Title = title,
                Filter = filter
            };

            if (_openFileDialog.ShowDialog() == DialogResult.OK)
            {
                path = _openFileDialog.FileName;
                return !string.IsNullOrEmpty(path);
            }

            path = null;
            return false;
        }

        public static bool GetFolderPath(out string path)
        {
            var folderBrowserDialog = new FolderBrowserDialog();

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                path = folderBrowserDialog.SelectedPath;
                return !string.IsNullOrEmpty(path);
            }

            path = null;
            return false;
        }

        public static bool GetColor(out Color color, Color startColor)
        {
            ColorDialog colorDialog = new ColorDialog()
            {
                AnyColor = true,
                FullOpen = true,
                Color = startColor
            };

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                color = colorDialog.Color;
                return true;
            }

            color = startColor;
            return false;
        }

        public static bool GetColor(out Color color)
        {
            return GetColor(out color, Color.White);
        }
    }
}
