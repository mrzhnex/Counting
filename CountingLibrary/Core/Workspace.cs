using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CountingLibrary.Events;
using CountingLibrary.Handlers;
using CountingLibrary.Main;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Office.Interop.Word;

namespace CountingLibrary.Core
{
    public class Workspace : INotifyPropertyChanged
    {
        private HardDriveManager HardDriveManager { get; set; }
        public Settings Settings { get; private set; } = new();
        internal Dictionary<string, List<string>> FileInfos { get; private set; } = new();
        public ObservableCollection<SymbolInfo> SymbolInfos { get; private set; } = new();
        public List<string> SymbolsOne { get; private set; } = new();
        public List<string> SymbolsTwo { get; private set; } = new();
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

        public Workspace(string path)
        {
            HardDriveManager = new(new(path));
            Settings = HardDriveManager.LoadSettings(out _);
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
            switch (Settings.GetProcessingType())
            {
                case ProcessingType.OneSymbol:
                case ProcessingType.TwoSymbols:
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
            foreach (string fullFileName in HardDriveManager.GetFiles())
            {
                try
                {
                    if (IsMicrosoftOfficeNormalFile(fullFileName))
                    {
                        WordprocessingDocument wordprocessingDocument = WordprocessingDocument.Open(fullFileName, false);
                        FileInfos.Add(fullFileName, new List<string>() { wordprocessingDocument.MainDocumentPart.Document.Body.InnerText });                      
                    }
                    else if (IsMicrosoftOfficeFile(fullFileName))
                    {
                        Document document = new Application().Documents.Open(fullFileName, ReadOnly: true);
                        FileInfos.Add(fullFileName, new List<string>() { document.Content.Text });                       
                    }
                    else
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
            string word = string.Empty;
            char oldC = '\n';
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
                            foreach (char c in keyValuePair.Value[i])
                            {
                                ManualResetEvent.WaitOne();

                                if (!IsRunning)
                                    break;
                                word = string.Concat(oldC, c).ToLower();
                                
                                if (SymbolInfos.Any(x => x.Symbol == word))
                                {
                                    SymbolsCount++;
                                    SymbolInfos.First(x => x.Symbol == word).AddCount();
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
                                oldC = c;
                                TimeSpent = Stopwatch.Elapsed.ToString(Info.Default.TimeParseString);
                                if (!TimeLeftIsOver)
                                    TimeLeft = CalculateTimeLeft();
                                if (Settings.UpdateInRealTime && Stopwatch.Elapsed.TotalSeconds > seconds)
                                {
                                    seconds = Stopwatch.Elapsed.TotalSeconds + 2.5d;
                                    SortBy(Sort);
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
                                    word = word.Remove(0);

                                if (SymbolInfo.Symbol == word)
                                {
                                    SymbolsCount++;
                                    SymbolInfo.AddCount();
                                }
                                else
                                {
                                    WrongSymbolsCount++;
                                }
                                TimeSpent = Stopwatch.Elapsed.ToString(Info.Default.TimeParseString);
                                if (!TimeLeftIsOver)
                                    TimeLeft = CalculateTimeLeft();
                            }
                            break;
                    }
                    
                    if (Settings.GetProcessingType() == ProcessingType.OneSymbol)
                    {
                        if (i != keyValuePair.Value.Count - 1)
                        {
                            SymbolsCount++;
                            SymbolInfos.First(x => x.Symbol == "\n").AddCount();
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
            for (int i = 0; i < 2; i++)
            {
                SymbolsOne.Add(Info.Default.Symbols[i].ToString());
            }
            for (int i = 0; i < Info.Default.Numbers.Length; i++)
            {
                SymbolsOne.Add(Info.Default.Numbers[i].ToString());
            }
            for (int i = 2; i < Info.Default.Symbols.Length; i++)
            {
                SymbolsOne.Add(Info.Default.Symbols[i].ToString());
            }

            List<string> symbols = new();
            for (int i = 0; i < Info.Default.Alphabet.Letters.Length; i++)
            {
                symbols.Add(Info.Default.Alphabet.Letters[i].ToString());
            }
            for (int i = 2; i < 9; i++)
            {
                symbols.Add(Info.Default.Symbols[i].ToString());
            }
            foreach (string s in symbols)
            {
                foreach (string s2 in symbols)
                {
                    SymbolsTwo.Add(s + s2);
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
        private bool IsMicrosoftOfficeFile(string fileName)
        {
            return new string[] { ".doc", ".docs", ".odt" }.Contains(Path.GetExtension(fileName).ToLower());
        }
        private bool IsMicrosoftOfficeNormalFile(string fileName)
        {
            return new string[] { ".docx" }.Contains(Path.GetExtension(fileName).ToLower());
        }
        #endregion
    }
}