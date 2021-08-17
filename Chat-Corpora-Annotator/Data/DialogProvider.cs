using System.Windows.Forms;

namespace ChatCorporaAnnotator.Data
{
    internal static class DialogProvider
    {
        public static bool OpenCsvFile(out string path)
        {
            int filterIndex = 0;
            string title = "Open a separated-value file";
            string filter = "CSV files|*.csv|TSV files|*.tsv";

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
    }
}
