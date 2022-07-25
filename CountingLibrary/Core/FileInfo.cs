namespace CountingLibrary.Core
{
    public class FileInfo
    {
        public string FullName { get; set; }
        public List<SymbolInfo> SymbolInfos { get; set; } = new();
        public int SymbolsCount { get; set; } = 0;

        internal FileInfo(string fullName)
        {
            FullName = fullName;
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