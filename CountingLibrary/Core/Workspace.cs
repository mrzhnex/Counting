using CountingLibrary.Main;

namespace CountingLibrary.Core
{
    public class Workspace
    {
        private DirectoryInfo DirectoryInfo { get; set; }
        public List<string> FileExtensions { get; private set; } = Info.Default.FileExtensions.ToList();
        public bool IncludeSubfolders { get; set; } = true;
        public string DirectoryPath
        {
            get { return DirectoryInfo is null ? string.Empty : DirectoryInfo.FullName; }
        }

        public Workspace(DirectoryInfo directoryInfo)
        {
            DirectoryInfo = directoryInfo;
        }

        public bool RemoveFileExtension(string extension)
        {
            return FileExtensions.Remove(extension);
        }
        public bool AddFileExtension(string extension)
        {
            if (FileExtensions.Contains(extension))
                return false;
            FileExtensions.Add(extension);
            return true;
        }
        public string[] GetFiles()
        {
            try
            {
                return DirectoryInfo.GetFiles("*.*", SearchOption.AllDirectories).Where(x => FileExtensions.Contains(Path.GetExtension(x.Name).ToLower())).Select(x => x.FullName).ToArray();
            }
            catch (UnauthorizedAccessException)
            {
                return Array.Empty<string>();
            }
        }
    }
    internal struct SymbolInfo
    {
        internal char Symbol { get; set; }
        internal int Count { get; set; }
        internal double Percent { get; set; }
    }
}