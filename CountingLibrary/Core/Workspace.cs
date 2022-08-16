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
        private HardDriveManager HardDriveManager { get; set; }
        public Settings Settings { get; private set; } = new();
        internal Dictionary<string, List<string>> FileInfos { get; private set; } = new();
        public List<SymbolInfo> SymbolInfos { get; private set; } = new();
        public List<char> Symbols { get; private set; } = new();
        private Stopwatch Stopwatch { get; set; } = new();

        private string timeSpent = Info.Default.InitialTime;
        public string TimeSpent
        {
            get { return timeSpent; }
            private set
            {
                timeSpent = value;
                OnPropertyChanged();
            }
        }
        private string timeLeft = Info.Default.InitialTime;
        public string TimeLeft
        {
            get { return timeLeft; }
            private set
            {
                timeLeft = value;
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
        private ulong wrongSymbolsCount;
        public ulong WrongSymbolsCount
        {
            get { return wrongSymbolsCount; }
            private set
            {
                wrongSymbolsCount = value;
                OnPropertyChanged();
            }
        }

        public ManualResetEvent ManualResetEvent { get; private set; } = new(false);
        public bool IsRunning { get; private set; } = false;
        public static Workspace WorkspaceInstance { get; set; } = new(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        public Sort Sort { get; private set; } = Sort.Default;
        private ulong TotalSymbolsCount { get; set; }
        private bool TimeLeftIsOver { get; set; }

        public Workspace(string path)
        {
            HardDriveManager = new(new(path));
            Settings = HardDriveManager.LoadSettings();
            PrepareSymbols();
            PrepareSymbolInfos();
            WorkspaceInstance = this;
        }
        public Workspace(string path, Settings settings)
        {
            HardDriveManager = new(new(path));
            Settings = settings;
            PrepareSymbols();
            PrepareSymbolInfos();
            WorkspaceInstance = this;
        }

        #region Settings
        public void SaveSettings()
        {
            HardDriveManager.SaveSettings();
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
                case Sort.Default:
                    SymbolInfos = SymbolInfos.OrderBy(x => Symbols.IndexOf(x.Symbol)).ToList();
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
            foreach (string fullFileName in HardDriveManager.GetFiles())
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
            foreach (KeyValuePair<string, List<string>> keyValuePair in FileInfos)
            {
                for (int i = 0; i < keyValuePair.Value.Count; i++)
                {
                    TotalSymbolsCount += (ulong)keyValuePair.Value[i].Length;
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
                            if (Settings.UpdateInRealTime)
                            {
                                foreach (SymbolInfo symbolInfo in SymbolInfos)
                                {
                                    symbolInfo.UpdatePercent();
                                }
                            }
                        }
                        else
                        {
                            WrongSymbolsCount++;
                        }
                
                        TimeSpent = Stopwatch.Elapsed.ToString(Info.Default.TimeParseString);
                        if (!TimeLeftIsOver)
                            TimeLeft = CalculateTimeLeft();
                        if (Settings.UpdateInRealTime && Stopwatch.Elapsed.TotalSeconds > seconds)
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
            foreach (SymbolInfo symbolInfo in SymbolInfos)
            {
                symbolInfo.ForceUpdate();
                symbolInfo.UpdatePercent();
            }
            SortBy(Sort);
            Action.Main.Manage.ManageInstance.ExecuteEvent<IEventHandlerSort>(new SortEvent());
            ManualResetEvent.Reset();
            Stopwatch.Stop();
            if (!IsRunning)
            {
                TimeSpent = Info.Default.InitialTime;
                TimeLeft = Info.Default.InitialTime;
                TimeLeftIsOver = false;
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
            WrongSymbolsCount = 0;
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
        private void PrepareSymbols()
        {
            for (int i = 0; i < Info.Default.Alphabet.Letters.Length; i++)
            {
                Symbols.Add(Info.Default.Alphabet.Letters[i]);
            }
            for (int i = 0; i < 2; i++)
            {
                Symbols.Add(Info.Default.Symbols[i]);
            }
            for (int i = 0; i < Info.Default.Numbers.Length; i++)
            {
                Symbols.Add(Info.Default.Numbers[i]);
            }
            for (int i = 2; i < Info.Default.Symbols.Length; i++)
            {
                Symbols.Add(Info.Default.Symbols[i]);
            }
        }
        private string CalculateTimeLeft()
        {
            ulong processedSymbolsCount = SymbolsCount + WrongSymbolsCount;
            double averageTimePerSymbol = Stopwatch.Elapsed.TotalSeconds / processedSymbolsCount;
            try
            {
                return TimeSpan.FromSeconds(averageTimePerSymbol * (TotalSymbolsCount - processedSymbolsCount)).ToString(Info.Default.TimeParseString);
            }
            catch (OverflowException)
            {
                TimeLeftIsOver = true;
                return Info.Default.InitialTime;
            }
        }
        #endregion
    }
}