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
            List<SymbolInfo> symbolInfos = manager.Workspace.SymbolInfos;
            foreach (SymbolInfo symbolInfo in symbolInfos)
            {
                Console.WriteLine($"{symbolInfo}:{symbolInfo.Count}; percent:{symbolInfo.Percent}");
            }
            Console.WriteLine("END");
            Console.ReadKey();
        }
    }
}