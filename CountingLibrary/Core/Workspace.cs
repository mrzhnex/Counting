using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CountingLibrary.Events;
using CountingLibrary.Handlers;
using CountingLibrary.Main;

namespace CountingLibrary.Core
{
    public class Workspace : INotifyPropertyChanged
    {
        private DirectoryInfo DirectoryInfo { get; set; }
        public Settings Settings { get; private set; } = new();
        internal Dictionary<string, List<string>> FileInfos { get; private set; } = new();
        public List<SymbolInfo> SymbolInfos { get; private set; } = new();
        public List<char> Symbols { get; private set; } = new();
        private Stopwatch Stopwatch { get; set; } = new();

        private string time = Info.Default.InitialTime;
        public string Time
        {
            get { return time; }
            set
            {
                time = value;
                OnPropertyChanged();
            }
        }

        private ulong symbolsCount;
        public ulong SymbolsCount
        {
            get { return symbolsCount; }
            private set
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
        public static Workspace WorkspaceInstance { get; set; } = new(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        public Sort Sort { get; private set; } = Sort.Alphabet;

        public Workspace(DirectoryInfo directoryInfo)
        {
            DirectoryInfo = directoryInfo;
            PrepareSymbols();
            PrepareSymbolInfos();
            WorkspaceInstance = this;
        }
        public Workspace(string path)
        {
            DirectoryInfo = new(path);
            PrepareSymbols();
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
        public void SortBy(Sort sort)
        {
            Sort = sort;
            switch (Sort)
            {
                case Sort.Alphabet:
                    SymbolInfos = SymbolInfos.OrderBy(x => x.Symbol).ToList();
                    break;
                case Sort.Count:
                    SymbolInfos = SymbolInfos.OrderBy(x => x.Count).Reverse().ToList();
                    break;
                default:
                    break;
            }
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
            double seconds = 0;
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
                        ManualResetEvent.WaitOne();

                        if (!IsRunning)
                            break;
                        if (SymbolInfos.Any(x => x.Symbol == char.ToLower(c)))
                        {
                            SymbolsCount++;
                            SymbolInfos.First(x => x.Symbol == char.ToLower(c)).AddCount();
                        }

                        foreach (SymbolInfo symbolInfo in SymbolInfos)
                        {
                            symbolInfo.UpdatePercent();
                        }
                        Time = Stopwatch.Elapsed.ToString();
                        if (Stopwatch.Elapsed.TotalSeconds > seconds)
                        {
                            seconds = Stopwatch.Elapsed.TotalSeconds + 0.5d;
                            SortBy(Sort);
                            Action.Main.Manage.ManageInstance.ExecuteEvent<IEventHandlerSort>(new SortEvent());
                        }
                    }
                    if (i != keyValuePair.Value.Count - 1)
                    {
                        SymbolsCount++;
                        SymbolInfos.First(x => x.Symbol == char.ToLower('\n')).AddCount();
                    }
                }
            }
            SortBy(Sort);
            Action.Main.Manage.ManageInstance.ExecuteEvent<IEventHandlerSort>(new SortEvent());
            ManualResetEvent.Reset();
            Stopwatch.Stop();
            if (!IsRunning)
            {
                Time = Info.Default.InitialTime;
                ResetOldData();
            }
            IsRunning = false;
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
                SymbolInfos.Add(new SymbolInfo(char.ToLower(Info.Default.Symbols[i])));
            }
            for (int i = 0; i < Info.Default.Numbers.Length; i++)
            {
                SymbolInfos.Add(new SymbolInfo(char.ToLower(Info.Default.Numbers[i])));
            }
            for (int i = 0; i < Info.Default.Alphabet.Letters.Length; i++)
            {
                SymbolInfos.Add(new SymbolInfo(char.ToLower(Info.Default.Alphabet.Letters[i])));
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
        private void PrepareSymbols()
        {
            for (int i = 0; i < Info.Default.Symbols.Length; i++)
            {
                Symbols.Add(Info.Default.Symbols[i]);
            }
            for (int i = 0; i < Info.Default.Numbers.Length; i++)
            {
                Symbols.Add(Info.Default.Numbers[i]);
            }
            for (int i = 0; i < Info.Default.Alphabet.Letters.Length; i++)
            {
                Symbols.Add(Info.Default.Alphabet.Letters[i]);
            }
            Symbols = Symbols.OrderBy(x => x).ToList();
        }
        #endregion
    }
}