using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CountingLibrary.Core
{
    public class SymbolInfo : INotifyPropertyChanged
    {
        public string Symbol { get; private set; }
        public string SymbolView { get; private set; }
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
        private int count;
        public double Percent
        {
            get { return percent; }
            private set
            {
                percent = value;
                OnPropertyChanged();
                PercentView = percent.ToString("0.00");
            }
        }
        private double percent;
        public string PercentView
        {
            get { return percentView; }
            set
            {
                percentView = value;
                OnPropertyChanged();
            }
        }
        private string percentView = "0.00";

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