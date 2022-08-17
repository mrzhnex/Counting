using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using CountingLibrary.Events;
using CountingLibrary.Handlers;
using CountingLibrary.Main;

namespace CountingLibrary.Core
{
    public class Workspace : INotifyPropertyChanged, IEventHandlerChangeProcessingType
    {
        private HardDriveManager HardDriveManager { get; set; }
        public Settings Settings { get; private set; } = new();
        internal Dictionary<string, List<string>> FileInfos { get; private set; } = new();
        public ObservableCollection<SymbolInfo> SymbolInfos { get; private set; } = new();
        public List<string> Symbols { get; private set; } = new();
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
            Settings = HardDriveManager.LoadSettings(out _);
            PrepareSymbols();
            PrepareSymbolInfos();
            WorkspaceInstance = this;
            Action.Main.Manage.ManageInstance.RegisterAllEvents(this);
        }
        public Workspace(string path, Settings settings)
        {
            HardDriveManager = new(new(path));
            Settings = settings;
            PrepareSymbols();
            PrepareSymbolInfos();
            WorkspaceInstance = this;
            Action.Main.Manage.ManageInstance.RegisterAllEvents(this);
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
                    SymbolInfos = new ObservableCollection<SymbolInfo>(SymbolInfos.OrderBy(x => x.Symbol));
                    break;
                case Sort.Count:
                    SymbolInfos = new ObservableCollection<SymbolInfo>(SymbolInfos.OrderBy(x => x.Count).Reverse());
                    break;
                case Sort.Default:
                    SymbolInfos = new ObservableCollection<SymbolInfo>(SymbolInfos.OrderBy(x => Symbols.IndexOf(x.Symbol)));
                    break;
                default:
                    break;
            }
            Action.Main.Manage.ManageInstance.ExecuteEvent<IEventHandlerSort>(new SortEvent(Sort));
        }
        public void OnChangeProcessingType(ChangeProcessingTypeEvent changeProcessingTypeEvent)
        {
            ResetOldData();
            ResetSymbols();
            PrepareSymbols();
            PrepareSymbolInfos();
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

                    switch (Settings.ProcessingTypes[Settings.ProcessingType])
                    {
                        case ProcessingType.OneSymbol:
                            foreach (char c in keyValuePair.Value[i])
                            {
                                ManualResetEvent.WaitOne();

                                if (!IsRunning)
                                    break;
                                if (SymbolInfos.Any(x => x.Symbol == c.ToString().ToLower()))
                                {
                                    SymbolsCount++;
                                    SymbolInfos.First(x => x.Symbol == c.ToString().ToLower()).AddCount();
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
                                }
                            }
                            break;
                        case ProcessingType.TwoSymbols:
                            foreach (SymbolInfo symbolInfo in SymbolInfos)
                            {
                                int count = 0;
                                try
                                {
                                    count = new Regex(symbolInfo.Symbol).Matches(keyValuePair.Value[i]).Count;
                                }
                                catch (RegexParseException) { }
                                if (count > 0)
                                {
                                    symbolInfo.AddCount(count);
                                    SymbolsCount += (ulong)count;
                                }
                                TimeSpent = Stopwatch.Elapsed.ToString(Info.Default.TimeParseString);
                                if (!TimeLeftIsOver)
                                    TimeLeft = CalculateTimeLeft();
                                if (Settings.UpdateInRealTime && Stopwatch.Elapsed.TotalSeconds > seconds)
                                {
                                    seconds = Stopwatch.Elapsed.TotalSeconds + 1.5d;
                                    SortBy(Sort);
                                }
                            }
                            break;
                        case ProcessingType.Word:

                            break;
                    }
                    
                    
                    if (i != keyValuePair.Value.Count - 1)
                    {
                        SymbolsCount++;
                        SymbolInfos.First(x => x.Symbol == "\n").AddCount();
                    }
                }
            }
            foreach (SymbolInfo symbolInfo in SymbolInfos)
            {
                symbolInfo.ForceUpdate();
            }
            SortBy(Sort);
            Action.Main.Manage.ManageInstance.ExecuteEvent<IEventHandlerSort>(new SortEvent(Sort));
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
            ResetSystemData();
            for (int i = 0; i < SymbolInfos.Count; i++)
            {
                SymbolInfos[i].ResetCount();
            }
        }
        private void ResetSystemData()
        {
            TimeSpent = Info.Default.InitialTime;
            TimeLeft = Info.Default.InitialTime;
            SymbolsCount = 0;
            WrongSymbolsCount = 0;
        }
        private void ResetSymbols()
        {
            Symbols.Clear();
            SymbolInfos.Clear();
        }
        private void PrepareSymbols()
        {
            switch (Settings.ProcessingTypes[Settings.ProcessingType])
            {
                case ProcessingType.OneSymbol:
                    for (int i = 0; i < Info.Default.Alphabet.Letters.Length; i++)
                    {
                        Symbols.Add(Info.Default.Alphabet.Letters[i].ToString());
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        Symbols.Add(Info.Default.Symbols[i].ToString());
                    }
                    for (int i = 0; i < Info.Default.Numbers.Length; i++)
                    {
                        Symbols.Add(Info.Default.Numbers[i].ToString());
                    }
                    for (int i = 2; i < Info.Default.Symbols.Length; i++)
                    {
                        Symbols.Add(Info.Default.Symbols[i].ToString());
                    }
                    break;
                case ProcessingType.TwoSymbols:
                    List<string> symbols = new();
                    for (int i = 0; i < Info.Default.Alphabet.Letters.Length; i++)
                    {
                        symbols.Add(Info.Default.Alphabet.Letters[i].ToString());
                    }
                    for (int i = 2; i < 9; i++)
                    {
                        symbols.Add(Info.Default.Symbols[i].ToString());
                    }
                    Symbols.Add("\n");
                    foreach (string s in symbols)
                    {
                        foreach (string s2 in symbols)
                        {
                            Symbols.Add(s + s2);
                        }
                    }
                    break;
                case ProcessingType.Word:
                    break;
            }
        }
        private void PrepareSymbolInfos()
        {
            for (int i = 0; i < Symbols.Count; i++)
            {
                SymbolInfos.Add(new(Symbols[i]));
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