using System.ComponentModel;
using System.Runtime.CompilerServices;

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