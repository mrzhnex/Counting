using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CountingLibrary.Core
{
    public class FileInfo : INotifyPropertyChanged
    {
        public string FullName { get; set; }
        public List<SymbolInfo> SymbolInfos { get; set; } = new();
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

        internal FileInfo(string fullName)
        {
            FullName = fullName;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        internal void AddSymbol(char symbol)
        {
            SymbolsCount++;
            if (SymbolInfos.Where(x => x.Symbol == symbol).Any())
                SymbolInfos.Where(x => x.Symbol == symbol).First().AddCount();
            else
                SymbolInfos.Add(new SymbolInfo(symbol));
        }
        public float GetPercent(char symbol)
        {
            return SymbolInfos.Where(x => x.Symbol == symbol).Any() ? SymbolInfos.Where(x => x.Symbol == symbol).First().Count / (float)SymbolsCount * 100 : 0.0f;
        }
    }
}