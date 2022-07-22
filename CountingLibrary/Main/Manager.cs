using CountingLibrary.Core;

namespace CountingLibrary.Main
{
    public class Manager
    {
        public Workspace Workspace { get; private set; }

        public bool AddWorkspace(string path)
        {
            DirectoryInfo d = new(path);
            if (!d.Exists)
                return false;
            Workspace = new(d);
            return true;
        }
    }
}