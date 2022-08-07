using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CountingLibrary.Main;

namespace CountingLibrary.Core
{
    public class Workspace : INotifyPropertyChanged
    {
        private DirectoryInfo DirectoryInfo { get; set; }
        public Settings Settings { get; private set; } = new();
        internal Dictionary<string, List<string>> FileInfos { get; private set; } = new();
        public List<SymbolInfo> SymbolInfos { get; set; } = new();
        private Stopwatch Stopwatch { get; set; } = new();

        private string time = "00:00:00:00";
        public string Time
        {
            get { return time; }
            set
            {
                time = value;
                OnPropertyChanged();
            }
        }

        private int symbolsCount;
        public int SymbolsCount
        {
            get { return symbolsCount; }
            set
            {
                symbolsCount = value;
                OnPropertyChanged();
            }
        }
        public string DirectoryPath
        {
            get { return DirectoryInfo is null ? string.Empty : DirectoryInfo.FullName; }
        }
        public ManualResetEvent ManualResetEvent { get; private set; } = new(false);
        public bool IsRunning { get; private set; } = false;
        public static Workspace WorkspaceInstance { get; set; }

        public Workspace(DirectoryInfo directoryInfo)
        {
            DirectoryInfo = directoryInfo;
            PrepareSymbolInfos();
            WorkspaceInstance = this;
        }
        public Workspace(string path)
        {
            DirectoryInfo = new(path);
            PrepareSymbolInfos();
            WorkspaceInstance = this;
        }
        #region Settings
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
        #endregion

        #region Scan
        public void FastScan()
        {
            FileInfos.Clear();
            foreach (string fullFileName in GetFiles())
            {
                try
                {
                    FileInfos.Add(fullFileName, File.ReadAllLines(fullFileName).ToList());
                }
                catch (IOException)
                {
                    //log
                }
            }
        }
        private void Scan()
        {
            foreach (KeyValuePair<string, List<string>> keyValuePair in FileInfos)
            {
                if (!IsRunning)
                    break;
                for (int i = 0; i < keyValuePair.Value.Count; i++)
                {
                    if (!IsRunning)
                        break;
                    foreach (char c in keyValuePair.Value[i])
                    {
                        //add char c
                        //добавить проверку на соответствие символа с заданными параметрами символов
                        //например: не добавлять латинские буквы, если выбран русский алфавит

                        ManualResetEvent.WaitOne();

                        if (!IsRunning)
                            break;
                        SymbolsCount++;
                        if (SymbolInfos.Where(x => x.Symbol == c).Any())
                            SymbolInfos.Where(x => x.Symbol == c).First().AddCount();
                        foreach (SymbolInfo symbolInfo in SymbolInfos)
                        {
                            symbolInfo.UpdatePercent();
                        }
                        Time = Stopwatch.Elapsed.ToString();
                    }
                }
            }
            ManualResetEvent.Reset();
            IsRunning = false;
            Stopwatch.Stop();
        }
        public void PrepareScan()
        {
            if (!FileInfos.Any())
                FastScan();
        }
        #endregion
        
        #region Start and Stop
        public void Start()
        {
            Stopwatch.Reset();
            Stopwatch.Start();
            IsRunning = true;
            ManualResetEvent.Set();
            ResetOldData();
            PrepareScan();
            Scan();
        }
        public void Pause()
        {
            ManualResetEvent.Reset();
            Stopwatch.Stop();
        }
        public void Continue()
        {
            Stopwatch.Start();
            ManualResetEvent.Set();
        }
        public void Stop()
        {
            ManualResetEvent.Set();
            IsRunning = false;
            ResetOldData();
        }
        #endregion

        #region Helper
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        private void ResetOldData()
        {
            SymbolsCount = 0;
            for (int i = 0; i < SymbolInfos.Count; i++)
            {
                SymbolInfos[i].ResetCount();
            }
        }
        private void PrepareSymbolInfos()
        {
            for (int i = 0; i < Info.Default.Symbols.Length; i++)
            {
                SymbolInfos.Add(new SymbolInfo(Info.Default.Symbols[i]));
            }
            for (int i = 0; i < Info.Default.Numbers.Length; i++)
            {
                SymbolInfos.Add(new SymbolInfo(Info.Default.Numbers[i]));
            }
            for (int i = 0; i < Info.Default.Alphabet.Letters.Length; i++)
            {
                SymbolInfos.Add(new SymbolInfo(Info.Default.Alphabet.Letters[i]));
            }
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
        #endregion
    }
}