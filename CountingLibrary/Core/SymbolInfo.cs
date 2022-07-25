namespace CountingLibrary.Core
{
    public class SymbolInfo
    {
        public char Symbol { get; set; }
        public int Count { get; set; } = 1;

        internal SymbolInfo(char symbol)
        {
            Symbol = symbol;
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