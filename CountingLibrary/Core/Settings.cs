using CountingLibrary.Main;

namespace CountingLibrary.Core
{
    public class Settings
    {
        public List<string> FileExtensions { get; private set; } = Info.Default.FileExtensions.ToList();
        public bool IncludeSubfolders { get; set; } = true;
        public Alphabet Alphabet { get; set; } = Alphabet.Ru;
    }
}