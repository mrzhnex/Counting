using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CountingLibrary.Core
{
    public class SymbolInfo : INotifyPropertyChanged
    {
        public string Symbol { get; private set; }
        public string SymbolView { get; private set; }
        private int count;
        public int Count
        {
            get { return count; }
            private set
            {
                count = value;
                if (Workspace.WorkspaceInstance.Settings.UpdateInRealTime)
                {
                    OnPropertyChanged();
                    UpdatePercent();
                }
            }
        }
        private double percent;
        public double Percent
        {
            get { return percent; }
            private set
            {
                percent = value;
                OnPropertyChanged();
            }
        }

        public SymbolInfo(string symbol)
        {
            Symbol = symbol;
            SymbolView = symbol.ToString();
            if (symbol == "\n")
                SymbolView = "enter";
            else if (symbol == " ")
                SymbolView = "пробел";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        internal void UpdatePercent()
        {
            if (Count == 0)
            {
                Percent = 0;
                return;
            }
            Percent = Math.Round(Workspace.WorkspaceInstance.SymbolInfos.Any(x => x.Symbol == Symbol) ? Workspace.WorkspaceInstance.SymbolInfos.First(x => x.Symbol == Symbol).Count / (float)Workspace.WorkspaceInstance.SymbolsCount * 100 : 0.0f, 2);
        }
        internal void AddCount()
        {
            Count++;
        }
        internal void AddCount(int count)
        {
            Count += count;
        }
        internal void ResetCount()
        {
            Count = 0;
            Percent = 0;
        }
        internal void ForceUpdate()
        {
            OnPropertyChanged(nameof(Count));
            UpdatePercent();
        }
        public override string ToString()
        {
            return Symbol.ToString();
        }
    }
}