using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using CountingLibrary.Events;
using CountingLibrary.Handlers;
using CountingLibrary.Main;
using Microsoft.Office.Interop.Word;

namespace CountingLibrary.Core
{
    public class Workspace : INotifyPropertyChanged
    {
        private HardDriveManager HardDriveManager { get; set; }
        public Settings Settings { get; private set; } = new();
        internal Dictionary<string, List<string>> FileInfos { get; private set; } = new();
        public ObservableCollection<SymbolInfo> SymbolInfos
        {
            get { return symbolInfos; }
            private set
            {
                symbolInfos = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<SymbolInfo> symbolInfos = new();
        public List<string> SymbolsOne { get; private set; } = new();
        public List<string> SymbolsTwo { get; private set; } = new();
        public List<string> SymbolsThree { get; private set; } = new();
        private Stopwatch Stopwatch { get; set; } = new();
        public string TimeSpent
        {
            get { return timeSpent; }
            private set
            {
                timeSpent = value;
                OnPropertyChanged();
            }
        }
        private string timeSpent = Info.Default.InitialTime;
        public string TimeLeft
        {
            get { return timeLeft; }
            private set
            {
                timeLeft = value;
                OnPropertyChanged();
            }
        }
        private string timeLeft = Info.Default.InitialTime;
        public ulong SymbolsCount
        {
            get { return symbolsCount; }
            private set
            {
                symbolsCount = value;
                OnPropertyChanged();
            }
        }
        private ulong symbolsCount;
        public ulong WrongSymbolsCount
        {
            get { return wrongSymbolsCount; }
            private set
            {
                wrongSymbolsCount = value;
                OnPropertyChanged();
            }
        }
        private ulong wrongSymbolsCount;
        public ManualResetEvent ManualResetEvent { get; private set; } = new(false);
        public bool IsRunning { get; private set; } = false;
        public static Workspace WorkspaceInstance { get; set; } = new();
        public Sort Sort { get; private set; } = Sort.Default;
        private ulong TotalSymbolsCount { get; set; }
        private bool TimeLeftIsOver { get; set; }
        public SymbolInfo SymbolInfo
        {
            get { return symbolInfo; }
            set
            {
                symbolInfo = value;
                OnPropertyChanged();
            }
        }
        private SymbolInfo symbolInfo = new(string.Empty);

        private Workspace()
        {
            HardDriveManager = new(Array.Empty<string>());
            Settings = HardDriveManager.LoadSettings(out _);
            PrepareSymbols();
            PrepareSymbolInfos();
            WorkspaceInstance = this;
        }
        public Workspace(string folder, Settings settings)
        {
            HardDriveManager = new(new DirectoryInfo(folder));
            Settings = settings;
            PrepareSymbols();
            PrepareSymbolInfos();
            WorkspaceInstance = this;
        }
        public Workspace(string[] files, Settings settings)
        {
            HardDriveManager = new(files);
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
            switch (Settings.GetProcessingType())
            {
                case ProcessingType.OneSymbol:
                case ProcessingType.TwoSymbols:
                case ProcessingType.ThreeSymbols:
                    switch (Sort)
                    {
                        case Sort.Alphabet:
                            SymbolInfos = new ObservableCollection<SymbolInfo>(SymbolInfos.OrderBy(x => x.Symbol));
                            break;
                        case Sort.Count:
                            SymbolInfos = new ObservableCollection<SymbolInfo>(SymbolInfos.OrderBy(x => x.Count).Reverse());
                            break;
                        case Sort.Default:
                            SymbolInfos = new ObservableCollection<SymbolInfo>(SymbolInfos.OrderBy(x => SymbolsOne.IndexOf(x.Symbol)));
                            break;
                        default:
                            break;
                    }
                    break;
            }
            Action.Main.Manage.ManageInstance.ExecuteEvent<IEventHandlerSort>(new SortEvent(Sort));
        }
        public void ChangeProcessingType()
        {
            FullReset();
            Action.Main.Manage.ManageInstance.ExecuteEvent<IEventHandlerChangeProcessingType>(new ChangeProcessingTypeEvent(WorkspaceInstance.Settings.GetProcessingType()));
        }
        #endregion

        #region Scan
        public void FastScan()
        {
            FileInfos.Clear();
            foreach (string fullFileName in HardDriveManager.Files)
            {
                try
                {
                    if (IsMicrosoftOfficeFile(fullFileName))
                    {
                        Application application = new();
                        Document document = application.Documents.Open(fullFileName, ReadOnly: true);
                        FileInfos.Add(fullFileName, new List<string>() { document.Content.Text });
                        document.Close();
                        application.Quit();
                    }
                    else
                        FileInfos.Add(fullFileName, File.ReadAllLines(fullFileName).ToList());
                }
                catch (IOException) { }
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
            string word = string.Empty;
            new Thread(UpdatePercents).Start();
            new Thread(UpdateTime).Start();
            new Thread(UpdateSort).Start();
            foreach (KeyValuePair<string, List<string>> keyValuePair in FileInfos)
            {
                if (!IsRunning)
                    break;
                for (int i = 0; i < keyValuePair.Value.Count; i++)
                {
                    if (!IsRunning)
                        break;
                    switch (Settings.GetProcessingType())
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
                                }
                                else
                                {
                                    WrongSymbolsCount++;
                                }
                            }
                            break;
                        case ProcessingType.TwoSymbols:
                            foreach (char c in keyValuePair.Value[i])
                            {
                                ManualResetEvent.WaitOne();
                                if (!IsRunning)
                                    break;
                                word = string.Concat(word, c).ToLower();
                                if (word.Length > 2)
                                    word = word.Remove(0, 1);
                                if (SymbolInfos.Any(x => x.Symbol == word))
                                {
                                    SymbolsCount++;
                                    SymbolInfos.First(x => x.Symbol == word).AddCount();
                                }
                                else
                                {
                                    WrongSymbolsCount++;
                                }
                            }
                            break;
                        case ProcessingType.ThreeSymbols:
                            foreach (char c in keyValuePair.Value[i])
                            {
                                ManualResetEvent.WaitOne();
                                if (!IsRunning)
                                    break;
                                word = string.Concat(word, c).ToLower();
                                if (word.Length > 3)
                                    word = word.Remove(0, 1);
                                if (SymbolInfos.Any(x => x.Symbol == word))
                                {
                                    SymbolsCount++;
                                    SymbolInfos.First(x => x.Symbol == word).AddCount();
                                }
                                else if (SymbolsThree.Contains(word))
                                {
                                    SymbolsCount++;
                                    SymbolInfo newSymbolInfo = new(word);
                                    newSymbolInfo.AddCount();
                                    System.Windows.Application.Current.Dispatcher.Invoke(() => SymbolInfos.Add(newSymbolInfo));
                                }
                                else
                                {
                                    WrongSymbolsCount++;
                                }
                            }
                            break;
                        case ProcessingType.Word:
                            foreach (char c in keyValuePair.Value[i])
                            {
                                ManualResetEvent.WaitOne();
                                if (!IsRunning)
                                    break;
                                word = string.Concat(word, c).ToLower();
                                if (word.Length > SymbolInfo.Symbol.Length)
                                    word = word.Remove(0, 1);
                                if (SymbolInfo.Symbol == word)
                                {
                                    SymbolsCount++;
                                    SymbolInfo.AddCount();
                                }
                                else
                                {
                                    WrongSymbolsCount++;
                                }
                            }
                            break;
                    }

                    if (Settings.GetProcessingType() == ProcessingType.OneSymbol)
                    {
                        if (i != keyValuePair.Value.Count - 1)
                        {
                            SymbolsCount++;
                            SymbolInfos.First(x => x.Symbol == "\r").AddCount();
                        }
                    }
                    else
                    {
                        word = string.Concat(word, "\r").ToLower();
                        if (Settings.GetProcessingType() == ProcessingType.TwoSymbols && word.Length > 2)
                        {
                            word = word.Remove(0, 1);
                        }
                        else if (Settings.GetProcessingType() == ProcessingType.ThreeSymbols && word.Length > 3)
                        {
                            word = word.Remove(0, 1);
                        }
                    }
                }
            }
            foreach (SymbolInfo symbolInfo in SymbolInfos)
            {
                symbolInfo.ForceUpdate();
            }
            SymbolInfo.ForceUpdate();
            SortBy(Sort);
            Action.Main.Manage.ManageInstance.ExecuteEvent<IEventHandlerSort>(new SortEvent(Sort));
            ManualResetEvent.Reset();
            Stopwatch.Stop();
            if (!IsRunning)
            {
                TimeSpent = Info.Default.InitialTime;
                TimeLeftIsOver = false;
                ResetOldData();
            }
            IsRunning = false;
            TimeLeft = Info.Default.InitialTime;
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
            TimeSpent = Info.Default.InitialTime;
            TimeLeft = Info.Default.InitialTime;
            TimeLeftIsOver = false;
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
        public void FullReset()
        {
            ResetOldData();
            SymbolInfos.Clear();
            PrepareSymbolInfos();
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public void Save(string fileName)
        {
            HardDriveManager.SaveResult(fileName, Settings.FontFamily, Settings.FontSize, Settings.GetProcessingType());
        }
        public bool IsPaused()
        {
            return IsRunning && !Stopwatch.IsRunning;
        }
        private void ResetOldData()
        {
            ResetSystemData();
            for (int i = 0; i < SymbolInfos.Count; i++)
            {
                SymbolInfos[i].ResetCount();
            }
            SymbolInfo = new(Settings.Word);
        }
        private void ResetSystemData()
        {
            TimeSpent = Info.Default.InitialTime;
            TimeLeft = Info.Default.InitialTime;
            SymbolsCount = 0;
            WrongSymbolsCount = 0;
        }
        private void PrepareSymbols()
        {
            for (int i = 0; i < Info.Default.Alphabet.Letters.Length; i++)
            {
                SymbolsOne.Add(Info.Default.Alphabet.Letters[i].ToString());
            }
            for (int i = 0; i < 3; i++)
            {
                SymbolsOne.Add(Info.Default.Symbols[i].ToString());
            }
            for (int i = 0; i < Info.Default.Numbers.Length; i++)
            {
                SymbolsOne.Add(Info.Default.Numbers[i].ToString());
            }
            for (int i = 3; i < Info.Default.Symbols.Length; i++)
            {
                SymbolsOne.Add(Info.Default.Symbols[i].ToString());
            }

            List<string> symbols = new();
            for (int i = 0; i < Info.Default.Alphabet.Letters.Length; i++)
            {
                symbols.Add(Info.Default.Alphabet.Letters[i].ToString());
            }
            for (int i = 0; i < 9; i++)
            {
                symbols.Add(Info.Default.Symbols[i].ToString());
            }
            foreach (string s in symbols)
            {
                foreach (string s2 in symbols)
                {
                    SymbolsTwo.Add(s + s2);
                    foreach (string s3 in symbols)
                    {
                        SymbolsThree.Add(s + s2 + s3);
                    }
                }
            }
            SymbolInfo = new(Settings.Word);
        }
        private void PrepareSymbolInfos()
        {
            switch (Settings.GetProcessingType())
            {
                case ProcessingType.OneSymbol:
                    for (int i = 0; i < SymbolsOne.Count; i++)
                    {
                        SymbolInfos.Add(new(SymbolsOne[i]));
                    }
                    break;
                case ProcessingType.TwoSymbols:
                    for (int i = 0; i < SymbolsTwo.Count; i++)
                    {
                        SymbolInfos.Add(new(SymbolsTwo[i]));
                    }
                    break;
                case ProcessingType.ThreeSymbols:
                    break;
                case ProcessingType.Word:
                    SymbolInfo = new(Settings.Word);
                    break;
            }
        }
        private string CalculateTimeLeft()
        {
            ulong processedSymbolsCount = SymbolsCount + WrongSymbolsCount;
            double averageTimePerSymbol = Stopwatch.Elapsed.TotalSeconds / processedSymbolsCount;

            switch (Settings.GetProcessingType())
            {
                case ProcessingType.TwoSymbols:
                case ProcessingType.ThreeSymbols:
                    processedSymbolsCount = (SymbolsCount * 2) + WrongSymbolsCount;
                    break;
            }
            try
            {
                return TimeSpan.FromSeconds(averageTimePerSymbol * (TotalSymbolsCount - processedSymbolsCount)).ToString(Info.Default.TimeParseString);
            }
            catch (OverflowException)
            {
                TimeLeftIsOver = true;
            }
            return Info.Default.InitialTime;
        }
        private void UpdatePercents()
        {
            while (IsRunning)
            {
                ManualResetEvent.WaitOne();
                if (Settings.UpdateInRealTime)
                {
                    try
                    {
                        foreach (SymbolInfo symbolInfo in SymbolInfos)
                        {
                            symbolInfo.UpdatePercent();
                        }
                    }
                    catch (InvalidOperationException) { }
                }
                Thread.Sleep(300);
            }
        }
        private void UpdateTime()
        {
            while (IsRunning)
            {
                Thread.Sleep(100);
                TimeSpent = Stopwatch.Elapsed.ToString(Info.Default.TimeParseString);
                if (!TimeLeftIsOver)
                    TimeLeft = CalculateTimeLeft();
            }
        }
        private void UpdateSort()
        {
            double seconds = 0;
            double delay = 0.5d;
            switch (Settings.GetProcessingType())
            {
                case ProcessingType.TwoSymbols:
                case ProcessingType.ThreeSymbols:
                    delay = 2.5d;
                    break;
                case ProcessingType.Word:
                    return;
            }
            while (IsRunning)
            {
                if (Settings.UpdateInRealTime && Stopwatch.Elapsed.TotalSeconds > seconds)
                {
                    seconds = Stopwatch.Elapsed.TotalSeconds + delay;
                    SortBy(Sort);
                }
            }
        }
        private bool IsMicrosoftOfficeFile(string fileName)
        {
            return new string[] { ".docx", ".doc", ".docs", ".odt", ".rtf" }.Contains(Path.GetExtension(fileName).ToLower());
        }
        private bool IsMicrosoftOfficeNormalFile(string fileName)
        {
            return new string[] { ".docx" }.Contains(Path.GetExtension(fileName).ToLower());
        }
        #endregion
    }
}