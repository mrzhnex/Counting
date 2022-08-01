using System.ComponentModel;
using System.Runtime.CompilerServices;
using CountingLibrary.Main;

namespace CountingLibrary.Core
{
    public class SymbolInfo : INotifyPropertyChanged
    {
        public char Symbol { get; set; }
        private int count;
        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                OnPropertyChanged();
                UpdatePercent();
            }
        }
        private float percent;
        public float Percent
        {
            get { return percent; }
            set
            {
                percent = value;
                OnPropertyChanged();
            }
        }

        internal SymbolInfo(char symbol)
        {
            Symbol = symbol;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        private void UpdatePercent()
        {
            Percent = Manager.ManagerInstance.Workspace.SymbolInfos.Any(x => x.Symbol == Symbol) ? Manager.ManagerInstance.Workspace.SymbolInfos.First(x => x.Symbol == Symbol).Count / (float)Manager.ManagerInstance.Workspace.SymbolsCount * 100 : 0.0f;
        }
        internal void AddCount()
        {
            Count++;
        }
        public override string ToString()
        {
            return Symbol.ToString();
        }
    }
}