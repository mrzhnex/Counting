using CountingLibrary.Main;

namespace DevConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("START");


            Manager manager = new();
            manager.AddWorkspace("D:/Workspace");
            string[] files = manager.Workspace.GetFiles();
            foreach (string file in files)
            {
                Console.WriteLine($"filename '{file}'");
            }
            Console.WriteLine("END");
            Console.ReadKey();
        }
    }
}