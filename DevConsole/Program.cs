using CountingLibrary.Core;

namespace DevConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("START");

            Workspace workspace = new(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            string[] files = workspace.GetFiles();
            foreach (string file in files)
            {
                Console.WriteLine($"filename '{file}'");
            }
            workspace.Start();
            List<SymbolInfo> symbolInfos = workspace.SymbolInfos;
            foreach (SymbolInfo symbolInfo in symbolInfos)
            {
                Console.WriteLine($"{symbolInfo}:{symbolInfo.Count}; percent:{symbolInfo.Percent}");
            }
            Console.WriteLine("END");
            Console.ReadKey();
        }
    }
}