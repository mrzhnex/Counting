using CountingLibrary.Core;
using CountingLibrary.Main;

namespace DevConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("START");


            Manager manager = new();
            manager.AddWorkspace("D:/Workspace/Heap/Counting");
            string[] files = manager.Workspace.GetFiles();
            foreach (string file in files)
            {
                Console.WriteLine($"filename '{file}'");
            }
            manager.Workspace.FastScan();
            manager.Workspace.Scan();
            List<CountingLibrary.Core.FileInfo> fileInfos = manager.Workspace.Files;
            foreach (CountingLibrary.Core.FileInfo fileInfo in fileInfos)
            {
                Console.WriteLine($"{fileInfo.FullName}:{fileInfo.SymbolsCount}");
                float percent = 0.0f;
                foreach (SymbolInfo symbolInfo in fileInfo.SymbolInfos)
                {
                    Console.WriteLine($"{symbolInfo}:{symbolInfo.Count}; percent:{fileInfo.GetPercent(symbolInfo.Symbol)}");
                    percent += fileInfo.GetPercent(symbolInfo.Symbol);
                }
                Console.WriteLine($"Total percent:{percent}");
            }
            Console.WriteLine("END");
            Console.ReadKey();
        }
    }
}