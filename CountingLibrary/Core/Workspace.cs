namespace CountingLibrary.Core
{
    public class Workspace
    {
        private DirectoryInfo DirectoryInfo { get; set; }
        public Settings Settings { get; private set; } = new();
        internal Dictionary<string, List<string>> FileInfos { get; set; } = new();

        internal List<FileInfo> Files { get; set; } = new();

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
            return Settings.FileExtensions.Remove(extension);
        }
        public bool AddFileExtension(string extension)
        {
            if (Settings.FileExtensions.Contains(extension))
                return false;
            Settings.FileExtensions.Add(extension);
            return true;
        }
        public string[] GetFiles()
        {
            try
            {
                return DirectoryInfo.GetFiles("*.*", Settings.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Where(x => Settings.FileExtensions.Contains(Path.GetExtension(x.Name).ToLower())).Select(x => x.FullName).ToArray();
            }
            catch (UnauthorizedAccessException)
            {
                return Array.Empty<string>();
            }
        }
        public void FastScan()
        {
            Files.Clear();
            FileInfos.Clear();
            foreach (string fullFileName in GetFiles())
            {
                FileInfos.Add(fullFileName, File.ReadAllLines(fullFileName).ToList());
            }
        }
        public void Scan()
        {
            if (!FileInfos.Any())
                FastScan();
            foreach (KeyValuePair<string, List<string>> keyValuePair in FileInfos)
            {
                for (int i = 0; i < keyValuePair.Value.Count; i++)
                {
                    foreach (char c in keyValuePair.Value[i])
                    {
                        //add char c
                        //добавить проверку на соответствие символа с заданными параметрами символов
                        //например: не добавлять латинские буквы, если выбран русский алфавит
                        if (Files.Where(x => x.FullName == keyValuePair.Key).Any())
                            Files.Where(x => x.FullName == keyValuePair.Key).First().AddSymbol(c);
                        else
                        {
                            Files.Add(new FileInfo(keyValuePair.Key));
                            Files.Where(x => x.FullName == keyValuePair.Key).First().AddSymbol(c);
                        }
                    }
                }
            }
        }
        public List<FileInfo> GetFileInfos()
        {
            return Files;
        }
    }
}