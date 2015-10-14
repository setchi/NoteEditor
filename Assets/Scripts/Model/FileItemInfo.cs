namespace NoteEditor.Model
{
    public class FileItemInfo
    {
        public bool isDirectory;
        public string fullName;

        public FileItemInfo(bool isDirectory, string fullName)
        {
            this.isDirectory = isDirectory;
            this.fullName = fullName;
        }
    }
}
